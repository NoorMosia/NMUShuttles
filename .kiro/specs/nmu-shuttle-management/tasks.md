# Implementation Plan: NMU Shuttle Management Network

## Overview

This plan breaks the NMU Shuttle Management Network into sequenced, independently executable coding
tasks. Work flows in five phases:

1. **Backend Foundation** — project scaffolding, EF Core models, migrations, auth
2. **Backend Domain Features** — bookings, capacity, check-in, background services, admin endpoints, FCM
3. **Flutter Mobile App** — auth, home, booking, QR, maps, alerts, profile, marshal screens
4. **Flutter Web Admin Portal** — auth, dashboard, CRUD screens, bulk CSV upload
5. **Cross-Cutting Concerns** — FsCheck property-based tests, integration tests, security hardening

Tasks marked `*` are optional test sub-tasks that can be deferred for an MVP sprint.
Each task references the Requirements and Design Properties it satisfies.

---

## Tasks


## Phase 1 — Backend Foundation

- [ ] 1. Scaffold .NET 8 Web API project and solution structure
  - Create solution `NmuShuttle.sln` with projects: `NmuShuttle.Api`, `NmuShuttle.Tests`
  - Configure `appsettings.json` for connection strings, JWT secrets, FCM credentials, and SAST timezone
  - Add NuGet packages: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`,
    `Microsoft.AspNetCore.Authentication.JwtBearer`, `FluentValidation.AspNetCore`,
    `FirebaseAdmin`, `BCrypt.Net-Next`, `FsCheck.Xunit` (test project only)
  - Set up `Program.cs` with service registration stubs, CORS, HTTPS enforcement, HSTS header
  - Configure ASP.NET Core `RateLimiter` middleware (5 req/min on OTP endpoints)
  - _Requirements: 1, 15, 26_

- [ ] 2. Define EF Core entity models and `ShuttleDbContext`
  - [ ] 2.1 Create entity classes in `NmuShuttle.Api/Models/` for all 15 database tables:
    `Student`, `Marshal`, `Admin`, `Campus`, `Stop`, `ScheduleTime`, `CalendarException`,
    `Shuttle`, `TripInstance`, `TripShuttleAllocation`, `Booking`, `PrioritySchedule`,
    `DeviceRegistration`, `Notification`, `Report`
    - Apply all column constraints, nullable annotations, check constraints, and unique indexes
      exactly as specified in the DB schema (design §Database Schema)
    - Use `NVARCHAR`, `INT IDENTITY`, `DATETIME2`, `BIT`, `DATE`, `TIME` column type mappings
    - _Requirements: 2–6, 8–10, 17–22, 23–25_
  - [ ] 2.2 Create `ShuttleDbContext` in `NmuShuttle.Api/Data/`
    - Register all `DbSet<T>` properties
    - Configure composite unique indexes via Fluent API:
      `(ScheduleTimeId, StopId, TripDate)` on TripInstances,
      `(StudentId, TripInstanceId) WHERE BookingStatus != 'Cancelled'` on Bookings,
      `(TripInstanceId, ShuttleId)` on TripShuttleAllocations,
      `(OwnerId, OwnerType, Platform)` on DeviceRegistrations
    - Configure all hot-path indexes (design §Key Indexes)
    - _Requirements: 2.9, 3.8, 5.8, 23.6_

- [ ] 3. Generate and apply initial EF Core migration
  - Run `dotnet ef migrations add InitialCreate`
  - Verify migration script matches the schema specification; adjust as needed
  - Apply migration to a local SQL Server / LocalDB instance
  - Seed a Campus record and at least one Admin record for development
  - _Requirements: 2, 17–22_

- [ ] 4. Implement JWT infrastructure and RBAC middleware
  - Create `JwtTokenService` in `NmuShuttle.Api/Infrastructure/`
    - Issue HS256 tokens with claims: `sub`, `role`, `studentNumber`/`staffNumber`, `iat`, `exp`
    - Student/Marshal tokens: 24-hour expiry; Admin tokens: 8-hour expiry
  - Register JWT Bearer authentication in `Program.cs`
  - Define three ASP.NET Core authorization policies: `StudentPolicy`, `MarshalPolicy`, `AdminPolicy`
  - _Requirements: 1.3, 15.2, 26.4, 26.5, 26.6_


- [ ] 5. Implement OTP authentication endpoints (`AuthController`)
  - [ ] 5.1 Implement `OtpService` in `NmuShuttle.Api/Services/`
    - `GenerateOtp()` → 5-digit numeric string using `Random.Shared.Next(10000, 99999)`
    - `HashOtp(string otp)` → SHA-256 Base64 string
    - `StoreOtpAsync(userId, role, otp)` → hash + set expiry to `UtcNow + 5 minutes`
    - `VerifyOtpAsync(userId, role, submittedOtp)` → compare hash, check expiry, clear fields on any outcome
    - _Requirements: 1.1–1.5, 1.8_
  - [ ] 5.2 Implement `POST /api/v1/auth/student/otp/request` and `POST /api/v1/auth/student/otp/verify`
    - Request returns HTTP 404 if `StudentNumber` not found; HTTP 200 with success message otherwise
    - Verify returns HTTP 401 on expired/invalid OTP; HTTP 200 with JWT on success; clears OTP fields
    - _Requirements: 1.1, 1.3, 1.4, 1.5_
  - [ ] 5.3 Implement `POST /api/v1/auth/marshal/otp/request` and `POST /api/v1/auth/marshal/otp/verify`
    - Mirror student OTP flow using `StaffNumber` lookup
    - _Requirements: 1.2, 1.3, 1.4, 1.5_
  - [ ] 5.4 Implement `POST /api/v1/auth/admin/login`
    - Accept `email` + `password`; verify BCrypt hash; return JWT on success
    - Return HTTP 401 without disclosing which field was wrong
    - _Requirements: 15.2, 15.3_
  - [ ] 5.5 Implement `POST /api/v1/auth/device/register`
    - Upsert `DeviceRegistration` record using `(OwnerId, OwnerType, Platform)` unique key
    - Requires `[Student]` or `[Marshal]` role
    - _Requirements: 1.7_
  - [ ]* 5.6 Write unit tests for `OtpService`
    - Test: expired OTP rejected (T+5min+1sec), valid OTP accepted (T+4min59sec)
    - Test: second verification of the same OTP returns 401 (single-use)
    - Test: non-existent student returns 404
    - _Requirements: 1.1–1.5, 1.8_

- [ ] 6. Checkpoint — Auth layer complete
  - Ensure the solution builds with no errors or warnings
  - OTP request/verify endpoints return correct status codes against a seeded DB
  - Ask the user if any questions or adjustments are needed before proceeding


---

## Phase 2 — Backend Domain Features

- [ ] 7. Implement `CapacityService` and convoy capacity calculation
  - Create `ICapacityService` interface and `CapacityService` implementation
  - `GetTotalCapacityAsync(int tripInstanceId)` — sum `Shuttle.Capacity` for all
    `TripShuttleAllocations` where `IsOperational = TRUE` for the given trip
  - `RecalculateAndPersistAsync(int tripInstanceId)` — recompute on breakdown/overwrite events
  - Register service in DI container
  - _Requirements: 3.1, 3.7, 6.1, 6.4, 18.5_
  - [ ]* 7.1 Write FsCheck property test — Convoy Capacity Summation (Property 9 / P1)
    - Tag: `// Feature: nmu-shuttle-management, Property 9: convoy capacity summation invariant`
    - Generate: arbitrary sets of shuttle capacities with random `IsOperational` flags
    - Assert: `GetTotalCapacityAsync` result == sum of operational-only capacities
    - 100 iterations minimum
    - _Requirements: 3.1, 3.7 | Design Property 9, P1_

