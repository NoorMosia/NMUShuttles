# Implementation Plan: NMU Shuttle Management Network

## Overview

This plan converts the NMU Shuttle Management Network design into a sequenced set of coding tasks.
Tasks are ordered: backend foundation → backend domain features → Flutter mobile app → Flutter Web
admin portal → cross-cutting concerns (PBT, integration tests, security hardening).

Each task is independently executable, references the relevant requirements, and builds on the
previous steps so no code is left orphaned.

---

## Tasks


### Phase 1 — Backend Foundation

- [ ] 1. Scaffold the .NET 8 Web API project and solution structure
  - [ ] 1.1 Create the ASP.NET Core Web API project (`NmuShuttle.Api`) targeting .NET 8
    - Add projects: `NmuShuttle.Api`, `NmuShuttle.Tests`
    - Configure `Program.cs` with builder pattern, Swagger/OpenAPI, global `ProblemDetails` exception handler
    - Add packages: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools`,
      `FluentValidation.AspNetCore`, `BCrypt.Net-Next`, `System.IdentityModel.Tokens.Jwt`,
      `FirebaseAdmin`, `FsCheck.NUnit` (test project only)
    - Create folder skeleton: `Controllers/`, `Controllers/Admin/`, `Services/`, `BackgroundServices/`,
      `Data/`, `Data/Migrations/`, `Models/`, `DTOs/`, `Infrastructure/`
    - _Requirements: 26.1, 26.3_

  - [ ] 1.2 Define all EF Core entity models in `Models/`
    - Create C# classes for: `Student`, `Marshal`, `Admin`, `Campus`, `Stop`, `ScheduleTime`,
      `CalendarException`, `Shuttle`, `TripInstance`, `TripShuttleAllocation`, `Booking`,
      `PrioritySchedule`, `DeviceRegistration`, `Notification`, `Report`
    - Apply all column constraints, CHECK constraints (via `HasCheckConstraint`), and UNIQUE indexes
      from the DB schema section of the design document
    - Define all FK relationships and navigation properties
    - _Requirements: 3.1, 4.1, 5.8, 22.1, 25.2_

  - [ ] 1.3 Create `ShuttleDbContext` and configure EF Core mappings
    - Register all `DbSet<T>` properties
    - Override `OnModelCreating` to configure composite unique indexes
      (`(ScheduleTimeId, StopId, TripDate)`, `(StudentId, TripInstanceId)` filtered on non-Cancelled,
      `(TripInstanceId, ShuttleId)`, `(OwnerId, OwnerType, Platform)`)
    - Add hot-query indexes from design: `IX_Bookings_TripInstanceId_Status`, `IX_Bookings_QrCodeToken`,
      `IX_TripInstances_TripDate_Status`, etc.
    - Register `ShuttleDbContext` in `Program.cs` with SQL Server connection string from config
    - _Requirements: 3.6, 5.8_

  - [ ] 1.4 Generate and apply the initial EF Core migration
    - Run `dotnet ef migrations add InitialCreate` and review the generated migration
    - Verify all tables, columns, constraints, and indexes are present in the migration output
    - Apply migration to a local SQL Server instance: `dotnet ef database update`
    - _Requirements: 2.1, 3.1_


- [ ] 2. Implement JWT infrastructure and OTP-based authentication
  - [ ] 2.1 Implement `JwtTokenService` in `Infrastructure/`
    - Create `IJwtTokenService` interface with `GenerateToken(int id, string role, string identifier)` method
    - Implement HS256 signing with 256-bit secret from `IConfiguration`
    - Include claims: `sub`, `role`, `studentNumber`/`staffNumber`, `iat`, `exp`
    - Student/Marshal expiry: 24 hours; Admin expiry: 8 hours
    - Register JWT Bearer authentication middleware in `Program.cs` with `ValidateIssuerSigningKey = true`
    - _Requirements: 1.3, 15.2_

  - [ ] 2.2 Implement `OtpService` in `Services/`
    - Create `IOtpService` interface with `GenerateAsync(string identifier, string ownerType)` and
      `VerifyAsync(string identifier, string submittedOtp, string ownerType)` methods
    - `GenerateOtp()`: `Random.Shared.Next(10000, 99999).ToString()` — exactly 5 decimal digits
    - `HashOtp(string otp)`: SHA-256 via `SHA256.HashData`; never store plain text
    - Store `OtpHash` and `OtpExpiry = DateTime.UtcNow.AddMinutes(5)` in Student/Marshal rows
    - On verify: reject if expired (clear fields + return 401); reject if hash mismatch (clear + 401);
      on success clear fields and return JWT
    - Wire `OtpExpiry` clearing on both pass and fail paths (single-use enforcement)
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.8_

  - [ ] 2.3 Implement `AuthController` with OTP and admin login endpoints
    - `POST /api/v1/auth/student/otp/request` — calls `OtpService`; returns 404 if student not found
    - `POST /api/v1/auth/student/otp/verify` — calls `OtpService`; returns JWT on success
    - `POST /api/v1/auth/marshal/otp/request` — same pattern for marshals
    - `POST /api/v1/auth/marshal/otp/verify` — same pattern for marshals
    - `POST /api/v1/auth/admin/login` — BCrypt.Verify password; no disclosure of which field is wrong
    - Apply ASP.NET Core `RateLimiter` (sliding window, 5 req/min per IP) to OTP request endpoints
    - Register `[AllowAnonymous]` on all auth endpoints; apply `[Authorize]` globally via policy
    - _Requirements: 1.1–1.5, 15.1–15.3, 26.4_

  - [ ] 2.4 Implement `POST /api/v1/auth/device/register` — FCM device registration
    - Accept `{ fcmToken: string, platform: "android"|"ios" }`
    - Upsert `DeviceRegistration`: if row exists for `(OwnerId, OwnerType, Platform)` update token;
      otherwise insert
    - Extract `OwnerId` and `OwnerType` from the authenticated JWT claims
    - Require `[Authorize(Roles = "Student,Marshal")]`
    - _Requirements: 1.7_

  - [ ]* 2.5 Write unit tests for `OtpService`
    - Test: 5-digit format invariant (all generated OTPs are numeric and length == 5)
    - Test: expired OTP (clock + 5 min + 1 s) returns 401 and clears fields
    - Test: valid OTP within window returns JWT
    - Test: second verify of same OTP is rejected (single-use)
    - Test: non-existent StudentNumber returns 404
    - _Requirements: 1.1, 1.2, 1.4, 1.8_


- [ ] 3. Checkpoint — Backend Foundation
  - Verify the project builds with `dotnet build`; all migrations apply cleanly; JWT round-trip test passes;
    OTP unit tests green. Ask the user if any questions arise before continuing.

---

### Phase 2 — Backend Domain Features

- [ ] 4. Implement convoy capacity and booking services
  - [ ] 4.1 Implement `CapacityService` in `Services/`
    - Create `ICapacityService` with `GetTotalCapacityAsync(int tripInstanceId)` and
      `GetConfirmedCountAsync(int tripInstanceId)` methods
    - `GetTotalCapacityAsync`: LINQ join `TripShuttleAllocations` → `Shuttles` filtering
      `IsOperational == true`, sum `Capacity` — never cached
    - `GetConfirmedCountAsync`: count `Bookings` where `TripInstanceId` matches and
      `BookingStatus == Confirmed`
    - _Requirements: 3.1, 3.7_

  - [ ] 4.2 Implement `BookingService` in `Services/`
    - Create `IBookingService` with `CreateBookingAsync`, `CancelBookingAsync`, `GetStudentBookingsAsync`
    - `CreateBookingAsync`: check trip status is `Scheduling`; check duplicate booking (HTTP 409);
      compare confirmed count vs. total capacity → assign `Confirmed` or `WaitingList`;
      generate `QrCodeToken` via `Guid.NewGuid().ToString("N")` with 3-retry collision guard;
      persist and return `BookingDto`
    - `CancelBookingAsync`: verify caller owns booking; verify trip is `Scheduling` (else 400);
      set `BookingStatus = Cancelled`
    - `GetStudentBookingsAsync`: split into upcoming (TripDate >= today, status != Cancelled)
      and all (for past tab)
    - _Requirements: 3.2, 3.3, 3.6, 3.8, 4.2, 4.3, 5.8, 7.4, 7.8, 7.9_

  - [ ] 4.3 Implement `BookingsController` and `TripsController` (student-facing)
    - `GET /api/v1/trips` — query by `date`, `stopId`, `direction`; returns `TripAvailabilityDto[]`
      including `totalCapacity`, `confirmedCount`, `availableSeats`
    - `POST /api/v1/bookings` — delegates to `BookingService.CreateBookingAsync`
    - `GET /api/v1/bookings` — delegates to `BookingService.GetStudentBookingsAsync`
    - `GET /api/v1/bookings/{id}` — returns `BookingDto` with `qrCodeToken`; enforce ownership
    - `DELETE /api/v1/bookings/{id}` — delegates to `BookingService.CancelBookingAsync`
    - `GET /api/v1/trips/{id}` — returns trip detail accessible to Student, Marshal, Admin
    - Apply `[Authorize(Roles = "Student")]` to booking mutation endpoints
    - _Requirements: 3.2, 3.3, 4.2, 4.3, 7.1–7.9_

  - [ ]* 4.4 Write unit tests for `BookingService`
    - Test: confirmed when confirmed < capacity
    - Test: waitlisted when confirmed >= capacity
    - Test: duplicate booking returns 409
    - Test: cancel locked trip returns 400
    - Test: QR token is 32 hex characters
    - _Requirements: 3.2, 3.3, 3.6, 3.8, 7.8, 7.9_


- [ ] 5. Implement trip state machine, check-in, and breakdown services
  - [ ] 5.1 Implement `TripStateMachineService` in `Services/`
    - Create `ITripStateMachineService` with `IsValidTransition(TripStatus from, TripStatus to)` and
      `TransitionAsync(int tripInstanceId, TripStatus to)` methods
    - Implement the switch guard from the design for all 6 valid transitions
    - `TransitionAsync`: validate transition; set status + timestamp fields (`LockedAt`, `InTransitAt`,
      `CompletedAt`); on `Completed` set all `BoardedStatus = Pending` bookings to `NoShow`
    - Return 422 for invalid transitions
    - _Requirements: 22.1, 22.2, 22.3, 22.4, 22.5_

  - [ ] 5.2 Implement `CheckInService` in `Services/`
    - Implement `ICheckInService.CheckInAsync(string qrCodeToken, int shuttleId)` per design algorithm
    - Validate: token exists (404), booking is Confirmed (422), not already boarded (409),
      shuttle is operational on trip (422)
    - On success: set `BoardedStatus = Boarded`, `BoardedAt = DateTime.UtcNow`, `ShuttleId = shuttleId`
    - _Requirements: 5.1–5.7_

  - [ ] 5.3 Implement `CheckInController` and passenger manifest endpoint
    - `POST /api/v1/checkin` — `[Authorize(Roles = "Marshal")]`; delegates to `CheckInService`
    - `GET /api/v1/trips/{tripId}/passengers` — `[Authorize(Roles = "Marshal,Admin")]`;
      returns manifest DTO with totalConfirmed, boardedCount, pendingCount, passenger list
    - _Requirements: 5.1–5.6, 14.1, 14.2_

  - [ ] 5.4 Implement breakdown and standby overwrite logic in `TripsController`
    - `POST /api/v1/trips/{tripId}/breakdown` — `[Authorize(Roles = "Marshal")]`
      Set `TripShuttleAllocation.IsOperational = false`; recalculate capacity;
      transition trip to `IncidentActive`; if capacity < confirmed count dispatch FCM to all
      confirmed students (Req 6.3)
    - `PUT /api/v1/trips/{tripId}/standby` — `[Authorize(Roles = "Admin")]`
      Update allocation to replacement shuttle, set `IsOperational = true`, recalculate capacity,
      restore trip status; dispatch FCM to all confirmed students
    - `PATCH /api/v1/trips/{id}/status` — `[Authorize(Roles = "Admin")]`; manual status transition
      via `TripStateMachineService`
    - _Requirements: 6.1–6.7, 9.2, 9.3_

  - [ ]* 5.5 Write unit tests for `TripStateMachineService` and `CheckInService`
    - Test: all 6 valid transitions succeed
    - Test: invalid transition (e.g. `Scheduling → Completed`) returns 422
    - Test: `Completed` transition sets all Pending bookings to NoShow
    - Test: check-in with unknown token returns 404
    - Test: check-in with already-boarded booking returns 409
    - Test: check-in with non-operational shuttle returns 422
    - _Requirements: 5.2–5.6, 22.1, 22.2, 22.4_


- [ ] 6. Implement Rolling Injection Job and Lockout Gate background services
  - [ ] 6.1 Implement `RollingInjectionHostedService` in `BackgroundServices/`
    - Implement `IHostedService`; fire at 00:00:00 SAST daily using `PeriodicTimer` or
      `CronJob`-style logic with `TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time")`
    - Follow the 9-step algorithm from the design exactly:
      check CalendarException → check Sunday → determine ScheduleType → query ScheduleTimes →
      insert TripInstances (unique constraint guard for idempotency) →
      insert Priority Bookings for matching active PrioritySchedules
    - Wrap all inserts in a single `SERIALIZABLE` transaction; commit once
    - Log: "Injection complete. Trips created: N, Priority bookings created: M"
    - Catch all exceptions; log and exit cleanly without crashing the host
    - _Requirements: 2.1–2.10, 8.3, 8.4, 8.5_

  - [ ] 6.2 Implement `WaitlistPromotionService` in `Services/`
    - Create `IWaitlistPromotionService.RunWaitlistPromotionAsync(int tripInstanceId, IDbContextTransaction tx)`
    - Implement per design: compute capacity, confirmed count, available slots;
      promote up to `available` waitlisted bookings ordered by `CreatedAt ASC`;
      set `BookingStatus = Confirmed`; call `NotificationService.DispatchAsync` per student
    - _Requirements: 4.4, 4.5, 4.6, 4.7_

  - [ ] 6.3 Implement `LockoutGateHostedService` in `BackgroundServices/`
    - Implement the `LockoutGateLoop` algorithm from the design:
      query next `Scheduling` trip → sleep until `T-60 min` → call `LockTrip`
    - `LockTrip`: open `SERIALIZABLE` transaction with `UPDLOCK` hint;
      guard against re-lock; update status; call `WaitlistPromotionService`;
      commit; dispatch FCM to promoted students
    - On null next trip: sleep 5 minutes and re-query
    - Log failures per-trip and advance to next trip; never halt the loop
    - Register both background services in `Program.cs` via `builder.Services.AddHostedService<T>()`
    - _Requirements: 4.1–4.8_

  - [ ]* 6.4 Write unit tests for injection job and lockout gate
    - Test: Sunday target → zero TripInstances created
    - Test: CalendarException target → zero TripInstances created
    - Test: weekday target → one TripInstance per Weekday ScheduleTime
    - Test: Saturday target → one TripInstance per Saturday ScheduleTime
    - Test: re-running injection for same date → no duplicates (idempotency guard)
    - Test: active PrioritySchedule matching trip → Priority Booking created
    - Test: `LockTrip` with already-Locked trip → no-op (guard check)
    - _Requirements: 2.1–2.10, 4.1, 4.8_


