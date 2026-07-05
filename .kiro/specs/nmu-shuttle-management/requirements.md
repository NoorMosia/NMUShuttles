# Requirements Document

## Introduction

The NMU Shuttle Management Network is a university transit platform for Nelson Mandela University (NMU).
It coordinates student shuttle bookings between off-campus residential stops and university campuses,
supports marshal check-in operations at boarding platforms, and provides administrative oversight of the
entire fleet, schedule, and reporting ecosystem.

The system is composed of three surfaces:
- **Student & Marshal Mobile App** (Flutter — Android/iOS): used by students to book seats and view trip
  details, and by marshals to manage boarding and flag vehicle issues.
- **Admin Portal** (Flutter Web): used by university administrators to manage stops, shuttles, users,
  schedules, calendar exceptions, and operational reports.
- **Backend API** (.NET Web API, C#): the authoritative data and business-logic layer consumed by both
  surfaces.

The platform enforces a convoy model (multiple shuttles per trip), dynamic capacity, a rolling 4-day
booking window, a 1-hour lockout gate, OTP-based authentication, and priority pre-clearance for
disability-cleared students.

---

## Glossary

- **System**: The NMU Shuttle Management Network backend API and its associated client surfaces.
- **Student**: An NMU student registered in the System, identified by a student number consisting of
  the letter "s" followed by exactly 9 digits starting with 31 or 32 (e.g., "s312345678", "s329876543").
- **Marshal**: Field operations staff deployed at campus boarding platforms or off-campus stop
  locations, identified by a staff number consisting of exactly 7 digits (e.g., "4827391").
- **Admin**: A university administrator with access to the Admin Portal.
- **Stop**: A defined off-campus residential destination with a name, full street address, and GPS coordinates.
- **Campus**: A university campus location used as a marshal deployment and shuttle boarding point.
- **ScheduleTime**: A master timetable entry defining departure time, direction, and schedule type.
- **Shuttle**: A physical transit vehicle with a license plate, seating capacity, vendor company, and
  accessibility flag.
- **CalendarException**: A blackout date (public holiday or university recess) on which no trips are generated.
- **TripInstance**: A specific daily trip combining a Stop, ScheduleTime, and date.
- **TripShuttleAllocation**: A record linking one Shuttle to one TripInstance; includes an OrderPosition
  field (integer, 1-based) defining the shuttle's front-to-back sequence in the convoy, and an
  IsOperational flag that controls whether that shuttle's capacity counts toward the trip total.
- **Booking**: A student seat reservation linked to a TripInstance.
- **PrioritySchedule**: A recurring travel permit for a disability-cleared Student, enabling automatic
  pre-booking before the general booking window opens.
- **Report**: A student grievance filing against a vendor or trip.
- **Notification**: A push alert delivered to a Student's device via Firebase Cloud Messaging (FCM).
- **DeviceRegistration**: A mapping between a Student or Marshal identity and an FCM device push token.
- **OTP**: A one-time password — a 5-digit, time-limited code used to authenticate Students and Marshals.
- **Convoy**: The set of one or more Shuttles allocated to a single TripInstance, arranged in a defined
  front-to-back order via each TripShuttleAllocation's OrderPosition.
- **ScanWindow**: The 35-minute period before a TripInstance's scheduled DepartureTime during which
  QR code check-in scanning is permitted.
- **Total Capacity**: The runtime aggregate of Capacity values for all TripShuttleAllocations where
  IsOperational = TRUE for a given TripInstance.
- **Lockout Time**: The point exactly 60 minutes before a TripInstance's scheduled departure time on its
  TripDate, at which the TripInstance transitions to Locked status.
- **Waitlist Promotion Loop**: The process of promoting Waiting List bookings to Confirmed status,
  in CreatedAt ascending order, when space becomes available on a Locked TripInstance.
- **Rolling Injection Job**: The nightly background job at 00:00:00 that generates TripInstances for
  Today + 3 days and auto-creates Priority bookings.
- **QR Code Token**: A unique opaque token embedded in a Booking's QR code, used for check-in verification.
- **BoardedStatus**: The boarding state of a Booking — Pending, Boarded, or No Show.
- **Priority Booking**: A Booking of type Priority, automatically created for a Student with an active
  PrioritySchedule, before general bookings open.
- **Dual-Key Check-In**: The check-in procedure requiring both a BookingId (from the QR code) and a
  ShuttleId to validate boarding.
- **Incident Active**: A TripInstance status indicating a vehicle breakdown has been flagged for the trip.
- **FCM**: Firebase Cloud Messaging — the push notification service used by the System.
- **CSV Upload**: Bulk user provisioning via a comma-separated values file uploaded in the Admin Portal.

---

## Requirements

### Requirement 1: OTP-Based Authentication

**User Story:** As a Student or Marshal, I want to log in using my institutional identifier and a
one-time password, so that only registered university members can access the System.

#### Acceptance Criteria

1. WHEN a Student submits a StudentNumber matching the format "s" followed by exactly 9 digits
   starting with 31 or 32 (regex: `^s3[12]\d{7}$`), THE System SHALL generate a 5-digit OTP, store
   a hashed copy with an expiry of exactly 5 minutes from generation, and send it to the Student's
   registered UniversityEmail.
2. WHEN a Marshal submits a StaffNumber matching the format of exactly 7 digits (regex: `^\d{7}$`),
   THE System SHALL generate a 5-digit OTP, store a hashed copy with an expiry of exactly 5 minutes
   from generation, and send it to the Marshal's registered UniversityEmail.
3. WHEN an OTP submission is received within the 5-minute window and matches the stored hashed code,
   THE System SHALL issue a signed authentication token and clear the OTP fields.
4. IF an OTP submission is received after the 5-minute expiry window, THEN THE System SHALL reject
   the submission with HTTP 401 and clear the expired OTP fields.
5. IF a StudentNumber or StaffNumber submitted for OTP generation does not exist in the System,
   THEN THE System SHALL return HTTP 404.
6. IF a StudentNumber is submitted that does not match the format `^s3[12]\d{7}$`, THEN THE System
   SHALL reject the request with HTTP 422 and a descriptive format error.
7. IF a StaffNumber is submitted that does not match the format `^\d{7}$`, THEN THE System SHALL
   reject the request with HTTP 422 and a descriptive format error.
8. THE System SHALL present a role-switcher UI on the login screen allowing the user to select either
   the Student or Marshal login flow before submitting their identifier.
9. WHEN a Student or Marshal successfully authenticates, THE System SHALL register or update the
   DeviceRegistration record for the authenticated device with the current FCM device token and platform.
10. FOR ALL valid OTP codes, verifying the code then attempting to verify the same code again SHALL
    result in rejection on the second attempt (single-use property).

---

### Requirement 2: Rolling 4-Day Trip Injection

**User Story:** As an Admin, I want the System to automatically generate trip instances for the next
3 days every night, so that students always have an accessible 4-day booking window without manual
schedule entry.

#### Acceptance Criteria

1. WHEN the Rolling Injection Job fires at 00:00:00, THE System SHALL calculate TargetDate as the
   current calendar date plus 3 days.
2. WHEN the Rolling Injection Job executes and TargetDate exists in the CalendarException table,
   THE System SHALL skip all TripInstance generation for that TargetDate and halt without error.
3. WHEN TargetDate is not a CalendarException and TargetDate falls on a weekday (Monday–Friday),
   THE System SHALL create one TripInstance record per ScheduleTime entry where ScheduleType = 'Weekday'.
4. WHEN TargetDate is not a CalendarException and TargetDate falls on a Saturday, THE System SHALL
   create one TripInstance record per ScheduleTime entry where ScheduleType = 'Saturday'.
5. WHEN TargetDate is not a CalendarException and TargetDate falls on a Sunday, THE System SHALL
   skip TripInstance generation for that date.
6. WHEN TripInstance records are created, THE System SHALL set their initial Status to 'Scheduling'.
7. WHEN TripInstance records for TargetDate have been created, THE System SHALL query all active
   PrioritySchedule records whose DayOfWeek matches the day of TargetDate and whose StopId and
   ScheduleTimeId match a newly created TripInstance.
8. WHEN matching PrioritySchedule records are found, THE System SHALL insert a Booking record for
   each matched Student with BookingStatus = 'Confirmed', BookingType = 'Priority', and
   BoardedStatus = 'Pending', before any general booking window opens for that TripInstance.
9. FOR ALL TripInstances generated by the Rolling Injection Job on a given TargetDate, the
   TripDate field SHALL equal TargetDate (injection date invariant).
10. FOR ALL Priority Bookings created by the Rolling Injection Job, the Booking.TripInstanceId SHALL
    reference a TripInstance whose TripDate equals the TargetDate used in that job run (round-trip
    binding property).

---

### Requirement 3: Dynamic Convoy Capacity Enforcement

**User Story:** As a Student, I want the System to accurately reflect how many seats are available on a
trip based on all assigned shuttles, so that I know whether I get a confirmed booking or a waiting list
position.

#### Acceptance Criteria

1. THE System SHALL compute Total Capacity for a TripInstance as the sum of Shuttle.Capacity for all
   TripShuttleAllocation records linked to that TripInstance where IsOperational = TRUE.
2. WHEN a Student submits a new booking request and the count of Bookings with BookingStatus = 'Confirmed'
   for that TripInstance is less than Total Capacity, THE System SHALL create the Booking with
   BookingStatus = 'Confirmed'.
3. WHEN a Student submits a new booking request and the count of Bookings with BookingStatus = 'Confirmed'
   for that TripInstance is greater than or equal to Total Capacity, THE System SHALL create the Booking
   with BookingStatus = 'Waiting List'.
4. WHEN a TripShuttleAllocation record is set to IsOperational = FALSE, THE System SHALL immediately
   recalculate Total Capacity downward for the affected TripInstance without modifying existing Confirmed
   Bookings.
5. WHEN a replacement Shuttle is assigned to a TripShuttleAllocation and IsOperational is set to TRUE,
   THE System SHALL immediately recalculate Total Capacity upward for the affected TripInstance.
6. FOR ALL TripInstances, the count of Confirmed Bookings SHALL never exceed Total Capacity at the moment
   a booking is created (capacity ceiling invariant).
7. FOR ALL convoys with N operational shuttles of capacities c₁, c₂, …, cₙ, the Total Capacity SHALL
   equal c₁ + c₂ + … + cₙ (summation invariant).
8. FOR ALL booking requests where a Student already has an active Confirmed or Waiting List booking for
   the same TripInstance, THE System SHALL reject the duplicate with HTTP 409.

---

### Requirement 4: 1-Hour Lockout Gate and Waitlist Promotion

**User Story:** As an Admin, I want trips to automatically lock 60 minutes before departure, so that the
system can close bookings, promote waitlisted students, and give marshals a stable passenger manifest.

#### Acceptance Criteria

1. WHEN the current time reaches exactly 60 minutes before a TripInstance's scheduled DepartureTime on
   its TripDate, THE System SHALL set TripInstance.Status = 'Locked'.
2. WHILE a TripInstance.Status = 'Locked', THE System SHALL reject all POST /bookings requests for that
   TripInstance with HTTP 400.
3. WHILE a TripInstance.Status = 'Locked', THE System SHALL reject all DELETE /bookings cancellation
   requests for that TripInstance with HTTP 400.
4. WHEN a TripInstance transitions to Locked status, THE System SHALL execute the Waitlist Promotion Loop:
   fetch all Bookings for that TripInstance with BookingStatus = 'Waiting List', ordered by CreatedAt ASC,
   and promote as many as will fit within the available remaining capacity to BookingStatus = 'Confirmed'.
5. WHEN a Booking is promoted from Waiting List to Confirmed during the Waitlist Promotion Loop, THE
   System SHALL create a Notification record for the affected Student and dispatch an FCM push notification.
6. FOR ALL TripInstances in Locked status, the total number of Confirmed Bookings after the Waitlist
   Promotion Loop SHALL not exceed Total Capacity (post-lock capacity invariant).
7. FOR ALL Waiting List Bookings promoted during a single Waitlist Promotion Loop execution, the ordering
   SHALL preserve CreatedAt ASC (first-come-first-served invariant).
8. IF a TripInstance is already in Locked, In Transit, Completed, or Incident Active status, THEN THE
   System SHALL reject any attempt to re-trigger the lockout transition with HTTP 409.

---

### Requirement 5: Dual-Key QR Check-In

**User Story:** As a Marshal, I want to scan a student's QR code at the boarding point and have the
System assign them to the next available shuttle in sequence, so that boarding is orderly and each
vehicle fills front-to-back. For "To Campus" trips, boarding and scanning occur at the off-campus
Stop. For "From Campus" trips, boarding and scanning occur at the on-campus platform.

#### Acceptance Criteria

1. WHEN a Marshal submits a check-in request with a QrCodeToken and the current time is within 35
   minutes of the TripInstance's scheduled DepartureTime, THE System SHALL look up the Booking record
   matching the QrCodeToken.
2. IF a check-in request is submitted more than 35 minutes before the TripInstance's scheduled
   DepartureTime, THEN THE System SHALL reject the request with HTTP 422 and a message indicating
   scanning is not yet available.
3. WHEN the Booking's TripInstance is looked up, THE System SHALL verify that the Marshal's assigned
   location matches the trip's boarding point: for "ToCampus" trips, the Marshal's AssignedStopId
   must equal the TripInstance's StopId; for "FromCampus" trips, the Marshal's AssignedCampusId must
   equal the TripInstance's CampusId (via ScheduleTime). IF the locations do not match, THE System
   SHALL reject the check-in with HTTP 403 and a message indicating the marshal is not authorised
   to scan at this location.
4. IF the QrCodeToken does not match any Booking record, THEN THE System SHALL reject the check-in
   with HTTP 404.
5. IF the matched Booking has BookingStatus != 'Confirmed', THEN THE System SHALL reject the check-in
   with HTTP 422 and return a descriptive error indicating the booking is not in Confirmed status.
6. WHEN all validation assertions pass, THE System SHALL assign the Student to the next shuttle in
   the convoy that has not yet reached its capacity, following OrderPosition ASC (front-to-back fill).
7. WHEN the shuttle assignment is determined, THE System SHALL update Booking.BoardedStatus = 'Boarded',
   Booking.BoardedAt = DateTime.UtcNow (UTC), and Booking.ShuttleId = the assigned ShuttleId.
8. IF a check-in request is submitted for a Booking where BoardedStatus is already 'Boarded', THEN
   THE System SHALL reject the request with HTTP 409 to prevent duplicate boarding.
9. FOR ALL successfully checked-in Bookings, the assigned Booking.ShuttleId SHALL reference a Shuttle
   that was IsOperational = TRUE in a TripShuttleAllocation for the same TripInstance at the time of
   check-in (shuttle assignment integrity invariant).
10. THE System SHALL generate a unique QrCodeToken for each Booking at creation time such that no two
    Bookings share the same token (uniqueness invariant).
11. FOR ALL check-in operations on a TripInstance, Bookings SHALL be assigned to shuttles in
    OrderPosition ASC order, filling each shuttle to capacity before advancing to the next
    (sequential fill invariant).
12. FOR ALL check-in attempts, the Marshal's assigned location (Campus or Stop) SHALL match the
    TripInstance's boarding location for the scan to succeed (location binding invariant).

---

### Requirement 6: Vehicle Breakdown and Admin Resolution

**User Story:** As a Marshal, I want to simply flag a shuttle as not arrived or broken down with minimal
effort, so that I can focus on boarding; and as an Admin, I want to fill in replacement details or cancel
the trip, so that accurate records are maintained and students are notified appropriately.

#### Acceptance Criteria

1. WHEN a Marshal submits a breakdown flag for a ShuttleId on a TripInstance, THE System SHALL set the
   corresponding TripShuttleAllocation.IsOperational = FALSE, recalculate Total Capacity downward, and
   record the flag timestamp.
2. THE Marshal breakdown flag interface SHALL require only a single action (select shuttle and confirm flag)
   — the Marshal SHALL NOT be required to provide replacement shuttle details, reasons, or resolution steps.
3. WHEN a breakdown is flagged, THE System SHALL set TripInstance.Status = 'Incident Active'.
4. WHEN a shuttle is flagged as not arrived or broken down, Students already scanned (BoardedStatus =
   'Boarded') onto that shuttle SHALL remain registered as 'Boarded' in the System — the flag SHALL NOT
   alter their BoardedStatus.
5. WHEN an Admin opens an Incident Active TripInstance in the Admin Portal, THE Admin Portal SHALL
   present an incident resolution form with options to: (a) assign a replacement ShuttleId, or
   (b) mark the affected allocation as cancelled.
6. WHEN an Admin submits a replacement ShuttleId for an Incident Active TripInstance, THE System SHALL
   update TripShuttleAllocation.ShuttleId to the replacement, set IsOperational = TRUE, recalculate
   Total Capacity upward, and set TripInstance.Status back to the appropriate pre-incident status.
7. WHEN an Admin marks the incident as cancelled (no replacement available), THE System SHALL keep
   TripShuttleAllocation.IsOperational = FALSE and record the cancellation reason.
8. WHEN an Admin completes the incident resolution (either replacement assigned or trip cancelled),
   THE System SHALL dispatch FCM push notifications to all Confirmed Booking holders for the affected
   TripInstance with details of the resolution (replacement vehicle info or cancellation notice).
9. THE System SHALL NOT dispatch any push notification to Students at the time the Marshal flags the
   breakdown — notifications SHALL only be sent after Admin resolution.
10. THE System SHALL tally each breakdown and cancellation event against both the specific Shuttle record
    and the Shuttle's VendorCompanyName for analytics reporting purposes.
11. FOR ALL breakdown flags, the Total Capacity after flagging SHALL equal the Total Capacity before
    flagging minus the flagged Shuttle's Capacity (capacity reduction invariant).
12. FOR ALL replacement overwrites, the Total Capacity after overwrite SHALL reflect the replacement
    Shuttle's Capacity in place of the removed Shuttle's Capacity (capacity replacement invariant).
13. FOR ALL Students with BoardedStatus = 'Boarded' on a flagged shuttle, their BoardedStatus SHALL
    remain 'Boarded' after the flag is applied (boarded persistence invariant).

---

### Requirement 7: Student Booking Management

**User Story:** As a Student, I want to create, view, and cancel my shuttle bookings through the mobile
app, so that I can manage my travel between campus and my off-campus residence.

#### Acceptance Criteria

1. WHEN a Student opens the booking form, THE App SHALL present a direction selector (To Campus /
   From Campus), an off-campus Stop search field, an on-campus Campus selector, a 4-day date picker
   showing today through today + 3 days, and a departure time selector filtered by the selected date.
2. WHEN a Student selects a departure time that has available capacity, THE App SHALL display a
   "Confirm Booking" call-to-action.
3. WHEN a Student selects a departure time where Total Capacity is at or above the Confirmed Booking
   count, THE App SHALL display a "Place on Waiting List" call-to-action and a capacity notice in
   place of the confirm button.
4. WHEN a Student confirms a booking, THE System SHALL create the Booking and return the Booking
   details including a generated QrCodeToken within 3 seconds under normal load.
5. WHEN a Booking is created, THE App SHALL navigate the Student to the Booking Details screen
   displaying the QR code, trip direction, Stop name, Campus, date, and departure time.
6. THE App SHALL present the Student's upcoming Bookings (those with a TripDate >= today and
   BookingStatus != 'Cancelled') on the Home screen Upcoming tab.
