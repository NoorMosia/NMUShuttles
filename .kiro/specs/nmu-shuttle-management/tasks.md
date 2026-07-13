# Implementation Plan: NMU Shuttle Management Network

## Overview

This plan implements the full NMU Shuttle Management Network across five phases: backend foundation, backend domain features, Flutter mobile app, Flutter web admin portal, and cross-cutting concerns (property-based tests, integration tests, security hardening). The backend is .NET 9 Web API with EF Core and Azure SQL; the mobile app and admin portal are Flutter with Riverpod, go_router, and Dio.

## Tasks

- [x] 1. Backend Foundation — Project Setup, Data Layer, Auth, and Seed Data

  - [x] 1.1 Create .NET 9 Web API project structure and configure dependencies
    - Create solution `NmuShuttle.sln` with project `NmuShuttle.Api`
    - Add NuGet packages: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `FluentValidation.AspNetCore`, `FirebaseAdmin`, `FsCheck`, `FsCheck.Xunit`, `BCrypt.Net-Next`, `Swashbuckle.AspNetCore`
    - Create folder structure: Controllers/, Controllers/Admin/, Services/, BackgroundServices/, Data/, Models/, DTOs/, Infrastructure/, Validators/
    - Configure `Program.cs` with DI registrations, CORS, rate limiting, Swagger, and ProblemDetails exception handler
    - _Requirements: 26.3, 26.4, 26.5, 26.6_

  - [x] 1.2 Define EF Core entity models with all constraints
    - Create entity classes: Student, Marshal, Admin, Campus, Stop, ScheduleTime, CalendarException, Shuttle, TripInstance, TripShuttleAllocation, Booking, PrioritySchedule, DeviceRegistration, Notification, Report, BreakdownLog
    - Stop entity MUST include `FullAddress` (NVARCHAR 300, NOT NULL)
    - Marshal entity MUST include nullable `AssignedCampusId` and nullable `AssignedStopId` with XOR CHECK constraint
    - TripShuttleAllocation MUST include `OrderPosition` (INT, NOT NULL) with unique constraint per trip
    - Booking MUST include `ShuttleId` (nullable FK, set at check-in or priority pre-assignment)
    - BreakdownLog MUST include `VendorCompanyName` (denormalized), `ResolutionType`, `ReplacementShuttleId`, `ResolvedByAdminId`
    - Apply all CHECK constraints, UNIQUE constraints, and filtered indexes as defined in the design schema
    - _Requirements: 1.1, 3.1, 5.9, 6.10, 17.5, 18.6, 24.1, 27.1, 27.5_

  - [x] 1.3 Create ShuttleDbContext with Fluent API configuration and key indexes
    - Configure all entity relationships, cascade behaviors, and navigation properties
    - Add XOR CHECK constraint on Marshal: `CHECK ((AssignedCampusId IS NOT NULL AND AssignedStopId IS NULL) OR (AssignedCampusId IS NULL AND AssignedStopId IS NOT NULL))`
    - Add filtered unique index on Bookings: `UNIQUE (StudentId, TripInstanceId) WHERE BookingStatus != 'Cancelled'`
    - Add composite unique constraints: `(ScheduleTimeId, StopId, TripDate)` on TripInstances, `(TripInstanceId, ShuttleId)` and `(TripInstanceId, OrderPosition)` on TripShuttleAllocations
    - Add all performance indexes from design (IX_Bookings_TripInstanceId_Status, IX_BreakdownLogs_VendorCompanyName, etc.)
    - _Requirements: 3.8, 5.10, 27.5_

  - [x] 1.4 Create initial EF Core migration and seed data from firstSeed.md
    - Generate initial migration with `dotnet ef migrations add InitialCreate`
    - Implement `IEntityTypeConfiguration` seed or a `HasData()` approach for: 2 admins (BCrypt-hashed passwords), 3 campuses, 8 stops (with FullAddress), 15 students (3 priority), 11 marshals (3 campus-assigned + 8 stop-assigned), 8 shuttles (2 accessible), 18 schedule times, 10 calendar exceptions, 14 priority schedules
    - Admin passwords seeded as BCrypt hashes (cost factor 12)
    - Marshal seed data must respect XOR constraint (CampusId XOR StopId)
    - _Requirements: 2.1, 8.1_

  - [x] 1.5 Implement JWT token service and authentication middleware
    - Create `JwtTokenService` issuing HS256 tokens with claims: `sub` (userId), `role` (Student/Marshal/Admin), identifier (studentNumber/staffNumber/email)
    - Student/Marshal tokens: 24-hour expiry; Admin tokens: 8-hour expiry
    - Configure ASP.NET Core `AddAuthentication().AddJwtBearer()` with validation parameters
    - Create role-based authorization policies: `[Authorize(Roles = "Student")]`, `[Authorize(Roles = "Marshal")]`, `[Authorize(Roles = "Admin")]`
    - Secret key loaded from environment variable / configuration
    - _Requirements: 1.3, 15.2, 26.4, 26.5, 26.6_

  - [x] 1.6 Implement OTP authentication endpoints (Student and Marshal)
    - Create `OtpService` with: `GenerateOtp()` (5-digit numeric), `HashOtp()` (SHA-256), verification logic with expiry check and single-use clearing
    - Create `AuthController` with endpoints: `POST /api/v1/auth/student/otp/request`, `POST /api/v1/auth/student/otp/verify`, `POST /api/v1/auth/marshal/otp/request`, `POST /api/v1/auth/marshal/otp/verify`
    - Validate StudentNumber format: `^s3[12]\d{7}$` — reject with HTTP 422 if invalid
    - Validate StaffNumber format: `^\d{7}$` — reject with HTTP 422 if invalid
    - Return HTTP 404 if identifier not found, HTTP 401 if OTP expired/invalid
    - OTP fields cleared after any verification attempt (pass or fail)
    - Rate limiting: 5 requests/minute per IP on OTP request endpoints
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.10_

  - [x] 1.7 Implement Admin email+password login endpoint
    - Create `POST /api/v1/auth/admin/login` accepting email + password
    - Verify against BCrypt-hashed password in Admins table
    - Return JWT on success, HTTP 401 on failure (no disclosure of which field was wrong)
    - _Requirements: 15.1, 15.2, 15.3_

  - [x] 1.8 Implement device registration endpoint and global error handling middleware
    - Create `POST /api/v1/auth/device/register` (Student + Marshal roles) — upsert DeviceRegistration with FcmToken and Platform
    - Implement `IExceptionHandler` middleware returning RFC 7807 ProblemDetails JSON for all errors
    - Ensure HTTP status in response header matches status in body (consistency invariant)
    - _Requirements: 1.9, 9.8, 26.1, 26.2, 26.3, 26.7_

