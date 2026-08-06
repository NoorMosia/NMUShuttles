# Implementation Plan — Convoy and Route Model

## Overview

This plan implements the convoy-and-route-model improvements in five phases: schema and migration,
backend services and APIs, code sweep (CampusId transition), Flutter admin portal changes, and
Flutter marshal app changes. Each phase is independently deployable. The backend and code sweep
must be completed before either Flutter phase.

---

## Tasks

- [x] 1. Schema Migration — New Tables and Modified Columns

  - [x] 1.1 Create EF Core models for StopShuttleDefault, ScheduleTimeCampus, and TripNote
    - Add `StopShuttleDefault` entity: StopId, ShuttleId, OrderPosition, CreatedAt
    - Add UNIQUE constraints: (StopId, ShuttleId) and (StopId, OrderPosition)
    - Add `ScheduleTimeCampus` entity: ScheduleTimeId, CampusId, StopOrder
    - Add UNIQUE constraints: (ScheduleTimeId, CampusId) and (ScheduleTimeId, StopOrder)
    - Add `TripNote` entity: TripInstanceId, NoteType CHECK IN ('Incident','Replacement','General','NoShow'), Body NVARCHAR(MAX), CreatedByAdminId, CreatedAt, IsResolutionAction BIT DEFAULT 0
    - Add navigation properties on Stop, ScheduleTime, TripInstance, Admin
    - Add `DbSet<StopShuttleDefault>`, `DbSet<ScheduleTimeCampus>`, `DbSet<TripNote>` to ShuttleDbContext
    - _Requirements: 1.1, 3.1, 6.1, 6.2_

  - [x] 1.2 Add RequiresScanning to ScheduleTime and IsActive to Shuttle
    - Add `RequiresScanning` BIT NOT NULL DEFAULT 1 to ScheduleTime entity
    - Add `IsActive` BIT NOT NULL DEFAULT 1 to Shuttle entity
    - Add filtered index `IX_Shuttles_IsActive` (WHERE IsActive = 1)
    - _Requirements: 4.1, 8.1_

  - [x] 1.3 Write and apply EF Core migration (Phase 1 — additive)
    - Generate migration: `dotnet ef migrations add AddConvoyAndRouteModelTables`
    - Migration must:
      a. Add Shuttle.IsActive column (BIT NOT NULL DEFAULT 1)
      b. Create StopShuttleDefaults table with indexes
      c. Create ScheduleTimeCampuses table with indexes
      d. Create TripNotes table with indexes
      e. Add RequiresScanning column to ScheduleTimes (default 1)
      f. Migrate existing ScheduleTime.CampusId → one ScheduleTimeCampus row per record (StopOrder=1)
      g. Mark ScheduleTime.CampusId as nullable (do NOT drop yet)
      h. Add filtered index IX_Shuttles_IsActive
    - Add performance indexes from design (IX_StopShuttleDefaults_StopId, etc.)
    - Verify migration is idempotent (re-running does not error)
    - _Requirements: 1.1, 3.1, 4.1, 6.1, 8.1_

  - [x] 1.4 Write EF Core migration (Phase 2 — column drop, applied after code sweep verified)
    - Generate migration: `dotnet ef migrations add DropScheduleTimeCampusIdColumn`
    - Migration drops ScheduleTime.CampusId column and FK constraint
    - Remove Campus.ScheduleTimes navigation property from model config
    - This migration is applied ONLY after verifying the junction table code path works
    - _Requirements: 3.1_