7. THE App SHALL present the Student's past Bookings (those with TripDate < today or
   BookingStatus = 'Cancelled') on the Home screen Past tab.
8. WHEN a Student cancels a Booking and the TripInstance.Status is 'Scheduling', THE System SHALL
   set BookingStatus = 'Cancelled' and return HTTP 200.
9. IF a Student attempts to cancel a Booking for a TripInstance with Status = 'Locked', 'In Transit',
   'Completed', or 'Incident Active', THEN THE System SHALL reject the cancellation with HTTP 400.
10. THE App SHALL display a floating action button (FAB) on the Home screen that navigates to the
    New Booking form.
11. THE App SHALL provide a bottom navigation bar with four items: Home, Maps, Alerts, and Profile.

---

### Requirement 8: Priority Schedule Management

**User Story:** As an Admin, I want to issue recurring travel permits to disability-cleared students,
so that their seats are reserved automatically before the general booking window opens.

#### Acceptance Criteria

1. THE System SHALL allow an Admin to create a PrioritySchedule record linking a Student, a Stop, a
   ScheduleTime, and a DayOfWeek.
2. WHEN a PrioritySchedule is created for a Student who does not have IsPriorityUser = TRUE, THE
   System SHALL automatically set Student.IsPriorityUser = TRUE.