- [ ] 8. Implement `BookingService` and booking endpoints
  - [ ] 8.1 Implement `IBookingService` / `BookingService`
    - `CreateBookingAsync(studentId, tripInstanceId)`:
      - Reject if trip is not in `Scheduling` status → HTTP 400
      - Reject if student already has an active booking for the same trip → HTTP 409
      - Determine status: `Confirmed` if `confirmedCount < totalCapacity`, else `WaitingList`
      - Generate `QrCodeToken` via `Guid.NewGuid().ToString("N")` with up-to-3-retry loop
      - Return `BookingDto`
    - `CancelBookingAsync(bookingId, studentId)`:
      - Reject if trip status is `Locked`, `InTransit`, `Completed`, or `IncidentActive` → HTTP 400
      - Set `BookingStatus = Cancelled`
    - `GetStudentBookingsAsync(studentId)` — split upcoming / past by `TripDate`
    - _Requirements: 3.2, 3.3, 3.6, 3.8, 7.4, 7.6–7.9, 23.1_
  - [ ] 8.2 Implement booking endpoints in `BookingsController`
    - `GET /api/v1/trips` — filterable by `date`, `stopId`, `direction`; returns `TripAvailabilityDto`
    - `POST /api/v1/bookings` — delegates to `BookingService.CreateBookingAsync`
    - `GET /api/v1/bookings` — own bookings list
    - `GET /api/v1/bookings/{id}` — booking detail with QR token
    - `DELETE /api/v1/bookings/{id}` — cancel
    - Apply `[Authorize(Roles = "Student")]` on all booking endpoints
    - _Requirements: 7.1–7.9_
  - [ ]* 8.3 Write FsCheck property test — Booking Status Determined by Capacity (Property 10 / P2)
    - Tag: `// Feature: nmu-shuttle-management, Property 10: booking status determined by capacity`
    - Generate: `totalCapacity` (positive int), `existingConfirmed` (non-negative int)
    - Assert: new booking is `Confirmed` iff `existingConfirmed < totalCapacity`; else `WaitingList`
    - Assert: confirmed count never exceeds `totalCapacity` after any single booking
    - _Requirements: 3.2, 3.3, 3.6 | Design Property 10, P2_
  - [ ]* 8.4 Write FsCheck property test — QR Token Uniqueness (Property 13 / P9)
    - Tag: `// Feature: nmu-shuttle-management, Property 13: QR token uniqueness`
    - Generate: `bookingCount` between 1 and 1000
    - Assert: all generated tokens form a set of size == `bookingCount` (no duplicates)
    - _Requirements: 5.8, 23.1, 23.6 | Design Property 13, P9_


- [ ] 9. Implement `TripStateMachineService` and trip status transitions
  - Create `TripStateMachineService` with `IsValidTransition(TripStatus from, TripStatus to)` switch
  - Valid transitions (design §TripInstance State Machine):
    `Scheduling→Locked`, `Scheduling→IncidentActive`, `Locked→InTransit`,
    `InTransit→Completed`, `InTransit→IncidentActive`, `IncidentActive→InTransit`
  - `TransitionAsync(tripId, newStatus)` — validate, write timestamp fields, reject invalid with HTTP 422
  - On `InTransit`: record `InTransitAt = UtcNow`
  - On `Completed`: set `BoardedStatus = NoShow` for all `Pending` bookings on that trip
  - `PATCH /api/v1/trips/{id}/status` endpoint — Admin only
  - _Requirements: 22.1–22.5, 25.5_
  - [ ]* 9.1 Write FsCheck property test — State Machine Completeness (Property 20)
    - Tag: `// Feature: nmu-shuttle-management, Property 20: state machine completeness`
    - Generate: all `(TripStatus from, TripStatus to)` enum pairs via FsCheck generator
    - Assert: `IsValidTransition(from, to)` returns `true` iff pair is in the defined valid set
    - Assert: no path from `Scheduling` to `Completed` can bypass `Locked` and `InTransit`
    - _Requirements: 22.1, 22.2, 22.5 | Design Property 20_

- [ ] 10. Implement `CheckInService` and check-in endpoint
  - Implement `CheckInAsync(qrCodeToken, shuttleId)` (design §Dual-Key Check-In Validation):
    - Return `NotFound` (HTTP 404) if token not found
    - Return `NotConfirmed` (HTTP 422) if `BookingStatus != Confirmed`
    - Return `AlreadyBoarded` (HTTP 409) if `BoardedStatus == Boarded`
    - Validate shuttle is `IsOperational = TRUE` in an allocation for the same trip → HTTP 422
    - On success: set `BoardedStatus = Boarded`, `BoardedAt = UtcNow`, `ShuttleId = shuttleId`
  - Implement `POST /api/v1/checkin` in `CheckInController` — Marshal role only
  - Implement `GET /api/v1/trips/{tripId}/passengers` — returns `PassengerManifestDto`
  - _Requirements: 5.1–5.7, 14.1–14.5_
  - [ ]* 10.1 Write FsCheck property test — Check-In Shuttle Integrity (Property 14)
    - Tag: `// Feature: nmu-shuttle-management, Property 14: check-in shuttle integrity`
    - Generate: trip with random mix of operational and non-operational shuttle allocations
    - Assert: only check-ins with an `IsOperational = TRUE` shuttle succeed
    - Assert: all successful check-ins record a `ShuttleId` from the operational set
    - _Requirements: 5.7 | Design Property 14_

