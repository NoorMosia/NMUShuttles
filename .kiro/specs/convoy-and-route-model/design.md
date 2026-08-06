# Design Document — Convoy and Route Model

## Overview

This document specifies the schema changes, service modifications, and API additions required to
implement the convoy-and-route-model improvements. It is an additive and corrective layer on top
of the `nmu-shuttle-management` design. Where this document conflicts with the original design,
this document takes precedence.

---

## Schema Changes

### New Table: StopShuttleDefault

Holds the standing default convoy roster per Stop. Every trip injected for a Stop inherits these
allocations automatically.

| Column | Type | Constraints |
|---|---|---|
| StopShuttleDefaultId | INT IDENTITY | PK |
| StopId | INT | FK → Stops NOT NULL |
| ShuttleId | INT | FK → Shuttles NOT NULL |
| OrderPosition | INT | NOT NULL — 1-based, accessible shuttles first |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |
| UNIQUE | (StopId, ShuttleId) | — no duplicate shuttle per stop |
| UNIQUE | (StopId, OrderPosition) | — unique position per stop roster |

**Ordering rule:** `IsAccessibleVehicle = TRUE` shuttles always get the lowest `OrderPosition`
values. When an accessible shuttle is added, non-accessible entries shift up by 1. This mirrors
the existing `ConvoyOrderingService` logic exactly.

**Advisory-only approach:** All write operations on this table (INSERT, UPDATE, DELETE) are
accepted immediately. There is no server-side date enforcement — the data model inherently
protects existing trips because `StopShuttleDefault` only feeds into the nightly Rolling Injection
Job. Trips already created (T+0 through T+2) have their own `TripShuttleAllocation` rows and are
unaffected. The admin portal displays an advisory banner communicating this.

**Shuttle decommissioning:** When a Shuttle is deactivated or deleted, a cascade removes all
`StopShuttleDefault` records referencing it. The service re-normalises OrderPositions on each
affected Stop to maintain contiguous ordering.


---

### New Table: ScheduleTimeCampus

Replaces the single `ScheduleTime.CampusId` foreign key with an ordered junction table that
captures the full sequence of campuses a shuttle visits on a route.

| Column | Type | Constraints |
|---|---|---|
| ScheduleTimeCampusId | INT IDENTITY | PK |
| ScheduleTimeId | INT | FK → ScheduleTimes NOT NULL |
| CampusId | INT | FK → Campuses NOT NULL |
| StopOrder | INT | NOT NULL — 1-based physical sequence along the route |
| UNIQUE | (ScheduleTimeId, CampusId) | — no duplicate campus per route |
| UNIQUE | (ScheduleTimeId, StopOrder) | — unique position per route |

**Migration note:** The existing `ScheduleTime.CampusId` column is removed. All existing
`ScheduleTime` records must be migrated: for each record, create one `ScheduleTimeCampus` row
with `StopOrder = 1` and `CampusId` equal to the old value. Remaining campuses on the route are
then added via the admin portal. This migration is non-destructive; the old column is dropped
only after the junction table is populated and validated.

---

### Modified Table: Shuttles

| Change | Detail |
|---|---|
| Add `IsActive` column | BIT NOT NULL DEFAULT 1 — enables soft-deactivation while preserving history |

The `Shuttle` entity currently has no lifecycle flag. Adding `IsActive` aligns it with the
existing pattern used by `Stop.IsActive` and `Student.IsActive`. All shuttle pickers, default
convoy operations, and injection queries filter by `IsActive = TRUE`.

---

### Modified Table: ScheduleTimes

| Change | Detail |
|---|---|
| Remove `CampusId` column | Replaced by `ScheduleTimeCampus` junction table |
| Add `RequiresScanning` column | BIT NOT NULL DEFAULT 1 — controls QR scan gate per schedule slot |


---

### New Table: TripNotes

Freeform admin notes attached to a TripInstance. Replaces the structured resolution fields as the
primary incident narrative mechanism, while the `BreakdownLog` structured fields are retained for
analytics.

| Column | Type | Constraints |
|---|---|---|
| TripNoteId | INT IDENTITY | PK |
| TripInstanceId | INT | FK → TripInstances NOT NULL |
| NoteType | NVARCHAR(20) | NOT NULL CHECK IN ('Incident','Replacement','General','NoShow') |
| Body | NVARCHAR(MAX) | NOT NULL (min 1 char enforced by API) |
| CreatedByAdminId | INT | FK → Admins NOT NULL |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |
| IsResolutionAction | BIT | NOT NULL DEFAULT 0 — set TRUE when note triggers BreakdownLog resolution |

**Resolution trigger:** When an Admin creates a `TripNote` with `NoteType IN ('Incident',
'Replacement')` on an `IncidentActive` trip, the backend:
1. Sets `IsResolutionAction = TRUE` on the note.
2. Updates the open `BreakdownLog`: `ResolvedAt = UtcNow`, `ResolvedByAdminId = admin`.
3. Optionally updates `BreakdownLog.ResolutionType` and `BreakdownLog.ReplacementShuttleId` if
   the admin provides them in the same request.
