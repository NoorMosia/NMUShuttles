# Shuttle Assignment — How It Works

This document describes the full mechanics of shuttle assignment in the NMU Shuttle Management
system: how shuttles are assigned to trips, how students are assigned to specific shuttles, and
how every surrounding process (nightly injection, lockout, check-in, breakdowns) participates in
that flow.

---

## Part 1 — Key Entities Involved

Before diving into flows, here are the four entities at the heart of the story.

```
┌─────────────────┐        ┌─────────────────────────┐        ┌───────────────┐
│  ScheduleTime   │        │      TripInstance        │        │    Shuttle    │
│─────────────────│        │─────────────────────────│        │───────────────│
│ ScheduleTimeId  │◄──────►│ TripInstanceId (PK)     │        │ ShuttleId (PK)│
│ DepartureTime   │        │ ScheduleTimeId (FK)     │        │ LicensePlate  │
│ Direction       │        │ StopId (FK)   ◄─────────┼───┐    │ Capacity      │
│ ScheduleType    │        │ TripDate                │   │    │ VendorId (FK) │
│ CampusId (FK)   │        │ Status                  │   │    │ IsAccessible  │
│ StopId (FK)     │        └──────────┬──────────────┘   │    └───────┬───────┘
└─────────────────┘                   │                   │            │
                                      │ 1                 │            │ N
                                      ▼ N                 │            ▼
                          ┌───────────────────────────┐   │   ┌────────────────────────┐
                          │   TripShuttleAllocation   │   │   │       Booking          │
                          │───────────────────────────│   │   │────────────────────────│
                          │ AllocationId (PK)         │   │   │ BookingId (PK)         │
                          │ TripInstanceId (FK)       │   │   │ StudentId (FK)         │
                          │ ShuttleId (FK)            │   │   │ TripInstanceId (FK)    │
                          │ OrderPosition  ← KEY      │   └───│ StopId (derived)       │
                          │ IsOperational             │       │ ShuttleId (FK, NULL)◄──┼── set at check-in
                          │ AssignedAt                │       │ BookingStatus          │
                          └───────────────────────────┘       │ BookingType            │
                                                               │ BoardedStatus          │
                                                               │ QrCodeToken            │
                                                               └────────────────────────┘
```

Two distinct levels of "assignment" exist in the system:

| Level | Question answered | When it happens | Stored on |
|---|---|---|---|
| **Shuttle → Trip** | Which physical vehicles serve this trip? | Admin action (manual) | `TripShuttleAllocation` |
| **Student → Shuttle** | Which specific vehicle does this student board? | At QR scan (check-in) | `Booking.ShuttleId` |

These are completely independent steps. A student can have a confirmed booking with
`Booking.ShuttleId = NULL` right up until the moment their QR code is scanned.

---

## Part 2 — Stop-to-Trip Assignment (How a Stop Is Tied to a Trip)

There is **no separate assignment step** for stops. The stop is baked into the trip at creation
time and never changes.

```
ScheduleTime
  StopId ─────────────► TripInstance.StopId (set at trip creation by RollingInjectionService)
  ScheduleTimeId ──────► TripInstance.ScheduleTimeId
```

When the nightly `RollingInjectionService` generates trips three days ahead, it copies the
`StopId` directly from each `ScheduleTime` template record into the new `TripInstance`. This is a
one-to-one copy — one `TripInstance` per `ScheduleTime`, per day. There is no many-stop-per-trip
concept; a trip serves exactly one stop.

The `ScheduleTime` additionally carries:
- `Direction` — `"ToCampus"` or `"FromCampus"`, governing where marshals must stand for
  location-binding checks.
- `CampusId` — the destination/origin campus.
- `DepartureTime` — the SAST time used for the 60-minute lockout and the 35-minute scan window.

---

## Part 3 — Shuttle-to-Trip Assignment (Building the Convoy)

This is a **manual admin action**. No shuttles are automatically allocated to trips by the
nightly job.

### How it is triggered

An admin POSTs to:

```
POST /api/v1/admin/trips/{tripId}/shuttles
Body: { "ShuttleId": 7 }
```