- [ ] 11. Implement vehicle breakdown and standby overwrite
  - `POST /api/v1/trips/{tripId}/breakdown` (Marshal role):
    - Set matching `TripShuttleAllocation.IsOperational = FALSE`
    - Recalculate Total Capacity downward via `CapacityService`
    - Set `TripInstance.Status = IncidentActive`
    - If new capacity < confirmed count: dispatch FCM to all confirmed passengers
    - Return `{ newTotalCapacity, status }`
  - `PUT /api/v1/trips/{tripId}/standby` (Admin role):
    - Update `TripShuttleAllocation.ShuttleId` to replacement; set `IsOperational = TRUE`
    - Recalculate Total Capacity upward
    - Restore trip status to pre-incident status
    - Dispatch FCM to all confirmed passengers
    - Return `{ newTotalCapacity, status }`
  - _Requirements: 6.1–6.7_
  - [ ]* 11.1 Write FsCheck property test — Breakdown Capacity Reduction (Property 15 / P12)
    - Tag: `// Feature: nmu-shuttle-management, Property 15: capacity reduction on breakdown`
    - Generate: convoy with random shuttle capacities; random index of shuttle to break
    - Assert: `newCapacity == oldCapacity - brokenShuttle.Capacity`
    - _Requirements: 6.6 | Design Property 15, P12_
  - [ ]* 11.2 Write FsCheck property test — Standby Capacity Replacement (Property 16 / P13)
    - Tag: `// Feature: nmu-shuttle-management, Property 16: capacity replacement on standby overwrite`
    - Generate: convoy; flag breakdown; apply standby with random replacement capacity
    - Assert: `finalCapacity == originalCapacity - brokenCapacity + replacementCapacity`
    - _Requirements: 6.7 | Design Property 16, P13_


- [ ] 12. Implement `RollingInjectionHostedService`
  - Create `RollingInjectionHostedService : IHostedService` in `BackgroundServices/`
  - Use `PeriodicTimer` or cron-style scheduling to fire once daily at 00:00:00 SAST
  - Implement the full injection algorithm (design §Rolling Injection Job):
    1. Compute `targetDate = LocalDate.Today(SAST) + 3`
    2. Skip if `targetDate` in `CalendarExceptions` table
    3. Skip if `targetDate` is Sunday
    4. Determine `scheduleType` (`Weekday` / `Saturday`)
    5. Insert one `TripInstance` per matching `ScheduleTime` (idempotency via unique constraint)
    6. For each new `TripInstance`, query active `PrioritySchedules` with matching `DayOfWeek` + `ScheduleTimeId` + `StopId`
    7. Insert `Priority` bookings for matched students with `BookingStatus = Confirmed`, `BookingType = Priority`
    8. Wrap steps 5–7 in a single transaction; log outcome
  - _Requirements: 2.1–2.10, 8.4–8.6_
  - [ ]* 12.1 Write FsCheck property test — Injection Date Invariant (Property 4 / P5)
    - Tag: `// Feature: nmu-shuttle-management, Property 4: injection date invariant`
    - Generate: random seed dates (weekdays and Saturdays, not in CalendarExceptions)
    - Assert: every `TripInstance.TripDate` created equals `seedDate + 3`
    - _Requirements: 2.1, 2.9 | Design Property 4, P5_
  - [ ]* 12.2 Write FsCheck property test — Calendar Exception Skip (Property 5 / P6)
    - Tag: `// Feature: nmu-shuttle-management, Property 5: calendar exception skip`
    - Generate: random date sets with mixed CalendarException membership
    - Assert: zero `TripInstances` created for dates in CalendarExceptions; > 0 for valid non-Sunday dates
    - _Requirements: 2.2 | Design Property 5, P6_
  - [ ]* 12.3 Write FsCheck property test — Priority Booking Pre-Clearance (Property 8)
    - Tag: `// Feature: nmu-shuttle-management, Property 8: priority booking pre-clearance invariant`
    - Generate: random active `PrioritySchedule` sets with matching trips
    - Assert: every matched student gets a booking with `BookingType = Priority` and `BookingStatus = Confirmed`
    - _Requirements: 2.7, 2.8, 2.10, 8.5 | Design Property 8_

- [ ] 13. Implement `LockoutGateHostedService` and `WaitlistPromotionService`
  - [ ] 13.1 Create `WaitlistPromotionService` (design §Waitlist Promotion Loop):
    - `RunWaitlistPromotionAsync(tripInstanceId, tx)`:
      - Compute available slots (`totalCapacity - confirmedCount`)
      - Fetch up to `available` `WaitingList` bookings ordered by `CreatedAt ASC`
      - Set each to `Confirmed`; insert `Notification` record; dispatch FCM
    - Return count of promoted bookings
    - _Requirements: 4.4–4.6, 9.1_
  - [ ] 13.2 Create `LockoutGateHostedService : IHostedService`:
    - Continuously queries next `Scheduling` trip by earliest `DepartureTime`
    - Sleeps until `T - 60 minutes`; wakes and calls `LockTrip(tripInstanceId)`
    - `LockTrip` uses `SERIALIZABLE` transaction with `UPDLOCK` on `TripInstances`
    - Sets `Status = Locked`, `LockedAt = UtcNow`; then calls `WaitlistPromotionService`
    - Advances to next trip after each lock; logs per-trip failures without halting loop
    - _Requirements: 4.1–4.4, 4.6–4.8_
  - [ ]* 13.3 Write FsCheck property test — Post-Lock Capacity Ceiling (Property 11 / P7 ordering)
    - Tag: `// Feature: nmu-shuttle-management, Property 11+12: waitlist promotion ceiling and ordering`
    - Generate: random `capacity`, random list of `WaitingList` bookings with distinct `CreatedAt` timestamps
    - Assert: `promotedCount <= capacity`; promoted set is the N earliest by `CreatedAt`
    - Assert: `confirmedCount <= totalCapacity` after promotion
    - _Requirements: 4.4, 4.6, 4.7 | Design Property 11, 12_

- [ ] 14. Implement `FcmNotificationSender` and `NotificationService`
  - Create `FcmNotificationSender` in `Infrastructure/` (design §FCM Push Notification Flow):
    - Wrap `FirebaseAdmin` `FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync`
    - Log per-token failures; never throw on partial failure (Req 9.8)
    - Configure `AndroidConfig` (priority high, channel `shuttle_alerts`) and `ApnsConfig`
  - Create `NotificationService`:
    - `DispatchAsync(studentId, title, message, tripInstanceId?)` — look up `DeviceRegistration` tokens, call sender, insert `Notification` row
    - `DispatchToTripPassengersAsync(tripInstanceId, title, message)` — fetch all confirmed students' tokens, call sender, insert rows
  - Implement `GET /api/v1/notifications` and `PATCH /api/v1/notifications/{id}/read` (Student role)
  - _Requirements: 9.1–9.8_

- [ ] 15. Checkpoint — Core backend domain complete
  - Ensure all services compile and are registered in DI
  - Run existing unit tests; all must pass
  - Manually test: OTP login → create booking → cancel booking → confirm capacity math
  - Ask the user if any questions arise before proceeding to admin endpoints