4. Dispatches FCM push notifications to all Confirmed Booking holders on the trip.
5. Transitions `TripInstance.Status` back to `InTransit` (if resolution type is replacement) or
   leaves it as `IncidentActive` (if cancelled, no replacement).

---

### Indexes to Add

```sql
CREATE INDEX IX_StopShuttleDefaults_StopId ON StopShuttleDefaults (StopId);
CREATE INDEX IX_ScheduleTimeCampuses_ScheduleTimeId ON ScheduleTimeCampuses (ScheduleTimeId);
CREATE INDEX IX_TripNotes_TripInstanceId ON TripNotes (TripInstanceId);
CREATE INDEX IX_TripNotes_TripInstanceId_NoteType ON TripNotes (TripInstanceId, NoteType);
CREATE INDEX IX_Shuttles_IsActive ON Shuttles (IsActive) WHERE IsActive = 1;
```


---

## Updated Entity Relationship

```
Stop ──< StopShuttleDefault ──> Shuttle   (default convoy roster per stop)

ScheduleTime ──< ScheduleTimeCampus ──> Campus   (ordered campus waypoints)
ScheduleTime.RequiresScanning  (new flag — controls scan gate)

TripInstance ──< TripNote ──> Admin   (freeform incident and operational notes)

Shuttle.IsActive  (new flag — enables soft-deactivation with history preservation)
```

All other relationships from the original design are unchanged.

---

## Service Changes

### StopDefaultConvoyService (new)

Manages `StopShuttleDefault` records with accessible-first ordering. No date enforcement is
needed — edits only affect future injection runs.

```
AssignShuttleToDefaultAsync(stopId, shuttleId):
  1. Validate shuttle exists and IsActive = true → 422 if inactive
  2. Load existing StopShuttleDefault records for stop, ordered by OrderPosition
  3. Apply same accessible-first insertion logic as ConvoyOrderingService:
     - accessible shuttle → insert after last accessible, shift non-accessible up
     - non-accessible shuttle → append at end
  4. INSERT new StopShuttleDefault record
  5. Save

RemoveShuttleFromDefaultAsync(stopId, shuttleId):
  1. DELETE StopShuttleDefault record
  2. Re-normalise OrderPositions (contiguous from 1)
  3. Save

ReorderDefaultAsync(stopId, orderedShuttleIds[]):
  1. Validate accessible-first invariant in proposed order
  2. UPDATE OrderPosition for each record to match proposed order
  3. Save

CleanupDecommissionedShuttleAsync(shuttleId):
  1. Find all StopShuttleDefault records for shuttleId
  2. For each affected Stop:
     a. DELETE the record
     b. Re-normalise remaining OrderPositions (contiguous from 1)
  3. Save
```


### RollingInjectionService (modified)

Step 5 of the existing algorithm gains a new sub-step after creating each `TripInstance`:

```
After INSERT TripInstance:
  defaultAllocations = SELECT * FROM StopShuttleDefaults
                       JOIN Shuttles ON Shuttles.ShuttleId = StopShuttleDefaults.ShuttleId
                       WHERE StopId = tripInstance.StopId
                         AND Shuttles.IsActive = TRUE
                       ORDER BY OrderPosition ASC

  FOR EACH default IN defaultAllocations:
    INSERT TripShuttleAllocation (
      TripInstanceId = tripInstance.TripInstanceId,
      ShuttleId = default.ShuttleId,
      OrderPosition = default.OrderPosition,
      IsOperational = TRUE,
      AssignedAt = UtcNow
    )
    ON CONFLICT (TripInstanceId, ShuttleId) → SKIP (idempotency)
```

The `IsActive` filter is a defensive guard — normally inactive shuttles are removed from
`StopShuttleDefault` immediately on deactivation, but the filter protects against race conditions.

The priority booking injection step (Step 6) is unchanged — it still reads `OrderPosition = 1`
from `TripShuttleAllocations`, which is now populated automatically.

---

### CheckInService (modified — FromCampus location binding)

The existing `FromCampus` check is replaced:

```
// OLD (single campus FK)
if (marshal.AssignedCampusId != scheduleTime.CampusId) → 403

// NEW (junction table — any campus on the route is valid)
var routeCampusIds = await _db.ScheduleTimeCampuses
    .Where(stc => stc.ScheduleTimeId == scheduleTime.ScheduleTimeId)
    .Select(stc => stc.CampusId)
    .ToListAsync();

if (!routeCampusIds.Contains(marshal.AssignedCampusId)) → 403
```

Additionally, a `RequiresScanning` check is retained as a safety net. If a scan request arrives
for a non-scanning trip (e.g., from an outdated client), it returns a soft rejection:

```
if (!scheduleTime.RequiresScanning)
    return new CheckInResult {
        Success = false,
        Message = "This trip is open boarding — no QR scan required.",
        StatusCode = StatusCodes.Status200OK
    };
```

The primary enforcement is client-side: the marshal app disables the scan button and shows an
informational banner when `requiresScanning = false` on the trip.


### TripNoteService (new)

