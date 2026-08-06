# Requirements Document

## Introduction

This spec captures a targeted set of improvements to the convoy management, route model, and
operational workflow of the NMU Shuttle Management Network. It builds on the foundation
established in the `nmu-shuttle-management` spec and corrects several design issues discovered
during analysis of how the physical university shuttle system actually operates.

The changes fall into five concern areas:

1. **Stop-centric default convoys** — each Stop maintains a standing roster of shuttles that are
   automatically applied when trips are created, replacing the current manual per-trip assignment
   workflow.
2. **Route model correction** — the current design incorrectly models each route as terminating at
   a single campus. NMU shuttles physically stop at all three campuses on every run. The campus
   relationship on `ScheduleTime` is restructured accordingly.
3. **Marshal convoy awareness** — marshals need visibility into the convoy order and live boarding
   counts so they can physically direct shuttle drivers and passengers at the boarding point.
4. **Trip notes** — admin incident resolution is simplified from a structured form into freeform
   trip notes, with only minimal structured fields retained for analytics.
5. **Shuttle lifecycle** — shuttles gain an `IsActive` flag enabling soft-deactivation with
   automatic cleanup of convoy defaults and filtering from operational queries.

---

## Glossary

Terms not defined here are inherited from the `nmu-shuttle-management` requirements glossary.

- **StopShuttleDefault**: A record linking a Shuttle to a Stop with an OrderPosition, defining
  the default convoy roster that applies to all trips generated for that Stop.
- **ScheduleTimeCampus**: A junction record linking a ScheduleTime to a Campus with a StopOrder,
  defining the ordered sequence of campuses the shuttle passes through on that route.
- **TripNote**: A freeform text record created by an Admin against a TripInstance to document
  incidents, replacements, or general operational information.
- **RequiresScanning**: A flag on ScheduleTime that controls whether QR check-in scanning is
  enforced for trips generated from that template. When false, boarding is open (no scan required).
- **Default Convoy**: The set of shuttles associated with a Stop via StopShuttleDefault records,
  automatically applied as TripShuttleAllocations when new TripInstances are created for that Stop.