- [ ] 16. Implement Admin Stop and Shuttle endpoints
  - [ ] 16.1 Create `Admin/StopsController` with:
    - `GET /api/v1/admin/stops` — list all stops
    - `POST /api/v1/admin/stops` — create stop; validate non-empty `StopName` and `GpsCoordinates`
    - `GET /api/v1/admin/stops/{id}` — get by ID
    - `PUT /api/v1/admin/stops/{id}` — edit stop
    - `DELETE /api/v1/admin/stops/{id}` — reject with HTTP 409 if TripInstances reference the stop
    - Implement GPS coordinate parser: accept `"lat,lon"` decimal format; reject out-of-range with HTTP 422
    - _Requirements: 17.1–17.6, 24.1–24.3_
  - [ ]* 16.2 Write FsCheck property test — GPS Coordinate Round-Trip (Property / P10)
    - Tag: `// Feature: nmu-shuttle-management, Property GPS-roundtrip: coordinate round-trip`
    - Generate: random valid `(lat, lon)` pairs within `[-90,90] × [-180,180]`
    - Assert: `Format(Parse(Format(lat, lon)))` parses to the same coordinate pair
    - _Requirements: 24.4 | Design P10_
  - [ ]* 16.3 Write FsCheck property test — GPS Coordinate Range Validation (P11)
    - Tag: `// Feature: nmu-shuttle-management, Property GPS-range: coordinate range validation`
    - Generate: out-of-range lat (< -90 or > 90) or lon (< -180 or > 180) values
    - Assert: parse returns validation error (HTTP 422)
    - _Requirements: 24.5 | Design P11_
  - [ ] 16.4 Create `Admin/ShuttlesController` with:
    - `GET /api/v1/admin/shuttles` — list all shuttles
    - `POST /api/v1/admin/shuttles` — create shuttle; validate `Capacity > 0`, unique `LicensePlate`
    - `GET /api/v1/admin/shuttles/{id}` — get by ID
    - `PUT /api/v1/admin/shuttles/{id}` — edit; on `Capacity` change, call `CapacityService.RecalculateAndPersistAsync` for all active allocations
    - _Requirements: 18.1–18.6_
  - [ ]* 16.5 Write FsCheck property test — Shuttle Capacity Validity (Property 18 / P18)
    - Tag: `// Feature: nmu-shuttle-management, Property 18: shuttle capacity validity`
    - Generate: `Capacity` values including zero and negatives
    - Assert: any `Capacity <= 0` is rejected with HTTP 422
    - _Requirements: 18.6 | Design Property 18_

- [ ] 17. Implement Admin User Management endpoints
  - [ ] 17.1 Create `Admin/UsersController`:
    - `GET /api/v1/admin/users` — paginated, searchable list (name, identifier, email); role filter
    - `POST /api/v1/admin/users` — single add; validate uniqueness of email and identifier; return HTTP 409 on conflict
    - `GET /api/v1/admin/users/{id}` — get user detail
    - `PUT /api/v1/admin/users/{id}` — edit `FirstName`, `LastName`, `UniversityEmail`, `IsPriorityUser`
    - _Requirements: 19.1–19.7_
  - [ ] 17.2 Implement `CsvUserImportService` and bulk upload endpoint:
    - `POST /api/v1/admin/users/bulk` — accept `multipart/form-data`; enforce 10 MB max via `[RequestSizeLimit]`
    - Parse CSV rows; validate `role`, `identifier`, `FirstName`, `LastName`, `UniversityEmail` per row
    - Skip duplicates (within file and against DB); record skip reasons; continue processing
    - Return `{ created, skipped: [{ row, reason }] }`
    - _Requirements: 20.1–20.8_
  - [ ]* 17.3 Write FsCheck property test — Bulk CSV Idempotency (Property 19 / P16)
    - Tag: `// Feature: nmu-shuttle-management, Property 19: bulk CSV idempotency`
    - Generate: random valid CSV files; upload once; then upload identical file again
    - Assert: second upload creates zero new users; all rows in skip list
    - _Requirements: 20.7, 20.8 | Design Property 19, P16_

- [ ] 18. Implement Admin Schedule, Priority Schedule, and Calendar Exception endpoints
  - `GET/POST /api/v1/admin/schedule-times` — list / create ScheduleTime records
  - `PUT/DELETE /api/v1/admin/schedule-times/{id}` — edit/delete (reject delete if future TripInstances reference it)
  - `GET/POST /api/v1/admin/calendar-exceptions` — list (date ordered) / create; reject duplicate `TargetDate` with HTTP 409
  - `DELETE /api/v1/admin/calendar-exceptions/{id}` — remove blackout date
  - `GET/POST /api/v1/admin/priority-schedules` — list / create PrioritySchedule; auto-set `IsPriorityUser = TRUE`
  - `PATCH /api/v1/admin/priority-schedules/{id}` — deactivate (`IsActive = FALSE`)
  - _Requirements: 8.1–8.3, 21.1–21.7_

- [ ] 19. Implement Admin Reports and Analytics endpoints
  - `GET /api/v1/admin/reports` — paginated list with columns: ID, category, studentId, date, status
  - `GET /api/v1/admin/reports/{id}` — full detail: description, adminNotes, linked trip, status
  - `PATCH /api/v1/admin/reports/{id}` — update `Status` (`UnderReview` / `Resolved`), `AdminNotes`; set `UpdatedAt = UtcNow`
  - `GET /api/v1/admin/analytics/overview` — top 3 most-demanded stops, top 3 least-demanded, report status counts
  - `GET /api/v1/admin/analytics/detailed` — expanded booking and demand statistics
  - `POST /api/v1/reports` and `GET /api/v1/reports` (Student role) — submit and list own reports
  - Validate: reject report with empty `Description` with HTTP 422
  - _Requirements: 10.1–10.7, 16.1–16.6_

- [ ] 20. Implement Admin TripInstance management endpoints
  - `GET /api/v1/admin/trips` — list TripInstances filterable by `date`, `status`
  - `GET /api/v1/trips/{id}` — detail (Student, Marshal, Admin roles)
  - `POST /api/v1/admin/trips/{id}/shuttles` — assign a shuttle to a TripInstance (creates `TripShuttleAllocation`)
  - _Requirements: 22.1–22.5_

- [ ] 21. Implement global error handling middleware
  - Add `IExceptionHandler` middleware producing RFC 7807 `ProblemDetails` JSON on all errors
  - Map domain exceptions to HTTP status codes (design §HTTP Status Code Conventions)
  - Ensure `body.status` always equals the HTTP response status code
  - Add `FluentValidation` pipeline behaviour to return HTTP 422 with per-field error names on missing fields
  - _Requirements: 26.1–26.7_
  - [ ]* 21.1 Write FsCheck property test — API Error Status Code Consistency (P17)
    - Tag: `// Feature: nmu-shuttle-management, Property API-errors: error status code consistency`
    - Generate: requests designed to trigger all documented error paths (401, 403, 404, 409, 422)
    - Assert: HTTP header status code == `body.status` for every error response
    - _Requirements: 26.7 | Design P17_

- [ ] 22. Checkpoint — Full backend complete
  - Ensure the solution builds and all unit/property tests pass
  - Smoke-test all major endpoint groups with a REST client (OTP → booking → checkin → admin CRUD)
  - Ask the user if any changes are needed before starting Flutter work


---

## Phase 3 — Flutter Mobile App