3. THE System SHALL allow an Admin to deactivate a PrioritySchedule by setting IsActive = FALSE,
   after which the Rolling Injection Job SHALL no longer create Priority Bookings for that schedule.
4. WHEN the Rolling Injection Job runs and an active PrioritySchedule matches the TargetDate's
   DayOfWeek and the matching TripInstance exists, THE System SHALL create a Priority Booking even
   if a standard Booking for that Student on that TripInstance already exists (the Priority record
   replaces contention with pre-clearance).
5. WHEN a Priority Booking is auto-created by the Rolling Injection Job, THE System SHALL pre-assign
   the Booking to the shuttle with OrderPosition = 1 (the first, accessible shuttle in the convoy)
   so that disability-cleared Students are allocated to the front accessible vehicle.
6. FOR ALL Priority Bookings auto-created by the Rolling Injection Job, BookingType SHALL equal
   'Priority' and BookingStatus SHALL equal 'Confirmed' at creation (priority pre-clearance invariant).
7. FOR ALL Priority Bookings auto-created by the Rolling Injection Job, the creation of those bookings
   SHALL occur before any general student can access the TripInstance for booking (pre-clearance
   ordering invariant).
8. FOR ALL Priority Bookings auto-created by the Rolling Injection Job, the pre-assigned ShuttleId
   SHALL reference the Shuttle at OrderPosition = 1 in the convoy (first-shuttle assignment invariant).

