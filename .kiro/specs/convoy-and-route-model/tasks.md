# Implementation Plan — Convoy and Route Model

## Overview

This plan implements the convoy-and-route-model improvements in four phases: schema and migration,
backend services and APIs, Flutter admin portal changes, and Flutter marshal app changes. Each
phase is independently deployable. The backend must be completed before either Flutter phase.

---

## Tasks

- [ ] 1. Schema Migration — New Tables and Modified Columns

  - [ ] 1.1 Create EF Core models for StopShuttleDefault, ScheduleTimeCampus, and TripNote
    - Add `StopShuttleDefault` entity: StopId, ShuttleId, OrderPosition, CreatedAt
    - Add UNIQUE constraints: (StopId, ShuttleId) and (StopId, OrderPosition)
    - Add `ScheduleTimeCampus` entity: ScheduleTimeId, CampusId, StopOrder
    - Add UNIQUE constraints: (ScheduleTimeId, CampusId) and (ScheduleTimeId, StopOrder)
    - Add `TripNote` entity: TripInstanceId, NoteType CHECK IN ('Incident','Replacement','General','NoShow'), Body NVARCHAR(MAX), CreatedByAdminId, CreatedAt, IsResolutionAction BIT DEFAULT 0
    - Add navigation properties on Stop, ScheduleTime, TripInstance, Admin
    - _Requirements: 1.1, 3.1, 6.1, 6.2_

  - [ ] 1.2 Add RequiresScanning to ScheduleTime model
    - Add `RequiresScanning` BIT NOT NULL DEFAULT 1 to ScheduleTime entity
    - _Requirements: 4.1_

  - [ ] 1.3 Write and apply EF Core migration
    - Generate migration: `dotnet ef migrations add ConvoyAndRouteModel`
    - Migration must:
      a. Create StopShuttleDefaults table with indexes
      b. Create ScheduleTimeCampuses table with indexes
      c. Create TripNotes table with indexes
      d. Add RequiresScanning column to ScheduleTimes (default 1)
      e. Migrate existing ScheduleTime.CampusId → one ScheduleTimeCampus row per record (StopOrder=1)
      f. Drop ScheduleTime.CampusId column
    - Add performance indexes from design (IX_StopShuttleDefaults_StopId, etc.)
    - Verify migration is idempotent (re-running does not error)
    - _Requirements: 1.1, 3.1, 4.1, 6.1_