- [ ] 23. Scaffold Flutter mobile app project structure
  - Create Flutter project `nmu_shuttle_app` (targets Android and iOS)
  - Add dependencies to `pubspec.yaml`:
    `flutter_riverpod`, `riverpod_annotation`, `go_router`, `dio`, `flutter_secure_storage`,
    `mobile_scanner`, `google_maps_flutter`, `geolocator`, `permission_handler`,
    `firebase_core`, `firebase_messaging`, `qr_flutter`, `jwt_decoder`
  - Create directory structure as specified in design §Flutter Mobile App Architecture:
    `lib/core/{api,auth,router}/`, `lib/features/{auth,student_home,booking,maps,alerts,profile,marshal_home,passenger_list,qr_scan}/`, `lib/shared/{widgets,theme}/`
  - Configure `go_router` with all routes from design §Navigation (auth gate, student routes, marshal routes)
  - Set up Riverpod `ProviderScope` at app root
  - _Requirements: 7.11, 1.6, 13, 14_

- [ ] 24. Implement mobile app API client and auth infrastructure
  - Create `ApiClient` in `lib/core/api/` using `Dio`:
    - Base URL from environment config
    - Request interceptor: attach `Authorization: Bearer <token>` from `flutter_secure_storage`
    - Response interceptor: on 401, clear token and redirect to login via `go_router`
  - Create `AuthNotifier` (`StateNotifier`) in `lib/core/auth/`:
    - `requestOtp(identifier, role)` → calls `POST /auth/{role}/otp/request`
    - `verifyOtp(identifier, otp, role)` → calls `POST /auth/{role}/otp/verify`; stores JWT; decodes role
    - `logout()` → clears token from `flutter_secure_storage`
  - Implement auth guard in `go_router`: redirect to `/login` if no valid token
  - After successful auth: read `role` from JWT via `JwtDecoder`; navigate to `/student/home` or `/marshal/home`
  - Register FCM token post-login: call `FirebaseMessaging.instance.getToken()`, POST to `/auth/device/register`
  - _Requirements: 1.3, 1.6, 1.7, 15.4, 15.5_

- [ ] 25. Implement Login screen (role-switcher UI)
  - Build `LoginScreen` at route `/login`:
    - Role selector (Student / Marshal toggle) — shown before identifier entry (Req 1.6)
    - Identifier input field (`StudentNumber` or `StaffNumber` based on role)
    - "Send OTP" button → calls `AuthNotifier.requestOtp`
    - OTP input field (5-digit) + "Verify" button → calls `AuthNotifier.verifyOtp`
    - Show loading state during requests; show inline error on 404 / 401
  - _Requirements: 1.1–1.6_
  - [ ]* 25.1 Write widget test for `LoginScreen`
    - Test: role-switcher changes label and identifier placeholder
    - Test: invalid OTP shows error message inline
    - _Requirements: 1.1–1.6_

- [ ] 26. Implement Student Home screen (Upcoming / Past tabs + FAB)
  - Build `StudentHomeScreen` at `/student/home`:
    - Bottom navigation bar with four items: Home, Maps, Alerts, Profile (Req 7.11)
    - Upcoming tab: list of bookings where `TripDate >= today` and `BookingStatus != Cancelled`
    - Past tab: list of bookings where `TripDate < today` or `BookingStatus == Cancelled`
    - Each booking card shows: direction, stop name, date, departure time, status badge
    - FAB (bottom-right) navigates to `/student/booking/new`
    - Wire to `upcomingBookingsProvider` and `pastBookingsProvider` (FutureProvider.autoDispose)
  - _Requirements: 7.6, 7.7, 7.10, 7.11_

- [ ] 27. Implement New Booking form screen
  - Build `NewBookingScreen` at `/student/booking/new`:
    - Direction selector (To Campus / From Campus)
    - Stop search field (filters stops from `GET /api/v1/admin/stops` or a stops endpoint)
    - Campus selector
    - 4-day date picker (today through today + 3 days)
    - Departure time selector filtered by selected date — calls `GET /api/v1/trips`
    - Show available seats; if capacity full, show "Place on Waiting List" CTA with capacity notice
    - "Confirm Booking" / "Place on Waiting List" button → POST `/bookings`
    - On success: navigate to `/student/booking/:id` (Booking Detail screen)
    - _Requirements: 7.1–7.5_
  - [ ]* 27.1 Write widget test for booking form capacity display
    - Test: when `availableSeats > 0`, CTA reads "Confirm Booking"
    - Test: when `availableSeats == 0`, CTA reads "Place on Waiting List" with notice visible
    - _Requirements: 7.2, 7.3_

- [ ] 28. Implement Booking Detail screen (QR code display)
  - Build `BookingDetailScreen` at `/student/booking/:id`:
    - Display QR code image from `qr_flutter` widget rendering the `QrCodeToken`
    - Minimum rendered size 256×256 logical pixels
    - Display: trip direction, stop name, campus, date, departure time, booking status
    - "Download QR Code" action saving QR image to device photo gallery
    - Cancel button (visible only when `TripInstance.Status == Scheduling`) → DELETE `/bookings/{id}`
    - _Requirements: 5.8, 7.5, 7.8, 7.9, 23.2, 23.3, 23.4_

- [ ] 29. Implement Maps screen
  - Build `MapsScreen` at `/student/maps`:
    - Display `GoogleMap` widget with all active stop markers from the stops list
    - Tap on marker: show popup with stop name + "Get Directions" button
    - "Get Directions" → launch device navigation intent with stop GPS coordinates
    - Search field filtering stop markers by `StopName` in real time
    - "Nearby Stops" list sorted by approximate proximity using device GPS
    - Request location permission before using device GPS; show permission prompt if denied
    - _Requirements: 11.1–11.6_

- [ ] 30. Implement Alerts screen and Alert Detail screen
  - Build `AlertsScreen` at `/student/alerts`:
    - List of `Notification` records from `GET /api/v1/notifications`, ordered `CreatedAt DESC`
    - Unread notification badge on Alerts bottom nav item when `unreadCount > 0`
    - Tap notification → navigate to `/student/alerts/:id`
  - Build `AlertDetailScreen` at `/student/alerts/:id`:
    - Display full `Notification.Title` and `Notification.Message`
    - On screen open: call `PATCH /api/v1/notifications/{id}/read`
  - Wire to `unreadNotificationCountProvider`
  - _Requirements: 9.4–9.7_

- [ ] 31. Implement Profile screen and Report Form screen
  - Build `ProfileScreen` at `/student/profile`:
    - Display `FirstName`, `LastName`, `StudentNumber`, `UniversityEmail`
    - "My Reports" section: list of student's reports ordered by `CreatedAt DESC`, showing Category + Status
    - Tap report item → show report detail (Category, Description, Status, AdminNotes if present)
    - Logout button: clear token → navigate to `/login`
  - Build `ReportFormScreen` at `/student/profile/report/new`:
    - Category selector: Driver Behavior, Timetable Issue, Vehicle Quality, Other
    - Optional TripInstance reference field
    - Free-text description field
    - Submit → POST `/reports`; reject empty description client-side
  - _Requirements: 10.1–10.4, 12.1–12.4_