- [x] 2. Backend — Services and API Endpoints

  - [x] 2.1 Implement StopDefaultConvoyService (no date enforcement)
    - Create `IStopDefaultConvoyService` with:
      - `AssignShuttleToDefaultAsync(stopId, shuttleId)`: validate shuttle IsActive, accessible-first ordering, INSERT
      - `RemoveShuttleFromDefaultAsync(stopId, shuttleId)`: DELETE, re-normalise positions
      - `ReorderDefaultAsync(stopId, orderedShuttleIds[])`: accessible-first invariant check, UPDATE positions
      - `GetDefaultConvoyAsync(stopId)`: SELECT ordered by OrderPosition
      - `CleanupDecommissionedShuttleAsync(shuttleId)`: remove shuttle from all Stop defaults, re-normalise
    - Validate shuttle IsActive=true before adding to default (reject inactive shuttles with 422)
    - No date enforcement needed — data model inherently protects existing trips
    - Accessible-first logic mirrors `ConvoyOrderingService.AssignShuttleToTripAsync` exactly
    - _Requirements: 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.9, 1.10, 1.11, 8.3, 8.5_

  - [x] 2.2 Add default convoy endpoints to StopsAdminController
    - `GET /api/v1/admin/stops/{stopId}/default-convoy` — returns ordered StopShuttleDefault list
    - `POST /api/v1/admin/stops/{stopId}/default-convoy` — body: `{ "shuttleId": N }`; returns 201 with new record
    - `DELETE /api/v1/admin/stops/{stopId}/default-convoy/{shuttleId}` — returns 204
    - `PATCH /api/v1/admin/stops/{stopId}/default-convoy/reorder` — body: `{ "orderedShuttleIds": [3,7,2] }`
    - Apply `[Authorize(Roles = "Admin")]` on all endpoints
    - No date enforcement — all edits accepted immediately
    - _Requirements: 1.2, 1.3, 1.6, 1.7, 1.8_

  - [x] 2.3 Modify RollingInjectionService to populate allocations from StopShuttleDefault
    - After each TripInstance is inserted, query `StopShuttleDefault WHERE StopId = tripInstance.StopId AND Shuttle.IsActive = true ORDER BY OrderPosition`
    - For each default record, INSERT a `TripShuttleAllocation` (TripInstanceId, ShuttleId, OrderPosition, IsOperational=true, AssignedAt=UtcNow)
    - The IsActive filter is a defensive guard against race conditions during deactivation
    - Handle ON CONFLICT (TripInstanceId, ShuttleId) silently (idempotency guard)
    - Log: "Auto-populated {N} allocations from default convoy for StopId={stopId}"
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 8.6_

  - [x] 2.4 Implement ScheduleTimeCampus management service and endpoints
    - Create `IScheduleTimeCampusService` with add, remove, reorder, and get operations
    - Enforce UNIQUE (ScheduleTimeId, CampusId) and contiguous StopOrder
    - Add endpoints to ScheduleAdminController:
      - `GET /api/v1/admin/schedule-times/{id}/campuses`
      - `POST /api/v1/admin/schedule-times/{id}/campuses` — body: `{ "campusId": N, "stopOrder": N }`
      - `DELETE /api/v1/admin/schedule-times/{id}/campuses/{campusId}`
      - `PATCH /api/v1/admin/schedule-times/{id}/campuses/reorder` — body: `{ "orderedCampusIds": [1,2,3] }`
    - _Requirements: 3.1, 3.2, 3.3_

  - [x] 2.5 Fix CheckInService — FromCampus location binding and RequiresScanning safety net
    - Replace single `scheduleTime.CampusId` check with junction table lookup:
      `var routeCampusIds = db.ScheduleTimeCampuses.Where(stc => stc.ScheduleTimeId == ...).Select(stc => stc.CampusId)`
      Reject with HTTP 403 if `marshal.AssignedCampusId` not in `routeCampusIds`
    - Add `RequiresScanning` safety net before the 35-min window check:
      If `!scheduleTime.RequiresScanning` → return informational 200 response "This trip is open boarding — no QR scan required" (non-error, client should prevent this path)
    - Update `ScheduleTime` include in CheckInService query to load `ScheduleTimeCampuses`
    - _Requirements: 3.4, 3.5, 4.5, 4.6_

  - [x] 2.6 Add RequiresScanning toggle to ScheduleTime admin endpoint
    - Add `RequiresScanning` field to ScheduleTime create/edit DTOs and controller
    - Admin can set it to false for specific slots (e.g. post-09:45 ToCampus slots)
    - _Requirements: 4.1, 4.2_

  - [x] 2.7 Add convoy status endpoint to existing TripsController
    - Add `GET /api/v1/trips/{tripId}/convoy` action to the existing `TripsController` (route prefix `/api/v1/trips`)
    - Apply `[Authorize(Roles = "Marshal,Admin")]` on this specific action (controller already has `[Authorize]` class-level)
    - Returns all TripShuttleAllocations for the trip with live `boardedCount` per shuttle
    - Include `requiresScanning` flag from the trip's ScheduleTime in the response
    - `boardedCount` = COUNT of Bookings WHERE TripInstanceId=trip AND ShuttleId=this AND BoardedStatus='Boarded'
    - Response shape: `ConvoyStatusDto` with `RequiresScanning` field as per design document
    - _Requirements: 5.1, 5.2, 5.5, 5.6, 4.5_

  - [x] 2.8 Implement TripNoteService and trip notes endpoints
    - Create `ITripNoteService.CreateTripNoteAsync(tripInstanceId, noteType, body, resolveIncident, replacementShuttleId?, adminId)`
    - Validate body is non-empty → 422 if blank
    - Resolution trigger only fires when `resolveIncident = true` (explicit admin confirmation)
    - Implement resolution trigger logic (see design document service section)
    - Create `POST /api/v1/admin/trips/{tripId}/notes` (Admin role) — request includes `resolveIncident` bool
    - Create `GET /api/v1/admin/trips/{tripId}/notes` (Admin role) — returns notes ordered by CreatedAt ASC
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6, 6.7, 6.8, 6.9, 6.10_

  - [x] 2.9 Add stop-centric trip view endpoint
    - Create `GET /api/v1/admin/stops/{stopId}/trips` (Admin role)
    - Query params: `fromDate` (default today), `toDate` (default today+3), `status` (optional filter)
    - For each trip: include shuttleCount, totalCapacity, confirmedCount, hasEmptyConvoy flag
    - Order by TripDate ASC, DepartureTime ASC
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

  - [x] 2.10 Add shuttle lifecycle endpoints to ShuttlesAdminController
    - `PATCH /api/v1/admin/shuttles/{id}/deactivate` — sets IsActive=false, triggers CleanupDecommissionedShuttleAsync
    - `PATCH /api/v1/admin/shuttles/{id}/reactivate` — sets IsActive=true (does NOT re-add to any default convoy)
    - Update shuttle listing endpoint to include `IsActive` field in response
    - Update shuttle picker queries (used by default convoy + per-trip assignment) to filter `IsActive = true`
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.7, 8.8_


