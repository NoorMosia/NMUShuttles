# 1. System Overview — NMU Shuttle Management System

---

## 1.1 Problem Description

Nelson Mandela University operates a network of shuttle buses that transport students
between off-campus residential stops and university campuses in Gqeberha, South Africa.
The current unmanaged system creates several compounding problems:

- **Safety risk at unsociable hours.** Students queue at off-campus stops early in the
  morning with no guarantee of boarding, creating physical safety exposure.
- **Capacity is invisible.** There is no mechanism for students to know whether seats
  are available before they travel to the stop, resulting in missed trips and wasted
  journeys.
- **Overcrowding at high-demand stops.** Popular stops consistently reach or exceed
  shuttle capacity while low-demand routes run underutilised vehicles.
- **Accessibility is reactive, not guaranteed.** Students with mobility disabilities have
  no reliable way to secure accessible seating in advance.
- **No operational visibility for marshals or administrators.** Field staff cannot see
  who is expected, and administrators lack the data to make allocation decisions.

### Core Solutions the System Provides

1. Students reserve seats digitally up to 3 days in advance, eliminating the need to
   queue physically.
2. Automated trip generation and convoy allocation run nightly, ensuring every
   scheduled trip has a confirmed shuttle roster before students can book.
3. Priority disability accommodation is pre-booked automatically — accessible shuttles
   are always positioned at the front of every convoy.
4. Marshals verify boarding with QR-code scanning at the stop, replacing manual
   head-counting.
5. Administrators operate a web portal that provides full CRUD control over stops,
   shuttles, schedules, and users, plus vendor reliability analytics.

---

## 1.2 Business Rules & System Assumptions

### Authentication & Access

| Rule | Detail |
|------|--------|
| Student and Marshal login | One-time password (OTP) delivered to the user's NMU university email. No stored password. |
| Admin login | Email and BCrypt-hashed password via the web portal only. |
| Role assignment | Three roles exist: **Student**, **Marshal**, **Admin**. The mobile app switches role context from the JWT claims; no separate app is needed for marshals. |
| Provisioning | Administrators create and manage all user accounts. Students are bulk-imported via CSV. |

### Trip Generation & Scheduling

| Rule | Detail |
|------|--------|
| Rolling window | The system generates trip instances exactly **3 days ahead** at midnight SAST every day. |
| Skip days | Sundays, public holidays, and university recesses (stored as `CalendarException` records) are skipped — no trips are generated for those dates. |
| Schedule types | Weekday and Saturday schedules are supported. The schedule type is determined by the day of the week of the target date. |
| Convoy default | Each stop has a default shuttle roster (`StopShuttleDefault`). When a trip is generated, shuttles are automatically allocated from this default, preserving their convoy order position. Accessible shuttles always hold the lowest order positions (front of convoy). |

### Bookings & Waitlists

| Rule | Detail |
|------|--------|
| Booking window | Students may book from the moment a trip is generated (3 days ahead) until the trip is locked. |
| Lock time | A trip locks exactly **60 minutes before departure**. After locking, no new bookings or cancellations are accepted. |
| Capacity | Total trip capacity is the **sum of capacities of all operational shuttles** allocated to that trip — not a single fixed number. |
| Waitlist | When confirmed seat count reaches total capacity, further bookings receive `BookingStatus = "Waitlisted"` in FIFO order. |
| Waitlist promotion | At lock time, the `LockoutGateService` promotes waitlisted students in `CreatedAt` order until capacity is filled. Each promoted student receives a push notification. |
| One active booking | A student may hold only one confirmed or waitlisted booking per trip instance. |
| Priority bookings | Students flagged `IsPriorityUser = true` receive automatic `Priority` type bookings injected by the nightly `RollingInjectionService`. These are pre-assigned to the front (accessible) shuttle. |

### Tickets & Boarding

| Rule | Detail |
|------|--------|
| QR token | A unique, single-use token (`QrCodeToken`) is generated at booking confirmation. |
| Scanning opens | The marshal's scanner activates **35 minutes before departure**. |
| Shuttle assignment | Students are assigned to a specific shuttle at check-in time by `CheckInService`, filling the convoy from the front. |
| Priority pre-assignment | Priority bookings are pre-assigned to the shuttle at `OrderPosition = 1` at injection time. |
| Invalid scans | If a token does not match a confirmed booking for the active trip, the marshal's app displays a rejection response. |

### Breakdown Handling

| Rule | Detail |
|------|--------|
| Flagging | A marshal flags a breakdown for a specific shuttle on an active trip. This marks the allocation `IsOperational = false` and creates a `BreakdownLog`. |
| Resolution | An admin resolves the breakdown by either replacing the shuttle or cancelling it. Push notifications are sent to all confirmed booking holders only after admin resolution. |
| Analytics | Every breakdown is logged against the vendor (`VendorCompanyName` denormalised at flag time) for reliability analytics. |

### Payments & Infrastructure

| Rule | Detail |
|------|--------|
| No in-app payments | The shuttle service is university-funded. No monetary transactions occur in the system. |
| University-managed data | Stop locations, schedules, shuttle rosters, and eligible student lists are all managed by NMU administrators within the system. |

---

## 1.3 System Constraints

| Constraint | Detail |
|------------|--------|
| Platform | Android mobile devices only (Flutter). No iOS build target. |
| Admin portal | Flutter Web — browser-based, no mobile build. |
| Connectivity | Active internet connection required for bookings, QR scanning, and notifications. |
| Time zone | All scheduling logic operates in SAST (UTC+2). All database timestamps are stored in UTC. |
| Auth | JWT Bearer tokens. All routes except OTP request/verify and admin login require a valid token. |
| Push notifications | Firebase Cloud Messaging (FCM) is used for all push notifications to student and marshal devices. |
| Email (OTP delivery) | Azure Communication Services Email is the exclusive OTP delivery channel. |