- **Default Convoy Advisory**: A UX notice informing the admin that default convoy edits only
  affect trips created by future injection runs (earliest: tonight's run for T+3). Trips for
  T+0 through T+2 already have their own allocations and are unaffected by default changes.

---

## Requirements

### Requirement 1: Stop Default Convoy Management

**User Story:** As an Admin, I want to define a default set of shuttles for each Stop, so that
trips are automatically allocated their convoy when generated each night without manual intervention
for every individual trip.

#### Acceptance Criteria

1. THE System SHALL maintain a `StopShuttleDefault` table that maps Shuttles to Stops with an
   OrderPosition (1-based integer), representing the standing convoy roster for that Stop.
2. THE Admin Portal SHALL display a "Default Convoy" panel on the Stop Detail Screen (see
   Requirement 9), showing the current roster of shuttles in OrderPosition order with license
   plate, accessibility flag, and capacity.
3. THE Admin Portal SHALL allow an Admin to add a Shuttle to a Stop's default convoy, which inserts
   a `StopShuttleDefault` record with the next available OrderPosition.
4. WHEN an accessible Shuttle (`IsAccessibleVehicle = TRUE`) is added to a Stop's default convoy,
   THE System SHALL assign it the lowest available OrderPosition and shift all non-accessible
   shuttle positions up by 1, maintaining the accessible-first ordering invariant.
5. WHEN a non-accessible Shuttle is added to a Stop's default convoy, THE System SHALL append it
   after all existing entries (highest OrderPosition + 1).
6. THE Admin Portal SHALL allow an Admin to remove a Shuttle from a Stop's default convoy.
7. THE Admin Portal SHALL allow an Admin to reorder shuttles within a Stop's default convoy by
   reassigning OrderPosition values, subject to the accessible-first constraint.
8. THE Admin Portal SHALL display an advisory banner on the Default Convoy panel stating: "Changes
   apply to trips created from tonight onwards (earliest: {T+3 date}). Trips for the next 3 days
   already have their convoy assigned — edit those individually from the trip detail screen."
9. WHEN a Shuttle is deactivated (`IsActive` set to `FALSE`), THE System SHALL automatically remove
   all `StopShuttleDefault` records referencing that Shuttle and re-normalise OrderPositions on
   each affected Stop to maintain contiguous ordering (see Requirement 8).
10. FOR ALL `StopShuttleDefault` records on a given Stop, OrderPosition values SHALL be unique and
    contiguous starting from 1 (ordering uniqueness invariant).
11. FOR ALL `StopShuttleDefault` records on a given Stop, every record where
    `Shuttle.IsAccessibleVehicle = TRUE` SHALL have an OrderPosition less than or equal to every
    record where `Shuttle.IsAccessibleVehicle = FALSE` (accessible-first invariant).

---

### Requirement 2: Automatic Convoy Population on Trip Injection

**User Story:** As an Admin, I want trips to be automatically allocated their default convoy when
they are generated each night, so that I only need to manage exceptions rather than assigning
shuttles to every trip manually.

#### Acceptance Criteria

1. WHEN the Rolling Injection Job creates a new `TripInstance` for a Stop, THE System SHALL query
   the `StopShuttleDefault` records for that Stop and create one `TripShuttleAllocation` per
   record, copying the `ShuttleId` and `OrderPosition` directly from the default.
2. IF a Stop has no `StopShuttleDefault` records at the time of injection, THE System SHALL create
   the `TripInstance` with no allocations (empty convoy), and the admin may assign shuttles
   manually via the per-trip assignment endpoint.
3. FOR ALL `TripShuttleAllocation` records created by automatic convoy population, `IsOperational`
   SHALL be set to `TRUE` and `AssignedAt` SHALL be set to the current UTC timestamp.
4. Automatic convoy population from `StopShuttleDefault` SHALL occur only for trips on dates
   T+3 and beyond (i.e., trips being newly created by tonight's injection). Existing allocations
   on trips for T+0 through T+2 SHALL NOT be modified by a default convoy edit.
5. FOR ALL TripInstances created with automatic allocations, the convoy order SHALL preserve the
   `OrderPosition` values from `StopShuttleDefault` exactly (convoy order preservation invariant).

---

### Requirement 3: Route Model — Multi-Campus Waypoints

**User Story:** As the System, I want the route model to reflect that shuttles stop at all campuses
along their route rather than terminating at a single campus, so that data accurately represents
physical operations and marshal location binding works correctly at all campus boarding points.

#### Acceptance Criteria

1. THE System SHALL replace the single `CampusId` foreign key on `ScheduleTime` with a
   `ScheduleTimeCampus` junction table that maps `ScheduleTimeId` to `CampusId` with a `StopOrder`
   integer (1-based), defining the ordered sequence of campuses the shuttle visits on that route.
2. THE Admin Portal SHALL allow an Admin to define the ordered campus sequence for a ScheduleTime
   via the `ScheduleTimeCampus` records (add campus to route, remove, reorder).
3. FOR ALL `ScheduleTimeCampus` records on a given ScheduleTime, `StopOrder` values SHALL be unique
   and contiguous starting from 1 (campus sequence uniqueness invariant).
4. WHEN a Marshal attempts check-in for a `FromCampus` trip, THE System SHALL verify that the
   Marshal's `AssignedCampusId` is present in the `ScheduleTimeCampus` records for that trip's
   ScheduleTime. IF the marshal's campus is not in the route, THE System SHALL reject the check-in
   with HTTP 403.
5. WHEN a Marshal attempts check-in for a `ToCampus` trip, THE System SHALL verify that the
   Marshal's `AssignedStopId` matches `TripInstance.StopId` (unchanged from current behaviour).
6. THE System SHALL NOT require or record which specific campus a student is destined for or
   boards at. Campus is only relevant for marshal location binding.
7. FOR ALL student-facing API responses and app screens, the campus sequence information SHALL NOT
   be exposed as a student's "destination" — the student's booking is identified solely by their
   Stop and direction.

---

### Requirement 4: Scanning Control Per Schedule Slot

**User Story:** As an Admin, I want to mark certain schedule slots as not requiring QR scanning,
so that late-morning ToCampus trips that operate as open boarding are not disrupted by unnecessary
technology.

#### Acceptance Criteria

1. THE `ScheduleTime` entity SHALL include a `RequiresScanning` boolean field (default `TRUE`).
2. THE Admin Portal SHALL allow an Admin to toggle `RequiresScanning` on any ScheduleTime record.
3. WHEN `RequiresScanning = FALSE` for a TripInstance's ScheduleTime, THE App SHALL disable the
   QR scan button on the Marshal's trip detail screen and display an informational message:
   "This trip is open boarding — no QR scan required."
4. WHEN `RequiresScanning = FALSE` for a TripInstance's ScheduleTime, THE System SHALL still track
   the TripInstance normally (generate it, lock it, complete it) — only the QR scan gate is bypassed.
5. THE API `GET /trips/{tripId}/convoy` and trip detail endpoint SHALL include the
   `requiresScanning` flag so the marshal app can determine whether to show or hide the scan button.
6. FOR ALL TripInstances where `ScheduleTime.RequiresScanning = TRUE`, the existing 35-minute scan
   window enforcement SHALL continue to apply (scan rejected if before T-35 min).
7. FOR ALL TripInstances where `ScheduleTime.RequiresScanning = FALSE`, capacity tracking, booking
   creation, and waitlist promotion SHALL continue to function normally.

---

### Requirement 5: Marshal Convoy Panel

**User Story:** As a Marshal, I want to see the convoy order for my current trip — including each
shuttle's license plate, position, and live boarding count — so that I can direct shuttle drivers
to park in the correct order and manage passenger flow at the boarding point.

#### Acceptance Criteria

1. THE App SHALL display a Convoy Panel on the Marshal's trip detail screen showing all
   `TripShuttleAllocation` records for that trip, ordered by `OrderPosition` ASC.
2. FOR EACH shuttle in the Convoy Panel, THE App SHALL display: OrderPosition, LicensePlate,
   `IsAccessibleVehicle` indicator, Capacity, and current boarded count
   (count of Bookings where `ShuttleId = this shuttle AND BoardedStatus = 'Boarded'`).
3. WHEN a Marshal performs a successful QR scan, THE App SHALL immediately invalidate the convoy
   data provider and refresh boarded counts without waiting for the next poll cycle. This ensures
   the scanning marshal sees instant feedback for their own scans.
4. THE App SHALL poll the convoy endpoint on a 30-second interval to pick up boarded count changes
   caused by other marshals scanning at different campus stops on the same route.
5. THE App SHALL clearly distinguish accessible shuttles from non-accessible ones in the Convoy
   Panel (e.g., a visual accessibility indicator).
6. THE API SHALL provide the boarded count per shuttle as part of the convoy status endpoint so
   the marshal app can display accurate live counts.

---

### Requirement 6: Trip Notes

**User Story:** As an Admin, I want to add freeform notes to a trip instance — especially during
and after incidents — so that I can record what actually happened (breakdown details, replacement
vehicle info, driver behaviour) without being forced into a structured form that may not fit the
situation.

#### Acceptance Criteria

1. THE System SHALL maintain a `TripNote` table linked to `TripInstance`, with fields for note
   type, freeform body text, the admin who created it, and a creation timestamp.
2. THE `TripNote.NoteType` field SHALL be constrained to the values: `'Incident'`, `'Replacement'`,
   `'General'`, `'NoShow'`.
3. THE Admin Portal SHALL allow an Admin to create one or more TripNotes for any TripInstance at
   any time, regardless of the trip's current Status.
4. THE Admin Portal SHALL display all TripNotes for a TripInstance in the trip detail view, ordered
   by `CreatedAt` ASC.
5. WHEN an Admin creates a TripNote with NoteType = `'Replacement'` or `'Incident'` on an
   `IncidentActive` TripInstance, THE Admin Portal SHALL display a confirmation dialog:
   "This note will resolve the active incident and send push notifications to all confirmed
   passengers. Proceed?" Only after admin confirmation SHALL the request be submitted.
6. WHEN the confirmed TripNote creation request is processed, THE System SHALL treat this as the
   admin resolution action: set `BreakdownLog.ResolvedAt = DateTime.UtcNow`,
   `BreakdownLog.ResolvedByAdminId` = the admin, and dispatch FCM push notifications to all
   Confirmed Booking holders on that TripInstance.
7. THE request body SHALL include an explicit `"resolveIncident": true` flag when the admin
   confirms resolution. If the flag is absent or false, the TripNote is created as a normal note
   without triggering resolution logic, even if the NoteType is 'Incident' or 'Replacement'.
8. THE push notification dispatched on admin resolution SHALL include the NoteType and a summary
   derived from the first 100 characters of the TripNote body.
9. FOR ALL TripNotes, the `Body` field SHALL be non-empty (minimum 1 character). THE System SHALL
   reject empty body submissions with HTTP 422.
10. The existing `BreakdownLog.ResolutionType` and `BreakdownLog.ReplacementShuttleId` structured
    fields SHALL be retained for analytics purposes, but completing them SHALL be optional. If an
    Admin provides a `replacementShuttleId` alongside the TripNote, THE System SHALL update the
    allocation and tally accordingly. If not provided, the TripNote body alone records the event.

---

### Requirement 7: Admin Stop-Centric Trip View

**User Story:** As an Admin, I want to view and manage trips grouped by Stop, so that I can quickly
see the state of all convoys at a given boarding point without needing to search through a flat
list of all trip instances.

#### Acceptance Criteria

1. THE Admin Portal SHALL provide a "Trips by Stop" view accessible from the Stop management screen
   or the trips panel, showing TripInstances grouped by Stop.
2. FOR EACH Stop, THE view SHALL list upcoming TripInstances (TripDate >= today) with their
   departure time, direction, current Status, and convoy summary (shuttle count, total capacity,
   confirmed booking count).
3. THE view SHALL be filterable by date (default: today and the next 3 days) and by trip Status.
4. FROM the stop-centric view, THE Admin Portal SHALL allow the Admin to navigate to an individual
   TripInstance to assign or remove shuttles from that specific trip's convoy.
5. THE stop-centric view SHALL visually flag trips that have an empty convoy (zero allocations) so
   the admin can identify and address them quickly.

---

### Requirement 8: Shuttle Lifecycle — Soft Deactivation

**User Story:** As an Admin, I want to deactivate a shuttle that is no longer in service, so that
it is excluded from new convoy assignments and trip allocations while preserving its historical
data for analytics.

#### Acceptance Criteria

1. THE `Shuttle` entity SHALL include an `IsActive` boolean field (default `TRUE`).
2. THE Admin Portal SHALL allow an Admin to toggle a Shuttle's `IsActive` status from the shuttle
   management screen.
3. WHEN a Shuttle is deactivated (`IsActive` set to `FALSE`), THE System SHALL automatically remove
   all `StopShuttleDefault` records referencing that Shuttle and re-normalise OrderPositions on
   each affected Stop.
4. WHEN a Shuttle is deactivated, THE System SHALL NOT remove existing `TripShuttleAllocation`
   records for already-created trips. Those trips retain the shuttle (marked `IsOperational` as
   appropriate by admin action).
5. THE Admin Portal shuttle picker (used when adding shuttles to default convoys or per-trip
   assignments) SHALL only show active shuttles (`IsActive = TRUE`).
6. WHEN the Rolling Injection Job populates convoy allocations from `StopShuttleDefault`, it SHALL
   only include shuttles where `Shuttle.IsActive = TRUE` (defensive guard against race conditions
   if deactivation cleanup is still processing).
7. WHEN a deactivated Shuttle is reactivated (`IsActive` set back to `TRUE`), it SHALL NOT be
   automatically re-added to any Stop's default convoy. An admin must manually re-add it.
8. All historical `TripShuttleAllocation`, `BreakdownLog`, and `Booking` records referencing a
   deactivated shuttle SHALL be preserved for analytics queries.

---

### Requirement 9: Admin Detail Screens

**User Story:** As an Admin, I want dedicated detail screens for Stops and Trips in the web portal,
so that I have sufficient screen real estate to view and manage convoy configurations, trip notes,
and operational details without cramming everything into list row expansions.

#### Acceptance Criteria

1. THE Admin Portal SHALL provide a Stop Detail Screen, navigable from the stops list by tapping
   a stop row or a "View Details" action. The screen SHALL contain: stop metadata (name, address,
   GPS, active status), the Default Convoy panel (Requirement 1), and a "View Trips" button
   leading to the stop-centric trip view (Requirement 7).
2. THE Admin Portal SHALL provide a Trip Detail Screen, navigable from the trips list or the
   stop-centric trip view by tapping a trip row. The screen SHALL contain: trip metadata (date,
   departure time, direction, stop, status), convoy allocation panel (current shuttles assigned to
   this specific trip with ability to add/remove/reorder), and the Trip Notes section
   (Requirement 6).
3. THE Admin Portal Trip Detail Screen SHALL also display the ScheduleTime campus route information
   (ordered list of campuses from `ScheduleTimeCampus`) as a read-only informational section.
4. THE Admin Portal Stop Detail Screen and Trip Detail Screen SHALL use standard back-navigation
   (breadcrumb or back arrow) to return to the originating list view.