---

### Requirement 9: Push Notification Delivery

**User Story:** As a Student, I want to receive push notifications about my bookings, so that I am
informed of waitlist promotions, vehicle changes, and trip incidents without opening the app.

#### Acceptance Criteria

1. WHEN a Booking transitions from Waiting List to Confirmed during the Waitlist Promotion Loop,
   THE System SHALL dispatch an FCM push notification to all DeviceRegistration tokens associated
   with that Student.
2. WHEN an Admin completes the incident resolution for a TripInstance (replacement shuttle assigned
   or trip cancelled), THE System SHALL dispatch an FCM push notification to all Students with
   Confirmed Bookings on that TripInstance containing the resolution details.
3. THE System SHALL NOT dispatch breakdown-related push notifications at the time a Marshal flags
   the incident — notifications SHALL only be sent after Admin resolution.
4. THE App SHALL display all Notification records for the authenticated Student in the Alerts screen,
   ordered by CreatedAt DESC.
5. WHEN a Student views a Notification, THE System SHALL update Notification.IsRead = TRUE.
6. THE App SHALL display an unread notification badge on the Alerts navigation item when the Student
   has one or more Notifications where IsRead = FALSE.
7. THE App SHALL present an Alert Detail screen when a Student taps a notification, showing the full
   Notification.Title and Notification.Message.
8. IF an FCM dispatch fails for one DeviceRegistration token, THEN THE System SHALL log the failure
   and continue dispatching to remaining tokens for the same Student without aborting the operation.

---

### Requirement 10: Student Grievance Reporting

**User Story:** As a Student, I want to file a grievance report about a trip or driver, so that the
university can investigate and improve the shuttle service.

#### Acceptance Criteria

1. THE App SHALL provide a Report Form accessible from the Student's Profile screen, containing a
   category selector (Driver Behavior, Timetable Issue, Vehicle Quality, Other), an optional
   TripInstance reference, and a free-text description field.
2. WHEN a Student submits a Report, THE System SHALL create a Report record with Status = 'Submitted',
   the Student's StudentId, the selected Category, the Description, and optionally the TripInstanceId.
3. THE App SHALL display all Reports submitted by the authenticated Student in the Profile screen's
   "My Reports" section, ordered by CreatedAt DESC.
4. IF a Student submits a Report with an empty Description, THEN THE System SHALL reject the request
   with HTTP 422.
5. WHEN an Admin updates a Report's Status to 'Under Review' or 'Resolved' and optionally adds
   AdminNotes, THE System SHALL persist the updated fields and set UpdatedAt = DateTime.UtcNow.
6. THE Admin Portal SHALL display all Reports in a paginated list with columns for report ID, category,
   student ID, submission date, and current status.
7. THE Admin Portal SHALL allow an Admin to view the full details of a Report including Description,
   AdminNotes, linked TripInstance, and status history.

---

### Requirement 11: Maps and Stop Discovery

**User Story:** As a Student, I want to see nearby shuttle stops on a map and get directions, so that
I can find and navigate to my boarding point.

#### Acceptance Criteria

1. THE App SHALL present a Maps screen displaying all active Stops as markers on an interactive map
   using each Stop's GpsCoordinates.
2. WHEN a Student taps a Stop marker on the map, THE App SHALL display a popup with the Stop's name
   and an option to get directions.
3. WHEN a Student selects "Get Directions" for a Stop, THE App SHALL launch the device's navigation
   intent with the Stop's GpsCoordinates as the destination.
4. THE App SHALL provide a search field on the Maps screen that filters the displayed Stop markers
   by StopName or FullAddress in real time.
5. THE App SHALL provide a "Nearby Stops" view on the Maps screen that lists Stops sorted by
   approximate proximity to the Student's current device location.
6. IF the Student has denied location permission, THE App SHALL display a permission prompt before
   attempting to use device GPS for the Nearby Stops view.

---

### Requirement 12: Student Profile

**User Story:** As a Student, I want to view my profile information and access my submitted reports,
so that I can verify my details and track my grievance history.

#### Acceptance Criteria

1. THE App SHALL display a Profile screen showing the authenticated Student's FirstName, LastName,
   StudentNumber, and UniversityEmail.
2. THE App SHALL display on the Profile screen a list of all Reports submitted by the Student,
   ordered by CreatedAt DESC, with each item showing the Category and current Status.
3. WHEN a Student taps a Report item on the Profile screen, THE App SHALL navigate to the Report
   detail view showing Category, Description, Status, and AdminNotes (if present).
4. THE App SHALL provide a logout action on the Profile screen that clears the local authentication
   token and returns the Student to the login screen.

---

### Requirement 13: Marshal Home Dashboard

**User Story:** As a Marshal, I want to see my assigned boarding stop and the remaining trip schedule
for my shift, so that I can efficiently manage boarding at my platform.

#### Acceptance Criteria

1. THE App SHALL display a Marshal Home screen (presented when a Marshal role is authenticated)
   showing the Marshal's assigned boarding location (Campus or Stop) and a chronological list of
   upcoming TripInstances scheduled at that location on the current date. For marshals assigned to
   a Campus, the screen SHALL show TripInstances with Direction = 'FromCampus' at that Campus.
   For marshals assigned to a Stop, the screen SHALL show TripInstances with Direction = 'ToCampus'
   at that Stop.
2. THE App SHALL display a "Next Boarding" card on the Marshal Home screen showing the departure time
   and status of the soonest TripInstance that is not yet Completed.
3. THE App SHALL present the remaining schedule as a list of departure times for TripInstances that
   have not yet reached Completed status.
4. THE App SHALL provide a prominent QR Code Scanner button on the Marshal Home screen that navigates
   to the scan interface.
5. THE App SHALL show a directions button on the Marshal Home screen that launches navigation to the
   Marshal's assigned location (Campus or Stop) using its GPS coordinates.

---

### Requirement 14: Marshal Passenger List and Check-In

**User Story:** As a Marshal, I want to view the passenger manifest for a trip and mark students as
boarded by scanning their QR code, so that I can maintain an accurate boarding record.

#### Acceptance Criteria

1. WHEN a Marshal selects a TripInstance from the schedule, THE App SHALL display a Passenger List
   screen showing all Confirmed Bookings for that TripInstance, including each Student's name and
   BoardedStatus.
2. THE App SHALL display real-time counts of total confirmed passengers, boarded passengers, and
   pending passengers on the Passenger List screen.