- [ ] 32. Implement Marshal Home screen
  - Build `MarshalHomeScreen` at `/marshal/home`:
    - Display marshal's assigned Campus boarding point
    - "Next Boarding" card: departure time + status of the soonest non-Completed trip
    - Chronological list of remaining TripInstances for today at the marshal's campus
    - "QR Code Scanner" button → navigate to `/marshal/scan`
    - Directions button → launch navigation to marshal's assigned campus stop GPS
    - _Requirements: 13.1–13.5_

- [ ] 33. Implement Marshal Passenger List screen
  - Build `PassengerListScreen` at `/marshal/trip/:id/passengers`:
    - List all Confirmed Bookings: student name, `BoardedStatus` per row
    - Real-time counts: total confirmed, boarded, pending
    - Poll `GET /api/v1/trips/{tripId}/passengers` every 30 seconds via `passengerManifestProvider`
    - QR scanner button → navigate to `/marshal/scan` with active tripId context
    - "Flag Breakdown" button → calls `POST /api/v1/trips/{tripId}/breakdown` with selected shuttle
    - _Requirements: 14.1–14.6_

- [ ] 34. Implement QR Scan screen
  - Build `QrScanScreen` at `/marshal/scan`:
    - Use `mobile_scanner` package for camera-based QR code scanning
    - On scan: decode `QrCodeToken` → POST `/checkin` with `{ qrCodeToken, shuttleId }`
    - On success: immediately invalidate `passengerManifestProvider` for the active trip (instant feedback)
    - On failure: display inline error (not confirmed, wrong shuttle, already boarded)
    - _Requirements: 14.3–14.5_
  - [ ]* 34.1 Write widget test for QR scan result feedback
    - Test: on check-in success, success indicator is shown
    - Test: on HTTP 409 (already boarded), error message "Already boarded" shown inline
    - _Requirements: 14.4, 14.5_

- [ ] 35. Checkpoint — Flutter mobile app complete
  - Run `flutter analyze`; fix all warnings and errors
  - Run `flutter test` (unit + widget tests); all must pass
  - Manually smoke-test on an Android emulator: login → book → view QR → marshal scan
  - Ask the user if any adjustments are needed before starting the admin portal


---

## Phase 4 — Flutter Web Admin Portal

- [ ] 36. Scaffold Flutter Web admin portal project and layout
  - Create separate Flutter Web entry point `admin_portal/` with its own `main.dart`
  - Add dependencies: `flutter_riverpod`, `go_router`, `dio`, `file_picker`, `csv`
  - Create directory structure: `admin_portal/lib/core/{api,auth,router}/`, `features/{auth,overview,stops,shuttles,users,schedule,reports,trips}/`
  - Implement persistent sidebar layout (design §Flutter Web Admin Portal Architecture):
    - Permanent `NavigationDrawer` / custom `SideRail` with links: Overview, Shuttle Stops, Shuttles, Users, Schedule, Reports
    - Active route highlighted via `GoRouterState`
  - Configure `go_router` for admin routes: `/admin/login`, `/admin/overview`, `/admin/stops`, etc.
  - Token stored in `localStorage` (web); Admin JWT interceptor in Dio client
  - _Requirements: 15, 16, 17–21_

- [ ] 37. Implement Admin Login screen
  - Build `AdminLoginScreen` at `/admin/login`:
    - Email address field + password field
    - "Login" button → POST `/auth/admin/login`
    - On success: store token, redirect to `/admin/overview`
    - On 401: show inline error without disclosing which field was wrong
    - On token expiry (401 from any subsequent request): redirect to `/admin/login` with session-expiry message
  - Logout action in sidebar: invalidate token, return to login screen
  - _Requirements: 15.1–15.5_

- [ ] 38. Implement Operations Overview dashboard
  - Build `OverviewScreen` at `/admin/overview` (default landing page after login):
    - "Top 3 Most In-Demand Stops" panel with relative progress bars
    - "Top 3 Least In-Demand Stops" panel with relative progress bars
    - Reports Summary panel: count of Submitted / Under Review / Resolved
    - "View Detailed Analytics" button → navigate to detailed analytics view
    - Wire to `GET /api/v1/admin/analytics/overview`
  - Build `DetailedAnalyticsScreen`: expanded booking and demand statistics from `GET /api/v1/admin/analytics/detailed`
  - _Requirements: 16.1–16.6_

- [ ] 39. Implement Stops management screens
  - `StopsListScreen` at `/admin/stops`:
    - Table listing all stops with columns: StopName, GpsCoordinates, IsActive
    - "New Stop" button → navigate to `/admin/stops/new`
    - Row tap → navigate to `/admin/stops/:id/edit`
  - `NewStopScreen` at `/admin/stops/new`:
    - Fields: StopName, GpsCoordinates
    - Submit → POST `/admin/stops`; show HTTP 409 uniqueness error inline
  - `EditStopScreen` at `/admin/stops/:id/edit`:
    - Pre-filled form; submit → PUT `/admin/stops/{id}`
    - Delete button → DELETE `/admin/stops/{id}`; show HTTP 409 error if TripInstances reference it
  - _Requirements: 17.1–17.6_

- [ ] 40. Implement Shuttles management screens
  - `ShuttlesListScreen` at `/admin/shuttles`:
    - Table: LicensePlate, Capacity, VendorCompanyName, IsAccessibleVehicle
    - "New Shuttle" and row edit navigation
  - `NewShuttleScreen` / `EditShuttleScreen`:
    - Fields: LicensePlate, Capacity (must be > 0), VendorCompanyName, IsAccessibleVehicle toggle
    - Submit → POST or PUT `/admin/shuttles`; show HTTP 409 on duplicate LicensePlate
    - Capacity `<= 0` → client-side validation error before submit
  - _Requirements: 18.1–18.6_

- [ ] 41. Implement Users management screens (individual add)
  - `UsersListScreen` at `/admin/users`:
    - Paginated, searchable table: identifier, name, role, email
    - Role filter buttons: All / Students / Marshals / Admins
    - Search field filtering by name, identifier, email in real time
    - "Add Single User" button → navigate to `/admin/users/new`
    - Row tap → navigate to `/admin/users/:id/edit`
  - `AddUserScreen` at `/admin/users/new`:
    - Role selector (Student / Marshal / Admin), identifier, FirstName, LastName, UniversityEmail
    - Submit → POST `/admin/users`; show HTTP 409 on duplicate email or identifier
  - `EditUserScreen` at `/admin/users/:id/edit`:
    - Edit FirstName, LastName, UniversityEmail, IsPriorityUser toggle
  - _Requirements: 19.1–19.7_

