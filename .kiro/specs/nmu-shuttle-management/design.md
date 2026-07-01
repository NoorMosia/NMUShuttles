# Design Document — NMU Shuttle Management Network

## Overview

The NMU Shuttle Management Network is a university transit platform serving Nelson Mandela University
(NMU). It coordinates shuttle bookings between off-campus residential stops and university campuses,
supports marshal check-in operations, and gives administrators full oversight of the fleet, schedule,
and reporting ecosystem.

### Design Goals

- **Correctness first**: bookings, capacity enforcement, and the state machine must be race-condition
  safe and testable through property-based tests.
- **No real-time coupling**: the system deliberately avoids WebSockets; marshals poll for updates on
  a ≤30 s interval.
- **Clear role boundaries**: three JWT roles (Student, Marshal, Admin) enforce RBAC at every API
  endpoint.
- **Background automation**: two hosted services (Rolling Injection Job and Lockout Gate) drive all
  time-sensitive state changes without manual operator intervention.

---

## Architecture

### System Context Diagram

```
┌──────────────────────────────────────────────────────────────────────────┐
│                          NMU Shuttle Platform                            │
│                                                                          │
│  ┌─────────────────────┐    HTTPS/REST    ┌────────────────────────┐    │
│  │  Flutter Mobile App │◄───────────────►│  .NET 8 Web API        │    │
│  │  (Android / iOS)    │                 │  (ASP.NET Core)        │    │
│  │  Student + Marshal  │                 │                        │    │
│  └─────────────────────┘                 │  ┌──────────────────┐  │    │
│                                          │  │  EF Core         │  │    │
│  ┌─────────────────────┐    HTTPS/REST   │  │  (Code-First)    │  │    │
│  │  Flutter Web        │◄───────────────►│  └────────┬─────────┘  │    │
│  │  Admin Portal       │                 │           │            │    │
│  └─────────────────────┘                 │  ┌────────▼─────────┐  │    │
│                                          │  │  SQL Server      │  │    │
│  ┌─────────────────────┐    FCM Push     │  └──────────────────┘  │    │
│  │  Firebase Cloud     │◄───────────────│                        │    │
│  │  Messaging (FCM)    │                 │  ┌──────────────────┐  │    │
│  └─────────────────────┘                 │  │  IHostedService  │  │    │
│                                          │  │  (Rolling Job +  │  │    │
│                                          │  │   Lockout Gate)  │  │    │
│                                          │  └──────────────────┘  │    │
│                                          └────────────────────────┘    │
└──────────────────────────────────────────────────────────────────────────┘
```

### Component Responsibilities

| Component | Responsibility |
|---|---|
| .NET 8 Web API | All business logic, data access, JWT issuance, FCM dispatch orchestration |
| SQL Server | Authoritative persistent store for all domain entities |
| Rolling Injection Job | Nightly TripInstance generation + Priority Booking pre-population |
| Lockout Gate Service | Per-trip countdown timer — fires at T-60 min to lock trips and run waitlist promotion |
| Flutter Mobile App | Student booking UI + Marshal check-in + QR scanner (role-switched via JWT claim) |
| Flutter Web Admin Portal | Admin CRUD for stops, shuttles, users, schedules, calendar exceptions, reports |
| FCM | Delivery of push notifications to student and marshal devices |


---

## Database Schema

All tables use `INT IDENTITY` surrogate PKs unless otherwise noted. All timestamps are stored as
`DATETIME2` in UTC. The EF Core `DbContext` is named `ShuttleDbContext`.

### Entity Relationship Overview

```
Campus ──< ScheduleTime
Stop ──< PrioritySchedule ──> Student
Stop ──< TripInstance ──> ScheduleTime
TripInstance ──< TripShuttleAllocation ──> Shuttle
TripInstance ──< Booking ──> Student
Booking ──> Shuttle (boarded shuttle)
Student ──< DeviceRegistration
Student ──< Notification
Student ──< Report
Student ──< PrioritySchedule
Marshal ──> Campus (assigned)
CalendarException (standalone)
```

### Tables

#### Students
| Column | Type | Constraints |
|---|---|---|
| StudentId | INT IDENTITY | PK |
| StudentNumber | NVARCHAR(20) | UNIQUE NOT NULL |
| FirstName | NVARCHAR(100) | NOT NULL |
| LastName | NVARCHAR(100) | NOT NULL |
| UniversityEmail | NVARCHAR(200) | UNIQUE NOT NULL |
| IsPriorityUser | BIT | NOT NULL DEFAULT 0 |
| OtpHash | NVARCHAR(256) | NULL |
| OtpExpiry | DATETIME2 | NULL |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |

#### Marshals
| Column | Type | Constraints |
|---|---|---|
| MarshalId | INT IDENTITY | PK |
| StaffNumber | NVARCHAR(20) | UNIQUE NOT NULL |
| FirstName | NVARCHAR(100) | NOT NULL |
| LastName | NVARCHAR(100) | NOT NULL |
| UniversityEmail | NVARCHAR(200) | UNIQUE NOT NULL |
| AssignedCampusId | INT | FK → Campuses NOT NULL |
| OtpHash | NVARCHAR(256) | NULL |
| OtpExpiry | DATETIME2 | NULL |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |

#### Admins
| Column | Type | Constraints |
|---|---|---|
| AdminId | INT IDENTITY | PK |
| Email | NVARCHAR(200) | UNIQUE NOT NULL |
| PasswordHash | NVARCHAR(256) | NOT NULL |
| FirstName | NVARCHAR(100) | NOT NULL |
| LastName | NVARCHAR(100) | NOT NULL |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |

#### Campuses
| Column | Type | Constraints |
|---|---|---|
| CampusId | INT IDENTITY | PK |
| CampusName | NVARCHAR(100) | UNIQUE NOT NULL |
| GpsCoordinates | NVARCHAR(50) | NOT NULL |

#### Stops
| Column | Type | Constraints |
|---|---|---|
| StopId | INT IDENTITY | PK |
| StopName | NVARCHAR(100) | UNIQUE NOT NULL |
| GpsCoordinates | NVARCHAR(50) | NOT NULL |
| IsActive | BIT | NOT NULL DEFAULT 1 |

#### ScheduleTimes
| Column | Type | Constraints |
|---|---|---|
| ScheduleTimeId | INT IDENTITY | PK |
| DepartureTime | TIME | NOT NULL |
| Direction | NVARCHAR(20) | NOT NULL CHECK IN ('ToC ampus','FromCampus') |
| ScheduleType | NVARCHAR(20) | NOT NULL CHECK IN ('Weekday','Saturday') |
| CampusId | INT | FK → Campuses NOT NULL |
| StopId | INT | FK → Stops NOT NULL |

#### CalendarExceptions
| Column | Type | Constraints |
|---|---|---|
| CalendarExceptionId | INT IDENTITY | PK |
| TargetDate | DATE | UNIQUE NOT NULL |
| ExceptionType | NVARCHAR(30) | NOT NULL CHECK IN ('PublicHoliday','UniversityRecess') |
| Description | NVARCHAR(500) | NULL |