```
CreateTripNoteAsync(tripInstanceId, noteType, body, resolveIncident, replacementShuttleId?, adminId):
  1. Validate body is non-empty → 422 if empty
  2. Create TripNote record (IsResolutionAction = false initially)
  3. If resolveIncident == true AND noteType IN ('Incident', 'Replacement') AND trip.Status == 'IncidentActive':
       a. Set TripNote.IsResolutionAction = true
       b. Find open BreakdownLog for this trip (ResolvedAt IS NULL)
       c. Set BreakdownLog.ResolvedAt = UtcNow, ResolvedByAdminId = adminId
       d. If replacementShuttleId provided:
            - Patch allocation: ShuttleId = replacementShuttleId, IsOperational = true
            - Set BreakdownLog.ResolutionType = 'Replaced'
            - Set BreakdownLog.ReplacementShuttleId = replacementShuttleId
            - Set TripInstance.Status = 'InTransit'
          Else:
            - Set BreakdownLog.ResolutionType = 'Cancelled'
            - TripInstance.Status remains 'IncidentActive'
       e. Dispatch FCM to all confirmed passengers (title: "Trip Update", body: noteType + first 100 chars)
  4. If resolveIncident == false OR noteType NOT IN ('Incident', 'Replacement') OR trip.Status != 'IncidentActive':
       → Note is saved as a normal informational note without side effects
  5. Save all changes
  6. Return TripNote
```

---

## API Changes

### New Endpoints

| Method | Route | Role | Description |
|---|---|---|---|
| GET | `/api/v1/admin/stops/{stopId}/default-convoy` | Admin | Get the default convoy for a stop |
| POST | `/api/v1/admin/stops/{stopId}/default-convoy` | Admin | Add a shuttle to the default convoy |
| DELETE | `/api/v1/admin/stops/{stopId}/default-convoy/{shuttleId}` | Admin | Remove a shuttle from the default convoy |
| PATCH | `/api/v1/admin/stops/{stopId}/default-convoy/reorder` | Admin | Reorder the default convoy |
| GET | `/api/v1/admin/stops/{stopId}/trips` | Admin | List trips for a stop (stop-centric view) |
| GET | `/api/v1/admin/schedule-times/{id}/campuses` | Admin | Get ordered campus sequence for a route |
| POST | `/api/v1/admin/schedule-times/{id}/campuses` | Admin | Add a campus to a route |
| DELETE | `/api/v1/admin/schedule-times/{id}/campuses/{campusId}` | Admin | Remove a campus from a route |
| PATCH | `/api/v1/admin/schedule-times/{id}/campuses/reorder` | Admin | Reorder campuses on a route |
| POST | `/api/v1/admin/trips/{tripId}/notes` | Admin | Create a trip note (may trigger incident resolution) |
| GET | `/api/v1/admin/trips/{tripId}/notes` | Admin | Get all notes for a trip |
| GET | `/api/v1/trips/{tripId}/convoy` | Marshal, Admin | Get convoy order + live boarded counts per shuttle |

**Convoy endpoint routing:** The convoy status endpoint is accessible to both Marshal and Admin
roles. It is added as a new action on the **existing `TripsController`** (route prefix
`/api/v1/trips`, already `[Authorize]` for all roles). The specific action uses
`[Authorize(Roles = "Marshal,Admin")]` to restrict access. This avoids creating a separate
controller at the same route prefix and keeps convoy data co-located with other trip endpoints.


### Key Request / Response Shapes

**POST `/api/v1/admin/stops/{stopId}/default-convoy`**
```json
// Request
{ "shuttleId": 7 }
// Response 201
{
  "stopShuttleDefaultId": 12,
  "shuttleId": 7,
  "licensePlate": "EC 142-837",
  "isAccessibleVehicle": true,
  "orderPosition": 1,
  "capacity": 22
}
// Response 409: shuttle already in default for this stop
```

**POST `/api/v1/admin/trips/{tripId}/notes`**
```json
// Request
{
  "noteType": "Replacement",
  "body": "Shuttle ABC 123 broke down 2km from stop. Covered by Bay Transit Corp vehicle EC 999-ZZZ (not yet registered). Driver: John M.",
  "resolveIncident": true,
  "replacementShuttleId": 9
}
// Response 201
{
  "tripNoteId": 44,
  "noteType": "Replacement",
  "body": "Shuttle ABC 123 broke down...",
  "isResolutionAction": true,
  "createdAt": "2026-08-06T11:43:00Z",
  "notificationsSent": 32
}
// Response 422: empty body
```

**GET `/api/v1/trips/{tripId}/convoy`**
```json
// Response 200 (used by marshal app for convoy panel)
{
  "tripInstanceId": 42,
  "requiresScanning": true,
  "shuttles": [
    {
      "allocationId": 5,
      "orderPosition": 1,
      "shuttleId": 3,
      "licensePlate": "EC 101-NMU",
      "isAccessibleVehicle": true,
      "capacity": 22,
      "boardedCount": 14,
      "isOperational": true
    },
    {
      "allocationId": 6,
      "orderPosition": 2,
      "shuttleId": 7,
      "licensePlate": "EC 142-837",
      "isAccessibleVehicle": false,
      "capacity": 35,
      "boardedCount": 0,
      "isOperational": true
    }
  ]
}
```