- [x] 3. Code Sweep — CampusId Transition and DTO Updates

  - [ ] 3.1 Update ScheduleTime model and DbContext for nullable CampusId
    - Mark `ScheduleTime.CampusId` as `int?` (nullable) in model
    - Update `ConfigureScheduleTime` in DbContext to make FK optional
    - Add `ConfigureScheduleTimeCampus` configuration method to DbContext
    - Add `DbSet<ScheduleTimeCampus> ScheduleTimeCampuses` to ShuttleDbContext
    - Keep `Campus.ScheduleTimes` navigation property temporarily (removed in Phase 2)
    - _Requirements: 3.1_

  - [x] 3.2 Update ScheduleAdminController DTOs and endpoints
    - Update `ScheduleTimeResponseDto`: add `Campuses: List<ScheduleTimeCampusDto>` + `RequiresScanning: bool`, mark `CampusId` nullable
    - Update `CreateScheduleTimeDto`: accept `CampusIds: int[]` (ordered list), keep `CampusId: int?` for backward compat
    - Update create logic: if `CampusIds` provided, create ScheduleTimeCampus records; else if `CampusId` provided, create single record with StopOrder=1
    - Update GET responses to load campuses from junction table
    - _Requirements: 3.1, 3.2, 4.1_

  - [x] 3.3 Update TripsAdminController to use junction table
    - Replace `.ThenInclude(st => st.Campus)` with `.ThenInclude(st => st.ScheduleTimeCampuses)` (then ThenInclude Campus)
    - Update trip list response: replace single `CampusName` string with `campuses: [{campusId, campusName, stopOrder}]` array
    - Update trip detail response similarly
    - _Requirements: 3.1_

  - [x] 3.4 Update TripsController (non-admin) and BookingsController for backward compat
    - `TripsController.cs`: replace `.ThenInclude(st => st.Campus)` with junction table include
    - Compute `campusName` as first campus on route (StopOrder=1) from ScheduleTimeCampus
    - Keep returning `campusName: string` in response (backward compat for mobile app models)
    - `BookingsController.cs`: same pattern — load from junction, return first campus as `campusName`
    - `BookingService.cs`: replace `.ThenInclude(st => st.Campus)` with `.ThenInclude(st => st.ScheduleTimeCampuses)`
    - This ensures student and marshal Flutter models (`trip_model.dart`, `booking_model.dart`, `marshal_trip_model.dart`) continue working without changes
    - _Requirements: 3.1, 3.7_

  - [x] 3.5 Update SeedData to populate ScheduleTimeCampus records
    - For each seeded ScheduleTime, also seed a corresponding ScheduleTimeCampus record (StopOrder=1, CampusId = same value)
    - Keep setting nullable CampusId on ScheduleTime for backward compat until Phase 2 migration
    - _Requirements: 3.1_

  - [x] 3.6 Update any remaining CampusId references in services and controllers
    - Search codebase for `scheduleTime.CampusId` and `.Campus.` on ScheduleTime-related queries
    - Replace with junction table queries or ScheduleTimeCampuses navigation
    - Ensure CheckInService (task 2.5), TripsAdminController (task 3.3), TripsController + BookingsController (task 3.4) are all done
    - Verify backend compiles with no references to the non-nullable CampusId
    - _Requirements: 3.1_