This calls `ConvoyOrderingService.AssignShuttleToTripAsync()`, which creates one
`TripShuttleAllocation` row.

### The accessible-first ordering rule

The convoy is ordered front-to-back using `OrderPosition` (1-based integer). Accessible vehicles
must always occupy the front positions. The algorithm works as follows:

```
Is the shuttle being added an accessible vehicle (IsAccessibleVehicle = true)?
    │
    ├── YES ─► Find the highest OrderPosition currently held by any accessible shuttle.
    │          Insert the new shuttle at (that position + 1).
    │          Shift all non-accessible allocations up by 1 to make room.
    │
    └── NO ──► Append at the end (max current OrderPosition + 1).
```

**Example — building a 3-shuttle convoy:**

```
Step 1: Add shuttle A (accessible)
  Convoy: [ 1: A(accessible) ]

Step 2: Add shuttle B (non-accessible)
  Convoy: [ 1: A(accessible), 2: B ]

Step 3: Add shuttle C (accessible)
  "last accessible position" = 1 (shuttle A)
  new position = 1 + 1 = 2
  shift B up → B becomes position 3
  Convoy: [ 1: A(accessible), 2: C(accessible), 3: B ]
```

The service also exposes `ReorderConvoyAsync()`, which re-normalises all positions if the convoy
state ever becomes inconsistent (accessible first, then non-accessible, both groups sorted by
their existing relative order).

### What gets stored

```
TripShuttleAllocation
  AllocationId = <new PK>
  TripInstanceId = <the trip>
  ShuttleId = <the vehicle>
  OrderPosition = <computed above>
  IsOperational = true
  AssignedAt = DateTime.UtcNow
```

### Capacity is derived, not stored

There is no `Capacity` column on `TripInstance`. Every time capacity is needed, the system
calculates it live:

```
Total Capacity = SUM(Shuttle.Capacity)
                 WHERE TripShuttleAllocation.TripInstanceId = <trip>
                   AND TripShuttleAllocation.IsOperational = true
```

This means adding or removing a shuttle from the convoy immediately changes the trip's effective
seat count without any further updates.

---

## Part 4 — The Booking (No Shuttle Assigned Yet)

When a student books a seat, no shuttle is assigned. The booking record is created with
`ShuttleId = NULL`.

```
Student taps "Book Seat"
    │
    ▼
BookingService.CreateBookingAsync()
    │
    ├── Trip status must be "Scheduling" (not Locked, InTransit, etc.)
    │
    ├── No duplicate active booking for same student + trip
    │
    ├── Count confirmed bookings vs. total capacity
    │       confirmed < capacity  →  BookingStatus = "Confirmed"
    │       confirmed >= capacity →  BookingStatus = "WaitingList"
    │
    └── Create Booking row:
            StudentId = <student>
            TripInstanceId = <trip>
            ShuttleId = NULL          ← no shuttle yet
            BookingStatus = "Confirmed" or "WaitingList"
            BookingType = "Standard"
            BoardedStatus = "Pending"
            QrCodeToken = <new GUID>
```

The student receives a QR code but does not yet know which physical shuttle they will board.

---

## Part 5 — Priority Pre-Assignment (Exception to the NULL Rule)

Disability-cleared students bypass the normal booking flow. The nightly job creates their booking
**automatically and pre-assigns them to the front shuttle**.

```
RollingInjectionService (runs at midnight SAST, targets Today+3)
    │
    ├── For each active PrioritySchedule matching the target day:
    │       Find the matching TripInstance
    │       Query TripShuttleAllocations WHERE OrderPosition = 1 AND IsOperational = true
    │       ShuttleId = that allocation's ShuttleId  (may be NULL if no shuttles assigned yet)
    │
    └── Create Booking:
            BookingType = "Priority"
            BookingStatus = "Confirmed"
            BoardedStatus = "Pending"
            ShuttleId = <front shuttle ID, or NULL if convoy not built yet>
```