#### Shuttles
| Column | Type | Constraints |
|---|---|---|
| ShuttleId | INT IDENTITY | PK |
| LicensePlate | NVARCHAR(20) | UNIQUE NOT NULL |
| Capacity | INT | NOT NULL CHECK (Capacity > 0) |
| VendorCompanyName | NVARCHAR(100) | NOT NULL |
| IsAccessibleVehicle | BIT | NOT NULL DEFAULT 0 |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |

#### TripInstances
| Column | Type | Constraints |
|---|---|---|
| TripInstanceId | INT IDENTITY | PK |
| ScheduleTimeId | INT | FK → ScheduleTimes NOT NULL |
| StopId | INT | FK → Stops NOT NULL |
| TripDate | DATE | NOT NULL |
| Status | NVARCHAR(20) | NOT NULL DEFAULT 'Scheduling' CHECK IN ('Scheduling','Locked','InTransit','Completed','IncidentActive') |
| LockedAt | DATETIME2 | NULL |
| InTransitAt | DATETIME2 | NULL |
| CompletedAt | DATETIME2 | NULL |
| UNIQUE | (ScheduleTimeId, StopId, TripDate) | — prevents duplicate injection |

#### TripShuttleAllocations
| Column | Type | Constraints |
|---|---|---|
| AllocationId | INT IDENTITY | PK |
| TripInstanceId | INT | FK → TripInstances NOT NULL |
| ShuttleId | INT | FK → Shuttles NOT NULL |
| IsOperational | BIT | NOT NULL DEFAULT 1 |
| AssignedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |
| UNIQUE | (TripInstanceId, ShuttleId) | — no duplicate allocations |

#### Bookings
| Column | Type | Constraints |
|---|---|---|
| BookingId | INT IDENTITY | PK |
| StudentId | INT | FK → Students NOT NULL |
| TripInstanceId | INT | FK → TripInstances NOT NULL |
| BookingStatus | NVARCHAR(20) | NOT NULL DEFAULT 'Confirmed' CHECK IN ('Confirmed','WaitingList','Cancelled') |
| BookingType | NVARCHAR(20) | NOT NULL DEFAULT 'Standard' CHECK IN ('Standard','Priority') |
| BoardedStatus | NVARCHAR(20) | NOT NULL DEFAULT 'Pending' CHECK IN ('Pending','Boarded','NoShow') |
| QrCodeToken | NVARCHAR(128) | UNIQUE NOT NULL |
| ShuttleId | INT | FK → Shuttles NULL (set at check-in) |
| BoardedAt | DATETIME2 | NULL |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |
| UNIQUE | (StudentId, TripInstanceId) WHERE BookingStatus != 'Cancelled' | — no duplicate active bookings |

#### PrioritySchedules
| Column | Type | Constraints |
|---|---|---|
| PriorityScheduleId | INT IDENTITY | PK |
| StudentId | INT | FK → Students NOT NULL |
| StopId | INT | FK → Stops NOT NULL |
| ScheduleTimeId | INT | FK → ScheduleTimes NOT NULL |
| DayOfWeek | NVARCHAR(10) | NOT NULL CHECK IN ('Monday','Tuesday','Wednesday','Thursday','Friday','Saturday') |
| IsActive | BIT | NOT NULL DEFAULT 1 |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |

#### DeviceRegistrations
| Column | Type | Constraints |
|---|---|---|
| DeviceRegistrationId | INT IDENTITY | PK |
| OwnerId | INT | NOT NULL (StudentId or MarshalId) |
| OwnerType | NVARCHAR(10) | NOT NULL CHECK IN ('Student','Marshal') |
| FcmToken | NVARCHAR(512) | NOT NULL |
| Platform | NVARCHAR(10) | NOT NULL CHECK IN ('android','ios') |
| UpdatedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |
| UNIQUE | (OwnerId, OwnerType, Platform) | — one token per platform per user |

#### Notifications
| Column | Type | Constraints |
|---|---|---|
| NotificationId | INT IDENTITY | PK |
| StudentId | INT | FK → Students NOT NULL |
| Title | NVARCHAR(200) | NOT NULL |
| Message | NVARCHAR(1000) | NOT NULL |
| IsRead | BIT | NOT NULL DEFAULT 0 |
| RelatedTripInstanceId | INT | FK → TripInstances NULL |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |

#### Reports
| Column | Type | Constraints |
|---|---|---|
| ReportId | INT IDENTITY | PK |
| StudentId | INT | FK → Students NOT NULL |
| Category | NVARCHAR(30) | NOT NULL CHECK IN ('DriverBehavior','TimetableIssue','VehicleQuality','Other') |
| Description | NVARCHAR(2000) | NOT NULL |
| Status | NVARCHAR(20) | NOT NULL DEFAULT 'Submitted' CHECK IN ('Submitted','UnderReview','Resolved') |
| TripInstanceId | INT | FK → TripInstances NULL |
| AdminNotes | NVARCHAR(2000) | NULL |
| CreatedAt | DATETIME2 | NOT NULL DEFAULT GETUTCDATE() |
| UpdatedAt | DATETIME2 | NULL |

### Key Indexes

```sql
-- Hot query paths
CREATE INDEX IX_Bookings_TripInstanceId_Status ON Bookings (TripInstanceId, BookingStatus);
CREATE INDEX IX_Bookings_StudentId ON Bookings (StudentId);
CREATE INDEX IX_Bookings_QrCodeToken ON Bookings (QrCodeToken);
CREATE INDEX IX_TripInstances_TripDate_Status ON TripInstances (TripDate, Status);
CREATE INDEX IX_TripShuttleAllocations_TripInstanceId ON TripShuttleAllocations (TripInstanceId);
CREATE INDEX IX_DeviceRegistrations_OwnerId_Type ON DeviceRegistrations (OwnerId, OwnerType);
CREATE INDEX IX_Notifications_StudentId_IsRead ON Notifications (StudentId, IsRead);
CREATE INDEX IX_PrioritySchedules_DayOfWeek_Active ON PrioritySchedules (DayOfWeek, IsActive);
```


---

## API Design

All routes are prefixed `/api/v1`. JWT Bearer authentication is required on all routes except the OTP
initiation and admin login endpoints. Role guards are noted as `[Student]`, `[Marshal]`, `[Admin]`.

### Auth Endpoints

| Method | Route | Roles | Description |
|---|---|---|---|
| POST | `/auth/student/otp/request` | Public | Generate OTP for a StudentNumber |
| POST | `/auth/student/otp/verify` | Public | Verify OTP → returns JWT |
| POST | `/auth/marshal/otp/request` | Public | Generate OTP for a StaffNumber |
| POST | `/auth/marshal/otp/verify` | Public | Verify OTP → returns JWT |
| POST | `/auth/admin/login` | Public | Email + password → returns JWT |
| POST | `/auth/device/register` | Student, Marshal | Upsert FCM DeviceRegistration |

**POST `/auth/student/otp/request`**
```json
// Request
{ "studentNumber": "s2194827" }
// Response 200
{ "message": "OTP sent to registered email." }
// Response 404: student not found
```