- [x] 2. Checkpoint — Backend foundation verified
  - Ensure all tests pass, ask the user if questions arise.

- [x] 3. Backend Domain Features — Bookings, Capacity, Check-In, Breakdown, Background Services

  - [x] 3.1 Implement CapacityService and ConvoyOrderingService
    - Create `ICapacityService` with `GetTotalCapacityAsync(tripInstanceId)` — SUM of operational shuttle capacities
    - Create `IConvoyOrderingService` with `AssignOrderPositionAsync(tripInstanceId, shuttleId)` — accessible shuttles get lowest positions, non-accessible appended after
    - When accessible shuttle added: shift non-accessible allocations up by 1
    - Ensure OrderPosition values are contiguous starting from 1
    - _Requirements: 3.1, 3.7, 27.1, 27.2, 27.5, 27.6_

  - [x] 3.2 Implement BookingService with capacity enforcement and waitlist logic
    - Create `IBookingService` with `CreateBookingAsync`, `CancelBookingAsync`, `GetStudentBookingsAsync`
    - On create: if confirmed count < Total Capacity → BookingStatus = 'Confirmed'; else → 'WaitingList'
    - Generate QrCodeToken via `Guid.NewGuid().ToString("N")` (32 hex chars) with retry loop (up to 3 attempts)
    - Reject duplicate active booking for same student + trip with HTTP 409
    - Reject booking on Locked/InTransit/Completed/IncidentActive trips with HTTP 400
    - On cancel: only allowed while TripInstance.Status = 'Scheduling'; reject otherwise with HTTP 400
    - _Requirements: 3.2, 3.3, 3.6, 3.8, 7.4, 7.8, 7.9, 23.1, 25.1_

  - [x] 3.3 Implement Booking and Trip API controllers (Student-facing)
    - Create `BookingsController` with: `POST /api/v1/bookings`, `GET /api/v1/bookings`, `GET /api/v1/bookings/{id}`, `DELETE /api/v1/bookings/{id}`
    - Create `TripsController` with: `GET /api/v1/trips` (query: date, stopId, direction) returning availability with capacity summary
    - `GET /api/v1/trips/{id}` for trip detail (Student + Marshal + Admin)
    - Apply `[Authorize(Roles = "Student")]` on booking endpoints
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5, 7.6, 7.7, 7.8, 7.9_

  - [x] 3.4 Implement CheckInService with 35-min scan window, location binding, and auto-assignment
    - Create `ICheckInService.CheckInAsync(qrCodeToken, marshalId)` implementing the full check-in algorithm:
      1. Look up Booking by QrCodeToken (HTTP 404 if not found)
      2. Verify scan window: reject if current time < DepartureTime - 35 min (HTTP 422)
      3. Verify marshal location binding: ToCampus → Marshal.AssignedStopId == TripInstance.StopId; FromCampus → Marshal.AssignedCampusId == TripInstance.CampusId (via ScheduleTime). Reject with HTTP 403 if mismatch
      4. Verify BookingStatus == 'Confirmed' (HTTP 422 if not)
      5. Verify BoardedStatus != 'Boarded' (HTTP 409 if already boarded)
      6. Auto-assign shuttle: query operational allocations ordered by OrderPosition ASC, find first with BoardedCount < Capacity
      7. If no shuttle has space → HTTP 422 (convoy full)
      8. Update Booking: BoardedStatus='Boarded', BoardedAt=UtcNow, ShuttleId=assigned
    - No `shuttleId` input from marshal — system determines assignment automatically
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6, 5.7, 5.8, 5.9, 5.11, 5.12, 27.3, 27.4, 27.7_

  - [x] 3.5 Implement CheckIn and Marshal Trip API controllers
    - Create `CheckInController` with `POST /api/v1/checkin` (Marshal role) — extracts marshalId from JWT, delegates to CheckInService
    - Create `GET /api/v1/trips/{tripId}/passengers` (Marshal role) — returns passenger manifest with counts (totalConfirmed, boardedCount, pendingCount) and passenger list
    - _Requirements: 5.1, 14.1, 14.2_

  - [x] 3.6 Implement BreakdownService (Marshal flag + Admin resolution + BreakdownLog)
    - Create `IBreakdownService.FlagBreakdownAsync(tripInstanceId, shuttleId, marshalId)`:
      1. Set TripShuttleAllocation.IsOperational = false
      2. Create BreakdownLog with denormalized VendorCompanyName from Shuttle record at flag time
      3. Set TripInstance.Status = 'IncidentActive'
      4. Do NOT dispatch any notifications
    - Create `IBreakdownService.ResolveBreakdownAsync(breakdownLogId, resolutionType, replacementShuttleId?, adminId)`:
      1. If 'Replaced': update allocation ShuttleId, set IsOperational=true, restore trip status to InTransit
      2. If 'Cancelled': keep IsOperational=false, record cancellation
      3. Set ResolvedAt, ResolvedByAdminId on BreakdownLog
      4. Dispatch FCM notifications to ALL confirmed booking holders on the trip
    - Create trip management endpoints: `POST /api/v1/trips/{tripId}/breakdown` (Marshal), `POST /api/v1/trips/{tripId}/breakdown/{breakdownLogId}/resolve` (Admin)
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 6.6, 6.7, 6.8, 6.9, 6.10, 6.11, 6.12, 6.13_

  - [x] 3.7 Implement TripStateMachineService and status transition endpoint
    - Create `ITripStateMachineService.IsValidTransition(from, to)` implementing valid transitions: Scheduling→Locked, Scheduling→IncidentActive, Locked→InTransit, InTransit→Completed, InTransit→IncidentActive, IncidentActive→InTransit
    - On transition to Completed: set all Pending BoardedStatus bookings to 'NoShow'
    - Record transition timestamps (LockedAt, InTransitAt, CompletedAt)
    - Create `PATCH /api/v1/trips/{id}/status` (Admin role) — reject invalid transitions with HTTP 422
    - _Requirements: 22.1, 22.2, 22.3, 22.4, 22.5_

  - [x] 3.8 Implement RollingInjectionHostedService (nightly trip generation + priority bookings)
    - Implement `IHostedService` firing at 00:00:00 SAST daily
    - Algorithm: calculate TargetDate = today + 3; skip if in CalendarExceptions or Sunday
    - Determine ScheduleType (Weekday/Saturday); create TripInstances for each matching ScheduleTime with Status='Scheduling'
    - Idempotency via UNIQUE constraint on (ScheduleTimeId, StopId, TripDate)
    - For each new TripInstance: query active PrioritySchedules matching DayOfWeek + ScheduleTimeId + StopId
    - Create Priority Bookings with: BookingStatus='Confirmed', BookingType='Priority', BoardedStatus='Pending'
    - Pre-assign ShuttleId to the shuttle at OrderPosition=1 (front accessible shuttle) if allocations exist; else leave NULL
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 2.10, 8.4, 8.5, 8.6, 8.7, 8.8_

  - [x] 3.9 Implement LockoutGateHostedService (1-hour lockout + waitlist promotion)
    - Implement `IHostedService` that loops: query next Scheduling trip, sleep until T-60 min, then lock
    - `LockTrip()` within SERIALIZABLE transaction with UPDLOCK: set Status='Locked', LockedAt=UtcNow
    - Implement `IWaitlistPromotionService.RunAsync(tripInstanceId)`: promote WaitingList bookings in CreatedAt ASC order up to available capacity
    - For each promoted booking: create Notification record + dispatch FCM push notification
    - After lock: reject all booking creates and cancellations for that trip (HTTP 400)
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 4.8_

  - [x] 3.10 Implement NotificationService and FCM dispatch
    - Create `INotificationService` with `DispatchAsync(studentId, title, message, tripInstanceId?)` and `DispatchToTripPassengersAsync(tripInstanceId, title, message)`
    - Create `FcmNotificationSender` wrapping FirebaseAdmin SDK MulticastMessage
    - On partial failure: log per-token failure, continue dispatching remaining tokens (never abort)
    - Always create a Notification DB record alongside the FCM dispatch
    - _Requirements: 9.1, 9.2, 9.3, 9.8_

  - [x] 3.11 Implement Notification and Report student-facing endpoints
    - Create `NotificationsController`: `GET /api/v1/notifications` (Student), `PATCH /api/v1/notifications/{id}/read` (Student)
    - Create `ReportsController`: `POST /api/v1/reports` (Student), `GET /api/v1/reports` (Student)
    - Validate Report.Description is non-empty (HTTP 422 if empty)
    - _Requirements: 9.4, 9.5, 10.1, 10.2, 10.3, 10.4_

  - [x] 3.12 Implement Admin CRUD endpoints (Stops, Shuttles, Users, Schedule, Calendar, Priority)
    - Create Admin controllers: `StopsController`, `ShuttlesController`, `UsersController`, `ScheduleController`
    - Stops: GET/POST `/admin/stops`, GET/PUT/DELETE `/admin/stops/{id}` — validate StopName, FullAddress, GpsCoordinates non-empty; validate GPS format as decimal lat,lon with range checks; reject deletion if TripInstances reference stop (HTTP 409)
    - Shuttles: GET/POST `/admin/shuttles`, GET/PUT `/admin/shuttles/{id}` — validate Capacity > 0; reject duplicate LicensePlate (HTTP 409); recalculate capacity for affected trips on capacity edit
    - Users: GET/POST `/admin/users`, GET/PUT `/admin/users/{id}` — validate StudentNumber `^s3[12]\d{7}$`, StaffNumber `^\d{7}$`; reject duplicate email/identifier (HTTP 409)
    - ScheduleTimes: GET/POST `/admin/schedule-times`, PUT/DELETE `/admin/schedule-times/{id}`
    - CalendarExceptions: GET/POST `/admin/calendar-exceptions`, DELETE `/admin/calendar-exceptions/{id}` — reject duplicate TargetDate (HTTP 409)
    - PrioritySchedules: GET/POST `/admin/priority-schedules`, PATCH `/admin/priority-schedules/{id}` (deactivate)
    - When PrioritySchedule created for non-priority student: auto-set IsPriorityUser=true
    - _Requirements: 8.1, 8.2, 8.3, 17.1, 17.2, 17.3, 17.4, 17.5, 17.6, 18.1, 18.2, 18.3, 18.4, 18.5, 18.6, 19.1, 19.4, 19.5, 19.6, 19.7, 19.8, 19.9, 21.1, 21.2, 21.3, 21.4, 21.5, 21.6, 21.7, 24.1, 24.2, 24.3, 24.4, 24.5_

  - [x] 3.13 Implement CsvUserImportService and bulk upload endpoint
    - Create `POST /api/v1/admin/users/bulk` (Admin) accepting multipart/form-data .csv file
    - Enforce max 10 MB file size via `[RequestSizeLimit]`
    - Parse rows: validate role, identifier format, firstName, lastName, email per row
    - Skip rows with invalid format, duplicate email/identifier (within file or against DB) — record as errors
    - Return summary: `{ "created": N, "skipped": [{ "row": X, "reason": "..." }] }`
    - Never overwrite existing records (idempotent)
    - _Requirements: 20.1, 20.2, 20.3, 20.4, 20.5, 20.6, 20.7, 20.8, 20.9_

  - [x] 3.14 Implement VendorAnalyticsService and admin analytics endpoints
    - Create `IVendorAnalyticsService` querying BreakdownLogs grouped by ShuttleId and VendorCompanyName
    - Create `GET /api/v1/admin/analytics/overview` — top 3 most/least in-demand stops, report status counts, breakdown/cancellation tallies
    - Create `GET /api/v1/admin/analytics/detailed` — expanded booking demand stats
    - Create `GET /api/v1/admin/analytics/vendor-reliability` — per-shuttle and per-vendor breakdown/cancellation counts with optional date range filter (fromDate, toDate)
    - _Requirements: 6.10, 16.1, 16.2, 16.3, 16.4, 16.5, 16.6_

  - [x] 3.15 Implement Admin trip management and shuttle assignment endpoints
    - Create `GET /api/v1/admin/trips` — list TripInstances filterable by date, status
    - Create `POST /api/v1/admin/trips/{id}/shuttles` — assign shuttle to trip using ConvoyOrderingService (accessible-first ordering)
    - Create `GET /api/v1/admin/reports`, `GET/PATCH /api/v1/admin/reports/{id}` — report review with status update and AdminNotes
    - _Requirements: 10.5, 10.6, 10.7, 16.7, 27.2_