- [ ] 2. Backend — Services and API Endpoints

  - [ ] 2.1 Implement StopDefaultConvoyService with T+4 enforcement
    - Create `IStopDefaultConvoyService` with:
      - `AssignShuttleToDefaultAsync(stopId, shuttleId)`: T+4 validation, accessible-first ordering, INSERT
      - `RemoveShuttleFromDefaultAsync(stopId, shuttleId)`: T+4 validation, DELETE, re-normalise positions
      - `ReorderDefaultAsync(stopId, orderedShuttleIds[])`: T+4 validation, accessible-first invariant check, UPDATE positions
      - `GetDefaultConvoyAsync(stopId)`: SELECT ordered by OrderPosition
    - T+4 validation: compute `minAllowedDate = SAST_today + 4 days`; reject with HTTP 400 and message "Default convoy changes apply from {minAllowedDate}. Trips within the next 3 days are protected." if violated
    - Accessible-first logic mirrors `ConvoyOrderingService.AssignShuttleToTripAsync` exactly
    - _Requirements: 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8, 1.9, 1.10_

  - [ ] 2.2 Add default convoy endpoints to StopsAdminController
    - `GET /api/v1/admin/stops/{stopId}/default-convoy` — returns ordered StopShuttleDefault list
    - `POST /api/v1/admin/stops/{stopId}/default-convoy` — body: `{ "shuttleId": N }`; returns 201 with new record
    - `DELETE /api/v1/admin/stops/{stopId}/default-convoy/{shuttleId}` — returns 204
    - `PATCH /api/v1/admin/stops/{stopId}/default-convoy/reorder` — body: `{ "orderedShuttleIds": [3,7,2] }`
    - Apply `[Authorize(Roles = "Admin")]` on all endpoints
    - _Requirements: 1.2, 1.3, 1.6, 1.7, 1.8_

  - [ ] 2.3 Modify RollingInjectionService to populate allocations from StopShuttleDefault
    - After each TripInstance is inserted, query `StopShuttleDefault WHERE StopId = tripInstance.StopId ORDER BY OrderPosition`
    - For each default record, INSERT a `TripShuttleAllocation` (TripInstanceId, ShuttleId, OrderPosition, IsOperational=true, AssignedAt=UtcNow)
    - Handle ON CONFLICT (TripInstanceId, ShuttleId) silently (idempotency guard)
    - Log: "Auto-populated {N} allocations from default convoy for StopId={stopId}"
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5_

  - [ ] 2.4 Implement ScheduleTimeCampus management service and endpoints
    - Create `IScheduleTimeCampusService` with add, remove, reorder, and get operations
    - Enforce UNIQUE (ScheduleTimeId, CampusId) and contiguous StopOrder
    - Add endpoints to ScheduleAdminController:
      - `GET /api/v1/admin/schedule-times/{id}/campuses`
      - `POST /api/v1/admin/schedule-times/{id}/campuses` — body: `{ "campusId": N, "stopOrder": N }`
      - `DELETE /api/v1/admin/schedule-times/{id}/campuses/{campusId}`
      - `PATCH /api/v1/admin/schedule-times/{id}/campuses/reorder` — body: `{ "orderedCampusIds": [1,2,3] }`
    - _Requirements: 3.1, 3.2, 3.3_

  - [ ] 2.5 Fix CheckInService — FromCampus location binding and RequiresScanning gate
    - Replace single `scheduleTime.CampusId` check with junction table lookup:
      `var routeCampusIds = db.ScheduleTimeCampuses.Where(stc => stc.ScheduleTimeId == ...).Select(stc => stc.CampusId)`
      Reject with HTTP 403 if `marshal.AssignedCampusId` not in `routeCampusIds`
    - Add `RequiresScanning` gate before the 35-min window check:
      If `!scheduleTime.RequiresScanning` → throw CheckInException 422 "Scanning is not required for this trip"
    - Update `ScheduleTime` include in CheckInService query to load `ScheduleTimeCampuses`
    - _Requirements: 3.4, 3.5, 4.3, 4.5_

  - [ ] 2.6 Add RequiresScanning toggle to ScheduleTime admin endpoint
    - Add `RequiresScanning` field to ScheduleTime create/edit DTOs and controller
    - Admin can set it to false for specific slots (e.g. post-09:45 ToCampus slots)
    - _Requirements: 4.1, 4.2_

  - [ ] 2.7 Add convoy status endpoint for marshal app
    - Create `GET /api/v1/trips/{tripId}/convoy` (Marshal + Admin roles)
    - Returns all TripShuttleAllocations for the trip with live `boardedCount` per shuttle
    - `boardedCount` = COUNT of Bookings WHERE TripInstanceId=trip AND ShuttleId=this AND BoardedStatus='Boarded'
    - Response shape as per design document
    - _Requirements: 5.1, 5.2, 5.5_

  - [ ] 2.8 Implement TripNoteService and trip notes endpoints
    - Create `ITripNoteService.CreateTripNoteAsync(tripInstanceId, noteType, body, replacementShuttleId?, adminId)`
    - Validate body is non-empty → 422 if blank
    - Implement resolution trigger logic (see design document service section)
    - Create `POST /api/v1/admin/trips/{tripId}/notes` (Admin role)
    - Create `GET /api/v1/admin/trips/{tripId}/notes` (Admin role) — returns notes ordered by CreatedAt ASC
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6, 6.7, 6.8_

  - [ ] 2.9 Add stop-centric trip view endpoint
    - Create `GET /api/v1/admin/stops/{stopId}/trips` (Admin role)
    - Query params: `fromDate` (default today), `toDate` (default today+3), `status` (optional filter)
    - For each trip: include shuttleCount, totalCapacity, confirmedCount, hasEmptyConvoy flag
    - Order by TripDate ASC, DepartureTime ASC
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_