**POST `/auth/student/otp/verify`**
```json
// Request
{ "studentNumber": "s2194827", "otp": "83921" }
// Response 200
{ "token": "<jwt>", "expiresAt": "2025-07-10T12:00:00Z", "role": "Student" }
// Response 401: invalid or expired OTP
```

**POST `/auth/admin/login`**
```json
// Request
{ "email": "admin@nmmu.ac.za", "password": "..." }
// Response 200
{ "token": "<jwt>", "expiresAt": "..." }
// Response 401: invalid credentials (no disclosure)
```

### Booking Endpoints

| Method | Route | Roles | Description |
|---|---|---|---|
| GET | `/trips` | Student | List available TripInstances for a date range (with capacity summary) |
| POST | `/bookings` | Student | Create a booking |
| GET | `/bookings` | Student | List own bookings |
| GET | `/bookings/{id}` | Student | Get booking detail + QR token |
| DELETE | `/bookings/{id}` | Student | Cancel a booking |

**GET `/trips`** — query params: `date` (YYYY-MM-DD), `stopId`, `direction`

```json
// Response 200
[
  {
    "tripInstanceId": 42,
    "departureTime": "07:00",
    "direction": "ToCampus",
    "stopName": "Summerstrand Village",
    "campusName": "South Campus",
    "tripDate": "2025-07-10",
    "status": "Scheduling",
    "totalCapacity": 60,
    "confirmedCount": 45,
    "availableSeats": 15
  }
]
```

**POST `/bookings`**
```json
// Request
{ "tripInstanceId": 42 }
// Response 201
{
  "bookingId": 101,
  "bookingStatus": "Confirmed",
  "bookingType": "Standard",
  "qrCodeToken": "a3f9c...",
  "tripDate": "2025-07-10",
  "departureTime": "07:00",
  "direction": "ToCampus",
  "stopName": "Summerstrand Village"
}
// Response 400: trip locked
// Response 409: duplicate booking
```

**DELETE `/bookings/{id}`**
```json
// Response 200: { "message": "Booking cancelled." }
// Response 400: trip not in Scheduling status
```

### Check-In Endpoints

| Method | Route | Roles | Description |
|---|---|---|---|
| POST | `/checkin` | Marshal | Dual-key check-in (QR token + ShuttleId) |
| GET | `/trips/{tripId}/passengers` | Marshal | Passenger manifest for a TripInstance |

**POST `/checkin`**
```json
// Request
{ "qrCodeToken": "a3f9c...", "shuttleId": 7 }
// Response 200
{
  "bookingId": 101,
  "studentName": "Jane Doe",
  "boardedStatus": "Boarded",
  "boardedAt": "2025-07-10T06:55:12Z"
}
// Response 404: token not found
// Response 409: already boarded
// Response 422: not confirmed / wrong shuttle
```

**GET `/trips/{tripId}/passengers`**
```json
// Response 200
{
  "tripInstanceId": 42,
  "totalConfirmed": 45,
  "boardedCount": 12,
  "pendingCount": 33,
  "passengers": [
    {
      "bookingId": 101,
      "studentName": "Jane Doe",
      "studentNumber": "s2194827",
      "boardedStatus": "Pending",
      "bookingType": "Standard"
    }
  ]
}
```

### Trip Management Endpoints

| Method | Route | Roles | Description |
|---|---|---|---|
| GET | `/trips/{id}` | Student, Marshal, Admin | Get TripInstance detail |
| PATCH | `/trips/{id}/status` | Admin | Manual status transition |
| POST | `/trips/{tripId}/breakdown` | Marshal | Flag shuttle breakdown |
| PUT | `/trips/{tripId}/standby` | Admin | Replace broken shuttle with standby |

**POST `/trips/{tripId}/breakdown`**
```json
// Request
{ "shuttleId": 7 }
// Response 200: { "newTotalCapacity": 30, "status": "IncidentActive" }
```

**PUT `/trips/{tripId}/standby`**
```json
// Request
{ "brokenShuttleId": 7, "replacementShuttleId": 12 }
// Response 200: { "newTotalCapacity": 60, "status": "Scheduling" }
```


### Admin Endpoints

| Method | Route | Roles | Description |
|---|---|---|---|
| GET/POST | `/admin/stops` | Admin | List / create Stops |
| GET/PUT/DELETE | `/admin/stops/{id}` | Admin | Get / edit / delete Stop |
| GET/POST | `/admin/shuttles` | Admin | List / create Shuttles |
| GET/PUT | `/admin/shuttles/{id}` | Admin | Get / edit Shuttle |
| GET/POST | `/admin/users` | Admin | List / add single user |
| POST | `/admin/users/bulk` | Admin | Bulk CSV upload |
| GET/PUT | `/admin/users/{id}` | Admin | Get / edit user |
| GET/POST | `/admin/schedule-times` | Admin | List / create ScheduleTimes |
| PUT/DELETE | `/admin/schedule-times/{id}` | Admin | Edit / delete ScheduleTime |
| GET/POST | `/admin/calendar-exceptions` | Admin | List / create CalendarExceptions |
| DELETE | `/admin/calendar-exceptions/{id}` | Admin | Delete CalendarException |
| GET/POST | `/admin/priority-schedules` | Admin | List / create PrioritySchedules |
| PATCH | `/admin/priority-schedules/{id}` | Admin | Deactivate PrioritySchedule |
| GET | `/admin/reports` | Admin | Paginated report list |
| GET/PATCH | `/admin/reports/{id}` | Admin | View / update report status |
| GET | `/admin/analytics/overview` | Admin | Dashboard analytics (top stops, report counts) |
| GET | `/admin/analytics/detailed` | Admin | Expanded booking demand stats |
| GET | `/admin/trips` | Admin | List TripInstances (filterable by date, status) |
| POST | `/admin/trips/{id}/shuttles` | Admin | Assign a shuttle to a TripInstance |

### Notification Endpoints

| Method | Route | Roles | Description |
|---|---|---|---|
| GET | `/notifications` | Student | List own notifications, ordered by CreatedAt DESC |
| PATCH | `/notifications/{id}/read` | Student | Mark notification as read |

### Report Endpoints (Student)

| Method | Route | Roles | Description |
|---|---|---|---|
| POST | `/reports` | Student | Submit a grievance report |
| GET | `/reports` | Student | List own reports, ordered by CreatedAt DESC |

---

## Background Services Design

### Rolling Injection Job (`RollingInjectionHostedService`)

Implemented as `IHostedService` with a `PeriodicTimer` or a `CronJob`-style background task that
fires once per day at 00:00:00 server time (UTC+2 for SAST — configured via `TimeZoneInfo`).

#### Algorithm