- [x] 4. Checkpoint — Backend domain features verified
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Flutter Mobile App — Auth, Student Screens, Marshal Screens

  - [x] 5.1 Create Flutter mobile project structure with Riverpod, go_router, and Dio
    - Initialize Flutter project with folder structure: `lib/core/api/`, `lib/core/auth/`, `lib/core/router/`, `lib/features/`, `lib/shared/`
    - Add dependencies: `flutter_riverpod`, `go_router`, `dio`, `flutter_secure_storage`, `mobile_scanner`, `google_maps_flutter`, `jwt_decoder`, `qr_flutter`
    - Configure Dio HTTP client with base URL, JWT interceptor (attaches Bearer token), and error interceptor
    - Set up `flutter_secure_storage` for token persistence
    - _Requirements: 7.11, 12.4_

  - [x] 5.2 Implement auth flow with role-switcher login screen
    - Create login screen with Student/Marshal role selector toggle
    - Student flow: identifier input (StudentNumber) → OTP request → OTP verify → JWT stored → route to `/student/home`
    - Marshal flow: identifier input (StaffNumber) → OTP request → OTP verify → JWT stored → route to `/marshal/home`
    - Extract role from JWT claims to determine routing
    - Register FCM device token on successful auth via `POST /auth/device/register`
    - _Requirements: 1.8, 1.9_

  - [x] 5.3 Implement Student Home screen with Upcoming/Past tabs and bottom navigation
    - Create `StudentHomeScreen` with tab bar: Upcoming (TripDate >= today, BookingStatus != Cancelled) and Past (TripDate < today or Cancelled)
    - Display booking cards with direction, stop, campus, date, time, and status
    - Add floating action button (FAB) navigating to New Booking form
    - Implement bottom navigation bar with 4 items: Home, Maps, Alerts, Profile
    - Create Riverpod providers: `upcomingBookingsProvider`, `pastBookingsProvider`
    - _Requirements: 7.6, 7.7, 7.10, 7.11_

  - [x] 5.4 Implement New Booking form with direction selector, stop search, and capacity display
    - Create booking form with: direction selector (ToCampus/FromCampus), stop search field (filters by StopName OR FullAddress), campus selector, 4-day date picker (today through today+3), departure time selector
    - Query `GET /api/v1/trips` with selected filters to show available trips
    - Display available capacity: show "Confirm Booking" button if seats available, or "Place on Waiting List" with capacity notice if full
    - On confirm: `POST /api/v1/bookings` → navigate to Booking Detail screen
    - Create `tripAvailabilityProvider` with family parameter for query
    - _Requirements: 7.1, 7.2, 7.3, 7.4, 7.5_

  - [x] 5.5 Implement Booking Detail screen with QR code display
    - Display booking info: direction, stop, campus, date, departure time, booking status
    - Render QR code from `QrCodeToken` using `qr_flutter` at minimum 256×256 logical pixels
    - Provide "Download QR Code" action saving QR image to device gallery
    - _Requirements: 7.5, 23.2, 23.3, 23.4_

  - [x] 5.6 Implement Maps screen with stop markers, search, nearby stops, and directions
    - Display interactive Google Maps with markers for all active stops (using GpsCoordinates)
    - On marker tap: show popup with StopName, FullAddress, and "Get Directions" button
    - "Get Directions" launches device navigation intent to stop's GPS coordinates
    - Implement search field filtering markers by StopName or FullAddress in real time
    - Implement "Nearby Stops" view sorted by proximity to device GPS location
    - Handle location permission denied: show permission prompt before GPS access
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6_

  - [x] 5.7 Implement Alerts screen with notification list and unread badge
    - Create Alerts screen listing all notifications from `GET /api/v1/notifications` ordered by CreatedAt DESC
    - Mark notification as read on view: `PATCH /api/v1/notifications/{id}/read`
    - Show Alert Detail screen with full Title and Message on tap
    - Display unread badge on Alerts navigation item when unreadCount > 0
    - Create `unreadNotificationCountProvider`
    - _Requirements: 9.4, 9.5, 9.6, 9.7_

  - [x] 5.8 Implement Profile screen with logout and report filing
    - Display: FirstName, LastName, StudentNumber, UniversityEmail
    - Show "My Reports" list ordered by CreatedAt DESC (Category + Status per item)
    - Tap report → navigate to Report detail (Category, Description, Status, AdminNotes)
    - Report Form: category selector (DriverBehavior, TimetableIssue, VehicleQuality, Other), optional TripInstance reference, free-text description — submit via `POST /api/v1/reports`
    - Logout action: clear stored JWT, navigate to login screen
    - _Requirements: 10.1, 10.2, 10.3, 12.1, 12.2, 12.3, 12.4_

  - [x] 5.9 Implement Marshal Home screen with direction-filtered trip list and location display
    - Display Marshal's assigned location (Campus name or Stop name) at top
    - Show chronological list of upcoming TripInstances on current date matching marshal's location AND correct direction:
      - Campus-assigned marshals: show trips with Direction='FromCampus' at their campus
      - Stop-assigned marshals: show trips with Direction='ToCampus' at their stop
    - Display "Next Boarding" card with departure time and status of soonest non-Completed trip
    - Show remaining schedule as departure time list for non-Completed trips
    - Prominent QR Scanner button navigating to scan interface
    - Directions button launching navigation to assigned location GPS coordinates
    - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5_

  - [x] 5.10 Implement Marshal Passenger List and QR scan check-in flow
    - Passenger List screen: show all Confirmed bookings for selected trip with student name + BoardedStatus
    - Display real-time counts: total confirmed, boarded, pending
    - QR Scanner using `mobile_scanner` package: decode QrCodeToken from scanned code
    - Submit check-in: `POST /api/v1/checkin` with decoded token (no shuttleId — system auto-assigns)
    - On success: update scanned student to "Boarded" in list without full refresh (invalidate manifest provider)
    - On failure: display inline error with reason (not confirmed, already boarded, scan window not open, wrong location, convoy full)
    - Breakdown flag button: single confirmation action triggering `POST /api/v1/trips/{tripId}/breakdown` — marshal selects shuttle, confirms flag
    - Polling: `passengerManifestProvider` with 30-second Stream.periodic refresh
    - _Requirements: 14.1, 14.2, 14.3, 14.4, 14.5, 14.6_