**GET `/api/v1/admin/stops/{stopId}/trips`**
```json
// Response 200
{
  "stopId": 2,
  "stopName": "Summerstrand Village",
  "trips": [
    {
      "tripInstanceId": 42,
      "tripDate": "2026-08-06",
      "departureTime": "07:00",
      "direction": "ToCampus",
      "status": "Locked",
      "shuttleCount": 2,
      "totalCapacity": 57,
      "confirmedCount": 45,
      "hasEmptyConvoy": false
    },
    {
      "tripInstanceId": 47,
      "tripDate": "2026-08-06",
      "departureTime": "08:30",
      "direction": "ToCampus",
      "status": "Scheduling",
      "shuttleCount": 0,
      "totalCapacity": 0,
      "confirmedCount": 12,
      "hasEmptyConvoy": true
    }
  ]
}
```


---

## Flutter Changes

### Admin Portal

- **New: Stop Detail Screen** (`stop_detail_screen.dart`):
  - Navigated to from `stops_screen.dart` by tapping a stop row
  - Header: Stop name, full address, GPS, active status, edit button
  - "Default Convoy" panel (new):
    - Lists current `StopShuttleDefault` records in `OrderPosition` order
    - "Add Shuttle" button → shuttle picker dialog (filters by `IsActive = true`) → calls
      `POST /admin/stops/{id}/default-convoy`
    - Each row has a "Remove" action → calls `DELETE` endpoint
    - Drag-to-reorder capability → calls `PATCH reorder` endpoint
    - Advisory banner: "Changes apply to trips created from tonight onwards (earliest: {T+3 date}).
      Trips for the next 3 days already have their convoy — edit those from the trip detail screen."
  - "View Trips" button → navigates to stop-centric trip view

- **New: Trip Detail Screen** (`trip_detail_screen.dart`):
  - Navigated to from `trips_screen.dart` or the stop-centric trip view by tapping a trip row
  - Header: Trip date, departure time, direction, stop name, status badge
  - Campus Route section (read-only): ordered list of campuses from `ScheduleTimeCampus`
  - Convoy Allocation panel: current `TripShuttleAllocation` records with add/remove/reorder
  - Trip Notes section (new, Requirement 6):
    - Lists all `TripNote` records for the trip in chronological order
    - "Add Note" form: NoteType selector + Body textarea + optional Replacement Shuttle selector
    - Notes with `IsResolutionAction = true` are visually highlighted as resolution events
    - When NoteType is 'Incident' or 'Replacement' AND trip status is 'IncidentActive':
      - Show a confirmation dialog before submit: "This note will resolve the active incident and
        send push notifications to all confirmed passengers. Proceed?"
      - On confirm: submit with `resolveIncident: true`
      - On cancel: return to form (user can change NoteType or save as informational note with
        `resolveIncident: false`)

- **Modified: Stops Screen** (`stops_screen.dart`):
  - Each stop row now navigable → Stop Detail Screen (on tap or "View Details" action)

- **Modified: Trips Screen** (`trips_screen.dart`):
  - Each trip row now navigable → Trip Detail Screen (on tap)
  - Remove inline expansion pattern; all detail lives on the new detail screen

- **Modified: ScheduleTime management screen** (`schedule_screen.dart`) gains:
  - "Campus Route" section: ordered list of campuses in `StopOrder` order
  - Add / remove / reorder campuses on the route
  - Required: at least 1 campus per route (validation)
  - `RequiresScanning` toggle switch on create/edit form
  - Labelled "Requires QR Scanning" — default ON
  - Help text: "Disable for open-boarding trips (e.g. late-morning ToCampus)"

- **Stop-Centric Trip View** (sub-screen of Stop Detail):
  - Shows trips for the stop, filterable by date (default today to today+3) and status
  - Each trip card shows: time, direction, status, capacity bar, shuttle count
  - Empty-convoy trips flagged with warning icon
  - Tap trip → navigate to Trip Detail Screen

### Marshal App

- **Trip detail / Passenger List screen** gains a **Convoy Panel** tab or section:
  - Shows each shuttle's `OrderPosition`, `LicensePlate`, accessibility indicator
  - Shows live `boardedCount / capacity` for each shuttle
  - Data sourced from `GET /trips/{tripId}/convoy`, polled on the same 30-second interval as the
    passenger manifest (fallback for changes made by other marshals on the same route)
  - After each successful check-in by this marshal, convoy provider is immediately invalidated
    and refreshed (instant feedback for own scans, no waiting for next poll)
  - Non-operational shuttles (IsOperational=false) displayed with strikethrough or greyed-out style

- **Scan button gating**: When `requiresScanning = false` on the trip, the scan button is
  disabled and replaced with an informational banner: "This trip is open boarding — no QR scan
  required." No server-side 422 is involved; the client prevents the action entirely.

---

## Migration Strategy

The schema changes are split across **two EF Core migrations** for safety:

### Migration 1: `AddConvoyAndRouteModelTables`
1. Add `Shuttle.IsActive` column (BIT NOT NULL DEFAULT 1)
2. Add `StopShuttleDefaults` table
3. Add `ScheduleTimeCampuses` table
4. Add `TripNotes` table
5. Add `ScheduleTime.RequiresScanning` column (default 1 — all existing slots require scanning)
6. Migrate existing `ScheduleTime.CampusId` data → insert one `ScheduleTimeCampus` row per
   existing `ScheduleTime` with `StopOrder = 1`
7. Mark `ScheduleTime.CampusId` as nullable (but do NOT drop it yet)
8. Add filtered index `IX_Shuttles_IsActive`

After this migration, deploy new code that reads from `ScheduleTimeCampuses` instead of
`ScheduleTime.CampusId`. The old column remains as a fallback during rollout.

### Code Sweep (between Migration 1 and Migration 2)

All code that references `ScheduleTime.CampusId` or `ScheduleTime.Campus` must be updated to use
the `ScheduleTimeCampuses` junction table before Migration 2 can be applied. Known references:

| File | Current reference | Required change |
|---|---|---|
| `ScheduleTime.cs` (model) | `public int CampusId` + nav property | Mark nullable, keep for backward compat until Migration 2 |
| `ShuttleDbContext.cs` | `ConfigureScheduleTime` — FK relationship to Campus | Update to nullable FK; add `ScheduleTimeCampuses` DbSet and config |
| `ScheduleAdminController.cs` | DTOs read/write `CampusId` | Response DTO returns list of campuses from junction; create DTO accepts campus list OR single campusId (backward compat) |
| `TripsAdminController.cs` | `.ThenInclude(st => st.Campus)` + `CampusName` in response | Load campuses from junction table; response returns `campuses: [{name, stopOrder}]` array |
| `TripsController.cs` (non-admin) | `.ThenInclude(st => st.Campus)` + `CampusName` in GetTrips and GetTrip responses | Load first campus (StopOrder=1) from junction table; return as `campusName` string for backward compat with mobile apps |
| `BookingsController.cs` | `ScheduleTime.Campus.CampusName` in 3 response locations | Load first campus (StopOrder=1) from junction table; return as `campusName` string for backward compat |
| `BookingService.cs` | `.ThenInclude(st => st.Campus)` eager loading (2 locations) | Replace with `.ThenInclude(st => st.ScheduleTimeCampuses)` |
| `CheckInService.cs` | `scheduleTime.CampusId` for FromCampus check | Replace with junction table query (already specified in Service Changes) |
| `Campus.cs` (model) | `ICollection<ScheduleTime> ScheduleTimes` nav property | Remove after Migration 2 |
| `SeedData.cs` | Seeds ScheduleTimes with `CampusId` values | Update to seed both `CampusId` (nullable, backward compat) AND `ScheduleTimeCampus` records |

**Backward compatibility for student/marshal responses:** Per Requirement 3.7, student-facing
responses do NOT expose the full campus sequence. Instead, `TripsController`, `BookingsController`,
and marshal trip list responses continue returning a single `campusName` field, computed as the
name of the first campus on the route (StopOrder = 1 from ScheduleTimeCampus). Only admin
responses (TripsAdminController, ScheduleAdminController) switch to the full `campuses` array.
This avoids breaking the Flutter mobile app models which expect `campusName: String`.

### Migration 2: `DropScheduleTimeCampusIdColumn`
1. Drop `ScheduleTime.CampusId` column and FK constraint
2. Remove `Campus.ScheduleTimes` navigation property reference from model config
3. Remove nullable `CampusId` property from `ScheduleTime` model

This migration is applied only after the new code is verified working with the junction table.

### DTO Updates Required

Existing DTOs that reference `CampusId` must be updated:

| DTO | Current shape | New shape |
|---|---|---|
| `ScheduleTimeResponseDto` | `CampusId: int` | `Campuses: [{campusId, campusName, stopOrder}]`, `RequiresScanning: bool` |
| `CreateScheduleTimeDto` | `CampusId: int` | `CampusIds: int[]` (ordered, first = StopOrder 1) |
| Trip list responses | `CampusName: string` | `Campuses: [{name, stopOrder}]` |

All existing `TripShuttleAllocations` are unaffected — they remain tied to their trips as-is.
The `StopShuttleDefault` table starts empty; the admin populates it via the portal.


---

## Architecture

This feature adds a **stop-centric configuration layer** and a **route model correction** to the
existing NMU Shuttle Management architecture. No new infrastructure components are introduced; all
changes are within the .NET API and Flutter surfaces.

### Architectural Additions