```
RollingInjectionJob():
  today = LocalDate.Today  // SAST
  targetDate = today.PlusDays(3)

  1. IF CalendarExceptions contains targetDate → EXIT (log: skipped, holiday)

  2. dayOfWeek = targetDate.DayOfWeek
     IF dayOfWeek == Sunday → EXIT (log: skipped, Sunday)

  3. scheduleType = dayOfWeek == Saturday ? "Saturday" : "Weekday"

  4. scheduleTimes = SELECT * FROM ScheduleTimes WHERE ScheduleType = scheduleType

  5. FOR EACH st IN scheduleTimes:
       IF NOT EXISTS TripInstances WHERE ScheduleTimeId=st.Id AND TripDate=targetDate:
         INSERT TripInstance (ScheduleTimeId=st.Id, StopId=st.StopId, TripDate=targetDate, Status='Scheduling')

  6. newTripIds = SELECT TripInstanceId from step 5 inserts

  7. FOR EACH newTrip IN newTripIds:
       activePS = SELECT ps FROM PrioritySchedules
                  WHERE IsActive=1
                    AND DayOfWeek = dayOfWeek
                    AND ScheduleTimeId = newTrip.ScheduleTimeId
                    AND StopId = newTrip.StopId

       FOR EACH ps IN activePS:
         INSERT Booking (
           StudentId=ps.StudentId,
           TripInstanceId=newTrip.TripInstanceId,
           BookingStatus='Confirmed',
           BookingType='Priority',
           BoardedStatus='Pending',
           QrCodeToken=GenerateUniqueToken()
         )
         // ON CONFLICT (duplicate booking for same student+trip): SKIP

  8. COMMIT transaction
  9. LOG: "Injection complete. Trips created: N, Priority bookings created: M"
```

**Idempotency guard**: the `UNIQUE (ScheduleTimeId, StopId, TripDate)` constraint on TripInstances
prevents duplicate injection if the job is accidentally re-run.

### Lockout Gate Service (`LockoutGateHostedService`)

Runs continuously as an `IHostedService`. On startup, and after each transition, it queries the
next upcoming unlocked trip and sleeps until T-60 min before that trip's departure.

#### Algorithm

```
LockoutGateLoop():
  WHILE service running:
    nextTrip = SELECT TOP 1 FROM TripInstances
               WHERE Status = 'Scheduling'
                 AND DATETIMEFROMPARTS(TripDate, DepartureTime) > GETUTCDATE()
               ORDER BY DATETIMEFROMPARTS(TripDate, DepartureTime) ASC

    IF nextTrip IS NULL:
      SLEEP 5 minutes; CONTINUE

    lockoutTime = DATETIMEFROMPARTS(nextTrip.TripDate, nextTrip.DepartureTime) - 60 minutes
    sleepDuration = lockoutTime - GETUTCDATE()

    IF sleepDuration > 0:
      SLEEP sleepDuration

    LockTrip(nextTrip.TripInstanceId)

LockTrip(tripInstanceId):
  BEGIN TRANSACTION (SERIALIZABLE)
    trip = SELECT ... WITH (UPDLOCK) WHERE TripInstanceId = tripInstanceId
    IF trip.Status != 'Scheduling': ROLLBACK; RETURN  // already transitioned

    UPDATE TripInstances SET Status='Locked', LockedAt=GETUTCDATE()
    WHERE TripInstanceId = tripInstanceId

    // Waitlist Promotion Loop
    capacity = SUM(s.Capacity) FROM TripShuttleAllocations a
               JOIN Shuttles s ON a.ShuttleId = s.ShuttleId
               WHERE a.TripInstanceId = tripInstanceId AND a.IsOperational = 1

    confirmedCount = COUNT(*) FROM Bookings
                     WHERE TripInstanceId = tripInstanceId
                       AND BookingStatus = 'Confirmed'

    available = capacity - confirmedCount

    waitlisted = SELECT TOP (@available) BookingId, StudentId
                 FROM Bookings
                 WHERE TripInstanceId = tripInstanceId
                   AND BookingStatus = 'WaitingList'
                 ORDER BY CreatedAt ASC

    FOR EACH b IN waitlisted:
      UPDATE Bookings SET BookingStatus='Confirmed' WHERE BookingId = b.BookingId
      INSERT Notifications (StudentId=b.StudentId, Title='Seat Confirmed', ...)
      DISPATCH FCM to student's DeviceRegistration tokens

  COMMIT
```

**Concurrency safety**: the `SERIALIZABLE` transaction with `UPDLOCK` hint prevents a race condition
where two promotion loops run simultaneously for the same trip. The Lockout Gate is the only writer
that sets `Status = 'Locked'`.


---

## Key Algorithms

### Convoy Capacity Calculation

```csharp
public async Task<int> GetTotalCapacityAsync(int tripInstanceId)
{
    return await _db.TripShuttleAllocations
        .Where(a => a.TripInstanceId == tripInstanceId && a.IsOperational)
        .Join(_db.Shuttles, a => a.ShuttleId, s => s.ShuttleId,
              (a, s) => s.Capacity)
        .SumAsync();
}
```

This is always computed from the live database — never cached — to ensure breakdown/overwrite
events are reflected immediately.

### Waitlist Promotion Loop

```csharp
public async Task<int> RunWaitlistPromotionAsync(int tripInstanceId, IDbTransaction tx)
{
    int capacity = await GetTotalCapacityAsync(tripInstanceId);
    int confirmed = await _db.Bookings
        .CountAsync(b => b.TripInstanceId == tripInstanceId
                      && b.BookingStatus == BookingStatus.Confirmed);

    int available = capacity - confirmed;
    if (available <= 0) return 0;

    var toPromote = await _db.Bookings
        .Where(b => b.TripInstanceId == tripInstanceId
                 && b.BookingStatus == BookingStatus.WaitingList)
        .OrderBy(b => b.CreatedAt)
        .Take(available)
        .ToListAsync();

    foreach (var booking in toPromote)
    {
        booking.BookingStatus = BookingStatus.Confirmed;
        await CreateAndDispatchNotificationAsync(booking.StudentId,
            "Your seat has been confirmed!", tripInstanceId);
    }

    await _db.SaveChangesAsync();
    return toPromote.Count;
}
```

### Dual-Key Check-In Validation

```csharp
public async Task<CheckInResult> CheckInAsync(string qrCodeToken, int shuttleId)
{
    var booking = await _db.Bookings
        .FirstOrDefaultAsync(b => b.QrCodeToken == qrCodeToken);

    if (booking is null)
        return CheckInResult.NotFound();

    if (booking.BookingStatus != BookingStatus.Confirmed)
        return CheckInResult.NotConfirmed(booking.BookingStatus);

    if (booking.BoardedStatus == BoardedStatus.Boarded)
        return CheckInResult.AlreadyBoarded();

    bool shuttleValid = await _db.TripShuttleAllocations.AnyAsync(a =>
        a.TripInstanceId == booking.TripInstanceId
        && a.ShuttleId == shuttleId
        && a.IsOperational);

    if (!shuttleValid)
        return CheckInResult.InvalidShuttle();

    booking.BoardedStatus = BoardedStatus.Boarded;
    booking.BoardedAt = DateTime.UtcNow;
    booking.ShuttleId = shuttleId;
    await _db.SaveChangesAsync();

    return CheckInResult.Success(booking);
}
```

### QR Code Token Generation