- [ ] 42. Implement Bulk CSV Upload screen
  - `BulkUploadScreen` at `/admin/users/bulk`:
    - Drag-and-drop zone + "Browse" file picker fallback (`.csv` only)
    - Client-side: reject files > 10 MB before upload
    - Upload bytes as `multipart/form-data` → POST `/admin/users/bulk`
    - Show loading state during upload
    - On response: display confirmation table with success count + skipped rows with error reasons
    - _Requirements: 20.1–20.8_

- [ ] 43. Implement Schedule and Calendar Exception management screens
  - `ScheduleScreen` at `/admin/schedule`:
    - List `ScheduleTime` records; form to create (DepartureTime, Direction, ScheduleType, CampusId, StopId)
    - Edit / delete ScheduleTime (reject delete if future TripInstances reference it)
    - List `CalendarException` records in date order; form to create (TargetDate, Type, Description)
    - Delete CalendarException button; show HTTP 409 on duplicate TargetDate
  - _Requirements: 21.1–21.7_

- [ ] 44. Implement Reports review screens
  - `ReportsListScreen` at `/admin/reports`:
    - Paginated table: ID, Category, StudentId, submitted date, Status
  - `ReportDetailScreen` at `/admin/reports/:id`:
    - Full detail: Category, Description, AdminNotes, linked TripInstance, Status
    - Status selector (Submitted / Under Review / Resolved) + AdminNotes textarea
    - Submit → PATCH `/admin/reports/{id}`
  - _Requirements: 10.5–10.7_

- [ ] 45. Checkpoint — Admin portal complete
  - Run `flutter analyze` on admin portal; fix all warnings
  - Run `flutter test` on admin portal widget tests
  - Smoke-test in browser: login → overview → create stop → add user → bulk upload → report review
  - Ask the user if any changes are needed


---

## Phase 5 — Cross-Cutting Concerns

- [ ] 46. Write remaining FsCheck property-based tests
  - [ ]* 46.1 OTP Single-Use Property (Property 1 / P3)
    - Tag: `// Feature: nmu-shuttle-management, Property 1: OTP single-use enforcement`
    - Generate: random valid student/marshal identifiers; request OTP; verify once (expect success); verify same code again (expect 401)
    - Assert: second verification always returns 401 for any generated OTP
    - 100 iterations minimum
    - _Requirements: 1.8 | Design Property 1, P3_
  - [ ]* 46.2 OTP Format and Expiry Invariant (Property 2 / P4)
    - Tag: `// Feature: nmu-shuttle-management, Property 2: OTP format and expiry invariant`
    - Generate: random valid identifiers; request OTP
    - Assert: generated OTP is exactly 5 decimal digits; stored expiry is in `[now+4:55, now+5:05]`
    - For expiry boundary: verify at `T + 5min + 1s` → failure; verify at `T + 4min + 59s` → success
    - _Requirements: 1.1, 1.2 | Design Property 2, P4_
  - [ ]* 46.3 Device Registration Round-Trip (Property 3)
    - Tag: `// Feature: nmu-shuttle-management, Property 3: device registration round-trip`
    - Generate: random authenticated users, random FCM token strings
    - Assert: after upsert, `DeviceRegistration` maps that user's identity to exactly that token
    - _Requirements: 1.7 | Design Property 3_
  - [ ]* 46.4 Injected TripInstances have Scheduling status (Property 7)
    - Tag: `// Feature: nmu-shuttle-management, Property 7: injected TripInstances have Scheduling status`
    - Generate: random injection runs (weekday and Saturday dates)
    - Assert: every created TripInstance has `Status = Scheduling` at creation time
    - _Requirements: 2.6 | Design Property 7_
  - [ ]* 46.5 Stop Data Completeness (Property 17)
    - Tag: `// Feature: nmu-shuttle-management, Property 17: stop data completeness`
    - Generate: stop creation requests where `StopName` or `GpsCoordinates` is empty / whitespace
    - Assert: every such request is rejected with HTTP 422
    - _Requirements: 17.5 | Design Property 17_
  - [ ]* 46.6 Booking Status Lifecycle Domain Invariant (P14)
    - Tag: `// Feature: nmu-shuttle-management, Property booking-domain: booking status domain invariant`
    - Generate: arbitrary sequences of booking operations (create, cancel, promote, check-in, complete trip)
    - Assert: `BookingStatus ∈ {Confirmed, WaitingList, Cancelled}` at every step
    - Assert: `BoardedStatus ∈ {Pending, Boarded, NoShow}` at every step
    - Assert: `BookingType ∈ {Standard, Priority}` at every step
    - _Requirements: 25.1–25.4 | Design P14_
  - [ ]* 46.7 Completion Accounting Invariant (P15)
    - Tag: `// Feature: nmu-shuttle-management, Property completion-accounting: completion accounting invariant`
    - Generate: full trip lifecycles with random boarding percentages
    - Assert: after `Completed`, `COUNT(Boarded) + COUNT(NoShow) == COUNT(Confirmed)` for every trip
    - _Requirements: 25.5 | Design P15_
  - [ ]* 46.8 QR Token Round-Trip — encode → scan → decode (P8)
    - Tag: `// Feature: nmu-shuttle-management, Property QR-roundtrip: QR token round-trip`
    - Generate: random token strings of 32+ characters
    - Assert: `Decode(Encode(token)) == token` for all generated tokens
    - _Requirements: 23.5 | Design P8_
  - [ ]* 46.9 Schedule-Type Matching Invariant (Property 6 / P6 extended)
    - Tag: `// Feature: nmu-shuttle-management, Property 6: schedule-type matching invariant`
    - Generate: weekday dates, Saturday dates, Sunday dates — all not in CalendarExceptions
    - Assert weekday: exactly one TripInstance per `ScheduleType = Weekday` entry
    - Assert Saturday: exactly one TripInstance per `ScheduleType = Saturday` entry
    - Assert Sunday: zero TripInstances created
    - _Requirements: 2.3, 2.4, 2.5 | Design Property 6_

- [ ] 47. Write backend integration tests
  - Use `WebApplicationFactory<Program>` with a LocalDB or in-memory SQL Server test database
  - [ ]* 47.1 End-to-end booking flow:
    - OTP login (student) → create booking → verify `Booking` row in DB → `QrCodeToken` in response
    - _Requirements: 1, 3, 7, 23_
  - [ ]* 47.2 Rolling Injection Job — mocked clock:
    - Weekday date: verify TripInstances created + Priority Bookings pre-populated
    - CalendarException date: verify zero TripInstances created
    - Saturday: verify only `Saturday` schedule-type trips
    - _Requirements: 2, 8_
  - [ ]* 47.3 Lockout Gate — fast-forwarded clock:
    - Advance mock clock to T-60min; verify trip status → `Locked`; verify waitlist promotions fire; verify FCM dispatch
    - _Requirements: 4, 9_
  - [ ]* 47.4 Dual-key check-in flow:
    - Create booking → marshal check-in with correct shuttle → verify `BoardedStatus = Boarded`
    - Attempt re-check-in → verify HTTP 409
    - _Requirements: 5_
  - [ ]* 47.5 Admin CSV bulk upload:
    - Upload CSV with valid rows, duplicate rows, and malformed rows
    - Assert: `created` count matches valid-only rows; `skipped` list contains duplicate and malformed rows
    - Upload same file again → assert zero new users created
    - _Requirements: 20_

