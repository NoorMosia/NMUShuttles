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

**T+4 enforcement:** All write operations on this table (INSERT, UPDATE, DELETE) are validated at
the API layer. The backend computes `minAllowedDate = SAST_today + 4 days` and rejects any change
that would affect trips already in the T+0 to T+3 window with HTTP 400.


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
```


---

## Updated Entity Relationship

```
Stop ──< StopShuttleDefault ──> Shuttle   (default convoy roster per stop)

ScheduleTime ──< ScheduleTimeCampus ──> Campus   (ordered campus waypoints)
ScheduleTime.RequiresScanning  (new flag — controls scan gate)

TripInstance ──< TripNote ──> Admin   (freeform incident and operational notes)
```

All other relationships from the original design are unchanged.

---

## Service Changes

### StopDefaultConvoyService (new)

Manages `StopShuttleDefault` records with T+4 enforcement and accessible-first ordering.

```
AssignShuttleToDefaultAsync(stopId, shuttleId):
  1. Validate T+4 window — reject if today+4 would affect active trips
  2. Load existing StopShuttleDefault records for stop, ordered by OrderPosition
  3. Apply same accessible-first insertion logic as ConvoyOrderingService:
     - accessible shuttle → insert after last accessible, shift non-accessible up
     - non-accessible shuttle → append at end
  4. INSERT new StopShuttleDefault record
  5. Save

RemoveShuttleFromDefaultAsync(stopId, shuttleId):
  1. Validate T+4 window
  2. DELETE StopShuttleDefault record
  3. Re-normalise OrderPositions (contiguous from 1)
  4. Save

ReorderDefaultAsync(stopId, orderedShuttleIds[]):
  1. Validate T+4 window
  2. Validate accessible-first invariant in proposed order
  3. UPDATE OrderPosition for each record to match proposed order
  4. Save
```


### RollingInjectionService (modified)

Step 5 of the existing algorithm gains a new sub-step after creating each `TripInstance`:

```
After INSERT TripInstance:
  defaultAllocations = SELECT * FROM StopShuttleDefaults
                       WHERE StopId = tripInstance.StopId
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

Additionally, a `RequiresScanning` gate is added before the existing 35-min window check:

```
if (!scheduleTime.RequiresScanning)
    throw new CheckInException(
        "Scanning is not required for this trip — boarding is open.",
        StatusCodes.Status422UnprocessableEntity);
```


### TripNoteService (new)

```
CreateTripNoteAsync(tripInstanceId, noteType, body, replacementShuttleId?, adminId):
  1. Validate body is non-empty → 422 if empty
  2. Create TripNote record (IsResolutionAction = false initially)
  3. If noteType IN ('Incident', 'Replacement') AND trip.Status == 'IncidentActive':
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
  4. Save all changes
  5. Return TripNote
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
| GET | `/api/v1/trips/{tripId}/convoy` | Marshal | Get convoy order + live boarded counts per shuttle |


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
// Response 400: T+4 window violation
// Response 409: shuttle already in default for this stop
```

**POST `/api/v1/admin/trips/{tripId}/notes`**
```json
// Request
{
  "noteType": "Replacement",
  "body": "Shuttle ABC 123 broke down 2km from stop. Covered by Bay Transit Corp vehicle EC 999-ZZZ (not yet registered). Driver: John M.",
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

- **Stop detail screen** gains a "Default Convoy" panel:
  - Lists current `StopShuttleDefault` records in `OrderPosition` order
  - "Add Shuttle" button → shuttle selector → calls `POST /admin/stops/{id}/default-convoy`
  - Each row has a "Remove" action → calls `DELETE` endpoint
  - Drag-to-reorder capability → calls `PATCH reorder` endpoint
  - All edit actions display a warning: "Changes apply from [Today+4 date] onwards"
  - Shows count of trips in T+0–T+3 window that are unaffected by the change

- **ScheduleTime management screen** gains a "Campus Route" section:
  - Lists campuses in `StopOrder` order
  - Add / remove / reorder campuses on the route
  - Required: at least 1 campus per route (validation)

- **Trip detail screen** gains a "Trip Notes" section:
  - Lists all `TripNote` records for the trip in chronological order
  - "Add Note" form: NoteType selector + Body textarea + optional Replacement Shuttle selector
  - Notes with `IsResolutionAction = true` are visually highlighted as resolution events

- **Stop management screen** gains "View Trips" button → opens stop-centric trip view
  - Shows trips grouped within the stop, with visual flag for empty-convoy trips

### Marshal App

- **Trip detail / Passenger List screen** gains a **Convoy Panel** tab or section:
  - Shows each shuttle's `OrderPosition`, `LicensePlate`, accessibility indicator
  - Shows live `boardedCount / capacity` for each shuttle
  - Data sourced from `GET /trips/{tripId}/convoy`, polled on the same 30-second interval as the
    passenger manifest
  - After each successful check-in, convoy panel counts are updated immediately (same invalidation
    pattern as manifest provider)

---

## Migration Strategy

The schema changes require one EF Core migration:

1. Add `StopShuttleDefaults` table
2. Add `ScheduleTimeCampuses` table
3. Add `TripNotes` table
4. Add `ScheduleTime.RequiresScanning` column (default 1 — all existing slots require scanning)
5. Migrate existing `ScheduleTime.CampusId` data → insert one `ScheduleTimeCampus` row per
   existing `ScheduleTime` with `StopOrder = 1`
6. Drop `ScheduleTime.CampusId` column

Steps 5 and 6 must be in the same migration transaction to avoid data loss.
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
  └─► Stop Default Convoy Management UI
  └─► Campus Route Configuration UI
  └─► Trip Notes UI
  └─► Stop-Centric Trip View
        ↓ HTTPS/REST
  .NET 9 Web API
    ├─► StopDefaultConvoyService (new)
    ├─► ScheduleTimeCampusService (new)
    ├─► TripNoteService (new)
    └─► RollingInjectionService (modified — auto-populate from defaults)
          ↓
    SQL Server
      ├─► StopShuttleDefaults (new table)
      ├─► ScheduleTimeCampuses (new table)
      ├─► TripNotes (new table)
      └─► ScheduleTimes (RequiresScanning column added, CampusId removed)

Marshal App (Flutter Mobile)
  └─► Convoy Panel UI
        ↓ HTTPS/REST
  .NET 9 Web API
    └─► GET /trips/{id}/convoy (new endpoint)
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
3. **T+4 enforcement as a guardrail**: The 4-day buffer matches the nightly injection window
   exactly. Trips within T+0 to T+3 are either already created or will be created tonight, so no
   default edit can touch them.
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
        int tripInstanceId, string noteType, string body, int? replacementShuttleId, int adminId);
    Task<IList<TripNoteDto>> GetTripNotesAsync(int tripInstanceId);
}
```