- [x] 4. Flutter Admin Portal — Detail Screens, Default Convoy, Trip Notes, Stop-Centric View

  - [x] 4.1 Create Stop Detail Screen
    - New file: `WebPortalUI/lib/features/stops/screens/stop_detail_screen.dart`
    - Navigate from stops list (tap row or "View Details" action)
    - Header: Stop name, full address, GPS coordinates, active status, edit button
    - Sections: Default Convoy panel (task 4.2), "View Trips" button (task 4.6)
    - Back navigation to stops list
    - _Requirements: 9.1, 9.4_

  - [x] 4.2 Add Default Convoy panel to Stop Detail Screen
    - Display ordered list of StopShuttleDefault records: OrderPosition, LicensePlate, IsAccessibleVehicle indicator, Capacity
    - "Add Shuttle" → shuttle picker dialog (only active shuttles, IsActive=true) → POST to default-convoy endpoint
    - "Remove" action per row → DELETE endpoint
    - Drag-to-reorder (ReorderableListView) → PATCH reorder endpoint
    - Show advisory banner: "Changes apply to trips created from tonight onwards (earliest: {T+3 date}). Trips for the next 3 days already have their convoy — edit those from the trip detail screen."
    - _Requirements: 1.2, 1.3, 1.6, 1.7, 1.8, 8.5_

  - [x] 4.3 Create Trip Detail Screen
    - New file: `WebPortalUI/lib/features/trips/screens/trip_detail_screen.dart`
    - Navigate from trips list (tap row) or from stop-centric trip view (tap trip card)
    - Header: Trip date, departure time, direction, stop name, status badge
    - Campus Route section (read-only): ordered list of campuses from ScheduleTimeCampus
    - Convoy Allocation panel: current TripShuttleAllocation records with add/remove/reorder
    - Trip Notes section (task 4.4)
    - Back navigation to originating list
    - _Requirements: 9.2, 9.3, 9.4_

  - [x] 4.4 Add Trip Notes section to Trip Detail Screen
    - Display all TripNotes in chronological order (CreatedAt ASC)
    - Resolution notes (IsResolutionAction=true) visually highlighted with distinct colour/badge
    - "Add Note" form: NoteType dropdown, Body text field, optional Replacement Shuttle picker (active only)
    - When NoteType is 'Incident' or 'Replacement' AND trip status is 'IncidentActive':
      - Show confirmation dialog: "This note will resolve the active incident and send push notifications to all confirmed passengers. Proceed?"
      - On confirm: submit with `resolveIncident: true`
      - On cancel: return to form (save without resolution via `resolveIncident: false`)
    - Submit → POST /admin/trips/{id}/notes
    - If response shows `isResolutionAction=true`, display toast: "Incident resolved. Notifications sent to {N} passengers."
    - _Requirements: 6.3, 6.4, 6.5, 6.6, 6.7, 6.8, 6.9, 6.10_

  - [x] 4.5 Add Campus Route and RequiresScanning to ScheduleTime management screen
    - Display ordered list of ScheduleTimeCampus records: StopOrder, CampusName
    - Add / remove / reorder campuses on the route
    - Minimum 1 campus enforced client-side before submit
    - Add RequiresScanning toggle switch on create/edit form
    - Labelled "Requires QR Scanning" — default ON
    - Help text: "Disable for open-boarding trips (e.g. late-morning ToCampus)"
    - _Requirements: 3.1, 3.2, 3.3, 4.1, 4.2_

  - [x] 4.6 Add Stop-Centric Trip View
    - "View Trips" button on Stop Detail Screen navigates to stop-centric trips panel
    - Date range filter (default today to today+3)
    - Status filter
    - Each trip card shows: time, direction, status, capacity bar, shuttle count
    - Empty-convoy trips flagged with warning icon
    - Tap trip → navigate to Trip Detail Screen
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

  - [x] 4.7 Update Stops Screen for navigation to Stop Detail
    - Modify stops list rows to be tappable → navigate to Stop Detail Screen
    - Add "View Details" action button per row
    - _Requirements: 9.1_

  - [x] 4.8 Update Trips Screen for navigation to Trip Detail
    - Remove inline expansion pattern (`_expandedTripId` state)
    - Make trip rows tappable → navigate to Trip Detail Screen
    - _Requirements: 9.2_

  - [x] 4.9 Add shuttle lifecycle controls to Shuttles management screen
    - Show IsActive status indicator per shuttle row (active/inactive badge)
    - Add "Deactivate" / "Reactivate" action button per shuttle
    - Deactivate shows confirmation: "This will remove the shuttle from all default convoys. Proceed?"
    - Active/inactive filter on shuttle list
    - _Requirements: 8.1, 8.2, 8.5_