- [x] 6. Checkpoint — Flutter mobile app verified
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 7. Flutter Web Admin Portal — Auth, Dashboard, CRUD, Breakdown Resolution

  - [ ] 7.1 Create Flutter Web admin portal project with shared architecture
    - Initialize separate Flutter web entry point under `admin_portal/` directory
    - Set up folder structure: `lib/core/api/`, `lib/core/auth/`, `lib/core/router/`, `lib/features/`
    - Configure Dio client with admin JWT interceptor (token stored in localStorage for web)
    - Implement persistent sidebar navigation layout with `NavigationDrawer`: Overview, Shuttle Stops, Shuttles, Users, Schedule, Reports
    - Active route highlight via `GoRouterState`
    - _Requirements: 16.7_

  - [ ] 7.2 Implement Admin login screen and session management
    - Email + password login form
    - On success: store JWT, redirect to Operations Overview dashboard
    - On failure: inline error message (no disclosure of which field was wrong)
    - On token expiry: redirect to login with session-expiry message
    - Logout action in sidebar: invalidate token, return to login
    - _Requirements: 15.1, 15.2, 15.3, 15.4, 15.5_

  - [ ] 7.3 Implement Operations Overview dashboard with analytics panels
    - Display as default landing page after login
    - "Top 3 Most In-Demand Stops" panel with progress bars (from `/admin/analytics/overview`)
    - "Top 3 Least In-Demand Stops" panel with progress bars
    - Reports Summary panel: counts per status (Submitted, Under Review, Resolved)
    - "Breakdown & Cancellation Analytics" panel: counts per Shuttle and per VendorCompanyName with configurable date range
    - "View Detailed Analytics" button navigating to detailed view
    - _Requirements: 16.1, 16.2, 16.3, 16.4, 16.5, 16.6_

  - [ ] 7.4 Implement Stop and Shuttle management screens
    - Stops list: table with StopName, FullAddress, GpsCoordinates columns
    - "New Stop" form: StopName, FullAddress, GpsCoordinates fields with validation
    - "Edit Stop" form pre-filled with existing values
    - Shuttles list: table with LicensePlate, Capacity, VendorCompanyName, IsAccessibleVehicle columns
    - "New Shuttle" form: LicensePlate, Capacity, VendorCompanyName, IsAccessibleVehicle toggle
    - "Edit Shuttle" form pre-filled with existing values
    - Display validation errors inline (uniqueness, format)
    - _Requirements: 17.1, 17.2, 17.3, 17.4, 18.1, 18.2, 18.3, 18.4_

  - [ ] 7.5 Implement User management with individual add, edit, and bulk CSV upload
    - User list: searchable, paginated table with identifier, name, role, email columns
    - Role-based filter buttons: All, Students, Marshals, Admins
    - Real-time search by name, identifier, or email
    - "Add Single User" form: role selector, identifier, FirstName, LastName, UniversityEmail — format validation displayed inline
    - Edit user form: FirstName, LastName, UniversityEmail, IsPriorityUser flag
    - Marshal edit: assign location (CampusId XOR StopId)
    - "Upload CSV" interface: drag-and-drop zone + file browser (.csv only)
    - After upload: confirmation screen showing success count + skipped rows with error reasons
    - _Requirements: 19.1, 19.2, 19.3, 19.4, 19.5, 19.6, 19.7, 19.8, 19.9, 20.1, 20.6, 20.7_

  - [ ] 7.6 Implement Schedule, Calendar Exception, and Priority Schedule management screens
    - ScheduleTimes: list view, create form (DepartureTime, Direction, ScheduleType, CampusId, StopId), edit/delete
    - CalendarExceptions: list in date order, create form (TargetDate, ExceptionType, Description), delete action
    - PrioritySchedules: list view, create form (StudentId, StopId, ScheduleTimeId, DayOfWeek), deactivate action
    - _Requirements: 21.1, 21.2, 21.3, 21.4, 21.5, 21.6, 21.7, 8.1, 8.3_

  - [ ] 7.7 Implement Trip management with breakdown resolution and shuttle assignment
    - Trip list: filterable by date and status, showing TripInstance details
    - Trip detail: show convoy allocations, passenger count, current status
    - Incident resolution form for IncidentActive trips: options to assign replacement ShuttleId OR mark as cancelled
    - Shuttle assignment: `POST /admin/trips/{id}/shuttles` with shuttle selector
    - _Requirements: 6.5, 6.6, 6.7, 6.8_

  - [ ] 7.8 Implement Reports management screen
    - Paginated report list with columns: report ID, category, student ID, submission date, status
    - Report detail view: full Description, AdminNotes, linked TripInstance, status
    - Status update form: transition to 'Under Review' or 'Resolved', add AdminNotes
    - _Requirements: 10.5, 10.6, 10.7_