- [ ] 7. Implement FCM notification service, notifications, and report endpoints
  - [ ] 7.1 Implement `FcmNotificationSender` in `Infrastructure/`
    - Wrap `FirebaseAdmin` SDK; initialize `FirebaseApp` from service account credentials in config
    - Implement `SendAsync(IEnumerable<string> tokens, string title, string body)` using
      `MulticastMessage` with Android high-priority channel `shuttle_alerts` and APNS sound
    - Log individual token failures without aborting; never propagate FCM exceptions to callers
    - _Requirements: 9.1–9.3, 9.8_

  - [ ] 7.2 Implement `NotificationService` in `Services/`
    - Create `INotificationService` with `DispatchAsync(int studentId, string title, string message,
      int? tripInstanceId)` and `DispatchToTripPassengersAsync(int tripInstanceId, string title, string message)`
    - Insert `Notification` row in DB first; then call `FcmNotificationSender` with all active
      `DeviceRegistration` tokens for the student(s)
    - _Requirements: 9.1–9.3, 9.5_

  - [ ] 7.3 Implement `NotificationsController` (student-facing)
    - `GET /api/v1/notifications` — `[Authorize(Roles = "Student")]`; return all notifications
      for authenticated student ordered by `CreatedAt DESC`
    - `PATCH /api/v1/notifications/{id}/read` — set `IsRead = true`
    - _Requirements: 9.4, 9.5_

  - [ ] 7.4 Implement `ReportsController` (student-facing) and report submission
    - `POST /api/v1/reports` — `[Authorize(Roles = "Student")]`; validate description is non-empty
      (HTTP 422 if empty); create `Report` with `Status = Submitted`
    - `GET /api/v1/reports` — return own reports ordered by `CreatedAt DESC`
    - _Requirements: 10.2, 10.3, 10.4_

  - [ ]* 7.5 Write unit tests for notification and report services
    - Test: FCM failure for one token does not abort dispatch to remaining tokens
    - Test: Notification row is inserted before FCM dispatch
    - Test: Report with empty description returns 422
    - Test: Report submitted with optional `tripInstanceId` persists correctly
    - _Requirements: 9.8, 10.2, 10.4_