### Modified Services

**RollingInjectionService** gains a new step after each `TripInstance` creation:
```csharp
// After INSERT TripInstance
var defaults = await _db.StopShuttleDefaults
    .Where(d => d.StopId == tripInstance.StopId)
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

And adds a `RequiresScanning` gate before the 35-minute window check:
```csharp
if (!scheduleTime.RequiresScanning)
    throw new CheckInException("Scanning not required for this trip", 422);
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

---

## Correctness Properties

### Property C1: T+4 Enforcement Invariant

*For any* edit to a Stop's default convoy (add, remove, reorder), the System SHALL reject the
operation with HTTP 400 if the current SAST date plus 4 days is greater than the earliest
TripDate affected by the change.

**Validates:** Requirements 1.8

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

### Property C6: RequiresScanning Gate

*For any* TripInstance where `ScheduleTime.RequiresScanning = FALSE`, all check-in scan attempts
SHALL be rejected with HTTP 422, while booking creation, capacity tracking, and waitlist promotion
continue to function normally.

**Validates:** Requirements 4.3, 4.4, 4.6

---

### Property C7: Trip Note Body Non-Empty

*For any* TripNote creation request where `Body` is empty or whitespace-only, the System SHALL
reject the request with HTTP 422.

**Validates:** Requirements 6.7

---

### Property C8: Incident Resolution Trigger

*For any* TripNote created with `NoteType IN ('Incident', 'Replacement')` on a TripInstance with
`Status = 'IncidentActive'`, the System SHALL set the corresponding `BreakdownLog.ResolvedAt` and
dispatch FCM notifications to all Confirmed Booking holders on that trip.

**Validates:** Requirements 6.5, 6.6

---

## Error Handling

All new endpoints follow the existing global error handling strategy (RFC 7807 ProblemDetails).
Specific error scenarios:

| Scenario | HTTP Status | Message |
|---|---|---|
| T+4 constraint violated on default convoy edit | 400 | "Default convoy changes apply from {date}. Trips within the next 3 days are protected." |
| Duplicate shuttle in Stop's default convoy | 409 | "Shuttle {licensePlate} is already in the default convoy for this stop." |
| Attempt to reorder convoy with invalid accessible-first order | 422 | "Accessible shuttles must be positioned before non-accessible shuttles." |
| Empty TripNote body | 422 | "Trip note body cannot be empty." |
| Scan attempt on RequiresScanning=false trip | 422 | "QR scanning is not required for this trip. Boarding is open." |
| Marshal not on route (FromCampus) | 403 | "Marshal is not authorized to scan at this campus for this route." |
| Campus not found on route when adding | 404 | "Campus {id} not found." |
| Stop has no default convoy (GET) | 200 | Returns empty array `[]` (not an error — valid state) |

---

## Testing Strategy

### Unit Tests

- T+4 validation: attempt default convoy edit with dates T+0, T+3, T+4, T+5 → only T+4+ accepted
- Accessible-first ordering: add shuttles in various orders, verify OrderPosition invariant holds
- RequiresScanning gate: check-in on trip with RequiresScanning=false → 422
- Multi-campus route validation: marshal assigned to CampusA, route has [CampusA, CampusB] → accepted;
  marshal assigned to CampusC, route has [CampusA, CampusB] → 403
- Trip note resolution trigger: create TripNote with NoteType='Replacement' on IncidentActive trip
  → BreakdownLog.ResolvedAt set, notifications dispatched
- Empty convoy on trip creation: Stop with no defaults → TripInstance created with zero allocations

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

Property tests defined in Correctness Properties section (C1–C8) should be implemented using
FsCheck with minimum 100 iterations per property. These tests validate the core invariants
(T+4 enforcement, convoy ordering, campus sequence uniqueness, multi-campus route binding).