```
Admin Portal (Flutter Web)
  └─► Stop Detail Screen (new) — Default Convoy Management + View Trips
  └─► Trip Detail Screen (new) — Convoy Allocations + Trip Notes + Resolution
  └─► Campus Route Configuration (on ScheduleTime screen)
        ↓ HTTPS/REST
  .NET 9 Web API
    ├─► StopsAdminController (extended — default convoy endpoints)
    ├─► ScheduleAdminController (extended — campus route endpoints)
    ├─► TripsAdminController (extended — trip notes endpoints, stop-centric view)
    ├─► TripsController (extended — convoy status endpoint, shared Marshal+Admin)
    ├─► ShuttlesAdminController (extended — IsActive toggle + decommission cleanup)
    ├─► StopDefaultConvoyService (new)
    ├─► ScheduleTimeCampusService (new)
    ├─► TripNoteService (new)
    └─► RollingInjectionService (modified — auto-populate from defaults, IsActive filter)
          ↓
    SQL Server
      ├─► StopShuttleDefaults (new table)
      ├─► ScheduleTimeCampuses (new table)
      ├─► TripNotes (new table)
      ├─► Shuttles (IsActive column added)
      └─► ScheduleTimes (RequiresScanning column added, CampusId phased out)

Marshal App (Flutter Mobile)
  └─► Convoy Panel (on PassengerListScreen)
  └─► Scan button gating (RequiresScanning check)
        ↓ HTTPS/REST
  .NET 9 Web API
    └─► TripsController — GET /trips/{id}/convoy
          ↓
    SQL Server
      └─► TripShuttleAllocations + live boarded counts
```

### Design Principles

1. **Separation of concerns**: Default convoy configuration lives on `Stop` (static, long-term),
   per-trip allocations live on `TripInstance` (dynamic, operational). The injection job bridges
   the two.
2. **Backwards compatibility**: Existing `TripShuttleAllocation` records are unaffected.
   `StopShuttleDefault` starts empty; admin populates it over time. Trips with no default convoy
   continue to work as manual-assign trips.
3. **Inherent safety by design**: Default convoy edits only feed into future injection runs.
   Existing trips are protected by data model separation (StopShuttleDefault vs
   TripShuttleAllocation), not by date enforcement logic. An advisory banner communicates this
   to admins.
4. **Route model reflects physical reality**: Shuttles stop at all campuses on every route. The
   junction table captures this; `CheckInService` validates marshals are on the route.

---

## Components and Interfaces

### New Services

```csharp
public interface IStopDefaultConvoyService
{
    Task<StopShuttleDefaultDto> AssignShuttleToDefaultAsync(int stopId, int shuttleId);
    Task RemoveShuttleFromDefaultAsync(int stopId, int shuttleId);
    Task ReorderDefaultAsync(int stopId, int[] orderedShuttleIds);
    Task<IList<StopShuttleDefaultDto>> GetDefaultConvoyAsync(int stopId);
    Task CleanupDecommissionedShuttleAsync(int shuttleId);
}

public interface IScheduleTimeCampusService
{
    Task<ScheduleTimeCampusDto> AddCampusToRouteAsync(int scheduleTimeId, int campusId, int stopOrder);
    Task RemoveCampusFromRouteAsync(int scheduleTimeId, int campusId);
    Task ReorderRouteAsync(int scheduleTimeId, int[] orderedCampusIds);
    Task<IList<ScheduleTimeCampusDto>> GetRouteCampusesAsync(int scheduleTimeId);
}

public interface ITripNoteService
{
    Task<TripNoteDto> CreateTripNoteAsync(
        int tripInstanceId, string noteType, string body, bool resolveIncident,
        int? replacementShuttleId, int adminId);
    Task<IList<TripNoteDto>> GetTripNotesAsync(int tripInstanceId);
}
```

### Modified Services

**RollingInjectionService** gains a new step after each `TripInstance` creation:
```csharp
// After INSERT TripInstance
var defaults = await _db.StopShuttleDefaults
    .Include(d => d.Shuttle)
    .Where(d => d.StopId == tripInstance.StopId && d.Shuttle.IsActive)
    .OrderBy(d => d.OrderPosition)
    .ToListAsync();

foreach (var def in defaults)
{
    _db.TripShuttleAllocations.Add(new TripShuttleAllocation
    {
        TripInstanceId = tripInstance.TripInstanceId,
        ShuttleId = def.ShuttleId,
        OrderPosition = def.OrderPosition,
        IsOperational = true,
        AssignedAt = DateTime.UtcNow
    });
}
// ON CONFLICT handled by unique constraint — duplicates silently skipped
```

**ShuttlesAdminController** gains a deactivation endpoint:
```csharp
// PATCH /api/v1/admin/shuttles/{id}/deactivate
[HttpPatch("{id}/deactivate")]
public async Task<IActionResult> Deactivate(int id)
{
    var shuttle = await _db.Shuttles.FindAsync(id);
    if (shuttle == null) return NotFound();

    shuttle.IsActive = false;
    await _db.SaveChangesAsync();

    // Cleanup default convoy references
    await _stopDefaultConvoyService.CleanupDecommissionedShuttleAsync(id);

    return Ok(new { message = "Shuttle deactivated", shuttleId = id });
}

// PATCH /api/v1/admin/shuttles/{id}/reactivate
[HttpPatch("{id}/reactivate")]
public async Task<IActionResult> Reactivate(int id)
{
    var shuttle = await _db.Shuttles.FindAsync(id);
    if (shuttle == null) return NotFound();

    shuttle.IsActive = true;
    await _db.SaveChangesAsync();

    return Ok(new { message = "Shuttle reactivated", shuttleId = id });
}
```