This is the only path where `Booking.ShuttleId` may be non-NULL before check-in. The intent is
that priority students are guaranteed a seat in the first (accessible) shuttle at the front of
the convoy.

**Important note:** If no convoy has been built for the trip yet when the job runs, the priority
booking is still created, but `ShuttleId` will be NULL. The pre-assignment only works if the
admin has already linked shuttles to that trip before midnight.

---

## Part 6 — The Lockout Gate (T−60 Minutes)

`LockoutGateService` is an `IHostedService` that sleeps until 60 minutes before each trip's
departure, then fires.

```
T−60 min before departure (SAST)
    │
    ├── Trip.Status → "Locked"
    │   (no new bookings or cancellations accepted after this point)
    │
    └── WaitlistPromotionService.RunAsync()
            │
            ├── Recalculate available seats (capacity − confirmed count)
            ├── Fetch WaitingList bookings in FIFO order (CreatedAt ASC)
            ├── Promote up to <available slots> bookings to "Confirmed"
            └── Send FCM push notification to each promoted student
```

Waitlist promotion does **not** assign a shuttle (`ShuttleId` stays NULL). That still happens at
check-in.

---

## Part 7 — Check-In: Where Student-to-Shuttle Assignment Actually Happens

This is the primary shuttle assignment moment for standard bookings. The marshal scans a
student's QR code using the mobile app.

```
POST /api/v1/checkin
Body: { "qrCodeToken": "abc123..." }
```

`CheckInService.CheckInAsync()` runs the following steps:

```
Step 1 — Lookup
  Find Booking by QrCodeToken
  (404 if not found)

Step 2 — 35-minute scan window check
  Departure time (SAST) − 35 min = scan window open time
  Reject if current UTC time < scan window open time
  (422 "Scanning not yet available")

Step 3 — Marshal location binding
  Direction = "ToCampus"?
    → Marshal.AssignedStopId must equal TripInstance.StopId
  Direction = "FromCampus"?
    → Marshal.AssignedCampusId must equal ScheduleTime.CampusId
  (403 if location mismatch)

Step 4 — Status checks
  BookingStatus must be "Confirmed"  (not WaitingList, not Cancelled)
  BoardedStatus must not be "Boarded" (no double-scan)

Step 5 — Auto shuttle assignment (THE CORE STEP)
  Load all TripShuttleAllocations for this trip
    WHERE IsOperational = true
    ORDER BY OrderPosition ASC  ← front of convoy first

  For each allocation in order:
    Count bookings WHERE TripInstanceId = <trip>
                     AND ShuttleId = <this shuttle>
                     AND BoardedStatus = "Boarded"

    If boardedCount < shuttle.Capacity:
      assignedShuttleId = this shuttle  ← ASSIGN HERE
      break

  If no shuttle has room → 422 "Convoy is full"

Step 6 — Update Booking
  Booking.BoardedStatus = "Boarded"
  Booking.BoardedAt = DateTime.UtcNow
  Booking.ShuttleId = assignedShuttleId   ← written to database
```

Illustrated as a fill pattern for a two-shuttle convoy (capacity 15 + 20):

```
Convoy:  [ Shuttle A (pos 1, cap 15) ]  [ Shuttle B (pos 2, cap 20) ]
                   accessible                   standard

Scans 1–15 → all assigned to Shuttle A
  Booking.ShuttleId = A for each

Scan 16 → Shuttle A is full (15/15 boarded)
  Move to Shuttle B
  Booking.ShuttleId = B

Scans 17–35 → assigned to Shuttle B

Scan 36 → both shuttles full
  → 422 "Convoy is full"
```

The marshal does not choose the shuttle. The system decides automatically by filling front-to-back.

---

## Part 8 — Breakdown Handling (Mid-Trip Reassignment)

If a shuttle breaks down or fails to arrive, the marshal flags it via the app. This does **not**
immediately trigger notifications or reassignment — the admin handles resolution.