3. WHEN a Marshal taps the QR scanner button and scans a valid QR code, THE App SHALL submit a
   check-in request to the System with the decoded QrCodeToken and the Marshal's selected ShuttleId.
4. WHEN the check-in request succeeds, THE App SHALL update the scanned Student's status to "Boarded"
   in the Passenger List without requiring a full screen refresh.
5. IF the QR scanner returns a token that fails check-in validation, THE App SHALL display an inline
   error message on the scan screen indicating the reason (not confirmed, wrong shuttle, already boarded).
6. THE App SHALL allow a Marshal to flag a shuttle as "not arrived" or "broken down" from the
   Passenger List or trip detail screen with a single confirmation action, triggering the Vehicle
   Breakdown flow without requiring the Marshal to enter replacement details.

---

### Requirement 15: Admin Authentication

**User Story:** As an Admin, I want a secure login to the Admin Portal, so that only authorised
university staff can access fleet management and user data.

#### Acceptance Criteria

1. THE Admin Portal SHALL present a login screen with an email address field and a password field.
2. WHEN an Admin submits valid credentials, THE System SHALL issue an authenticated session token
   and redirect the Admin to the Operations Overview dashboard.
3. IF an Admin submits invalid credentials, THEN THE System SHALL return HTTP 401 and display an
   inline error message without disclosing whether the email or password was incorrect.
4. WHEN an Admin session token expires, THE Admin Portal SHALL redirect the Admin to the login
   screen with an appropriate session-expiry message.
5. THE Admin Portal SHALL provide a logout action accessible from the sidebar navigation that
   invalidates the session token and returns the Admin to the login screen.

---

### Requirement 16: Admin Operations Overview Dashboard

**User Story:** As an Admin, I want a high-level dashboard showing demand analytics and report status,
so that I can quickly assess the health of the shuttle network.

#### Acceptance Criteria

1. THE Admin Portal SHALL display an Operations Overview dashboard as the default landing page after
   login, accessible from the "Overview" sidebar navigation item.
2. THE Dashboard SHALL display a "Top 3 Most In-Demand Stops" panel showing the three Stops with
   the highest total Confirmed Booking counts, with relative progress bars.
3. THE Dashboard SHALL display a "Top 3 Least In-Demand Stops" panel showing the three Stops with
   the lowest total Confirmed Booking counts, with relative progress bars.
4. THE Dashboard SHALL display a Reports Summary panel showing the count of Reports in each of the
   three statuses: Submitted, Under Review, and Resolved.
5. THE Dashboard SHALL display a "Breakdown & Cancellation Analytics" panel showing the count of
   breakdowns and cancellations tallied per individual Shuttle and per VendorCompanyName, with the
   ability to view trends over a configurable date range.
6. THE Dashboard SHALL provide a "View Detailed Analytics" button that navigates to a detailed
   analytics view with expanded booking, demand, and reliability statistics.
7. THE Dashboard SHALL use a persistent sidebar navigation with links to: Overview, Shuttle Stops,
   Shuttles, Users, and Reports.

---

### Requirement 17: Admin Stop Management

**User Story:** As an Admin, I want to create, view, and edit shuttle stops, so that the System always
reflects the university's current set of off-campus residential destinations.

#### Acceptance Criteria

1. THE Admin Portal SHALL display a Stops management screen listing all Stop records with columns for
   StopName, FullAddress, and GpsCoordinates.
2. THE Admin Portal SHALL provide a "New Stop" form with fields for StopName, FullAddress, and
   GpsCoordinates, with a submit action that creates the Stop record.
3. IF a Stop is submitted with a StopName that already exists, THEN THE System SHALL reject the
   request with HTTP 409 and display a uniqueness error in the form.
4. THE Admin Portal SHALL provide an "Edit Stop" form pre-filled with the existing StopName,
   FullAddress, and GpsCoordinates, with a submit action that updates the record.
5. FOR ALL Stop records, StopName SHALL be non-empty, FullAddress SHALL be non-empty, and
   GpsCoordinates SHALL be a non-empty string (completeness invariant).
6. WHEN a Stop is deleted and one or more TripInstances reference that Stop, THE System SHALL reject
   the deletion with HTTP 409 and return a descriptive error.

---

### Requirement 18: Admin Shuttle Fleet Management

**User Story:** As an Admin, I want to register, view, and edit shuttle vehicles, so that the fleet
record is always accurate for capacity calculations and allocation.

#### Acceptance Criteria

1. THE Admin Portal SHALL display a Shuttles management screen listing all Shuttle records with columns
   for LicensePlate, Capacity, VendorCompanyName, and IsAccessibleVehicle.
2. THE Admin Portal SHALL provide a "New Shuttle" form with fields for LicensePlate, Capacity,
   VendorCompanyName, and an IsAccessibleVehicle toggle, with a submit action that creates the Shuttle.
3. IF a Shuttle is submitted with a LicensePlate that already exists, THEN THE System SHALL reject the
   request with HTTP 409.
4. THE Admin Portal SHALL provide an "Edit Shuttle" form pre-filled with existing values, with a
   submit action that updates the record.
5. WHEN a Shuttle's Capacity is updated and that Shuttle is part of one or more active
   TripShuttleAllocations with IsOperational = TRUE, THE System SHALL recalculate Total Capacity for
   all affected TripInstances immediately.
6. FOR ALL Shuttle records, Capacity SHALL be a positive integer greater than zero (capacity validity
   invariant).

---

### Requirement 19: Admin User Management — Individual Add

**User Story:** As an Admin, I want to add individual student, marshal, or admin user accounts, so
that I can provision system access for new university members.

#### Acceptance Criteria

1. THE Admin Portal SHALL display a Users management screen listing all users (Students, Marshals,
   Admins) in a searchable, paginated table with columns for identifier, name, role, email, and status.
2. THE Admin Portal SHALL provide role-based filter buttons (All, Students, Marshals, Admins) on the
   Users management screen.
3. THE Admin Portal SHALL provide a search field that filters the user list by name, identifier, or
   email in real time.
4. THE Admin Portal SHALL provide an "Add Single User" form that accepts role (Student / Marshal /
   Admin), identifier (StudentNumber or StaffNumber), FirstName, LastName, and UniversityEmail.
5. WHEN a new Student user is submitted, THE System SHALL validate that the StudentNumber matches
   the format `^s3[12]\d{7}$` (letter "s" followed by 9 digits starting with 31 or 32) and reject
   with HTTP 422 if the format is invalid.
6. WHEN a new Marshal user is submitted, THE System SHALL validate that the StaffNumber matches
   the format `^\d{7}$` (exactly 7 digits) and reject with HTTP 422 if the format is invalid.
7. WHEN a new user is submitted with a UniversityEmail that already exists, THE System SHALL reject
   the request with HTTP 409.
8. WHEN a new user is submitted with a StudentNumber or StaffNumber that already exists, THE System
   SHALL reject the request with HTTP 409.
9. THE Admin Portal SHALL allow an Admin to edit a user's FirstName, LastName, UniversityEmail, and
   IsPriorityUser flag via an edit form.

---

### Requirement 20: Admin User Management — Bulk CSV Upload

