# Domain Specification: NMU Shuttle Management Network

## 1. Relational Entity Matrix

### Group 1: Identity & Profile Network
#### `Student`
* **Description:** Represents an institutional academic student user. Holds flags defining automated accessibility routing.
* **Attributes:**
  * `StudentId` (PK, Int, Identity)
  * `StudentNumber` (VARCHAR, Unique, Indexed) - *e.g., "s2194827"*
  * `FirstName` (VARCHAR)
  * `LastName` (VARCHAR)
  * `UniversityEmail` (VARCHAR, Unique)
  * `IsPriorityUser` (BOOLEAN, Default: `FALSE`)
  * `OtpCode` (VARCHAR, Nullable) - *Stores temporary 5-minute expiring hashed token*
  * `OtpExpiry` (DateTime, Nullable)
  * `CreatedAt` (DateTime)

#### `Marshal`
* **Description:** Field operations staff deployed across platform stations to verify boarding manifests.
* **Attributes:**
  * `MarshalId` (PK, Int, Identity)
  * `StaffNumber` (VARCHAR, Unique, Indexed) - *e.g., "m77492"*
  * `FirstName` (VARCHAR)
  * `LastName` (VARCHAR)
  * `UniversityEmail` (VARCHAR, Unique)
  * `OtpCode` (VARCHAR, Nullable)
  * `OtpExpiry` (DateTime, Nullable)
  * `CreatedAt` (DateTime)

#### `DeviceRegistration`
* **Description:** Maps individual identity records to hardware push token addresses for Firebase Cloud Messaging (FCM).
* **Attributes:**
  * `RegistrationId` (PK, Int, Identity)
  * `StudentId` (FK to `Student`, Nullable)
  * `MarshalId` (FK to `Marshal`, Nullable)
  * `DeviceToken` (VARCHAR, Unique)
  * `Platform` (ENUM: `'Android'`, `'iOS'`)
  * `UpdatedAt` (DateTime)

---

### Group 2: Configuration Templates & Fleet Management
#### `Stop`
* **Description:** Defined off-campus residential destinations or neighborhood routing terminals. *Note: Core university campuses are intentionally omitted from this entity, as they act as sequential loading points for the physical vehicles.*
* **Attributes:**
  * `StopId` (PK, Int, Identity)
  * `StopName` (VARCHAR, Unique) - *e.g., "North Residence Hub"*
  * `GpsCoordinates` (VARCHAR)

#### `Campus`
* **Description:** Relational master list of university campuses used exclusively to deploy marshals to platform locations.
* **Attributes:**
  * `CampusId` (PK, Int, Identity)
  * `CampusName` (VARCHAR, Unique) - *e.g., "South Campus", "Second Avenue Campus"*

#### `ScheduleTime`
* **Description:** Master timetable matrices defining directional operating rules.
* **Attributes:**
  * `ScheduleTimeId` (PK, Int, Identity)
  * `DepartureTime` (TimeSpan) - *e.g., 14:30:00*
  * `Direction` (ENUM: `'To Campus'`, `'From Campus'`)
  * `ScheduleType` (ENUM: `'Weekday'`, `'Saturday'`)

#### `Shuttle`
* **Description:** Third-party outsourced physical transit vehicle assets. Seating capacities define hard limits for allocation math.
* **Attributes:**
  * `ShuttleId` (PK, Int, Identity)
  * `LicensePlate` (VARCHAR, Unique) - *e.g., "EC 123-456"*
  * `Capacity` (INT) - *e.g., 22, 35*
  * `VendorCompanyName` (VARCHAR) - *e.g., "Bay Transit Corp"*
  * `IsAccessibleVehicle` (BOOLEAN, Default: `FALSE`)

#### `CalendarException`
* **Description:** Blackout calendar windows where the rolling background engine skips trip layout generation.
* **Attributes:**
  * `ExceptionId` (PK, Int, Identity)
  * `TargetDate` (DATE, Unique)
  * `Type` (ENUM: `'Public Holiday'`, `'University Recess'`)
  * `Description` (VARCHAR)