- [ ] 48. Write Flutter integration tests (mobile app)
  - Use `integration_test` package targeting Android emulator
  - [ ]* 48.1 Full student flow smoke test:
    - Launch app → student login (OTP) → book a trip → view booking detail → verify QR code rendered
    - _Requirements: 1, 7, 23_
  - [ ]* 48.2 Marshal flow smoke test:
    - Launch app → marshal login → view home screen → navigate to passenger list
    - _Requirements: 13, 14_

- [ ] 49. Security hardening
  - [ ] 49.1 Verify rate limiting is applied to OTP request endpoints (5 req/min per IP)
    - Write integration test that makes 6 rapid requests to `POST /auth/student/otp/request`; assert 6th returns HTTP 429
    - _Requirements: 1 (design §Security Measures)_
  - [ ] 49.2 Verify RBAC enforcement on all endpoint groups
    - Write tests: Student token → Marshal-only endpoint → HTTP 403; Marshal token → Admin-only endpoint → HTTP 403
    - Unauthenticated request → any protected endpoint → HTTP 401
    - _Requirements: 26.4, 26.5, 26.6_
  - [ ] 49.3 Verify CSV upload rejects files > 10 MB and non-`.csv` content types
    - Write tests: upload an 11 MB file → HTTP 413; upload a `.txt` file → HTTP 422
    - _Requirements: 20.2_
  - [ ] 49.4 Verify SQL injection is not possible through booking/stop endpoints
    - Send SQL injection payloads in `StopName`, `Description`, and `studentNumber` fields
    - Assert: EF Core parameterized queries produce no SQL errors and return appropriate 422/404
    - _Requirements: 26 (design §Security Measures)_

- [ ] 50. Final checkpoint — All phases complete
  - Run the full .NET test suite: `dotnet test --verbosity normal`; all tests must pass
  - Run `flutter test` for both mobile app and admin portal; all tests must pass
  - Confirm the solution builds in Release mode: `dotnet build -c Release` and `flutter build apk`
  - Ask the user if there are any remaining questions or changes needed


---

## Notes

- Tasks marked with `*` are optional test sub-tasks and can be deferred for a faster MVP sprint.
  All production code tasks (no `*`) must be completed for a functional system.
- Each task references specific requirements for full traceability.
- Checkpoints at tasks 6, 15, 22, 35, 45, and 50 ensure incremental validation and give the team
  natural review points before advancing.
- Property-based tests use **FsCheck** (.NET) with a minimum of 100 iterations per property.
  Each test carries a tag comment in the format:
  `// Feature: nmu-shuttle-management, Property N: <property text>`
- Flutter tests use `flutter_test` (unit/widget), `mockito` (service mocks), and `integration_test`
  (emulator smoke tests).
- The admin portal (`admin_portal/`) is a separate Flutter Web entry point — it is compiled and
  deployed independently from the mobile app.

## Task Dependency Graph

```json
{
  "waves": [
    {
      "wave": 1,
      "tasks": ["1"],
      "description": "Scaffold .NET 8 Web API project and solution structure"
    },
    {
      "wave": 2,
      "tasks": ["2"],
      "description": "Define EF Core entity models and ShuttleDbContext"
    },
    {
      "wave": 3,
      "tasks": ["3"],
      "description": "Generate and apply initial EF Core migration"
    },
    {
      "wave": 4,
      "tasks": ["4"],
      "description": "Implement JWT infrastructure and RBAC middleware"
    },
    {
      "wave": 5,
      "tasks": ["5"],
      "description": "Implement OTP authentication endpoints"
    },
    {
      "wave": 6,
      "tasks": ["6"],
      "description": "Checkpoint — Auth layer complete"
    },
    {
      "wave": 7,
      "tasks": ["7"],
      "description": "Implement CapacityService and convoy capacity calculation"
    },
    {
      "wave": 8,
      "tasks": ["8"],
      "description": "Implement BookingService and booking endpoints"
    },
    {
      "wave": 9,
      "tasks": ["9", "12"],
      "description": "TripStateMachineService and RollingInjectionHostedService (parallel)"
    },
    {
      "wave": 10,
      "tasks": ["10", "13"],
      "description": "CheckInService and LockoutGateHostedService (parallel)"
    },
    {
      "wave": 11,
      "tasks": ["11", "14"],
      "description": "Breakdown/standby overwrite and FCM notification infrastructure (parallel)"
    },
    {
      "wave": 12,
      "tasks": ["15"],
      "description": "Checkpoint — Core backend domain complete"
    },
    {
      "wave": 13,
      "tasks": ["16", "17", "18", "19", "20"],
      "description": "Admin endpoints — stops, shuttles, users, schedule, reports, trips (parallel)"
    },
    {
      "wave": 14,
      "tasks": ["21"],
      "description": "Global error handling middleware"
    },
    {
      "wave": 15,
      "tasks": ["22"],
      "description": "Checkpoint — Full backend complete"
    },
    {
      "wave": 16,
      "tasks": ["23", "36"],
      "description": "Scaffold Flutter mobile app and Flutter Web admin portal (parallel)"
    },
    {
      "wave": 17,
      "tasks": ["24", "37"],
      "description": "Mobile API client + auth infrastructure and Admin Login screen (parallel)"
    },
    {
      "wave": 18,
      "tasks": ["25", "38"],
      "description": "Mobile Login screen and Admin Overview dashboard (parallel)"
    },
    {
      "wave": 19,
      "tasks": ["26", "39", "40", "41", "43", "44"],
      "description": "Student Home screen and Admin CRUD screens (parallel)"
    },
    {
      "wave": 20,
      "tasks": ["27", "42"],
      "description": "New Booking form and Bulk CSV upload screen (parallel)"
    },
    {
      "wave": 21,
      "tasks": ["28"],
      "description": "Booking Detail screen with QR code display"
    },
    {
      "wave": 22,
      "tasks": ["29", "30", "31", "32"],
      "description": "Maps, Alerts, Profile, and Marshal Home screens (parallel)"
    },
    {
      "wave": 23,
      "tasks": ["33"],
      "description": "Marshal Passenger List screen"
    },
    {
      "wave": 24,
      "tasks": ["34"],
      "description": "QR Scan screen"
    },
    {
      "wave": 25,
      "tasks": ["35", "45"],
      "description": "Checkpoints — Mobile app complete and Admin portal complete"
    },
    {
      "wave": 26,
      "tasks": ["46", "47", "48", "49"],
      "description": "Property-based tests, integration tests, and security hardening (parallel)"
    },
    {
      "wave": 27,
      "tasks": ["50"],
      "description": "Final checkpoint — all phases complete"
    }
  ]
}
```