Tokens are generated using `Guid.NewGuid().ToString("N")` (32 hex chars) at booking creation time.
The `UNIQUE` constraint on `Bookings.QrCodeToken` provides the database-level uniqueness guarantee.
For additional collision resistance under high-volume injection, a retry loop with up to 3 attempts
is applied before failing hard.

### OTP Generation

```csharp
public string GenerateOtp() =>
    Random.Shared.Next(10000, 99999).ToString(); // 5-digit numeric OTP

public string HashOtp(string otp) =>
    Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(otp)));
```

OTPs are stored as SHA-256 hashes. Verification compares `HashOtp(submittedOtp) == storedHash`
and checks `OtpExpiry > DateTime.UtcNow`. On any outcome (pass or fail), OTP fields are cleared.

### TripInstance State Machine

```
Valid transitions:
  Scheduling     → Locked          (Lockout Gate at T-60 min)
  Scheduling     → IncidentActive  (Marshal breakdown flag, pre-lock)
  Locked         → InTransit       (Admin manual / marshal action)
  InTransit      → Completed       (Admin manual)
  InTransit      → IncidentActive  (Marshal breakdown flag, in-transit)
  IncidentActive → InTransit       (Admin standby overwrite restores)
```

Implemented as a switch/guard in `TripStateMachineService`:

```csharp
public bool IsValidTransition(TripStatus from, TripStatus to) => (from, to) switch
{
    (Scheduling, Locked)         => true,
    (Scheduling, IncidentActive) => true,
    (Locked, InTransit)          => true,
    (InTransit, Completed)       => true,
    (InTransit, IncidentActive)  => true,
    (IncidentActive, InTransit)  => true,
    _                            => false
};
```

On completion, all bookings with `BoardedStatus = Pending` are set to `NoShow`.


---

## Components and Interfaces

### Backend Layer Structure (.NET 8)

```
NmuShuttle.Api/
├── Controllers/           # Thin HTTP layer — validate, delegate to services, return DTOs
│   ├── AuthController.cs
│   ├── BookingsController.cs
│   ├── CheckInController.cs
│   ├── TripsController.cs
│   ├── NotificationsController.cs
│   ├── ReportsController.cs
│   └── Admin/
│       ├── StopsController.cs
│       ├── ShuttlesController.cs
│       ├── UsersController.cs
│       ├── ScheduleController.cs
│       └── ReportsController.cs
├── Services/              # Business logic
│   ├── OtpService.cs
│   ├── BookingService.cs
│   ├── CapacityService.cs
│   ├── CheckInService.cs
│   ├── TripStateMachineService.cs
│   ├── WaitlistPromotionService.cs
│   ├── NotificationService.cs
│   └── CsvUserImportService.cs
├── BackgroundServices/
│   ├── RollingInjectionHostedService.cs
│   └── LockoutGateHostedService.cs
├── Data/
│   ├── ShuttleDbContext.cs
│   └── Migrations/
├── Models/                # EF Core entity models
├── DTOs/                  # Request / response shapes
├── Infrastructure/
│   ├── FcmNotificationSender.cs   # Wraps FirebaseAdmin SDK
│   └── JwtTokenService.cs
└── Program.cs
```

### Service Interfaces

```csharp
public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(int studentId, int tripInstanceId);
    Task CancelBookingAsync(int bookingId, int studentId);
    Task<IList<BookingDto>> GetStudentBookingsAsync(int studentId);
}

public interface ICapacityService
{
    Task<int> GetTotalCapacityAsync(int tripInstanceId);
    Task RecalculateAndPersistAsync(int tripInstanceId);
}

public interface ICheckInService
{
    Task<CheckInResult> CheckInAsync(string qrCodeToken, int shuttleId);
}

public interface INotificationService
{
    Task DispatchAsync(int studentId, string title, string message, int? tripInstanceId = null);
    Task DispatchToTripPassengersAsync(int tripInstanceId, string title, string message);
}
```

---

## Flutter Mobile App Architecture

### Role-Switching Model

A single Flutter app serves both Students and Marshals. The JWT returned by the API contains a
`role` claim (`Student` or `Marshal`). On startup, the app reads the stored token, extracts the
role, and routes to the appropriate home screen. No separate app build is needed.

```dart
// On auth success:
final role = JwtDecoder.decode(token)['role'];
if (role == 'Student') {
  context.go('/student/home');
} else if (role == 'Marshal') {
  context.go('/marshal/home');
}
```

### State Management

**Riverpod** is used for all state management. Key provider families:

```dart
// Auth state
final authProvider = StateNotifierProvider<AuthNotifier, AuthState>(...);

// Student bookings
final upcomingBookingsProvider = FutureProvider.autoDispose<List<BookingDto>>(...);
final pastBookingsProvider    = FutureProvider.autoDispose<List<BookingDto>>(...);

// Trip availability (booking form)
final tripAvailabilityProvider = FutureProvider.autoDispose.family<List<TripDto>, TripQuery>(...);

// Marshal: trip manifest (polled every ≤30s)
final passengerManifestProvider = StreamProvider.autoDispose.family<ManifestDto, int>(
  (ref, tripId) => Stream.periodic(
    const Duration(seconds: 30),
    (_) => ref.read(apiClientProvider).getPassengers(tripId),
  ).asyncMap((f) => f),
);

// Notifications (unread count badge)
final unreadNotificationCountProvider = FutureProvider.autoDispose<int>(...);
```

### Navigation

**go_router** handles all navigation. Route guards check `authProvider` state.

```
/                       → AuthGate (redirect to role home or login)
/login                  → LoginScreen
/student/
  home                  → StudentHomeScreen (Upcoming tab default)
  booking/new           → NewBookingScreen
  booking/:id           → BookingDetailScreen (QR code display)
  maps                  → MapsScreen
  alerts                → AlertsScreen
  alerts/:id            → AlertDetailScreen
  profile               → ProfileScreen
  profile/report/new    → ReportFormScreen
/marshal/
  home                  → MarshalHomeScreen
  trip/:id/passengers   → PassengerListScreen
  scan                  → QrScanScreen
/admin/                 → (Flutter Web only — separate entry point)
```

### Layer Structure

```
lib/
├── core/
│   ├── api/           # Dio HTTP client, interceptors, DTOs
│   ├── auth/          # JWT storage (flutter_secure_storage), auth providers
│   └── router/        # go_router config
├── features/
│   ├── auth/
│   ├── student_home/
│   ├── booking/
│   ├── maps/          # Google Maps Flutter + location permission
│   ├── alerts/
│   ├── profile/
│   ├── marshal_home/
│   ├── passenger_list/
│   └── qr_scan/       # mobile_scanner package
└── shared/
    ├── widgets/
    └── theme/
```

### Marshal Polling Strategy

The marshal app does **not** use WebSockets. The `passengerManifestProvider` issues a GET request
every 30 seconds via a `Stream.periodic`. If a check-in POST returns `200`, the manifest provider
is immediately invalidated (`ref.invalidate(passengerManifestProvider(tripId))`) to give the
marshal instant feedback without waiting for the next poll cycle.


---

## Flutter Web Admin Portal Architecture

The admin portal is a separate Flutter Web entry point (`web/`) compiled to a static SPA and hosted
independently (or served from the same API host as static files).