---

### Group 3: Core Transactional Infrastructure
#### `PrioritySchedule`
* **Description:** Master permits issued by the university's disability center mapping recurring travel needs to automatic pre-booking windows.
* **Attributes:**
  * `PriorityScheduleId` (PK, Int, Identity)
  * `StudentId` (FK to `Student`)
  * `StopId` (FK to `Stop`)
  * `ScheduleTimeId` (FK to `ScheduleTime`)
  * `DayOfWeek` (ENUM: `'Monday'`, `'Tuesday'`, `'Wednesday'`, `'Thursday'`, `'Friday'`)
  * `IsActive` (BOOLEAN, Default: `TRUE`)

#### `TripInstance`
* **Description:** The specific daily operational container execution block. *This holds no capacity metric directly; capacity is derived from its allocated vehicles.*
* **Attributes:**
  * `TripInstanceId` (PK, Int, Identity)
  * `StopId` (FK to `Stop`)
  * `ScheduleTimeId` (FK to `ScheduleTime`)
  * `TripDate` (DATE)
  * `Status` (ENUM: `'Scheduling'`, `'Locked'`, `'In Transit'`, `'Completed'`, `'Incident Active'`)

#### `TripShuttleAllocation`
* **Description:** The core bridge entity managing the convoy model. Maps multiple individual physical outsourced vehicles to a single operational trip block.
* **Attributes:**
  * `AllocationId` (PK, Int, Identity)
  * `TripInstanceId` (FK to `TripInstance`)
  * `ShuttleId` (FK to `Shuttle`)
  * `IsOperational` (BOOLEAN, Default: `TRUE`)

#### `Booking`
* **Description:** Seating transaction registers binding student profiles to a specific route runtime.
* **Attributes:**
  * `BookingId` (PK, Int, Identity)
  * `StudentId` (FK to `Student`)
  * `TripInstanceId` (FK to `TripInstance`)
  * `ShuttleId` (FK to `Shuttle`, Nullable) - *Assigned strictly at the physical point of check-in*
  * `QrCodeToken` (VARCHAR, Unique)
  * `BookingType` (ENUM: `'Standard'`, `'Priority'`)
  * `BookingStatus` (ENUM: `'Confirmed'`, `'Waiting List'`, `'Cancelled'`)
  * `BoardedStatus` (ENUM: `'Pending'`, `'Boarded'`, `'No Show'`)
  * `BoardedAt` (DateTime, Nullable)
  * `CreatedAt` (DateTime)

---

### Group 4: Performance Logs & Accountability
#### `Report`
* **Description:** Performance grievances filed by students regarding vendor operations.
* **Attributes:**
  * `ReportId` (PK, Int, Identity)
  * `StudentId` (FK to `Student`)
  * `TripInstanceId` (FK to `TripInstance`, Nullable)
  * `Category` (ENUM: `'Driver Behavior'`, `'Timetable Issue'`, `'Vehicle Quality'`, `'Other'`)
  * `Description` (TEXT)
  * `Status` (ENUM: `'Submitted'`, `'Under Review'`, `'Resolved'`)
  * `AdminNotes` (TEXT, Nullable)
  * `CreatedAt` (DateTime)
  * `UpdatedAt` (DateTime)

#### `Notification`
* **Description:** System status alerts pushed to student applications via the backend.
* **Attributes:**
  * `NotificationId` (PK, Int, Identity)
  * `StudentId` (FK to `Student`)
  * `Title` (VARCHAR)
  * `Message` (TEXT)
  * `IsRead` (BOOLEAN, Default: `FALSE`)
  * `CreatedAt` (DateTime)

---

## 2. Domain Business Logic Invariants

