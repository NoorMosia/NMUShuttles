# Changes from Initial Submission — Critical Review

This document compares the four files in `FSSB/initial/` against the four revised
files in `FSSB/kiros/`. It explains what was wrong, what was missing, and why each
change was made. The revised documents are grounded in the actual implemented
codebase rather than an early-design approximation.

---

## Document 1 — System Overview

### What the initial got right

The core problem statement was accurate: unsafe queueing, capacity invisibility,
and overcrowding at popular stops. The no-payment assumption was correct. The
Android-only constraint was documented.

### What was wrong or incomplete

**1. Wrong system name.**
The initial document is titled "LATT Mobile." The actual implemented system is the
NMU Shuttle Management System. Using the wrong name in a specification document is
a credibility problem, regardless of whether it was the original working title.

**2. Booking window stated as "24 hours prior to departure."**
This is factually incorrect. The `RollingInjectionService` creates trips exactly
**3 days ahead** at midnight SAST. A student can book from the moment a trip is
generated — which is 72 hours before departure, not 24. Stating the wrong window
directly contradicts the implemented business logic and would cause a lecturer to
question whether the team understood their own system.

**3. "Drivers" listed as a provisioned user type.**
The system has no Driver entity, no Driver role, and no driver-facing screen
anywhere in the codebase. The actual provisioned roles are Student, Marshal, and
Admin. Listing a non-existent actor is not a minor oversight — it suggests the
specification was written without checking the implementation.

**4. Authentication described as password-based for students and marshals.**
The initial implies standard username/password login. In reality, students and
marshals authenticate exclusively via **OTP delivered to their NMU university
email** — there is no stored password for either of these roles. Only admins use a
BCrypt-hashed password. This is a significant architectural difference that affects
the entire auth model.

**5. Capacity treated as a single fixed number.**
The business rules imply capacity is a simple per-shuttle figure. The real system
calculates capacity dynamically as the **sum of all operational shuttles** allocated
to a trip via `TripShuttleAllocation`. This is the convoy model: a trip can have
multiple shuttles, and total capacity changes if a shuttle is marked non-operational
due to a breakdown. This model was absent from the initial entirely.

**6. Lockout trigger described imprecisely.**
The initial says "bookings close 1 hour before departure" and "automated promotions
run until 1 hour prior." Both are partially correct, but the mechanism is more
specific: the `LockoutGateService` executes a single **serialisable database
transaction** that locks the trip at T-60 minutes and runs waitlist promotion
**at that exact moment** as a post-commit step. Continuous promotion until lockout
is not how it works — the distinction matters for concurrency correctness.

**7. The following features were missing entirely:**
- The **CalendarException** mechanism — public holidays and university recesses are
  stored as records that cause the nightly job to skip trip generation for that date
- The **convoy model** and the rule that accessible shuttles always occupy the
  lowest `OrderPosition` values (front of convoy)
- **Priority users** (`IsPriorityUser = true`) and the automatic pre-booking
  injection that assigns them to the front shuttle at trip creation time
- The **two-phase breakdown workflow**: marshal flags → admin resolves → FCM push
  is sent only after admin resolution, not immediately when the marshal flags
- The **admin web portal** as a separate Flutter Web platform distinct from the
  mobile app
- **SAST timezone handling** — scheduling logic runs in UTC+2, all DB timestamps
  stored in UTC
- **Azure Communication Services** for OTP email delivery
- **Firebase Cloud Messaging** for push notifications to devices

---

## Document 2 — Use Cases

This document had the most significant structural problems of the four.

### What the initial got right

Student, Marshal, and Admin as the three primary actors was correct. The partial
recognition of a time trigger actor was a good instinct. The re-scan loop on
failed QR validation was correctly identified in the notes.

### Problem 1 — Numbering based on developer names, not domain logic

The initial split use case IDs by team member:
A = Lusanda, B = Letlhogonolo, C = Anele, D = Tshepo.

This means use cases for the same actor scatter across all four letter ranges.
"Book Shuttle" is D400, "View Booking" is A200, and "Cancel Booking" is A400 —
all student use cases with three different prefixes. The ID should tell you which
actor and which domain the use case belongs to at a glance. The revised scheme
uses actor prefixes (S, M, A, T) and groups hundreds digits by domain:

| Prefix | Actor | Range groups |
|--------|-------|-------------|
| S | Student | S1xx = trips/bookings, S2xx = notifications, S3xx = reports |
| M | Marshal | M1xx = scanning/boarding, M2xx = breakdowns |
| A | Admin | A1xx = entity management, A2xx = schedules, A3xx = trips, A4xx = incidents, A5xx = analytics |
| T | TIME | T1xx = trip generation, T2xx = lockout/promotion |