### Architecture

```
admin_portal/
├── lib/
│   ├── core/
│   │   ├── api/         # Same Dio client, Admin JWT interceptor
│   │   ├── auth/        # Email+password login, token storage (localStorage for web)
│   │   └── router/      # go_router with sidebar navigation
│   └── features/
│       ├── auth/        # Login screen
│       ├── overview/    # Dashboard: top stops, report summary, analytics
│       ├── stops/       # CRUD for Stops
│       ├── shuttles/    # CRUD for Shuttles
│       ├── users/       # User list, single add, bulk CSV upload
│       ├── schedule/    # ScheduleTimes + CalendarExceptions
│       ├── reports/     # Report review
│       └── trips/       # TripInstance list + shuttle assignment
```

### Persistent Sidebar Navigation

The admin layout uses a `Scaffold` with a permanent `NavigationDrawer` (or a custom `SideRail`)
containing links to: Overview, Shuttle Stops, Shuttles, Users, Schedule, Reports.
Active route is highlighted via `GoRouterState`.

### CSV Upload Flow

```
1. Admin drags & drops .csv or clicks "Browse"
2. Flutter Web reads file bytes via `html.FileReader` (dart:html)
3. Bytes are sent as multipart/form-data to POST /admin/users/bulk
4. API streams row-by-row processing, returns:
   { "created": 142, "skipped": [{ "row": 3, "reason": "Duplicate email" }] }
5. Portal displays confirmation table with success count + skipped rows
```

---

## FCM Push Notification Flow

### Device Registration

```
App Startup / Auth Success:
  1. App calls FirebaseMessaging.instance.getToken()
  2. App POSTs { fcmToken, platform } to POST /auth/device/register
  3. API upserts DeviceRegistration (ON CONFLICT UPDATE token)
```

### Notification Dispatch (Backend)

```csharp
// FcmNotificationSender.cs
public async Task SendAsync(IEnumerable<string> tokens, string title, string body)
{
    var message = new MulticastMessage
    {
        Tokens = tokens.ToList(),
        Notification = new Notification { Title = title, Body = body },
        Android = new AndroidConfig
        {
            Priority = Priority.High,
            Notification = new AndroidNotification { ChannelId = "shuttle_alerts" }
        },
        Apns = new ApnsConfig
        {
            Aps = new Aps { Sound = "default", Badge = 1 }
        }
    };

    var response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);

    // Log individual failures; do not abort on partial failure (Req 9.8)
    foreach (var (result, token) in response.Responses.Zip(tokens))
    {
        if (!result.IsSuccess)
            _logger.LogWarning("FCM dispatch failed for token {Token}: {Error}", token, result.Exception);
    }
}
```

### Notification Triggers

| Event | Trigger Point | Recipients |
|---|---|---|
| Waitlist → Confirmed | `LockoutGateHostedService` (promotion loop) | Promoted student |
| Breakdown flagged | `POST /trips/{id}/breakdown` handler | All confirmed students on trip |
| Standby overwrite | `PUT /trips/{id}/standby` handler | All confirmed students on trip |

### Notification Record

Every FCM dispatch also creates a `Notifications` row in the database so the student can view
alerts in-app even if the push was missed.

```
Sequence Diagram: Waitlist Promotion Notification

LockoutGate ──► TripInstance.Status = Locked
             ──► WaitlistPromotionService.RunAsync(tripId)
                   ──► [for each promoted booking]
                         ──► INSERT Notification row
                         ──► FcmNotificationSender.SendAsync(studentTokens)
                               ──► Firebase FCM API
                                     ──► Student Device (push)
             ──► Student App polls /notifications → badge updates
```

---

## Security and RBAC Design

### JWT Structure

```json
{
  "sub": "42",
  "role": "Student",
  "studentNumber": "s2194827",
  "iat": 1720598400,
  "exp": 1720684800
}
```

- Students and Marshals: 24-hour token expiry
- Admins: 8-hour token expiry
- Tokens are signed with HS256 using a 256-bit secret stored in environment variables / Azure Key Vault

### Role Guards

```csharp
// Applied via ASP.NET Core policy attributes:
[Authorize(Roles = "Student")]
[Authorize(Roles = "Marshal")]
[Authorize(Roles = "Admin")]
[Authorize(Roles = "Student,Marshal")]  // shared endpoints
```

### RBAC Matrix

| Endpoint Group | Student | Marshal | Admin |
|---|---|---|---|
| OTP auth | ✓ | ✓ | — |
| Admin login | — | — | ✓ |
| Create/cancel bookings | ✓ | — | — |
| View own bookings | ✓ | — | — |
| Check-in (scan QR) | — | ✓ | — |
| View passenger manifest | — | ✓ | ✓ |
| Flag breakdown | — | ✓ | — |
| Standby overwrite | — | — | ✓ |
| Admin CRUD (stops, shuttles, users) | — | — | ✓ |
| Notifications | ✓ | — | — |
| Submit reports | ✓ | — | — |
| Review reports | — | — | ✓ |
| Analytics dashboard | — | — | ✓ |

### Security Measures

- **OTP hashing**: SHA-256 before storage; plain text never persisted.
- **Password hashing**: BCrypt (cost factor 12) for admin passwords.
- **SQL injection**: mitigated entirely by EF Core parameterised queries.
- **Input validation**: FluentValidation on all request DTOs; model binding rejects malformed payloads.
- **Rate limiting**: ASP.NET Core `RateLimiter` — OTP request endpoints limited to 5 requests/minute
  per IP to prevent OTP brute-force.
- **HTTPS only**: all traffic over TLS; HSTS header enforced.
- **CORS**: restricted to known Flutter Web origin in production.
- **CSV upload**: max 10 MB enforced via `RequestSizeLimit`; content-type and extension validated
  before processing.


---

## Data Models

### Core DTOs (C# Records)

```csharp
// Booking creation response
public record BookingDto(
    int BookingId,
    int TripInstanceId,
    string BookingStatus,
    string BookingType,
    string QrCodeToken,
    DateOnly TripDate,
    TimeOnly DepartureTime,
    string Direction,
    string StopName,
    string CampusName
);

// Trip availability (student booking form)
public record TripAvailabilityDto(
    int TripInstanceId,
    TimeOnly DepartureTime,
    string Direction,
    string StopName,
    string CampusName,
    DateOnly TripDate,
    string Status,
    int TotalCapacity,
    int ConfirmedCount,
    int AvailableSeats
);

// Check-in result
public record CheckInResultDto(
    int BookingId,
    string StudentName,
    string BoardedStatus,
    DateTime? BoardedAt
);

// Passenger manifest entry
public record PassengerDto(
    int BookingId,
    string StudentName,
    string StudentNumber,
    string BoardedStatus,
    string BookingType
);
```

---

## Error Handling

### HTTP Status Code Conventions

| Scenario | HTTP Status |
|---|---|
| Resource not found (non-existent ID, bad QR token) | 404 |
| Validation failure (empty description, negative capacity) | 422 |
| Business rule conflict (duplicate booking, re-locking) | 409 |
| Operation forbidden for current trip status (cancel locked trip) | 400 |
| Authentication failure (expired OTP, wrong password) | 401 |
| Authorisation failure (wrong role) | 403 |
| Server error | 500 |