**CheckInService** replaces the single campus FK check with a junction table query:
```csharp
// OLD
if (direction == "FromCampus" && marshal.AssignedCampusId != scheduleTime.CampusId)
    throw new CheckInException("Not authorized", 403);

// NEW
if (direction == "FromCampus")
{
    var routeCampuses = await _db.ScheduleTimeCampuses
        .Where(stc => stc.ScheduleTimeId == scheduleTime.ScheduleTimeId)
        .Select(stc => stc.CampusId)
        .ToListAsync();

    if (!routeCampuses.Contains(marshal.AssignedCampusId))
        throw new CheckInException("Marshal not on this route", 403);
}
```

And adds a `RequiresScanning` safety net before the 35-minute window check:
```csharp
if (!scheduleTime.RequiresScanning)
    return new CheckInResult
    {
        Success = false,
        Message = "This trip is open boarding — no QR scan required.",
        StatusCode = StatusCodes.Status200OK
    };
```

---

## Data Models

### DTOs

```csharp
public record StopShuttleDefaultDto(
    int StopShuttleDefaultId,
    int StopId,
    int ShuttleId,
    string LicensePlate,
    bool IsAccessibleVehicle,
    int Capacity,
    int OrderPosition,
    DateTime CreatedAt
);

public record ScheduleTimeCampusDto(
    int ScheduleTimeCampusId,
    int ScheduleTimeId,
    int CampusId,
    string CampusName,
    int StopOrder
);

public record TripNoteDto(
    int TripNoteId,
    int TripInstanceId,
    string NoteType,
    string Body,
    bool IsResolutionAction,
    int CreatedByAdminId,
    string CreatedByAdminName,
    DateTime CreatedAt,
    int? NotificationsSent
);

public record ConvoyStatusDto(
    int TripInstanceId,
    bool RequiresScanning,
    List<ConvoyShuttleDto> Shuttles
);

public record ConvoyShuttleDto(
    int AllocationId,
    int OrderPosition,
    int ShuttleId,
    string LicensePlate,
    bool IsAccessibleVehicle,
    int Capacity,
    int BoardedCount,
    bool IsOperational
);
```

### Existing DTO Updates

These existing DTOs must be modified during the code sweep:

```csharp
// BEFORE (existing)
public record ScheduleTimeResponseDto {
    public int CampusId { get; init; }
    // ...
}

// AFTER (updated)
public record ScheduleTimeResponseDto {
    public int? CampusId { get; init; }  // Nullable during transition, removed in Phase 2
    public List<ScheduleTimeCampusDto> Campuses { get; init; } = new();
    public bool RequiresScanning { get; init; }
    // ...
}

// BEFORE (existing)
public record CreateScheduleTimeDto {
    public int CampusId { get; init; }
    // ...
}

// AFTER (updated) — accepts either format during transition
public record CreateScheduleTimeDto {
    public int? CampusId { get; init; }        // Deprecated — kept for backward compat
    public List<int>? CampusIds { get; init; } // New — ordered list, first = StopOrder 1
    public bool RequiresScanning { get; init; } = true;
    // ...
}
```

---

## Correctness Properties

### Property C1: Default Convoy Inherent Safety

*For any* edit to a Stop's default convoy (add, remove, reorder), the System SHALL accept the
operation immediately. The edit takes effect only on TripInstances created by future Rolling
Injection Job runs. Existing TripInstances and their TripShuttleAllocations are never modified
by a default convoy edit.

**Validates:** Requirements 1.8 (advisory approach)

---

### Property C2: Stop Default Convoy Ordering Invariant

*For all* `StopShuttleDefault` records on a given Stop, OrderPosition values SHALL be unique,
contiguous starting from 1, and all accessible shuttles SHALL have OrderPosition values less than
or equal to all non-accessible shuttles.

**Validates:** Requirements 1.9, 1.10

---

### Property C3: Automatic Convoy Population Preservation

*For any* TripInstance created by the Rolling Injection Job for a Stop with N `StopShuttleDefault`
records, the System SHALL create exactly N `TripShuttleAllocation` records with OrderPosition
values matching the defaults' OrderPosition values exactly.

**Validates:** Requirements 2.1, 2.5

---

### Property C4: Campus Sequence Uniqueness

*For all* `ScheduleTimeCampus` records on a given ScheduleTime, StopOrder values SHALL be unique
and contiguous starting from 1.

**Validates:** Requirements 3.3

---

### Property C5: Marshal Multi-Campus Route Validation

*For any* check-in attempt by a Marshal on a FromCampus trip, the System SHALL accept the scan if
and only if the Marshal's `AssignedCampusId` exists in the set of `CampusId` values from
`ScheduleTimeCampus` records for that trip's ScheduleTime.

**Validates:** Requirements 3.4

---

### Property C6: RequiresScanning Client Gate

*For any* TripInstance where `ScheduleTime.RequiresScanning = FALSE`, the marshal app SHALL
disable the scan button client-side. If a scan request reaches the server despite client gating
(e.g., outdated app), the server returns a non-error 200 with informational message rather than
a 422. Booking creation, capacity tracking, and waitlist promotion continue normally regardless
of the RequiresScanning flag.

**Validates:** Requirements 4.3, 4.4, 4.7

---

### Property C7: Trip Note Body Non-Empty

*For any* TripNote creation request where `Body` is empty or whitespace-only, the System SHALL
reject the request with HTTP 422.