**User Story:** As an Admin, I want to upload a CSV file to provision multiple users at once, so that
I can efficiently onboard large cohorts at the start of an academic year.

#### Acceptance Criteria

1. THE Admin Portal SHALL provide a "Upload CSV" interface on the Users management screen with a
   drag-and-drop zone and a file browser fallback, accepting files with a .csv extension only.
2. THE System SHALL enforce a maximum CSV file size of 10 MB.
3. WHEN a CSV file is uploaded, THE System SHALL parse each row and validate that each row contains
   a role, identifier, FirstName, LastName, and UniversityEmail before processing.
4. WHEN a CSV file is uploaded, THE System SHALL validate that each Student identifier matches the
   format `^s3[12]\d{7}$` and each Marshal identifier matches `^\d{7}$`, skipping rows with invalid
   formats and recording them as format errors.
5. IF any row in the CSV contains a duplicate UniversityEmail or identifier (either within the file
   or against existing records), THE System SHALL skip that row, record it as an error, and continue
   processing the remaining rows.
6. WHEN processing completes, THE System SHALL return a summary of successfully created records and
   a list of skipped rows with their error reasons.
7. THE Admin Portal SHALL display a confirmation screen after upload showing the count of successfully
   added users and the list of failed rows with error reasons.
8. WHEN a CSV is uploaded, THE System SHALL not overwrite any existing user record.
9. FOR ALL users created via CSV upload, the same uniqueness and format rules as individual add SHALL
   apply (email uniqueness, identifier uniqueness, identifier format validation) — (idempotent
   bulk-add invariant).

---

### Requirement 21: Schedule and Calendar Exception Management

**User Story:** As an Admin, I want to manage the master timetable and define blackout dates, so that
the system generates the correct trips and skips service interruptions automatically.

#### Acceptance Criteria

1. THE Admin Portal SHALL allow an Admin to create ScheduleTime records specifying DepartureTime,
   Direction (To Campus / From Campus), and ScheduleType (Weekday / Saturday).
2. THE Admin Portal SHALL allow an Admin to edit or delete ScheduleTime records that have no future
   TripInstance references.
3. THE Admin Portal SHALL allow an Admin to create CalendarException records specifying TargetDate,
   Type (Public Holiday / University Recess), and Description.
4. IF a CalendarException is created for a TargetDate that already exists in the CalendarException
   table, THE System SHALL reject the request with HTTP 409.
5. THE Admin Portal SHALL allow an Admin to delete a CalendarException record, after which the
   Rolling Injection Job SHALL treat that date as a normal service day on the next nightly run.
6. THE Admin Portal SHALL display the list of all CalendarExceptions in date order.
7. FOR ALL CalendarException records, TargetDate SHALL be unique (no duplicate blackout date invariant).

---

### Requirement 22: Trip Instance State Machine

**User Story:** As an Admin, I want TripInstances to follow a well-defined lifecycle, so that the
System enforces correct state transitions and prevents invalid operations.

#### Acceptance Criteria

1. THE System SHALL enforce the following valid TripInstance status transitions only:
   Scheduling → Locked, Locked → In Transit, In Transit → Completed,
   In Transit → Incident Active, Incident Active → In Transit, and Scheduling → Incident Active.
2. IF any API action attempts to transition a TripInstance to a status not listed in the valid
   transitions above, THEN THE System SHALL reject the request with HTTP 422.
3. WHEN a TripInstance transitions to 'In Transit', THE System SHALL record the transition timestamp.
4. WHEN a TripInstance transitions to 'Completed', THE System SHALL set BoardedStatus = 'No Show'
   for all Bookings on that TripInstance where BoardedStatus is still 'Pending'.
5. FOR ALL TripInstances, the Status SHALL follow the defined state machine — no TripInstance SHALL
   transition directly from Scheduling to Completed without passing through Locked and In Transit
   (state machine completeness invariant).

---

### Requirement 23: QR Code Token Generation and Serialization

**User Story:** As the System, I want each Booking to carry a unique, verifiable QR code token, so
that check-in can be performed securely without exposing internal database identifiers.

#### Acceptance Criteria

1. WHEN a Booking is created, THE System SHALL generate a QrCodeToken that is globally unique across
   all Booking records and at least 32 characters in length using a cryptographically random generator.
2. THE System SHALL serialize the QrCodeToken into a scannable QR code image served to the Student
   App for display on the Booking Details screen.
3. THE Booking Details screen SHALL display the QR code image at a minimum rendered size of 256×256
   logical pixels to ensure reliable scanning.
4. THE App SHALL provide a "Download QR Code" action that saves the QR code image to the device.
5. FOR ALL QrCodeTokens generated by the System, parsing the QR code image produced from a token
   SHALL recover the original token value (round-trip property: encode → scan → decode = original token).
6. FOR ALL pairs of distinct Bookings, their QrCodeTokens SHALL be distinct (global uniqueness invariant).

---

### Requirement 24: GPS Coordinate Parsing and Formatting

**User Story:** As the System, I want to reliably parse and format GPS coordinates for Stops, so
that map features display correct locations and data round-trips without corruption.

#### Acceptance Criteria

1. THE System SHALL accept GpsCoordinates in decimal degree format "latitude,longitude" (e.g.,
   "-33.9599,25.6022") when creating or updating a Stop.
2. IF GpsCoordinates are submitted in a format that cannot be parsed into a valid latitude (−90 to +90)
   and longitude (−180 to +180) pair, THEN THE System SHALL reject the request with HTTP 422 and a
   descriptive error.
3. THE System SHALL format stored GpsCoordinates back into the canonical "latitude,longitude" string
   when returning Stop data in API responses.
4. FOR ALL valid GpsCoordinate strings accepted by the System, parsing the string and then re-formatting
   it SHALL produce a string that parses to the same latitude and longitude values (round-trip property).
5. FOR ALL latitude values l, l SHALL satisfy −90 ≤ l ≤ 90; for all longitude values g, g SHALL
   satisfy −180 ≤ g ≤ 180 (coordinate range invariant).

---

### Requirement 25: Booking Status Lifecycle Invariants

**User Story:** As the System, I want Booking status to follow consistent rules across all transitions,
so that data integrity is guaranteed and downstream processes (check-in, manifest, reporting) are reliable.

#### Acceptance Criteria

1. THE System SHALL enforce that a Booking's BookingStatus can only transition as follows:
   Confirmed → Cancelled (by student, while TripInstance.Status = Scheduling),
   Waiting List → Confirmed (by Waitlist Promotion Loop),
   Waiting List → Cancelled (by student, while TripInstance.Status = Scheduling),
   and no other transitions.
2. FOR ALL Bookings, BookingStatus SHALL NOT be set to a value outside ('Confirmed', 'Waiting List',
   'Cancelled') (status domain invariant).
3. FOR ALL Bookings with BookingStatus = 'Cancelled', BoardedStatus SHALL remain 'Pending' and
   SHALL NOT be changed to 'Boarded' or 'No Show' (cancelled booking boarding invariant).
4. FOR ALL Bookings with BookingType = 'Priority', the Booking SHALL have been created by the
   Rolling Injection Job — no API endpoint SHALL allow a client to set BookingType = 'Priority'
   directly (priority booking source invariant).