### Global Exception Handler

A `ProblemDetails`-based `IExceptionHandler` middleware normalises all errors into RFC 7807 format:

```json
{
  "type": "https://nmushuttle.ac.za/errors/booking-locked",
  "title": "Booking modification not allowed",
  "status": 400,
  "detail": "This trip is locked and no longer accepts booking changes.",
  "traceId": "00-abc..."
}
```

### Background Service Error Handling

- The Rolling Injection Job catches all exceptions, logs them with `ILogger<T>`, and exits cleanly
  without crashing the host process. The job retries on the next nightly trigger.
- The Lockout Gate logs failures per-trip and advances to the next scheduled trip rather than
  halting the entire loop.
- FCM dispatch failures are logged per-token (Req 9.8) and never propagate as exceptions to callers.

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a
system — essentially, a formal statement about what the system should do. Properties serve as the
bridge between human-readable specifications and machine-verifiable correctness guarantees.*

---

### Property 1: OTP Single-Use Enforcement

*For any* generated OTP code, successfully verifying it once SHALL cause all subsequent verification
attempts for the same code to be rejected.

**Validates: Requirements 1.8**

---

### Property 2: OTP Format and Expiry Invariant

*For any* valid Student or Marshal identifier, the OTP generated by the System SHALL be exactly
5 decimal digits, and the stored expiry SHALL be within [4 min 55 sec, 5 min 05 sec] of the
generation timestamp (allowing for test clock drift).

**Validates: Requirements 1.1, 1.2**

---

### Property 3: Device Registration Round-Trip

*For any* authenticated user and any FCM token value, authenticating and registering the device
SHALL result in a DeviceRegistration record that maps that user's identity to exactly that token.

**Validates: Requirements 1.7**

---

### Property 4: Injection Date Invariant

*For any* execution of the Rolling Injection Job at a given clock time T, every TripInstance
created during that run SHALL have TripDate equal to LocalDate(T) + 3 days.

**Validates: Requirements 2.1, 2.9**

---

### Property 5: Calendar Exception Skip

*For any* date that exists in the CalendarExceptions table, running the Rolling Injection Job with
that date as TargetDate SHALL produce zero new TripInstances.

**Validates: Requirements 2.2**

---

### Property 6: Schedule-Type Matching Invariant

*For any* weekday TargetDate not in CalendarExceptions, the Rolling Injection Job SHALL create
exactly one TripInstance per ScheduleTime with ScheduleType = 'Weekday'. *For any* Saturday
TargetDate, it SHALL create exactly one TripInstance per ScheduleTime with ScheduleType =
'Saturday'. *For any* Sunday TargetDate, zero TripInstances SHALL be created.

**Validates: Requirements 2.3, 2.4, 2.5**

---

### Property 7: Injected TripInstances Have Scheduling Status

*For any* injection run, every TripInstance created SHALL have Status = 'Scheduling' at creation
time.

**Validates: Requirements 2.6**

---

### Property 8: Priority Booking Pre-Clearance Invariant

*For any* active PrioritySchedule whose DayOfWeek and ScheduleTimeId match a newly injected
TripInstance, the Rolling Injection Job SHALL create a Booking for that Student with
BookingType = 'Priority' and BookingStatus = 'Confirmed' before the TripInstance is accessible
for general booking.

**Validates: Requirements 2.7, 2.8, 2.10, 8.5**

---

### Property 9: Convoy Capacity Summation Invariant

*For any* TripInstance with N operational TripShuttleAllocations having capacities c₁…cₙ, the
System's computed Total Capacity SHALL equal c₁ + c₂ + … + cₙ. Allocations with IsOperational =
FALSE SHALL contribute zero to this sum.

**Validates: Requirements 3.1, 3.7**

---

### Property 10: Booking Status Determined by Available Capacity

*For any* booking request submitted when confirmed count < Total Capacity, the created Booking
SHALL have BookingStatus = 'Confirmed'. *For any* booking request submitted when confirmed count ≥
Total Capacity, the created Booking SHALL have BookingStatus = 'WaitingList'.

**Validates: Requirements 3.2, 3.3, 3.6**

---

### Property 11: Post-Lock Capacity Ceiling

*For any* TripInstance that transitions to Locked status and undergoes the Waitlist Promotion Loop,
the count of Confirmed Bookings after promotion SHALL not exceed Total Capacity.

**Validates: Requirements 4.6**

---

### Property 12: Waitlist Promotion Ordering

*For any* set of Waiting List Bookings on a TripInstance, the Waitlist Promotion Loop SHALL
promote bookings in ascending CreatedAt order (first-in, first-promoted), never skipping an
earlier booking to promote a later one.

**Validates: Requirements 4.4, 4.7**

---

### Property 13: QR Token Uniqueness

*For any* set of N bookings created by the System, all N QrCodeToken values SHALL be distinct —
no two Bookings share a token.

**Validates: Requirements 5.8**

---

### Property 14: Check-In Shuttle Integrity

*For any* successfully completed check-in, the Booking.ShuttleId recorded SHALL reference a
Shuttle that has an IsOperational = TRUE TripShuttleAllocation for the same TripInstance at the
time of check-in.

**Validates: Requirements 5.7**

---

### Property 15: Capacity Reduction on Breakdown

*For any* TripInstance and any operational shuttle in its convoy, flagging that shuttle as broken
down SHALL reduce Total Capacity by exactly that shuttle's Capacity value.

**Validates: Requirements 6.6**

---

### Property 16: Capacity Replacement on Standby Overwrite

*For any* breakdown event followed by a standby overwrite replacing shuttle A (capacity cₐ) with
shuttle B (capacity c_b), the resulting Total Capacity SHALL equal the pre-breakdown Total Capacity
minus cₐ plus c_b.

**Validates: Requirements 6.7**

---

### Property 17: Stop Data Completeness

*For any* Stop creation attempt where StopName is empty or whitespace, or GpsCoordinates is empty
or whitespace, the System SHALL reject the request with a validation error.

**Validates: Requirements 17.5**

---

### Property 18: Shuttle Capacity Validity

*For any* Shuttle creation or edit attempt where Capacity ≤ 0, the System SHALL reject the request
with a validation error.

**Validates: Requirements 18.6**

---

### Property 19: Bulk CSV Idempotency

*For any* CSV upload containing rows with identifiers or emails that already exist in the database
or that are duplicated within the file, the System SHALL skip those rows without modifying existing
records, and the final state of existing records SHALL be identical to their pre-upload state.

**Validates: Requirements 20.7, 20.8**

---

### Property 20: State Machine Completeness

*For any* TripInstance, every attempted status transition SHALL either succeed (if it matches a
defined valid transition) or be rejected with HTTP 422. No TripInstance SHALL reach Completed
status without having passed through Locked and InTransit.

**Validates: Requirements 22.1, 22.2, 22.5**


---

## Testing Strategy

### Overview