```
Marshal flags shuttle
    │
    ├── BreakdownLog created (ShuttleId, TripInstanceId, FlaggedAt)
    └── TripInstance.Status → "IncidentActive"
        (TripShuttleAllocation.IsOperational stays true at flag time — set by admin on resolution)

Admin resolves breakdown
    │
    ├── Resolution = "Replaced"
    │       Allocation.ShuttleId → replacementShuttleId
    │       Allocation.IsOperational → true
    │       TripInstance.Status → "InTransit"
    │       FCM push to all confirmed passengers: "Replacement shuttle assigned"
    │
    └── Resolution = "Cancelled"
            IsOperational stays false (effective capacity drops)
            No further allocation changes
            FCM push to all confirmed passengers: "Shuttle cancelled, service affected"
```

In the "Replaced" case, the `TripShuttleAllocation` row is mutated in-place (the `ShuttleId`
column is overwritten with the replacement vehicle's ID). Existing `Booking.ShuttleId` values
pointing at the broken-down shuttle are **not retroactively updated** — the allocation record
becomes the source of truth for what physically arrived.

---

## Part 9 — Full Assignment Lifecycle (Timeline View)

```
T − 3 days (midnight SAST)
  ├─ RollingInjectionService creates TripInstance (StopId copied from ScheduleTime)
  └─ Priority bookings created; ShuttleId = OrderPosition-1 shuttle (or NULL if no convoy yet)

T − 3 days to T − 60 min
  ├─ Admin assigns shuttles via POST /admin/trips/{id}/shuttles
  │    ConvoyOrderingService places accessible vehicles at front
  │    TripShuttleAllocations created with OrderPosition
  └─ Students book seats
       Standard bookings: ShuttleId = NULL, status = Confirmed or WaitingList

T − 60 min
  ├─ LockoutGateService fires
  │    TripInstance.Status → "Locked"
  └─ WaitlistPromotionService promotes waitlisted bookings (still ShuttleId = NULL)

T − 35 min
  └─ Scan window opens (enforced by CheckInService)

T (departure time)
  └─ Marshal scans QR codes
       CheckInService assigns each student to the front non-full shuttle
       Booking.ShuttleId = <assigned shuttle>
       Booking.BoardedStatus = "Boarded"

During transit (if breakdown)
  ├─ Marshal flags shuttle → TripInstance.Status = "IncidentActive"
  └─ Admin resolves → allocation patched, notifications sent

After trip
  └─ TripStateMachineService marks trip "Completed"
       Pending bookings (no-shows) → BoardedStatus = "NoShow"
```

---

## Part 10 — Summary of Assignment Rules

| Scenario | Who assigns | When | Result stored on |
|---|---|---|---|
| Stop → Trip | Automated (nightly job copies from ScheduleTime) | Trip creation, T−3 days | `TripInstance.StopId` |
| Shuttle → Trip (convoy) | Admin manually via API | Any time before departure | `TripShuttleAllocation` |
| Priority student → front shuttle | Automated (nightly job) | Trip creation, T−3 days | `Booking.ShuttleId` (OrderPosition=1 shuttle) |
| Standard student → specific shuttle | Automated (CheckInService) | At QR scan, ≥T−35min | `Booking.ShuttleId` |
| Replacement shuttle → broken allocation | Admin manually | During incident | `TripShuttleAllocation.ShuttleId` (overwritten) |

### Three invariants that govern assignment behaviour

1. **Capacity is always derived** — never stored. It is the sum of `Shuttle.Capacity` for
   operational allocations. Removing a shuttle or marking it non-operational instantly reduces
   the trip's seat ceiling.

2. **Front-fill is enforced at scan time** — the system iterates allocations in
   `OrderPosition ASC` and assigns each arriving student to the first shuttle that still has
   empty seats. Accessible shuttles are guaranteed to be at positions 1, 2, … (lowest numbers)
   so they fill first.

3. **Booking.ShuttleId is write-once (effectively)** — for standard bookings it is NULL until
   check-in and is set exactly once when the student boards. For priority bookings it may be
   set at creation time but is overwritten at check-in if the student physically scans.
   No service currently updates `ShuttleId` after the check-in write except the breakdown
   resolution path (which patches the allocation, not the booking).