- [ ] 8. Checkpoint — Flutter web admin portal verified
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 9. Cross-Cutting — FsCheck Property-Based Tests, Integration Tests, Security Hardening

  - [ ]* 9.1 Write FsCheck property tests for OTP (Properties 1–3)
    - **Property 1: OTP Single-Use Enforcement** — verify once → success, verify same OTP again → failure
    - **Property 2: OTP Format and Expiry Invariant** — generated OTP is 5 digits, expiry within [4:55, 5:05] of generation
    - **Property 3: Device Registration Round-Trip** — auth + register → DeviceRegistration maps user to exact token
    - **Validates: Requirements 1.1, 1.2, 1.3, 1.4, 1.9, 1.10**

  - [ ]* 9.2 Write FsCheck property tests for Rolling Injection (Properties 4–8)
    - **Property 4: Injection Date Invariant** — all TripInstances created have TripDate = LocalDate + 3
    - **Property 5: Calendar Exception Skip** — dates in CalendarExceptions produce zero TripInstances
    - **Property 6: Schedule-Type Matching Invariant** — weekday→Weekday schedules, Saturday→Saturday schedules, Sunday→zero
    - **Property 7: Injected TripInstances Have Scheduling Status** — all created with Status='Scheduling'
    - **Property 8: Priority Booking Pre-Clearance Invariant** — matching PrioritySchedules produce Priority Confirmed bookings before general access
    - **Validates: Requirements 2.1, 2.2, 2.3, 2.4, 2.5, 2.6, 2.7, 2.8, 2.9, 2.10, 8.5**

  - [ ]* 9.3 Write FsCheck property tests for Capacity and Bookings (Properties 9–10)
    - **Property 9: Convoy Capacity Summation Invariant** — Total Capacity == SUM of operational shuttle capacities
    - **Property 10: Booking Status Determined by Available Capacity** — confirmed < capacity → Confirmed; confirmed >= capacity → WaitingList
    - **Validates: Requirements 3.1, 3.2, 3.3, 3.6, 3.7**

  - [ ]* 9.4 Write FsCheck property tests for Lockout and Waitlist (Properties 11–12)
    - **Property 11: Post-Lock Capacity Ceiling** — confirmed count after promotion never exceeds Total Capacity
    - **Property 12: Waitlist Promotion Ordering** — bookings promoted in CreatedAt ASC order (FIFO)
    - **Validates: Requirements 4.4, 4.6, 4.7**

  - [ ]* 9.5 Write FsCheck property tests for QR Token (Properties 13)
    - **Property 13: QR Token Uniqueness** — all generated QrCodeTokens form a set with no collisions
    - **Validates: Requirements 5.10, 23.1, 23.6**

  - [ ]* 9.6 Write FsCheck property tests for Check-In (Properties 14, 23, 24)
    - **Property 14: Check-In Shuttle Integrity** — assigned ShuttleId is operational with lowest OrderPosition having capacity
    - **Property 23: Sequential Front-to-Back Shuttle Fill** — shuttle at position i fills to capacity before position i+1 receives
    - **Property 24: 35-Minute Scan Window Time-Gate** — rejected before T-35 min, accepted within window
    - **Validates: Requirements 5.1, 5.2, 5.6, 5.9, 5.11, 27.3, 27.7**

  - [ ]* 9.7 Write FsCheck property tests for Breakdown (Properties 15–16, 25–27)
    - **Property 15: Capacity Reduction on Breakdown** — new capacity = old - flagged shuttle capacity
    - **Property 16: Capacity Replacement on Admin Resolution** — final capacity = pre-breakdown - broken + replacement
    - **Property 25: Breakdown Tallying Integrity** — BreakdownLog counts match event counts per shuttle and vendor
    - **Property 26: No Notification at Marshal Flag Time** — zero notifications/FCM during marshal flag
    - **Property 27: Boarded Persistence After Breakdown** — boarded students remain boarded after shuttle flag
    - **Validates: Requirements 6.4, 6.9, 6.10, 6.11, 6.12, 6.13**

  - [ ]* 9.8 Write FsCheck property tests for Data Validation (Properties 17–19)
    - **Property 17: Stop Data Completeness** — empty StopName/FullAddress/GpsCoordinates rejected
    - **Property 18: Shuttle Capacity Validity** — Capacity ≤ 0 rejected
    - **Property 19: Bulk CSV Idempotency** — uploading same CSV twice creates no additional records
    - **Validates: Requirements 17.5, 18.6, 20.8, 20.9**

  - [ ]* 9.9 Write FsCheck property tests for State Machine, Convoy Ordering, Priority (Properties 20–22)
    - **Property 20: State Machine Completeness** — only valid transitions accepted, invalid rejected with 422, no skip from Scheduling to Completed
    - **Property 21: Priority Booking First-Shuttle Assignment** — priority bookings pre-assigned to OrderPosition=1 shuttle
    - **Property 22: Accessible-First Convoy Ordering** — all accessible shuttles have lower OrderPosition than non-accessible
    - **Validates: Requirements 8.5, 8.8, 22.1, 22.2, 22.5, 27.2, 27.5, 27.6**

  - [ ]* 9.10 Write integration tests for end-to-end flows
    - Use `WebApplicationFactory<Program>` with in-memory SQL or LocalDB
    - Test flows: OTP login → booking creation → QR token in response → check-in auto-assignment
    - Test Rolling Injection Job with mocked clock: weekday, Saturday, CalendarException, Sunday
    - Test Lockout Gate: fast-forward to T-60 → verify Locked status + waitlist promotion
    - Test Check-in: verify 35-min window rejection, location binding (403), sequential fill, convoy full
    - Test Breakdown flow: marshal flag → no notification → admin resolve → notification dispatched → BreakdownLog
    - Test Vendor analytics: seed breakdowns → query endpoint → verify per-shuttle/vendor tallies
    - Test CSV bulk upload: valid + duplicate + malformed rows → verify counts
    - _Requirements: 1.1–27.7 (end-to-end coverage)_

  - [ ] 9.11 Security hardening and final configuration
    - Enforce HTTPS-only with HSTS header
    - Configure CORS to restrict to known Flutter Web origin
    - Apply rate limiting on OTP endpoints (5 req/min per IP) via ASP.NET Core RateLimiter
    - Validate all request DTOs with FluentValidation
    - Ensure no sensitive data in error responses (no password/OTP disclosure)
    - Verify all Admin endpoints require Admin role, Marshal endpoints require Marshal role, Student endpoints require Student role
    - CSV upload: validate content-type and .csv extension before processing
    - _Requirements: 15.3, 19.5, 19.6, 20.2, 26.1, 26.4, 26.5, 26.6_