This feature is strongly amenable to property-based testing because its most critical correctness
guarantees — capacity arithmetic, booking status assignment, waitlist ordering, state machine
transitions, and QR uniqueness — are all pure functions or near-pure logic that can be exercised
with generated inputs.

### Unit Tests

Unit tests cover concrete examples, edge cases, and error conditions not captured by properties:

- OTP expiry boundary: expired by 1 second vs. expired by 10 minutes
- Duplicate booking rejection (HTTP 409) for same student + trip
- Sunday injection yields zero instances
- Non-existent student OTP request returns HTTP 404
- Check-in with unknown QR token returns HTTP 404
- Check-in with already-boarded booking returns HTTP 409
- State machine: Scheduling → Completed (direct) is rejected with HTTP 422
- CSV upload with only duplicate rows returns zero created, full error list
- Report with empty description returns HTTP 422

### Property-Based Tests

The property-based testing library is **FsCheck** (C#/.NET), configured with a minimum of
**100 iterations per property**.

Each test is tagged with a comment referencing the corresponding design property:
`// Feature: nmu-shuttle-management, Property N: <property text>`

#### Property 1: OTP Single-Use (Req 1.8)
```csharp
// Feature: nmu-shuttle-management, Property 1: OTP single-use enforcement
[Property]
Property OtpCannotBeUsedTwice()
{
    // Generate: random valid student, random OTP
    // Act: verify OTP once → success; verify same OTP again → failure
    // Assert: second verification returns 401
}
```

#### Property 2: OTP Format and Expiry (Req 1.1, 1.2)
```csharp
// Feature: nmu-shuttle-management, Property 2: OTP format and expiry invariant
[Property]
Property GeneratedOtpIsFiveDigitsAndExpiresInFiveMinutes()
{
    // Generate: random valid student/marshal identifiers
    // Act: request OTP generation
    // Assert: otp.Length == 5, all chars are digits
    //         expiry is within [now+4:55, now+5:05]
}
```

#### Property 9: Convoy Capacity Summation (Req 3.1, 3.7)
```csharp
// Feature: nmu-shuttle-management, Property 9: convoy capacity summation invariant
[Property]
Property TotalCapacityEqualsSumOfOperationalShuttles(
    PositiveInt[] capacities, bool[] operationalFlags)
{
    // Generate: random convoy with varying capacities and operational flags
    // Act: compute GetTotalCapacityAsync
    // Assert: result == sum of capacities[i] where operationalFlags[i] == true
}
```

#### Property 10: Booking Status by Capacity (Req 3.2, 3.3, 3.6)
```csharp
// Feature: nmu-shuttle-management, Property 10: booking status determined by capacity
[Property]
Property BookingStatusReflectsAvailability(
    PositiveInt totalCapacity, NonNegativeInt existingConfirmed)
{
    // Generate: trip with totalCapacity, already-confirmed existingConfirmed bookings
    // Act: create a new booking
    // Assert: if existingConfirmed < totalCapacity → status == Confirmed
    //         if existingConfirmed >= totalCapacity → status == WaitingList
    //         confirmed count never exceeds totalCapacity after any booking
}
```

#### Property 11 & 12: Post-Lock Capacity Ceiling + Ordering (Req 4.4, 4.6, 4.7)
```csharp
// Feature: nmu-shuttle-management, Property 11+12: waitlist promotion ceiling and ordering
[Property]
Property WaitlistPromotionRespectsCapacityAndOrder(
    PositiveInt capacity, BookingWithTimestamp[] waitlisted)
{
    // Generate: random capacity, random list of waitlisted bookings with random CreatedAt
    // Act: run WaitlistPromotionService
    // Assert: promoted.Count <= capacity
    //         promoted bookings are the N earliest by CreatedAt
    //         confirmed count <= capacity after promotion
}
```

#### Property 13: QR Token Uniqueness (Req 5.8)
```csharp
// Feature: nmu-shuttle-management, Property 13: QR token uniqueness
[Property]
Property AllQrCodeTokensAreUnique(PositiveInt bookingCount)
{
    // Generate: bookingCount (1-1000) booking creation requests
    // Act: create all bookings
    // Assert: all QrCodeTokens form a set of size == bookingCount (no duplicates)
}
```

#### Property 14: Check-In Shuttle Integrity (Req 5.7)
```csharp
// Feature: nmu-shuttle-management, Property 14: check-in shuttle integrity
[Property]
Property CheckInRecordsValidOperationalShuttle(
    int[] operationalShuttleIds, int[] nonOperationalIds)
{
    // Generate: random trip with mixed operational/non-operational shuttle allocations
    // Act: check-in with various shuttleIds
    // Assert: only check-ins with operationalShuttleIds succeed
    //         all successful check-ins record a ShuttleId in operationalShuttleIds
}
```

#### Property 15 & 16: Breakdown and Standby Capacity (Req 6.6, 6.7)
```csharp
// Feature: nmu-shuttle-management, Property 15+16: breakdown and standby capacity arithmetic
[Property]
Property BreakdownReducesCapacityByShuttleCapacity(
    PositiveInt[] shuttleCapacities, int shuttleToBreakIndex)
{
    // Generate: random convoy, random index of shuttle to break
    // Act: flag breakdown
    // Assert: new capacity == old capacity - shuttleCapacities[shuttleToBreakIndex]
}

[Property]
Property StandbyRestoresCapacityCorrectly(
    PositiveInt[] shuttleCapacities, int breakIdx, PositiveInt replacementCapacity)
{
    // Generate: random convoy, flag breakdown, apply standby overwrite
    // Assert: final capacity == original capacity - broken capacity + replacement capacity
}
```

#### Property 20: State Machine Completeness (Req 22.1, 22.2, 22.5)
```csharp
// Feature: nmu-shuttle-management, Property 20: state machine completeness
[Property]
Property OnlyValidTransitionsAreAccepted(
    TripStatus from, TripStatus to)
{
    // Generate: all (from, to) pairs of TripStatus enum values
    // Act: call TripStateMachineService.IsValidTransition(from, to)
    // Assert: result == true iff (from, to) is in the defined valid set
    //         no path from Scheduling to Completed that bypasses Locked and InTransit
}
```

### Integration Tests

Integration tests use `WebApplicationFactory<Program>` with an in-memory or LocalDB SQL Server:

- End-to-end booking flow: OTP login → create booking → verify in DB → QR token in response
- Rolling Injection Job: given a mocked clock, verify TripInstances and Priority Bookings are
  created correctly for weekday, Saturday, and CalendarException dates
- Lockout Gate: fast-forward clock to T-60 min, verify trip status changes to Locked and
  waitlist promotions fire
- Check-in flow: create booking → marshal check-in → verify BoardedStatus = Boarded
- Admin CSV bulk upload: upload file with valid, duplicate, and malformed rows; verify counts

### Flutter Testing

- **Unit tests**: service/provider logic tested with `mockito` mocks of the API client
- **Widget tests**: key screens (booking form capacity display, passenger list counts, QR scan
  result feedback) tested with `flutter_test`
- **Integration tests**: `patrol` or `integration_test` package for full-flow smoke tests on an
  emulator (login → book → view booking)