### Problem 2 — Login and Change Password included as use cases

The initial includes "Login (D200)" and "Change Password (D300)" as use cases in
the diagram and the team responsibilities table. Under the standard Perfect Technology
Assumption, login would be excluded as a technology control. However, **the lecturer
confirmed that authentication must be included in this system** because OTP-based
login is not a generic system control — it is the domain mechanism that verifies
university affiliation. Only holders of an active NMU university email can access
the system, and the OTP process triggers a real domain event: an email is delivered
to the user's university inbox. That email dispatch must be modelled as a use case.

"Change Password (D300)" is still correctly excluded — there are no passwords for
students or marshals in the implemented system (OTP only), and admin password
management is a system control that does not warrant a use case.

The revised document includes three auth use cases under the `U` prefix:
- **U100 — Request OTP** (user submits email, system generates and stores OTP)
- **U110 — Send OTP Email Notification** (state event, `<<include>>`d by U100, executed by Email Service actor)
- **U120 — Verify OTP and Log In** (user submits code, system issues JWT)

A `User` generalisation actor is introduced so that Student, Marshal, and Admin all
inherit the auth use cases without duplicating them three times. An `Email Service`
external system actor is added to represent Azure Communication Services as the
participant in U110.

### Problem 3 — "Load Student Data (B100)" included as a use case

Loading student data via CSV import is an implementation convenience for the admin,
but it is not a standalone use case under the event decomposition technique — it is
an optional extension of user management. In the revised document it becomes
**A140 — Import Students via CSV**, correctly modelled as an `<<extend>>` of
**A130 — Manage Users**. Manage Users is complete and meaningful without CSV import;
the import just provides an optional bulk path.

### Problem 4 — The TIME actor was underused and unnamed

The initial diagram has a subgraph called "System & Time Triggers" but does not
model TIME as a formal actor with its own connections. According to the school
notes, temporal events should produce a TIME actor on the use case diagram. The
two most architecturally important operations in the system — nightly trip
generation and the pre-departure lockout gate — were either missing or collapsed
into vague labels like "Re-Allocate Shuttle (C400)" and "Check Waiting List (A500)."

The revised document models TIME as a first-class actor with four use cases:
T100 (Generate Daily Trips), T110 (Inject Priority Bookings), T200 (Lock Trip and
Promote Waitlist), T210 (Send Waitlist Promotion Notifications).

### Problem 5 — No `<<include>>` / `<<extend>>` relationships in the diagram

The initial diagram uses plain directed arrows for everything. The school notes
dedicate an entire unit to `<<include>>` (mandatory, shared behaviour) and
`<<extend>>` (optional, conditional behaviour). None of these relationships were
modelled. The revised document adds eight explicitly justified relationships with
a rationale table explaining why each one is include vs extend.

### Problem 6 — No use case narratives

The initial document had no fully developed use case narratives — the format the
school notes describe in Unit 8 with Use Case ID, Business Actors, Pre-Conditions,
Triggers, Post-Conditions, Basic Flow, and Alternate Flow. The revised document
includes five fully developed narratives for the five most complex use cases:
S110 (Book Shuttle Seat), M110 (Scan QR Code), T100 (Generate Daily Trips),
T200 (Lock Trip and Promote Waitlist), and A400 (Resolve Shuttle Breakdown).

### Problem 7 — Several real use cases were missing entirely

Use cases absent from the initial that exist in the implemented system:
- View Boarding Pass (separate from View Booking — the QR code is a distinct action)
- Mark Notification as Read
- View Issue Report Status
- View Passenger Manifest (marshal)
- Flag Shuttle Breakdown (marshal) — the initial had no breakdown workflow at all
- Manage Vendors (admin)
- Manage Calendar Exceptions (admin)
- Manage Priority Schedules (admin)
- Manage Trip Convoy (admin)
- Add Trip Note (admin)
- View Vendor Analytics (admin)
- Inject Priority Bookings (TIME)
- Send Waitlist Promotion Notifications (TIME)

---

## Document 3 — Requirements and Class Modelling

### What the initial got right

The core concept of domain class modelling was applied. `Booking`, `Stop`, `Shuttle`,
`Student`, `Admin`, `Marshal`, `Notification`, and `IssueReport` were all correctly
identified as domain classes. The idea of an issue report with a category and status
was correct and matches the real `Report` entity.

### Problem 1 — 14 classes where there are 20

The initial identified 14 domain classes. The revised document identifies 20. The
six missing classes are not edge cases — they are central to how the system works:

| Missing class | Why it matters |
|---|---|
| `Vendor` | Shuttles are supplied by external vendors. Vendor reliability analytics are a core admin feature. Without this class, the analytics have no subject. |
| `Campus` | Shuttles travel to campuses, not just from stops. The multi-campus routing model (`ScheduleTimeCampus`) requires Campus as an independent class. |
| `TripShuttleAllocation` | This is the association class that resolves the many-to-many between `TripInstance` and `Shuttle`. It carries `orderPosition` and `isOperational` — without it, the convoy model cannot be expressed. |
| `StopShuttleDefault` | The standing convoy roster per stop. This is how the nightly job knows which shuttles to allocate to new trips. Omitting it means the automated allocation mechanism has no representation in the model. |
| `PrioritySchedule` | The recurring schedule that drives automatic pre-bookings for priority (disability-cleared) students. Without this class, the priority accommodation feature has no data model. |
| `CalendarException` | The mechanism that causes the nightly job to skip public holidays and university recesses. Without this class, the skip logic cannot be represented. |

Three more classes were also absent: `BreakdownLog`, `TripNote`, and
`DeviceRegistration`. Their absence meant the breakdown workflow, admin notes, and
push notification infrastructure had no domain representation.

### Problem 2 — User superclass inheritance was modelled but not implemented

The initial diagram shows `Student`, `Marshal`, and `Admin` as subclasses of a
`User` superclass. This is a reasonable early design choice, but it does not reflect
the actual implementation. In the codebase, these are three completely separate
tables with no shared base class. The reason is important: the three types have
very different associations. `Student` links to `Booking`, `PrioritySchedule`,
`Notification`, and `Report`. `Marshal` links to `BreakdownLog` and has a
Stop-or-Campus XOR assignment. `Admin` links to `BreakdownLog` (as resolver) and
`TripNote`. Modelling them as subclasses of User would hide these differences and
imply shared database behaviour that does not exist.

The revised document explicitly argues against the superclass and justifies why
independent classes are the correct model for this domain.

### Problem 3 — `Shuttle` had wrong attributes

The initial `Shuttle` class included `routeID` and `driverName` as attributes.
Neither exists on the real `Shuttle` entity. Shuttles are not permanently assigned
to a route — they are dynamically allocated to trips via `TripShuttleAllocation`,
which is precisely why that association class exists. `driverName` was a leftover
from the "Drivers as users" misconception. The real `Shuttle` has `licensePlate`,
`capacity`, `isAccessibleVehicle`, `isActive`, and a FK to `Vendor`.

### Problem 4 — `WaitingList` modelled as a separate class

The initial has a `WaitingList` class with `listID` and `stopID`. In the real
system there is no separate waitlist table. Waitlist state is a `BookingStatus`
value (`"Waitlisted"`) on the `Booking` record itself. The waitlist is a FIFO queue
derived from ordering `Booking` records by `CreatedAt` where `BookingStatus =
"Waitlisted"`. A separate `WaitingList` entity would introduce unnecessary
complexity and a synchronisation problem.

### Problem 5 — `Route` and `RouteStop` do not exist

The initial models a `Route` class (departure, destination) with a `RouteStop`
junction to `Stop`. The actual system does not have a `Route` entity. Routing is
expressed through `ScheduleTime` (which has a direction, a stop, and a schedule
type) and `ScheduleTimeCampus` (which maps a schedule time to an ordered sequence
of campuses). The concept of a route is implicit in the combination of a stop,
a direction, and the campuses visited — not a first-class entity.

### Problem 6 — No association class identification or justification

The school notes dedicate Unit 7 to many-to-many associations and association
classes. The initial diagram shows no association classes and offers no discussion
of why certain relationships need them. The revised document identifies five
association classes (`Booking`, `TripShuttleAllocation`, `PrioritySchedule`,
`ScheduleTimeCampus`, `StopShuttleDefault`), justifies each one in a table, and
explains what attributes belong to the relationship rather than to either endpoint.

### Problem 7 — No multiplicity on most associations

The initial diagram shows several associations with no multiplicity notation at
either end. The school notes are explicit that multiplicity must be specified for
both ends of every association. The revised document states every multiplicity in
both reading directions in a written table before drawing the diagram, which is
the approach the school notes recommend.

### Problem 8 — Syntax error in the class diagram

The initial `03` file ends with a fenced code block that is never closed — the
closing triple-backtick is replaced by `---`. This means the Mermaid diagram does
not render in any viewer. The revised diagram is syntactically correct throughout.

---

## Document 4 — UI Guidelines

### What the initial got right