- [ ] 3. Flutter Admin Portal — Stop Default Convoy, Trip Notes, Stop-Centric View

  - [ ] 3.1 Add Default Convoy panel to Stop detail screen
    - Display ordered list of StopShuttleDefault records: OrderPosition, LicensePlate, IsAccessibleVehicle indicator, Capacity
    - "Add Shuttle" → shuttle picker dialog → POST to default-convoy endpoint
    - "Remove" action per row → DELETE endpoint
    - Drag-to-reorder (ReorderableListView) → PATCH reorder endpoint
    - Show advisory banner: "Changes apply from [Today+4]. Existing trips are unaffected."
    - Show error inline when T+4 constraint is violated (HTTP 400 from backend)
    - _Requirements: 1.2, 1.3, 1.6, 1.7, 1.8_

  - [ ] 3.2 Add Campus Route section to ScheduleTime management screen
    - Display ordered list of ScheduleTimeCampus records: StopOrder, CampusName
    - Add / remove / reorder campuses on the route
    - Minimum 1 campus enforced client-side before submit
    - _Requirements: 3.1, 3.2, 3.3_

  - [ ] 3.3 Add RequiresScanning toggle to ScheduleTime form
    - Toggle switch on ScheduleTime create and edit forms
    - Labelled "Requires QR Scanning" — default ON
    - Help text: "Disable for open-boarding trips (e.g. late-morning ToCampus)"
    - _Requirements: 4.1, 4.2_

  - [ ] 3.4 Add Trip Notes section to Trip detail screen
    - Display all TripNotes in chronological order (CreatedAt ASC)
    - Resolution notes (IsResolutionAction=true) visually highlighted with distinct colour/badge
    - "Add Note" form: NoteType dropdown, Body text field, optional Replacement Shuttle picker
    - Submit → POST /admin/trips/{id}/notes
    - If response shows `isResolutionAction=true`, display confirmation: "Incident resolved. Notifications sent to {N} passengers."
    - _Requirements: 6.3, 6.4, 6.5, 6.6, 6.7, 6.8_

  - [ ] 3.5 Add Stop-Centric Trip View
    - "View Trips" button on Stop management screen opens stop-centric trips panel
    - Date range filter (default today to today+3)
    - Status filter
    - Each trip card shows: time, direction, status, capacity bar, shuttle count
    - Empty-convoy trips flagged with warning icon
    - Tap trip → navigate to trip detail (existing trip detail screen)
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

- [ ] 4. Flutter Marshal App — Convoy Panel

  - [ ] 4.1 Add Convoy Panel to Marshal trip detail screen
    - Add "Convoy" tab or expandable section to the existing Passenger List / trip detail screen
    - Fetch data from `GET /trips/{tripId}/convoy`
    - Display each shuttle: position badge, license plate, accessibility icon, boarded/capacity counter
    - Poll on same 30-second interval as passenger manifest
    - After each successful check-in, invalidate convoy provider to refresh counts immediately
    - Non-operational shuttles (IsOperational=false) displayed with strikethrough or greyed-out style
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5_

---

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1", "1.2"] },
    { "id": 1, "tasks": ["1.3"] },
    { "id": 2, "tasks": ["2.1", "2.4", "2.6", "2.8"] },
    { "id": 3, "tasks": ["2.2", "2.5", "2.7", "2.9"] },
    { "id": 4, "tasks": ["2.3"] },
    { "id": 5, "tasks": ["3.1", "3.2", "3.3", "3.4", "3.5", "4.1"] }
  ]
}
```

## Notes

- Tasks in wave 5 (Flutter) can all be worked in parallel once the backend (waves 0–4) is complete.
- The T+4 enforcement is implemented entirely in the backend. The admin portal shows the advisory
  date as a UX aid, but the backend rejects invalid requests regardless of what the client sends.
- The `ScheduleTime.CampusId` column removal is a breaking change for any code that reads that
  field directly. After applying migration 1.3, any remaining references to `scheduleTime.CampusId`
  in the codebase will cause compile errors — these must be replaced with `ScheduleTimeCampuses`
  queries before the backend compiles.
- The `RequiresScanning` flag defaults to `true` for all existing records. No existing behaviour
  changes unless an admin explicitly toggles it.
- `TripNote` does not replace `BreakdownLog`. The log remains the structured analytics record.
  The note is the narrative record. Both coexist on the same incident.
