# Work Done — 6 August 2026

## Convoy and Route Model Spec (Review & Refinement)

- Reviewed the new `convoy-and-route-model` spec (requirements, design, tasks)
- Identified and fixed 7 design issues:
  1. Removed T+4 enforcement — replaced with advisory-only approach (data model is inherently safe)
  2. Changed RequiresScanning from server 422 to client-side scan button disable
  3. Added explicit `resolveIncident` flag + confirmation dialog for trip note resolution trigger
  4. Clarified convoy panel polling: own scans = instant invalidation, 30s poll = fallback for other marshals
  5. Added shuttle decommissioning handling (auto-cleanup of StopShuttleDefault)
  6. Split CampusId migration into safe two-phase approach
  7. Replaced FsCheck with xUnit Theory for property tests
- Analysed spec against codebase — identified 6 conflicts and 5 gaps
- Added Requirement 8 (Shuttle Lifecycle with IsActive), Requirement 9 (Admin Detail Screens)
- Added code sweep phase (tasks 3.1–3.6) for CampusId → junction table transition
- Expanded tasks from 17 to 28 subtasks across 5 phases + 8 dependency waves
- Three rounds of full re-analysis until spec was clean (0 conflicts, 0 gaps)

## Schema & Migration (Wave 0–1)

- Created 3 new EF Core entity models: `StopShuttleDefault`, `ScheduleTimeCampus`, `TripNote`
- Added `IsActive` (bool) to Shuttle model + filtered index
- Added `RequiresScanning` (bool) to ScheduleTime model
- Added navigation properties to Stop, Shuttle, ScheduleTime, TripInstance, Admin, Campus
- Added 3 DbSets + 3 configuration methods to ShuttleDbContext
- Made `ScheduleTime.CampusId` nullable (transition state)
- Generated and applied EF Core migration `ConvoyAndRouteModel`:
  - Creates 3 new tables with indexes, CHECK constraints, composite unique constraints
  - Adds Shuttle.IsActive + ScheduleTime.RequiresScanning columns
  - Migrates existing CampusId data → ScheduleTimeCampus junction rows (SQL data migration)
  - Drops CampusId column + FK

## Backend Services (Wave 2)

- `IStopDefaultConvoyService` + `StopDefaultConvoyService` — Assign (accessible-first), Remove, Reorder, Get, Cleanup. IsActive validation.
- `IScheduleTimeCampusService` + `ScheduleTimeCampusService` — Add, Remove, Reorder, Get with contiguous StopOrder enforcement
- `ITripNoteService` + `TripNoteService` — full resolution trigger logic (BreakdownLog update, allocation patch, FCM dispatch, status transition)
- All 3 registered in Program.cs with DI

## Backend API Endpoints (Wave 2–3)

- `GET/POST/DELETE/PATCH /admin/stops/{id}/default-convoy` — default convoy CRUD
- `GET /admin/stops/{id}/trips` — stop-centric trip view with convoy summary
- `GET/POST/DELETE/PATCH /admin/schedule-times/{id}/campuses` — campus route management
- `GET/POST /admin/trips/{id}/notes` — trip notes with resolution trigger
- `PATCH /admin/shuttles/{id}/deactivate` + `/reactivate` — shuttle lifecycle
- `GET /admin/shuttles/active` + `/available` — filtered shuttle lists
- `GET /trips/{id}/convoy` — convoy status with live boarded counts + requiresScanning flag
- RequiresScanning toggle on ScheduleTime create/update DTOs
- IsActive in ShuttleResponseDto

## CheckInService Rewrite (Wave 3)

- Replaced single `CampusId` check with junction table lookup (multi-campus route validation)
- Added RequiresScanning safety net (200 informational, not 422)
- Added ScheduleTimeCampuses eager loading

## RollingInjectionService Enhancement (Wave 3)

- Added Step 5b: auto-populates TripShuttleAllocations from StopShuttleDefaults
- Filters by Shuttle.IsActive (defensive guard)
- Idempotency check before insert

## Code Sweep (Wave 4)

- Updated all `ScheduleTime.Campus` references across 7 files:
  - ScheduleAdminController — full rewrite to load/return campuses array
  - TripsAdminController — campuses array in admin responses
  - TripsController (non-admin) — backward compat, returns first campus as `campusName`
  - BookingsController — backward compat for mobile app models
  - BookingService — updated eager loading includes
  - CheckInService — junction table query (done in Wave 3)
  - SeedData — updated for new fields
- Updated DTOs: ScheduleTimeResponseDto gains `Campuses` list, CreateScheduleTimeDto gains `CampusIds`
- Removed CampusId from model, Campus.ScheduleTimes nav property, FK relationship
- Final build: 0 errors, 0 warnings

## Flutter Admin Portal (Wave 5)

- **Stop Detail Screen** (`stop_detail_screen.dart`) — metadata card, back navigation, View Trips button
- **Default Convoy Panel** — ReorderableListView with drag-to-reorder, add/remove shuttles, advisory banner, shuttle picker from `/available` endpoint
- **Trip Detail Screen** (`trip_detail_screen.dart`) — metadata + status badge, convoy table, assign shuttle dialog
- **Trip Notes Section** — chronological notes list with type badges + resolution highlight, add note dialog with NoteType dropdown, resolution confirmation dialog, resolveIncident flag, notification count toast
- **Stop-Centric Trip View** (`stop_trips_screen.dart`) — trip cards with time, direction, status, capacity, empty convoy warning
- **Schedule Screen** — added RequiresScanning toggle (SwitchListTile) to form dialog
- **Shuttles Screen** — IsActive badge column, Deactivate button (with confirmation dialog), Reactivate button
- **Navigation** — added GoRouter routes `/stops/:id`, `/stops/:id/trips`, `/trips/:id`
- Updated stops list with "View Details" button, trips list to navigate instead of expanding
- Fixed sidebar highlighting for nested routes (startsWith matching)
- New providers: `default_convoy_provider.dart`, `stop_trips_provider.dart`, `trip_notes_provider.dart`