- [ ] 8. Implement all Admin CRUD endpoints
  - [ ] 8.1 Implement stops admin endpoints in `Admin/StopsController`
    - `GET /api/v1/admin/stops`, `POST /api/v1/admin/stops`
    - `GET /api/v1/admin/stops/{id}`, `PUT /api/v1/admin/stops/{id}`, `DELETE /api/v1/admin/stops/{id}`
    - Validate StopName non-empty, GpsCoordinates non-empty and parseable as decimal-degree lat/lon
      (−90≤lat≤90, −180≤lon≤180); return 422 on validation failure
    - Delete: reject with 409 if any TripInstance references the stop
    - Duplicate name: 409
    - Apply `[Authorize(Roles = "Admin")]` to all
    - _Requirements: 17.1–17.6, 24.1–24.5_

  - [ ] 8.2 Implement shuttles admin endpoints in `Admin/ShuttlesController`
    - `GET /POST /api/v1/admin/shuttles`, `GET/PUT /api/v1/admin/shuttles/{id}`
    - Validate `Capacity > 0` (422); duplicate LicensePlate returns 409
    - On capacity edit: recalculate Total Capacity for all affected active `TripShuttleAllocations`
    - _Requirements: 18.1–18.6_

  - [ ] 8.3 Implement users admin endpoints in `Admin/UsersController`
    - `GET /api/v1/admin/users` — paginated, filterable by role, searchable by name/identifier/email
    - `POST /api/v1/admin/users` — single user add (Student/Marshal/Admin); reject duplicate
      email or identifier with 409
    - `GET/PUT /api/v1/admin/users/{id}` — view and edit FirstName, LastName, Email, IsPriorityUser
    - _Requirements: 19.1–19.7_

  - [ ] 8.4 Implement `CsvUserImportService` and bulk upload endpoint
    - `CsvUserImportService`: parse CSV rows; validate each row has role, identifier, firstName,
      lastName, email; skip duplicates (within file and against DB); record skipped rows with reason
    - `POST /api/v1/admin/users/bulk` — multipart/form-data; enforce 10 MB `RequestSizeLimit`;
      validate `.csv` extension; return `{ created: N, skipped: [...] }`
    - _Requirements: 20.1–20.8_

  - [ ] 8.5 Implement schedule and calendar exception endpoints in `Admin/ScheduleController`
    - `GET/POST /api/v1/admin/schedule-times`, `PUT/DELETE /api/v1/admin/schedule-times/{id}`
    - `GET/POST /api/v1/admin/calendar-exceptions`, `DELETE /api/v1/admin/calendar-exceptions/{id}`
    - Duplicate CalendarException date: 409; delete ScheduleTime with future trips: 409
    - _Requirements: 21.1–21.7_

  - [ ] 8.6 Implement priority schedule endpoints and report admin endpoints
    - `GET/POST /api/v1/admin/priority-schedules` — create PrioritySchedule;
      auto-set `Student.IsPriorityUser = true` on creation
    - `PATCH /api/v1/admin/priority-schedules/{id}` — deactivate (set `IsActive = false`)
    - `GET /api/v1/admin/reports` — paginated report list
    - `GET/PATCH /api/v1/admin/reports/{id}` — view full details; update status + AdminNotes;
      set `UpdatedAt = DateTime.UtcNow`
    - _Requirements: 8.1–8.4, 10.5–10.7_

  - [ ] 8.7 Implement analytics and trip admin endpoints
    - `GET /api/v1/admin/analytics/overview` — top 3 / bottom 3 stops by confirmed bookings;
      report counts by status
    - `GET /api/v1/admin/analytics/detailed` — expanded booking demand stats
    - `GET /api/v1/admin/trips` — filterable TripInstance list (date, status)
    - `POST /api/v1/admin/trips/{id}/shuttles` — assign shuttle to TripInstance
    - _Requirements: 16.1–16.6_

  - [ ]* 8.8 Write unit tests for admin services
    - Test: Stop with empty StopName returns 422
    - Test: Stop with invalid GPS format returns 422
    - Test: Shuttle with Capacity = 0 returns 422
    - Test: Duplicate user email returns 409
    - Test: CSV upload with all-duplicate rows returns `created: 0`, full skip list
    - Test: PrioritySchedule creation sets `IsPriorityUser = true`
    - _Requirements: 17.5, 18.6, 19.5, 20.4, 24.2, 8.2_