**Validates:** Requirements 6.7

---

### Property C8: Incident Resolution Trigger

*For any* TripNote created with `NoteType IN ('Incident', 'Replacement')` AND `resolveIncident =
true` on a TripInstance with `Status = 'IncidentActive'`, the System SHALL set the corresponding
`BreakdownLog.ResolvedAt` and dispatch FCM notifications to all Confirmed Booking holders on that
trip. If `resolveIncident = false`, the note is stored without triggering resolution logic.

**Validates:** Requirements 6.5, 6.6, 6.7

---

### Property C9: Shuttle IsActive Filtering

*For any* Rolling Injection Job run, the System SHALL only create TripShuttleAllocation records
for shuttles where `Shuttle.IsActive = TRUE` in the StopShuttleDefault roster. Deactivated
shuttles are excluded even if their StopShuttleDefault record still exists (race condition guard).

**Validates:** Requirements 8.6

---

### Property C10: Shuttle Deactivation Cascade

*When* a Shuttle's `IsActive` is set to `FALSE`, all `StopShuttleDefault` records referencing that
Shuttle SHALL be removed and OrderPositions on affected Stops re-normalised to contiguous values.
Existing `TripShuttleAllocation` records SHALL NOT be removed.

**Validates:** Requirements 8.3, 8.4

---

## Error Handling

All new endpoints follow the existing global error handling strategy (RFC 7807 ProblemDetails).
Specific error scenarios:

| Scenario | HTTP Status | Message |
|---|---|---|
| Duplicate shuttle in Stop's default convoy | 409 | "Shuttle {licensePlate} is already in the default convoy for this stop." |
| Attempt to reorder convoy with invalid accessible-first order | 422 | "Accessible shuttles must be positioned before non-accessible shuttles." |
| Empty TripNote body | 422 | "Trip note body cannot be empty." |
| Scan attempt on RequiresScanning=false trip (server safety net) | 200 | "This trip is open boarding — no QR scan required." (non-error, informational) |
| Marshal not on route (FromCampus) | 403 | "Marshal is not authorized to scan at this campus for this route." |
| Campus not found on route when adding | 404 | "Campus {id} not found." |
| Stop has no default convoy (GET) | 200 | Returns empty array `[]` (not an error — valid state) |
| Shuttle deactivated | 200 | StopShuttleDefault records auto-removed, positions re-normalised |
| Attempt to add inactive shuttle to default convoy | 422 | "Shuttle {licensePlate} is inactive and cannot be added to a convoy." |
| Attempt to add inactive shuttle to per-trip allocation | 422 | "Shuttle {licensePlate} is inactive and cannot be assigned to a trip." |

---

## Testing Strategy

### Unit Tests

- Accessible-first ordering: add shuttles in various orders, verify OrderPosition invariant holds
- RequiresScanning client gate: trip detail response with RequiresScanning=false includes flag,
  server safety net returns 200 informational (not 422)
- Multi-campus route validation: marshal assigned to CampusA, route has [CampusA, CampusB] → accepted;
  marshal assigned to CampusC, route has [CampusA, CampusB] → 403
- Trip note resolution trigger: create TripNote with NoteType='Replacement' and resolveIncident=true
  on IncidentActive trip → BreakdownLog.ResolvedAt set, notifications dispatched
- Trip note without resolution: create TripNote with NoteType='Replacement' and resolveIncident=false
  on IncidentActive trip → note saved, no resolution triggered
- Empty convoy on trip creation: Stop with no defaults → TripInstance created with zero allocations
- Shuttle decommissioning: deactivate shuttle → StopShuttleDefault records removed, positions
  re-normalised on affected stops
- IsActive filter on injection: stop has 3 defaults, one shuttle deactivated → only 2 allocations
  created on new trip
- Shuttle picker filters: only active shuttles (IsActive=true) returned in shuttle selection queries
- DTO backward compat: CreateScheduleTimeDto with single CampusId still works (creates one
  ScheduleTimeCampus); CampusIds array takes precedence if both provided

### Integration Tests

- End-to-end default convoy workflow: admin adds shuttles to Stop default → nightly job runs →
  new TripInstances created with auto-populated allocations
- Campus migration correctness: existing ScheduleTime.CampusId values correctly migrated to
  ScheduleTimeCampus with StopOrder=1
- Marshal convoy panel data: create trip with convoy, board some students → GET /trips/{id}/convoy
  returns accurate boarded counts per shuttle
- Trip note + incident resolution: marshal flags breakdown → admin creates TripNote with
  replacement shuttle → BreakdownLog resolved, allocation patched, notifications sent
- Stop-centric trip view: create trips for same stop on different dates/times → GET
  /admin/stops/{id}/trips returns all, correctly grouped and flagged for empty convoy

### Property Tests

Property tests defined in Correctness Properties section (C1–C10) should be implemented using
xUnit `[Theory]` with `[InlineData]` and `[MemberData]` attributes covering boundary conditions
and representative scenarios. Each property should have at minimum 10 distinct test cases covering
edge cases (empty convoys, single shuttle, all-accessible, mixed, boundary dates, etc.).