The high-level intent was correct: NMU branding, high contrast for outdoor
readability, QR code screenshots accepted offline. The marshal re-scan flow
(green tick / red X) was conceptually correct.

### Problem 1 — Wrong brand colours

The initial specifies:
- Gold: `#F2A900`
- Navy: `#001F40`

The actual values defined in `AppTheme` and used throughout both apps are:
- Gold: `#FFCC00`
- Navy: `#132E51`

These are not minor variations. `#F2A900` is a noticeably darker amber. `#001F40`
is a different shade of navy. A UI guidelines document with wrong brand colour hex
values is unusable for implementation.

### Problem 2 — Wrong font

The initial specifies `Poppins Regular`. The actual font used throughout both
the mobile app and the web portal is **Hanken Grotesk**, loaded via
`GoogleFonts.hankenGroteskTextTheme()` in `AppTheme`. Again, not a minor detail —
this is the primary typography decision for the entire product.

### Problem 3 — No screen inventory

The initial has no list of screens or routes. A UI guidelines document should
document what screens exist, what each one is for, and what role can access it.
The revised document includes a full screen inventory for three contexts: Student
mobile (10 screens), Marshal mobile (4 screens), and Admin web portal (11 screens).

### Problem 4 — No component specifications

The initial describes no specific widget patterns. The revised document specifies:
- Exact dimensions and positioning of the floating hamburger button
  (`BorderRadius.circular(8)`, white background, top: statusBarHeight+12, left: 16)
- The QR code viewer configuration (256×256, embedded logo 48×48, RepaintBoundary
  for download)
- The QR scanner overlay frame (280×280, gold when idle, red on error)
- Trip card button states (Confirm = gold, Waitlist = orange tint)
- All status chip colour mappings for every trip and booking status value

These specifications are sourced directly from the implemented Flutter source code,
not invented.

### Problem 5 — Broken Mermaid diagrams

The initial `04` file contains a single fenced code block that mixes two completely
different Mermaid diagram types — a `stateDiagram-v2` and a `sequenceDiagram` —
inside the same block. This is invalid Mermaid syntax. Neither diagram renders.
The closing `---` is also used as a code fence terminator in some places, which
further breaks parsing.

The revised document uses three separate, correctly fenced diagrams — a
`flowchart TD` for the student booking flow, a `flowchart TD` for the marshal
check-in flow, and a `sequenceDiagram` for the breakdown resolution flow — each
in its own code block with correct syntax.

### Problem 6 — No navigation pattern documentation

The initial does not document how users navigate between screens. The revised
document documents the navigation pattern in full: no AppBar, floating hamburger
button opening a Drawer, `go_router` for all routing, `FloatingBackButton` for
back navigation, tab controller for the student home screen, bottom navigation
for top-level switching, and the persistent sidebar layout on the admin portal.

---

## Summary

| Area | Initial | Revised |
|------|---------|---------|
| System name | LATT Mobile (wrong) | NMU Shuttle Management System |
| Booking window | 24 hours (wrong) | 3 days (correct) |
| Auth model | Password-based for all (wrong) | OTP for students/marshals, password for admin only |
| Driver role | Listed (does not exist) | Removed |
| Capacity model | Single per-shuttle value | Sum of operational convoy shuttles |
| Domain classes | 14 (6 missing, 2 fabricated) | 20 (all verified against codebase) |
| Inheritance | User superclass for all 3 roles | Three independent classes (matches implementation) |
| Use case count | ~16 (approx, spread across 4 ranges) | 30 across 5 groups (U/S/M/A/T) |
| Login as use case | Included without justification | Included with lecturer-agreed justification; Change Password still excluded |
| OTP email notification | Not modelled | U110 — Send OTP Email Notification (state event, included by U100) |
| Email Service actor | Not present | Added as external system actor participating in U110 |
| User generalisation | Not present | User actor introduced; Student/Marshal/Admin inherit auth use cases |
| TIME actor | Partially present, unnamed | Formal actor with 4 use cases |
| Include/extend | Not modelled | 8 relationships with rationale table |
| Use case narratives | None | 5 fully developed narratives |
| Association classes | None identified | 5 identified and justified |
| Multiplicity | Partial, many ends missing | All associations stated in both directions |
| Brand gold hex | `#F2A900` (wrong) | `#FFCC00` (correct, from AppTheme) |
| Brand navy hex | `#001F40` (wrong) | `#132E51` (correct, from AppTheme) |
| Font | Poppins (wrong) | Hanken Grotesk (correct, from AppTheme) |
| Screen inventory | None | 25 screens across 3 platforms |
| Mermaid diagrams | Syntax errors, do not render | All valid and renderable |