- [ ] 9. Checkpoint — Backend Complete
  - Verify `dotnet build` passes; all controller routes respond correctly via Swagger UI;
    background services register without errors. Ask the user if any questions arise.


---

### Phase 3 — Flutter Mobile App

- [ ] 10. Scaffold Flutter mobile app project and core infrastructure
  - [ ] 10.1 Create Flutter project and configure core packages
    - Create Flutter project targeting Android and iOS
    - Add dependencies: `flutter_riverpod`, `go_router`, `dio`, `flutter_secure_storage`,
      `firebase_core`, `firebase_messaging`, `mobile_scanner`, `google_maps_flutter`,
      `qr_flutter`, `geolocator`, `jwt_decoder`, `permission_handler`
    - Create folder structure: `lib/core/api/`, `lib/core/auth/`, `lib/core/router/`,
      `lib/features/`, `lib/shared/widgets/`, `lib/shared/theme/`
    - Configure `AndroidManifest.xml` and `Info.plist` for camera, location, and notification permissions
    - _Requirements: 7.11, 11.6_

  - [ ] 10.2 Implement Dio HTTP client with JWT interceptor
    - Create `ApiClient` class in `lib/core/api/` using Dio
    - Add `AuthInterceptor` that reads JWT from `flutter_secure_storage` and adds `Authorization: Bearer`
      header to all requests
    - Add error interceptor that maps HTTP 401 → redirect to login, propagates other errors as typed
      exceptions
    - _Requirements: 1.3, 15.4_

  - [ ] 10.3 Implement auth state management and JWT role extraction
    - Create `AuthNotifier` (`StateNotifier<AuthState>`) in `lib/core/auth/`
    - On login success: store JWT via `flutter_secure_storage`; decode role claim with `jwt_decoder`
    - Expose `role`, `studentNumber`/`staffNumber`, `isAuthenticated`
    - Create `authProvider` (`StateNotifierProvider<AuthNotifier, AuthState>`)
    - _Requirements: 1.6, 1.7_

  - [ ] 10.4 Configure `go_router` navigation with role-based route guards
    - Define all routes from design: `/login`, `/student/home`, `/student/booking/new`,
      `/student/booking/:id`, `/student/maps`, `/student/alerts`, `/student/alerts/:id`,
      `/student/profile`, `/student/profile/report/new`, `/marshal/home`,
      `/marshal/trip/:id/passengers`, `/marshal/scan`
    - `AuthGate` at `/`: redirect to role home if authenticated, else `/login`
    - Route guard: unauthenticated access to protected routes redirects to `/login`
    - _Requirements: 1.6, 7.11, 13.1_

  - [ ] 10.5 Implement FCM device token registration on auth success
    - On successful login, call `FirebaseMessaging.instance.getToken()` then POST to
      `POST /api/v1/auth/device/register` with token and platform
    - Handle `FirebaseMessaging.onMessage` (foreground) to show in-app notification
    - _Requirements: 1.7, 9.1_