5. WHEN a TripInstance is Completed, the total count of Bookings with BoardedStatus = 'Boarded' plus
   BoardedStatus = 'No Show' SHALL equal the total count of Bookings with BookingStatus = 'Confirmed'
   for that TripInstance (completion accounting invariant).

---

### Requirement 26: API Input Validation and Error Handling

**User Story:** As a developer integrating with the System, I want all API endpoints to validate inputs
and return structured errors, so that client applications can handle failures gracefully without crashing.

#### Acceptance Criteria

1. WHEN any required field in an API request body is missing, THE System SHALL return HTTP 422 with a
   JSON error body identifying each missing field by name.
2. WHEN an API request references an entity by ID that does not exist (e.g., non-existent StudentId,
   TripInstanceId, ShuttleId), THE System SHALL return HTTP 404 with a descriptive message.
3. THE System SHALL return all error responses as JSON objects containing at least a "message" field
   and an HTTP status code consistent with the error type.
4. IF an unauthenticated request is made to any endpoint that requires authentication, THEN THE
   System SHALL return HTTP 401.
5. IF an authenticated Student attempts to access Marshal-only or Admin-only endpoints, THEN THE
   System SHALL return HTTP 403.
6. IF an authenticated Marshal attempts to access Admin-only endpoints, THEN THE System SHALL return
   HTTP 403.
7. FOR ALL API error responses, the HTTP status code in the response body SHALL match the HTTP
   status code in the response header (consistency invariant).

---

### Requirement 27: Convoy Ordering and Sequential Shuttle Fill

**User Story:** As an Admin, I want shuttles in a convoy to be arranged in a defined front-to-back order
with accessible vehicles placed first, so that students are assigned sequentially and priority students
board the front accessible shuttle.

#### Acceptance Criteria

1. THE System SHALL store an OrderPosition (integer, 1-based) on each TripShuttleAllocation record,
   defining the shuttle's front-to-back position within the convoy.
2. WHEN TripShuttleAllocations are created for a TripInstance, THE System SHALL assign accessible
   shuttles (IsAccessibleVehicle = TRUE) to the lowest OrderPosition values (front of convoy).
3. WHEN a Student is checked in via QR scan, THE System SHALL assign the Student to the operational
   shuttle with the lowest OrderPosition that has not yet reached its Capacity, filling shuttles
   sequentially from front to back.
4. WHEN all operational shuttles in a convoy have reached capacity, THE System SHALL reject further
   check-in attempts with HTTP 422 and a message indicating the convoy is full.
5. FOR ALL TripShuttleAllocations on a given TripInstance, OrderPosition values SHALL be unique and
   contiguous starting from 1 (ordering uniqueness invariant).
6. FOR ALL convoys, every TripShuttleAllocation where Shuttle.IsAccessibleVehicle = TRUE SHALL have
   an OrderPosition less than or equal to every TripShuttleAllocation where
   Shuttle.IsAccessibleVehicle = FALSE (accessible-first ordering invariant).
7. FOR ALL check-in sequences on a TripInstance, a Student SHALL NOT be assigned to a shuttle at
   OrderPosition N+1 while the shuttle at OrderPosition N is operational and has remaining capacity
   (sequential fill invariant).

---

## Correctness Properties for Property-Based Testing

The following properties are derived from the acceptance criteria above. Each property identifies a
testable invariant, round-trip, or metamorphic relationship that the System must satisfy for any valid
input within the domain.

---

### P1 — Convoy Capacity Summation (Invariant)

**Source:** Requirements 3, 6, 18

```
For any TripInstance T with a set of TripShuttleAllocations A:
  TotalCapacity(T) == SUM { a.Shuttle.Capacity | a ∈ A, a.IsOperational == TRUE }
```

**Test approach:** Generate arbitrary convoys with N shuttles having random capacities; toggle random
subsets to IsOperational = FALSE; assert Total Capacity always equals the sum of operational capacities.

---

### P2 — Confirmed Booking Count Never Exceeds Total Capacity (Invariant)

**Source:** Requirement 3.6

```
For any TripInstance T, at the moment each Booking is created:
  COUNT(Bookings where Status == Confirmed) <= TotalCapacity(T)
```

**Test approach:** Simulate concurrent booking requests up to and beyond capacity on generated
TripInstances; assert that no Confirmed Booking is ever created when the confirmed count would exceed
Total Capacity; assert that excess bookings receive Waiting List status.

---

### P3 — OTP Single-Use Property (Round-Trip / Idempotence)

**Source:** Requirement 1.8

```
For any valid OTP code C generated for user U:
  Verify(U, C) == Success
  Verify(U, C) == Failure  (second use of same code)
```

**Test approach:** Generate OTP, verify once (expect success), verify again with same code (expect
rejection). Run for many generated user identities.

---

### P4 — OTP Expiry Property (Error Condition)

**Source:** Requirement 1.4

```
For any OTP code C generated at time T with expiry window W:
  Verify(U, C, at time T + W + ε) == Failure for any ε > 0
  Verify(U, C, at time T + W - ε) == Success for any ε > 0 (within window)
```

**Test approach:** Generate OTPs; simulate verification at times straddling the 5-minute boundary;
assert acceptance inside and rejection outside the window.

---

### P5 — Rolling Injection Date Binding (Invariant)

**Source:** Requirement 2.9, 2.10

```
For any execution of the Rolling Injection Job on date D:
  All TripInstances generated.TripDate == D + 3
  All Priority Bookings created.TripInstance.TripDate == D + 3
```

**Test approach:** Run the injection logic for many random seed dates; assert all generated entities
carry the correct TargetDate.

---

### P6 — Calendar Exception Skip Property (Metamorphic)

**Source:** Requirement 2.2

```
For any date D that exists in CalendarException:
  COUNT(TripInstances generated for D) == 0
For any date D that does not exist in CalendarException and is not Sunday:
  COUNT(TripInstances generated for D) > 0
```

**Test approach:** Generate sets of dates with random CalendarException memberships; run the injection
logic; assert the count of TripInstances per date matches the exception table.

---

### P7 — Waitlist Promotion FIFO Ordering (Invariant)

**Source:** Requirement 4.7

```
For any set of Waiting List Bookings W for TripInstance T, sorted by CreatedAt ASC:
  When k slots become available, the first k bookings in W (ordered by CreatedAt) are promoted.
  No Booking at position i is promoted while a Booking at position j < i remains on Waiting List.
```

**Test approach:** Create arbitrary Waiting List queues with known CreatedAt values; trigger the
Waitlist Promotion Loop with varying available slot counts; assert the promoted set is always a
prefix of the FIFO-ordered queue.

---

### P8 — QR Token Round-Trip (Round-Trip)

**Source:** Requirement 23.5

```
For any QrCodeToken T:
  Decode(Encode(T)) == T
```

**Test approach:** Generate random token strings of valid length; encode to QR image bytes; decode
back to string; assert equality.

---

### P9 — QR Token Global Uniqueness (Invariant)

**Source:** Requirement 23.6

```
For any two distinct Booking IDs B1 and B2:
  B1.QrCodeToken != B2.QrCodeToken
```

**Test approach:** Generate large batches of tokens using the production token generator; assert no
collisions across thousands of generated tokens (birthday-paradox coverage).

---