## Flutter Marshal App (Wave 5)

- **Convoy Panel** on PassengerListScreen — horizontal scrollable chips showing position, plate, accessibility icon, boarded/capacity per shuttle
- **Convoy Provider** (`convoy_provider.dart`) — StreamProvider with 30s polling, same pattern as passengerManifestProvider
- Immediate convoy invalidation after successful QR scan
- **Scan Button Gating** — FAB hidden + amber banner when `requiresScanning = false`

## Bug Fixes

- Fixed sidebar highlighting for detail routes (`/stops/1` → highlights "Shuttle Stops")
- Fixed shuttle picker showing empty list (was reading stale cache, now awaits future properly)
- Fixed `ShuttleOption.fromJson` crash (expected `id` but backend returns `shuttleId`)
- Fixed available shuttles endpoint 400 error (route conflict with `{id}` — added `:int` constraint)
- Added server-side enforcement: shuttle can only be in one stop's default convoy
- Added `/admin/shuttles/available` endpoint (filters out already-assigned shuttles)
- Removed checkbox column from Reports DataTable
- Fixed EF migration PK conflict (seed data was duplicating migration's data population)

## Files Created

- `Backend/NmuShuttle.Api/Models/StopShuttleDefault.cs`
- `Backend/NmuShuttle.Api/Models/ScheduleTimeCampus.cs`
- `Backend/NmuShuttle.Api/Models/TripNote.cs`
- `Backend/NmuShuttle.Api/Services/IStopDefaultConvoyService.cs`
- `Backend/NmuShuttle.Api/Services/StopDefaultConvoyService.cs`
- `Backend/NmuShuttle.Api/Services/IScheduleTimeCampusService.cs`
- `Backend/NmuShuttle.Api/Services/ScheduleTimeCampusService.cs`
- `Backend/NmuShuttle.Api/Services/ITripNoteService.cs`
- `Backend/NmuShuttle.Api/Services/TripNoteService.cs`
- `Backend/NmuShuttle.Api/DTOs/Admin/ScheduleTimeCampusDtos.cs`
- `Backend/NmuShuttle.Api/DTOs/Admin/StopDefaultConvoyDtos.cs`
- `Backend/NmuShuttle.Api/DTOs/Admin/TripNoteDtos.cs`
- `Backend/NmuShuttle.Api/Migrations/20260806174036_ConvoyAndRouteModel.cs`
- `WebPortalUI/lib/features/stops/screens/stop_detail_screen.dart`
- `WebPortalUI/lib/features/stops/screens/stop_trips_screen.dart`
- `WebPortalUI/lib/features/stops/providers/default_convoy_provider.dart`
- `WebPortalUI/lib/features/stops/providers/stop_trips_provider.dart`
- `WebPortalUI/lib/features/trips/screens/trip_detail_screen.dart`
- `WebPortalUI/lib/features/trips/providers/trip_notes_provider.dart`
- `AppUI/lib/features/marshal/providers/convoy_provider.dart`

## Files Modified (Major Changes)

- All spec files (`.kiro/specs/convoy-and-route-model/` — requirements.md, design.md, tasks.md)
- `Backend/NmuShuttle.Api/Data/ShuttleDbContext.cs`
- `Backend/NmuShuttle.Api/Data/SeedData.cs`
- `Backend/NmuShuttle.Api/Models/` (ScheduleTime, Shuttle, Stop, TripInstance, Admin, Campus)
- `Backend/NmuShuttle.Api/Controllers/Admin/` (Stops, Schedule, Trips, Shuttles)
- `Backend/NmuShuttle.Api/Controllers/` (TripsController, BookingsController)
- `Backend/NmuShuttle.Api/Services/` (CheckInService, BookingService, RollingInjectionService)
- `Backend/NmuShuttle.Api/DTOs/Admin/ScheduleTimeDtos.cs`
- `Backend/NmuShuttle.Api/DTOs/Admin/ShuttleDtos.cs`
- `Backend/NmuShuttle.Api/Program.cs`
- `WebPortalUI/lib/core/router/admin_router.dart`
- `WebPortalUI/lib/shared/widgets/admin_scaffold.dart`
- `WebPortalUI/lib/features/stops/screens/stops_screen.dart`
- `WebPortalUI/lib/features/trips/screens/trips_screen.dart`
- `WebPortalUI/lib/features/trips/providers/trips_providers.dart`
- `WebPortalUI/lib/features/shuttles/providers/shuttles_providers.dart`
- `WebPortalUI/lib/features/shuttles/screens/shuttles_screen.dart`
- `WebPortalUI/lib/features/schedule/providers/schedule_providers.dart`
- `WebPortalUI/lib/features/schedule/screens/schedule_screen.dart`
- `WebPortalUI/lib/features/reports/screens/reports_screen.dart`
- `AppUI/lib/features/marshal/screens/passenger_list_screen.dart`

## Credits Used

- ~380