- [ ] 10. Final Checkpoint — All tests pass, system fully wired
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP delivery
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation at phase boundaries
- Property tests validate the 27 universal correctness properties defined in the design document using FsCheck (.NET)
- Integration tests validate end-to-end flows using WebApplicationFactory
- The backend uses C# (.NET 9), the mobile app uses Flutter (Dart), and the admin portal uses Flutter Web (Dart)
- Stop search in the booking form filters by StopName OR FullAddress (Requirement 11.4)
- Marshal location binding is enforced at the API level (Requirement 5.3) — not just the UI
- BreakdownLogs denormalize VendorCompanyName at flag time for stable vendor analytics
- Priority Bookings are pre-assigned to OrderPosition=1 shuttle (the front accessible vehicle)
- The 35-minute scan window and location binding are both enforced in the CheckInService

## Task Dependency Graph

```json
{
  "waves": [
    { "id": 0, "tasks": ["1.1"] },
    { "id": 1, "tasks": ["1.2"] },
    { "id": 2, "tasks": ["1.3"] },
    { "id": 3, "tasks": ["1.4", "1.5"] },
    { "id": 4, "tasks": ["1.6", "1.7"] },
    { "id": 5, "tasks": ["1.8"] },
    { "id": 6, "tasks": ["3.1", "3.7"] },
    { "id": 7, "tasks": ["3.2", "3.10"] },
    { "id": 8, "tasks": ["3.3", "3.8"] },
    { "id": 9, "tasks": ["3.4", "3.9", "3.11"] },
    { "id": 10, "tasks": ["3.5", "3.6"] },
    { "id": 11, "tasks": ["3.12"] },
    { "id": 12, "tasks": ["3.13", "3.14"] },
    { "id": 13, "tasks": ["3.15"] },
    { "id": 14, "tasks": ["5.1", "7.1"] },
    { "id": 15, "tasks": ["5.2", "7.2"] },
    { "id": 16, "tasks": ["5.3", "5.6", "7.3"] },
    { "id": 17, "tasks": ["5.4", "5.7", "5.8", "7.4"] },
    { "id": 18, "tasks": ["5.5", "5.9", "7.5"] },
    { "id": 19, "tasks": ["5.10", "7.6"] },
    { "id": 20, "tasks": ["7.7", "7.8"] },
    { "id": 21, "tasks": ["9.1", "9.2", "9.3"] },
    { "id": 22, "tasks": ["9.4", "9.5", "9.6"] },
    { "id": 23, "tasks": ["9.7", "9.8", "9.9"] },
    { "id": 24, "tasks": ["9.10"] },
    { "id": 25, "tasks": ["9.11"] }
  ]
}
```