### Invariant 1: Dynamic Convoy Capacity Resolution
The hard ceiling of available seats for any given `TripInstance` is an aggregate calculation resolved at runtime.
* **Mathematical Invariant:**
  $$\text{Total Capacity} = \sum_{a \in \text{Allocations}} a.\text{Shuttle}.\text{Capacity} \quad \text{where} \quad a.\text{IsOperational} == \text{TRUE}$$
* **Validation Rule:** When a student creates a reservation, the system counts all records where `BookingStatus == 'Confirmed'`. If the current count is equal to or greater than the resolved total capacity, the new transaction is forced to save as `Waiting List`.

### Invariant 2: Rolling 4-Day Injection & Priority Pre-clearance
An automated background processing engine fires nightly at `00:00:00` to maintain a structural 4-day booking boundary.
* **Logic Flow:**
  1. Calculate `TargetDate = Today + 3 Days`.
  2. Verify `TargetDate` does not exist inside the `CalendarException` matrix. If it matches, halt generation for that date.
  3. Query `ScheduleTime` templates matching the target day of the week to spawn `TripInstance` records.
  4. Before opening visibility to the general student population, query `PrioritySchedule` for all student profiles matching the target route and weekday.
  5. Automatically insert rows into `Booking` flagged as `BookingStatus = 'Confirmed'`, `BookingType = 'Priority'`, and `BoardedStatus = 'Pending'`. This ensures accessibility-cleared students bypass the standard capacity competition layer.

### Invariant 3: The 1-Hour Lockout Gate & Optimization Worker
Exactly 60 minutes before a `TripInstance.ScheduleTime.DepartureTime` on `TripDate`, a state machine lock drops onto the trip container.
* **Enforced Controls:**
  * Set `TripInstance.Status = 'Locked'`.
  * Instantly reject all downstream API calls attempting to change state fields (`POST /api/bookings` or `DELETE /api/bookings` return an HTTP 400 Bad Request exception).
  * Run the **Waitlist Promotion Loop**: If standard student cancellations or administrative vehicle upgrades created open space below the total convoy capacity threshold, fetch the `Waiting List` records for this trip sorted by `CreatedAt` ASC. Update their status to `Confirmed`, write corresponding alert items to the `Notification` ledger, and dispatch a push notification to Firebase Cloud Messaging (FCM).

### Invariant 4: Dual-Key Check-In Verification
When a student mounts a vehicle at a multi-campus loading hub, the field marshal scans their QR code. Because multiple shuttles can fulfill the exact same trip slot, check-in requires explicit asset identification.
* **API Validation Requirements:** The check-in endpoint requires parameters for `BookingId` and `ShuttleId`.
* **Database Verification Assertions:**
  1. Assert the parsed `Booking.QrCodeToken` exists and is marked as `Confirmed`.
  2. Assert the incoming `ShuttleId` is linked to an active `TripShuttleAllocation` record pointing to the target `TripInstanceId`.
* **State Updates:** Upon passing validation assertions, update `Booking.BoardedStatus = 'Boarded'`, `Booking.BoardedAt = DateTime.UtcNow`, and assign `Booking.ShuttleId = InputShuttleId`.

### Invariant 5: Isolated Breakdown and Standby Overwrite Controls
If a specific vehicle within a multi-shuttle convoy suffers a mechanical breakdown, the platform isolates the event without invalidating the trip context.
* **Logic Flow:**
  1. The marshal flags the specific vehicle via its allocation context.
  2. The system sets `TripShuttleAllocation.IsOperational = FALSE` for that row.
  3. The overall capacity pool for the `TripInstance` re-evaluates downward automatically.
  4. When an administrator coordinates a backup vehicle with the third-party transit provider's dispatch desk, they issue an overwrite update containing the new vehicle parameters.
  5. The backend patches the `ShuttleId` on that specific allocation row to map to the replacement asset, switches `IsOperational = TRUE`, and loops through the active booking manifest to dispatch a status update notification to the affected students.

---

## 3. Core System Life Cycles

### Event Flow A: High-Frequency Roster State Sync (No WebSockets)