### P10 — GPS Coordinate Round-Trip (Round-Trip)

**Source:** Requirement 24.4

```
For any valid GpsCoordinate string S:
  Format(Parse(S)) parses to the same (latitude, longitude) as S
```

**Test approach:** Generate random valid lat/lon pairs; format to string; parse back; format again;
assert the two formatted strings parse to equal coordinate pairs.

---

### P11 — GPS Coordinate Range Validation (Error Condition)

**Source:** Requirement 24.5

```
For any latitude l outside [-90, 90] or longitude g outside [-180, 180]:
  Parse(l, g) == Error
```

**Test approach:** Generate out-of-range coordinate pairs; submit via the Stop API; assert HTTP 422.

---

### P12 — Breakdown Capacity Reduction (Invariant)

**Source:** Requirement 6.11

```
For any TripInstance T and shuttle S in T's convoy:
  TotalCapacity(T after flagging S as IsOperational=FALSE)
    == TotalCapacity(T before) - S.Capacity
```

**Test approach:** Generate convoys with random shuttle sets; flag random subsets as broken; assert
capacity delta equals the sum of flagged shuttle capacities.

---

### P13 — Standby Overwrite Capacity Replacement (Invariant)

**Source:** Requirement 6.12

```
For any TripInstance T replacing shuttle S_old with S_new:
  TotalCapacity(T after overwrite)
    == TotalCapacity(T before) - S_old.Capacity + S_new.Capacity
```

**Test approach:** Generate allocation sets; simulate replacement with random capacity shuttles;
assert the net capacity change equals S_new.Capacity - S_old.Capacity.

---

### P14 — Booking Status Domain Invariant

**Source:** Requirement 25.2

```
For any Booking B at any point in its lifecycle:
  B.BookingStatus ∈ {'Confirmed', 'Waiting List', 'Cancelled'}
  B.BoardedStatus ∈ {'Pending', 'Boarded', 'No Show'}
  B.BookingType   ∈ {'Standard', 'Priority'}
```

**Test approach:** Apply arbitrary sequences of booking operations (create, cancel, promote, check-in,
complete trip); assert enum values never escape their defined domains.

---

### P15 — Completion Accounting Invariant

**Source:** Requirement 25.5

```
For any Completed TripInstance T:
  COUNT(Bookings.BoardedStatus == 'Boarded') + COUNT(Bookings.BoardedStatus == 'No Show')
    == COUNT(Bookings.BookingStatus == 'Confirmed')
```

**Test approach:** Simulate full trip lifecycles with random boarding percentages; assert the identity
holds after TripInstance is transitioned to Completed.

---

### P16 — Bulk CSV Idempotence (Idempotence)

**Source:** Requirement 20.9

```
For any CSV file F uploaded twice:
  Users created after upload 1 == Users created after upload 2
  (second upload creates no additional records; all duplicates are skipped)
```

**Test approach:** Generate random valid CSV files; upload once; record created user count; upload
the same file again; assert no additional users are created and the skip list contains all rows.

---

### P17 — API Error Status Code Consistency (Invariant)

**Source:** Requirement 26.7

```
For any error API response R:
  R.HTTP_Status_Header == R.body.status_code
```

**Test approach:** Generate requests designed to trigger all documented error paths (401, 403, 404,
409, 422); parse response headers and bodies; assert the status codes match.

---

### P18 — Scan Window Time-Gate (Error Condition)

**Source:** Requirement 5.2

```
For any TripInstance T with DepartureTime D and any check-in attempt at time t:
  IF t < D - 35 minutes THEN CheckIn(token, T) == Rejected (HTTP 422)
  IF t >= D - 35 minutes THEN CheckIn(token, T) proceeds to normal validation
```

**Test approach:** Generate TripInstances with random departure times; simulate check-in attempts
at various offsets from departure; assert rejection for attempts outside the 35-minute window and
acceptance within.

---

### P19 — Sequential Front-to-Back Shuttle Fill (Invariant)

**Source:** Requirement 27.7

```
For any TripInstance T with shuttles S1, S2, ..., Sn ordered by OrderPosition:
  No Booking is assigned to Si+1 while Si is operational and
  COUNT(Bookings assigned to Si) < Si.Capacity
```

**Test approach:** Generate convoys with multiple shuttles of varying capacities; simulate sequential
check-in streams; assert each shuttle fills to capacity before the next shuttle receives assignments.

---

### P20 — Accessible Shuttle Front Ordering (Invariant)

**Source:** Requirement 27.6

```
For any TripInstance T:
  MAX(OrderPosition where IsAccessibleVehicle = TRUE)
    <= MIN(OrderPosition where IsAccessibleVehicle = FALSE)
```

**Test approach:** Generate convoys with random mixes of accessible and non-accessible shuttles;
assert all accessible shuttles have lower OrderPosition values than non-accessible ones.

---

### P21 — Priority Booking First-Shuttle Assignment (Invariant)

**Source:** Requirement 8.8

```
For any Priority Booking B created by the Rolling Injection Job:
  B.ShuttleId == TripShuttleAllocation.ShuttleId WHERE OrderPosition = 1
```

**Test approach:** Run the injection job for various TripInstances with different convoy configurations;
assert all Priority Bookings are pre-assigned to the shuttle at OrderPosition 1.

---

### P22 — Boarded Persistence After Breakdown Flag (Invariant)

**Source:** Requirement 6.4, 6.13

```
For any Booking B with BoardedStatus = 'Boarded' assigned to shuttle S:
  After flagging S as IsOperational = FALSE:
    B.BoardedStatus == 'Boarded' (unchanged)
```

**Test approach:** Create bookings, board students onto shuttles, then flag those shuttles; assert
BoardedStatus remains 'Boarded' for all previously boarded students.

---

### P23 — Student Number Format Validation (Error Condition)

**Source:** Requirements 1.6, 19.5, 20.4

```
For any identifier I submitted as a StudentNumber:
  IF I does not match ^s3[12]\d{7}$ THEN System rejects with HTTP 422
  IF I matches ^s3[12]\d{7}$ THEN System proceeds with normal validation
```

**Test approach:** Generate strings that violate the pattern (wrong prefix, wrong length, non-numeric
suffixes) and valid strings; assert rejection for invalid and acceptance for valid formats.

---

### P24 — Staff Number Format Validation (Error Condition)

**Source:** Requirements 1.7, 19.6, 20.4

```
For any identifier I submitted as a StaffNumber:
  IF I does not match ^\d{7}$ THEN System rejects with HTTP 422
  IF I matches ^\d{7}$ THEN System proceeds with normal validation
```

**Test approach:** Generate strings of varying lengths, with non-numeric characters, and valid 7-digit
strings; assert rejection for invalid and acceptance for valid formats.

---

### P25 — Breakdown Notification Deferred Until Admin Action (Metamorphic)

**Source:** Requirement 6.8, 6.9, 9.3

```
For any breakdown flag event at time T_flag and admin resolution at time T_resolve:
  COUNT(notifications dispatched at T_flag) == 0
  COUNT(notifications dispatched at T_resolve) > 0
```

**Test approach:** Simulate breakdown flag events; assert no notifications are created; then simulate
admin resolution; assert notifications are dispatched to all affected students.