- [x] 5. Flutter Marshal App — Convoy Panel and Scan Gating

  - [x] 5.1 Add Convoy Panel to PassengerListScreen
    - Add "Convoy" tab or expandable section to the existing `passenger_list_screen.dart`
    - Create convoy provider (`convoy_provider.dart`) — StreamProvider with 30s polling from `GET /trips/{tripId}/convoy`
    - Display each shuttle: position badge, license plate, accessibility icon, boarded/capacity counter
    - Non-operational shuttles (IsOperational=false) displayed with strikethrough or greyed-out style
    - After each successful check-in by this marshal, immediately invalidate convoy provider to refresh counts
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6_

  - [x] 5.2 Add RequiresScanning scan button gating
    - Read `requiresScanning` from convoy endpoint response
    - When `requiresScanning = false`: disable scan button (grey out), show informational banner "This trip is open boarding — no QR scan required"
    - When `requiresScanning = true`: normal scan behaviour unchanged
    - _Requirements: 4.3, 4.5_

---

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2"] },
    { "id": 1, "tasks": ["1.3"] },
    { "id": 2, "tasks": ["2.1", "2.4", "2.6", "2.8", "2.10"] },
    { "id": 3, "tasks": ["2.2", "2.5", "2.7", "2.9", "2.3"] },
    { "id": 4, "tasks": ["3.1", "3.2", "3.3", "3.4", "3.5", "3.6"], "note": "Code sweep — all CampusId references updated to junction table" },
    { "id": 5, "tasks": ["4.1", "4.3", "4.5", "4.7", "4.8", "4.9", "5.1", "5.2"] },
    { "id": 6, "tasks": ["4.2", "4.4", "4.6"], "note": "Depends on detail screens from wave 5" },
    { "id": 7, "tasks": ["1.4"], "note": "Applied after code sweep verified — drops CampusId column" }
  ]
}
```

## Notes

- **Wave 4 (code sweep) is critical path.** After Migration 1 (wave 1), the CampusId column is
  nullable and the junction table is populated. Wave 4 updates all code to read from the junction
  table. Only after wave 4 is verified can Migration 2 (wave 7) safely drop the column.
- Waves 5 and 6 (Flutter) can be worked in parallel with the code sweep if desired, since the
  Flutter code only calls new endpoints that already use the junction table.
- Default convoy edits require no server-side date enforcement. The data model inherently protects
  existing trips. The admin portal shows an advisory banner as a UX aid.
- The `RequiresScanning` flag defaults to `true` for all existing records. No existing behaviour
  changes unless an admin explicitly toggles it. The marshal app disables the scan button
  client-side; the server has a safety net (200 informational) for outdated clients.
- `TripNote` does not replace `BreakdownLog`. The log remains the structured analytics record.
  The note is the narrative record. Both coexist on the same incident.
- Resolution is only triggered when the admin explicitly confirms (resolveIncident=true in request).
  Creating notes with 'Incident' or 'Replacement' type without the flag just saves an informational note.
- When a shuttle is deactivated (IsActive=false), its `StopShuttleDefault` records are automatically
  cleaned up and positions re-normalised. Existing TripShuttleAllocations are NOT removed.
  Reactivating a shuttle does NOT auto-add it back to any default convoy.
- The `Shuttle.IsActive` pattern mirrors the existing `Stop.IsActive` and `Student.IsActive`
  conventions already in the codebase.
- Stop Detail Screen and Trip Detail Screen are **new screens** (not modifications of existing list
  screens). The existing `stops_screen.dart` and `trips_screen.dart` remain as list views with
  navigation to the new detail screens.
- The convoy status endpoint is added as a new action on the **existing `TripsController`**
  (already at `/api/v1/trips` with `[Authorize]`). No new controller is created. The action-level
  `[Authorize(Roles = "Marshal,Admin")]` restricts access appropriately.
