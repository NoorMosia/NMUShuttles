# Scenario Runner & Historical Seeder

## Problem Statement

The system's key automated events (nightly trip injection, T-60 lockout gate, breakdown workflow)
fire on real-world timers that cannot be waited for during a live demo. Additionally, the admin
analytics and report screens require historical booking and trip data that does not exist in a
fresh development environment.

This plan describes two closely related tools built on a shared backend service:

1. **Scenario Runner** — a standalone HTML dashboard that scripts the full system lifecycle
   in a 7-step linear flow, one click at a time, for use during live demonstrations.
2. **Historical Seeder** — a single endpoint that runs N days of simulated past activity
   (trips, bookings, check-ins, breakdowns) to populate analytics, reports, and student
   booking history.

Both tools are powered by a single reusable `IDemoScenarioService` on the backend.

---

## Requirements

### Scenario Runner

- 7-step linear demo chronology following the real system timeline:
  Midnight → Booking Window → T-60 → T-35 → Breakdown → Resolve → End State
- Standalone single-file `demo.html` — no Flutter build, no npm, no dependencies
- Styled with NMU brand colors (`#ffcc00` gold, `#132e51` navy)
- API base URL configurable at the top of the page (input field + connect button)
- Each step is unlocked only after the previous step completes (linear enforcement)
- Step results displayed as formatted, readable JSON cards inline
- State (e.g., `tripInstanceId`, `breakdownLogId`) flows automatically between steps

### Historical Seeder

- Single POST endpoint: `POST /api/v1/demo/seed-history?days=30`
- Loops `RunDayAsync` for each of the past N days
- Respects the same business rules as the real nightly job (skip Sundays, calendar exceptions)
- Returns a summary: `{ daysProcessed, tripsCreated, bookingsCreated, skipped }`
- Exposed as a "Seed History" section in `demo.html` (separate from the 7-step linear flow)

### Backend Endpoints

- All under `/api/v1/demo/`
- No `[Authorize]` attribute — unprotected, intended for local/dev use only
- Each returns `{ success: bool, message: string, data: object }`

### Seed Data Used

| Purpose | IDs |
|---------|-----|
| Confirmed bookings | StudentIds 1, 2, 4, 5, 7 |
| Waitlist booking | StudentId 8 |
| Priority auto-injection (nightly job) | StudentIds 3, 6, 13 |
| Check-in simulation | MarshalId 1 |
| Breakdown resolution | AdminId 1 |

---

## System Timeline (Reference)

```
T-∞  (Midnight SAST, 3 days prior)
     └── Nightly job fires
         ├── Creates TripInstances for the day
         ├── Assigns default convoy (shuttles per stop)
         └── Injects priority bookings for disability-cleared students

T-3 days to T-60 min  (Open booking window)
     └── Students browse trips and book seats
         ├── Confirmed if capacity available
         └── Waitlisted if full

T-60 min  (Lockout Gate fires)
     ├── Trip status → "Locked"
     ├── No new bookings or cancellations accepted
     └── Waitlisted students promoted FIFO to fill remaining capacity

T-35 min  (Scanning window opens)
     └── Marshals can now scan QR codes at the boarding point

T-0  (Departure)
     └── Trip departs

During trip
     └── Marshal flags shuttle breakdown
         └── Admin resolves: replace shuttle or cancel trip
             └── Push notification sent to affected students
```

---

## Architecture

```
demo.html  ──────────────────────────────────────────────────────────────────────┐
(standalone HTML)                                                                │
  POST /run-nightly       ──►  DemoController  ──►  IDemoScenarioService        │
  POST /seed-bookings     ──►  DemoController  ──►  IDemoScenarioService        │
  POST /trigger-lockout   ──►  DemoController  ──►  IDemoScenarioService        │
  POST /seed-checkins     ──►  DemoController  ──►  IDemoScenarioService        │
  POST /flag-breakdown    ──►  DemoController  ──►  IDemoScenarioService        │
  POST /resolve-breakdown ──►  DemoController  ──►  IDemoScenarioService        │
  GET  /summary/:id       ──►  DemoController  ──►  IDemoScenarioService        │
  POST /seed-history      ──►  DemoController  ──►  IDemoScenarioService (loop) │
                                                                                │
IDemoScenarioService depends on:                                                │
  ├── RollingInjectionService  (internal RunInjectionAsync)                     │
  ├── ILockoutService          (new — extracted from LockoutGateService)        │
  ├── IBookingService                                                           │
  ├── IWaitlistPromotionService                                                 │
  ├── ICheckInService                                                           │
  ├── IBreakdownService                                                         │
  ├── ITripStateMachineService                                                  │
  └── ShuttleDbContext                                                          │
└────────────────────────────────────────────────────────────────────────────────┘
```

---

## Demo Endpoint Reference

| Method | Route | Step | Description |
|--------|-------|------|-------------|
| POST | `/api/v1/demo/run-nightly` | 1 | Runs nightly injection for today+3 |
| POST | `/api/v1/demo/seed-bookings` | 2 | Books seed students on a trip |
| POST | `/api/v1/demo/trigger-lockout` | 3 | Locks next Scheduling trip + promotes waitlist |
| _(none)_ | _(informational)_ | 4 | T-35 scanning window — static card, no API call |
| POST | `/api/v1/demo/flag-breakdown` | 5 | Marshal flags a shuttle breakdown |
| POST | `/api/v1/demo/resolve-breakdown` | 6 | Admin resolves the breakdown |
| GET | `/api/v1/demo/summary/{tripInstanceId}` | 7 | Full end-state summary of the trip |
| POST | `/api/v1/demo/seed-history?days={n}` | — | Bulk historical seeder |

---

## Task Breakdown

### Task 1 — Extract lockout logic into `ILockoutService`

**Objective:** Make the core trip-locking logic independently callable without the background timer.

**Implementation guidance:**

- Create `ILockoutService` interface with method `LockNextTripAsync()` returning a result object
  containing `tripInstanceId`, previous status, new status, and promoted bookings count
- Create `LockoutService` implementing `ILockoutService`:
  - Queries for the next `TripInstance` with `Status == "Scheduling"` ordered by date/time
  - Executes the lock within a SERIALIZABLE transaction (same logic as current `LockTripAsync`)
  - Calls `IWaitlistPromotionService.RunAsync(tripInstanceId)` after successful lock
- Refactor `LockoutGateService` to inject and call `ILockoutService` instead of its own private method
- Register `ILockoutService` → `LockoutService` as scoped in `Program.cs`

**Outcome:** `LockoutGateService` behaviour is unchanged. `ILockoutService` can be injected and called independently. Internal refactor only — no visible demo change.

---

### Task 2 — Create `IDemoScenarioService` and `DemoScenarioService`

**Objective:** Single service encapsulating all simulation steps, reusable by both the demo controller and the historical seeder.

**Interface methods:**

```csharp
Task<NightlyJobResult>       RunNightlyJobAsync(DateOnly? targetDate = null);
Task<SeedBookingsResult>     SeedBookingsAsync(int tripInstanceId, int confirmedCount = 5, int waitlistCount = 1);
Task<LockoutResult>          TriggerLockoutAsync(int tripInstanceId);
Task<CheckInResult>          SeedCheckInsAsync(int tripInstanceId, bool bypassGuards = false);
Task<BreakdownFlagResult>    FlagBreakdownAsync(int tripInstanceId, int shuttleId);
Task<BreakdownResolveResult> ResolveBreakdownAsync(int breakdownLogId, string resolutionType, int? replacementShuttleId = null);
Task<TripSummaryResult>      GetTripSummaryAsync(int tripInstanceId);
Task<SeedHistoryResult>      RunDayAsync(DateOnly date);
```

> **Note on `TriggerLockoutAsync`:** The method takes an explicit `tripInstanceId` rather than
> finding "the next Scheduling trip" automatically. This is required for `RunDayAsync` to guarantee
> the same trip that received bookings is the one that gets locked. The `DemoController` action
> reads `tripInstanceId` from the request body and passes it through.
>
> **Note on `SeedCheckInsAsync`:** The `bypassGuards` flag skips the 35-minute time window check
> and marshal location binding when `true`. Pass `bypassGuards: true` from `RunDayAsync`
> (historical seeder) and `bypassGuards: false` from the interactive demo flow to keep real
> guards active during live demonstrations.

**Seed student assignment:**

- Confirmed bookings: StudentIds `[1, 2, 4, 5, 7]` (first `confirmedCount` from this list)
- Waitlist booking: StudentId `8`
- Marshal for check-ins: MarshalId `1`
- Admin for resolution: AdminId `1`

**`RunDayAsync` behaviour:**

1. Calls `RunNightlyJobAsync(date)`
2. Calls `SeedBookingsAsync` for the first created trip
3. Calls `TriggerLockoutAsync`
4. Calls `SeedCheckInsAsync` for that trip
5. Skips gracefully if the date is a Sunday or calendar exception
6. Returns aggregate counts

**Result DTOs** (in `DTOs/Demo/`):

Each result type carries `success`, `message`, and relevant data fields. Examples:

| DTO | Key fields |
|-----|-----------|
| `NightlyJobResult` | `tripsCreated`, `priorityBookingsCreated`, `targetDate` |
| `LockoutResult` | `tripInstanceId`, `promotedCount`, `lockedAt` |
| `BreakdownFlagResult` | `breakdownLogId`, `shuttleId`, `flaggedAt` |
| `TripSummaryResult` | `tripInstanceId`, `status`, `bookings[]`, `convoy[]` |
| `SeedHistoryResult` | `daysProcessed`, `tripsCreated`, `bookingsCreated`, `skipped` |

Register `IDemoScenarioService` → `DemoScenarioService` as scoped in `Program.cs`.

---

### Task 3 — Add `DemoController`

**Objective:** Expose each scenario step as an HTTP endpoint with no auth requirement.

**Controller setup:**

```csharp
[Route("api/v1/demo")]
[ApiController]
public class DemoController : ControllerBase
```

- No `[Authorize]` attribute on the controller or any action
- Guard: check `app.Environment.IsDevelopment()` inside each action — return `403 Forbidden` in production

**Actions:**

| Method | Route | Handler |
|--------|-------|---------|
| POST | `/run-nightly` | `RunNightlyJobAsync()` |
| POST | `/seed-bookings` | `SeedBookingsAsync(tripInstanceId from body)` |
| POST | `/trigger-lockout` | `TriggerLockoutAsync(tripInstanceId from body)` |
| POST | `/seed-checkins` | `SeedCheckInsAsync(tripInstanceId from body)` |
| POST | `/flag-breakdown` | `FlagBreakdownAsync(tripInstanceId, shuttleId from body)` |
| POST | `/resolve-breakdown` | `ResolveBreakdownAsync(breakdownLogId, resolutionType, replacementShuttleId from body)` |
| GET | `/summary/{tripInstanceId}` | `GetTripSummaryAsync(tripInstanceId)` |
| POST | `/seed-history?days={n}` | loop `RunDayAsync` for past N days |

**Request body shapes** (all bodies are JSON, camelCase):

```json
// POST /seed-bookings
{ "tripInstanceId": 42 }

// POST /trigger-lockout
{ "tripInstanceId": 42 }

// POST /seed-checkins
{ "tripInstanceId": 42 }

// POST /flag-breakdown
{ "tripInstanceId": 42, "shuttleId": 7 }

// POST /resolve-breakdown
{ "breakdownLogId": 3, "resolutionType": "Replaced", "replacementShuttleId": 8 }
// replacementShuttleId is omitted when resolutionType is "Cancelled"
```

All request bodies map to simple record DTOs in `DTOs/Demo/Requests/`:

```csharp
public record SeedBookingsRequest(int TripInstanceId);
public record TriggerLockoutRequest(int TripInstanceId);
public record SeedCheckInsRequest(int TripInstanceId);
public record FlagBreakdownRequest(int TripInstanceId, int ShuttleId);
public record ResolveBreakdownRequest(int BreakdownLogId, string ResolutionType, int? ReplacementShuttleId);
```

**Response envelope:**

```json
{
  "success": true,
  "message": "Nightly job completed. 152 trips created.",
  "data": { ... }
}
```

**Outcome:** All endpoints testable in Swagger UI at `http://localhost:{port}/swagger`.

---

### Task 4 — Build `demo.html` (Standalone Demo Dashboard)

**Objective:** Single HTML file that guides a presenter through the 7-step demo chronology.

**Constraints:**

- Single file at repo root: `demo.html`
- No external dependencies — inline CSS and vanilla JS only
- NMU brand palette: gold `#ffcc00`, navy `#132e51`, white `#ffffff`

**Page layout:**

```
┌─────────────────────────────────────────────┐
│  NMU Shuttle  [logo text]                   │  ← Header (navy background)
│  Scenario Runner                            │
├─────────────────────────────────────────────┤
│  API Base URL: [____________] [Connect]     │  ← Config bar
├─────────────────────────────────────────────┤
│  ● Step 1  ○ Step 2  ○ ...  ○ Step 7        │  ← Progress dots
├─────────────────────────────────────────────┤
│  [Step card — label, description, button]   │  ← Active step
│  [Collapsed result card]                    │
├─────────────────────────────────────────────┤
│  ── Seed History ──                         │  ← Separate section below
│  Days: [30]  [Run Seeder]                   │
└─────────────────────────────────────────────┘
```

**Step definitions:**

| # | Label | Time marker | API call | Button text |
|---|-------|-------------|----------|-------------|
| 1 | Run Nightly Job | Midnight SAST | POST `/run-nightly` | Run Nightly Job |
| 2 | Simulate Student Bookings | Booking Window | POST `/seed-bookings` | Seed Bookings |
| 3 | Trigger Lockout Gate | T-60 min | POST `/trigger-lockout` | Trigger Lockout |
| 4 | Scanning Window Opens | T-35 min | _(none)_ | Continue |
| 5 | Marshal Flags Breakdown | During Trip | POST `/flag-breakdown` | Flag Breakdown |
| 6 | Admin Resolves Breakdown | Admin Action | POST `/resolve-breakdown` | Resolve Breakdown |
| 7 | End State Summary | Post-Trip | GET `/summary/:id` | View Summary |

**JS state object:**

```javascript
let state = {
  apiBase: '',
  currentStep: 1,
  tripInstanceId: null,    // set after step 1; passed to steps 2, 3, 4b, 5, 7
  breakdownLogId: null,    // set after step 5; passed to step 6
  shuttleOptions: [],      // set after step 3; populates step 5 dropdown
  selectedShuttleId: null, // set when presenter picks from step 5 dropdown; sent in flag-breakdown body
};
```

**Step 4** is a static informational card — no API call. Displays:

> "At T-35 minutes before departure, the scanning window opens. Marshals at the boarding point
> can now scan student QR codes to record boarding."

A **Continue →** button advances to step 5.

**Step 5** dynamically renders a shuttle selector dropdown populated from the trip's convoy
(fetched from the summary endpoint after step 3).

**Result cards:** Each step's JSON response renders below the button in a `<pre>` block with
syntax-highlighted keys (navy) and values (gold).

**Outcome:** Open `demo.html` in any browser, paste `http://localhost:5000`, click through all 7 steps.

---

### Task 5 — Historical Seeder: Wire and Validate

**Objective:** Confirm that `POST /api/v1/demo/seed-history?days=30` produces realistic data
visible in the admin analytics and reports screens.

**Implementation notes:**

- `RunDayAsync` must back-date `TripInstance.TripDate` and `Booking.CreatedAt` to the simulated
  past date — not `DateTime.UtcNow`
- Calendar exception and Sunday skipping must work correctly for historical dates
- For ≤30 days: process synchronously, return final summary
- For >30 days: consider returning immediately with a queued-day count to avoid gateway timeouts

**Validation checklist:**

- [ ] Admin analytics screen shows vendor breakdown counts from seeded breakdowns
- [ ] Student booking history shows past confirmed/cancelled bookings
- [ ] Reports screen shows completed trips with boarded passenger counts
- [ ] Waitlist promotion events appear (`BookingType = "Standard"`, promoted from `WaitingList`)

---

## File Locations

| File | Path |
|------|------|
| `ILockoutService.cs` | `Backend/NmuShuttle.Api/Services/` |
| `LockoutService.cs` | `Backend/NmuShuttle.Api/Services/` |
| `IDemoScenarioService.cs` | `Backend/NmuShuttle.Api/Services/` |
| `DemoScenarioService.cs` | `Backend/NmuShuttle.Api/Services/` |
| `DemoController.cs` | `Backend/NmuShuttle.Api/Controllers/` |
| Result DTOs | `Backend/NmuShuttle.Api/DTOs/Demo/` |
| Request DTOs | `Backend/NmuShuttle.Api/DTOs/Demo/Requests/` |
| `demo.html` | `/` (repo root) |

---

## Notes

- Demo endpoints return `403 Forbidden` when `IsDevelopment()` is false — safe to leave in the codebase after deployment
- `DemoScenarioService` uses hardcoded seed StudentIds and MarshalId — acceptable for a demo tool
- The historical seeder respects all real business rules (capacity, idempotency, calendar exceptions) so seeded data is indistinguishable from real operational data in analytics
- `LockoutGateService` and `RollingInjectionService` remain commented out in `Program.cs` for local dev; the demo controller manually triggers their logic instead

---

## Known Caveats & Implementation Notes

These gaps were identified by cross-referencing the plan against the actual codebase. Address each one before starting implementation to avoid rework.

---

### Caveat 1 — `RunInjectionAsync` cannot be used as-is for the historical seeder

**Problem:** `RollingInjectionService.RunInjectionAsync` hardcodes `targetDate = today + 3 days`. It
accepts only a `CancellationToken` — there is no way to pass a specific date. The historical seeder
calls `RunDayAsync(DateOnly date)` for past dates, so calling `RunInjectionAsync` directly would
always create trips 3 days in the future, not the past date being seeded.

**Fix:** Before implementing `DemoScenarioService`, modify `RunInjectionAsync` to accept an optional
`DateOnly? targetDate = null` parameter. When `null`, it falls back to the existing `today + 3 days`
logic. `RollingInjectionService` continues working unchanged; `DemoScenarioService` passes the
specific back-date when seeding history.

```csharp
// Proposed signature change in RollingInjectionService:
internal async Task RunInjectionAsync(CancellationToken cancellationToken, DateOnly? targetDate = null)
{
    var resolvedDate = targetDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddHours(2)).AddDays(3);
    // ... rest of method uses resolvedDate instead of hardcoded calculation
}
```

> **Scope isolation note:** `RunInjectionAsync` calls `_scopeFactory.CreateScope()` internally and
> resolves its own `ShuttleDbContext` instance. `DemoScenarioService` is scoped and has its own
> separate `DbContext`. This means the trip IDs written inside `RunInjectionAsync`'s scope are
> committed to the DB but are **not tracked by `DemoScenarioService`'s DbContext**. After calling
> `RunNightlyJobAsync`, `DemoScenarioService` must issue a fresh DB query to retrieve the created
> `TripInstanceId` values — do not attempt to read them from a cached or in-memory list held
> across the scope boundary.

---

### Caveat 2 — `LockoutGateService` bypasses `ITripStateMachineService`

**Problem:** The existing `LockTripAsync` in `LockoutGateService` directly writes
`trip.Status = "Locked"` and `trip.LockedAt = DateTime.UtcNow` to the DbContext — it does **not**
call `ITripStateMachineService.TransitionAsync`. If the new `ILockoutService` extraction mirrors
this pattern, the state machine is bypassed in two separate places. If instead it routes through
the state machine, the `LockedAt` timestamp must be set by the state machine's `TransitionAsync`
(which it currently does not do for the `Locked` state).

**Fix:** Use **Option B** — route through `ITripStateMachineService.TransitionAsync`. This is
already the correct choice because `TripStateMachineService` already handles `LockedAt` in its
`TransitionAsync` switch (the `"Locked"` case sets `tripInstance.LockedAt = now`). No migration is
needed because `TripInstance.LockedAt` is already nullable (`DateTime?`).

- **Option A ~~(recommended)~~:** ~~Keep the direct DB write pattern.~~ **Avoid** — creates a
  second divergent code path that sets `LockedAt` independently, meaning two places in the codebase
  apply the same timestamp logic and can drift apart silently.
- **Option B (recommended):** Route `LockoutService.LockTripAsync` through
  `ITripStateMachineService.TransitionAsync("Locked")`. The state machine already sets `LockedAt`
  correctly. Wrap the call in a SERIALIZABLE transaction as before. Refactor
  `LockoutGateService` to use the same path.

```csharp
// Inside LockoutService.LockTripAsync — preferred implementation:
await using var transaction = await db.Database.BeginTransactionAsync(
    IsolationLevel.Serializable, cancellationToken);

var trip = await db.TripInstances.FirstOrDefaultAsync(...);
// status guard checks ...

await _stateMachine.TransitionAsync(trip.TripInstanceId, "Locked"); // sets LockedAt internally
await transaction.CommitAsync(cancellationToken);
```

This keeps a single source of truth for status transitions and timestamp assignment.

---

### Caveat 3 — `ICheckInService.CheckInAsync` requires a QR token, not a student ID, and enforces two guards that block demo use

**Problem:** The plan describes `SeedCheckInsAsync(int tripInstanceId)` as simulating check-ins for
seed students. But the real `CheckInService.CheckInAsync(string qrCodeToken, int marshalId)` works
by resolving a QR token from the booking record — not by student ID. The demo service cannot simply
pass a student ID to it.

Beyond the QR token issue, `CheckInService` enforces **two independent guards** that will both
reject demo check-ins if called naively:

1. **35-minute time window guard** — rejects scans more than 35 minutes before departure. Historical
   seeder trips are always in the past, so `DateTime.UtcNow < scanWindowStart` is always false and
   this guard passes. But for the live demo, the trip date is today+3 and no real departure time
   has elapsed, so this guard will throw `422` unless bypassed.

2. **Marshal location binding guard** — validates that MarshalId 1's `AssignedStopId` matches the
   trip's `StopId` (for `ToCampus` trips) or that `AssignedCampusId` matches a campus on the route
   (for `FromCampus` trips). This check fires regardless of time. Because the specific trip created
   during the demo is not known until step 1 runs, there is no guarantee that Marshal 1 is assigned
   to that trip's stop. This will throw `403` silently for every check-in attempt in both the live
   demo and the historical seeder.

**Fix:** `SeedCheckInsAsync` must **not** call `CheckInService.CheckInAsync` when `bypassGuards`
is `true`. Instead, write directly to the DB to set `BoardedStatus = "Boarded"`,
`BoardedAt = DateTime.UtcNow`, and auto-assign a shuttle via `TripShuttleAllocations` — mirroring
the logic in `CheckInService` steps 6–7 but without the guards.

```csharp
// Inside DemoScenarioService.SeedCheckInsAsync when bypassGuards == true:
var confirmedBookings = await _db.Bookings
    .Where(b => b.TripInstanceId == tripInstanceId && b.BookingStatus == "Confirmed")
    .ToListAsync();

var allocations = await _db.TripShuttleAllocations
    .Where(a => a.TripInstanceId == tripInstanceId && a.IsOperational)
    .OrderBy(a => a.OrderPosition)
    .Include(a => a.Shuttle)
    .ToListAsync();

foreach (var booking in confirmedBookings)
{
    if (booking.BoardedStatus == "Boarded") continue; // idempotency

    // Assign shuttle front-to-back (same logic as CheckInService step 6)
    int? assignedShuttleId = null;
    foreach (var alloc in allocations)
    {
        var boardedCount = await _db.Bookings.CountAsync(b =>
            b.TripInstanceId == tripInstanceId &&
            b.ShuttleId == alloc.ShuttleId &&
            b.BoardedStatus == "Boarded");

        if (boardedCount < alloc.Shuttle.Capacity)
        {
            assignedShuttleId = alloc.ShuttleId;
            break;
        }
    }

    booking.BoardedStatus = "Boarded";
    booking.BoardedAt = DateTime.UtcNow;
    booking.ShuttleId = assignedShuttleId;
}

await _db.SaveChangesAsync();
```

When `bypassGuards` is `false` (live demo step 4b), call `CheckInService.CheckInAsync` normally —
the time window guard is intentionally active for live demos.

> **Prerequisite:** Confirm in `firstSeed.md` that Marshal 1 is assigned to the same stop used by
> the demo trip before relying on `bypassGuards: false` in the live flow. See Caveat 3a below.

---

### Caveat 3a — Seed data prerequisites for the live demo

**Problem:** Two runtime failures are silently caused by missing or mismatched seed data:

1. **Marshal 1 location binding** — `CheckInService` checks that `Marshal.AssignedStopId` matches
   the trip's `StopId` (ToCampus) or `Marshal.AssignedCampusId` matches a campus on the route
   (FromCampus). If Marshal 1 in `firstSeed.md` is not assigned to the stop of the trip selected
   in step 1, every check-in in step 4b will throw `403`. Before implementing, open `firstSeed.md`
   and confirm which stop Marshal 1 covers, then confirm that a `ScheduleTime` (and therefore a
   trip) exists for that stop so the nightly job will create a trip that Marshal 1 can service.

2. **StopShuttleDefaults required for convoy allocation** — `RunInjectionAsync` populates convoy
   allocations from `StopShuttleDefaults` for each created trip's `StopId`. If no
   `StopShuttleDefault` rows exist for a stop, the trip is created with an empty convoy.
   `FlagBreakdownAsync` then throws `KeyNotFoundException` because there are no
   `TripShuttleAllocation` rows to find. Before running the demo, verify that `firstSeed.md`
   includes at least one `StopShuttleDefault` entry for the stop associated with the trip that
   will be created in step 1.

**Fix:** No code change required — this is a seed data verification step. Add it as a pre-demo
checklist item:

- [ ] Marshal 1 `AssignedStopId` matches at least one active `ScheduleTime.StopId` (so the nightly
  job creates a trip Marshal 1 can service)
- [ ] At least one `StopShuttleDefault` exists for that stop with an active shuttle (so convoy
  allocations are created and the breakdown step has a shuttle to flag)

---

### Caveat 4 — The live demo (7-step flow) never actually seeds check-ins

**Problem:** `SeedCheckInsAsync` only appears in `RunDayAsync` (the historical seeder path). In
the 7-step interactive demo, step 4 is a static informational card and no check-in API call is
made. This means the end-state summary in step 7 will show zero boardings, which looks broken
during a live demo.

**Fix:** Add a step between the scanning window card (step 4) and the breakdown step (step 5):

| # | Label | API call | Button text |
|---|-------|----------|-------------|
| 4 | Scanning Window Opens | _(none — static card)_ | Continue |
| **4b** | **Simulate Passenger Check-ins** | **POST `/seed-checkins`** | **Simulate Check-ins** |
| 5 | Marshal Flags Breakdown | POST `/flag-breakdown` | Flag Breakdown |

Update `demo.html` accordingly and adjust the progress dots to 8 steps (or renumber).

---

### Caveat 5 — No convoy list endpoint for the step 5 shuttle dropdown

**Problem:** The plan says the shuttle dropdown in step 5 is "populated from the trip's convoy
fetched from the summary endpoint after step 3." But the summary endpoint (`GET /summary/:id`) is
step 7's endpoint — calling it early works, but it is not documented and the response shape may
carry far more data than needed for a simple dropdown.

**Fix:** Either:

- **Option A:** After step 3 (`trigger-lockout`), include `convoy: [{ shuttleId, name }]` in the
  `LockoutResult` response so the shuttle list arrives as part of that step's data.
- **Option B:** Add a lightweight `GET /api/v1/demo/convoy/{tripInstanceId}` endpoint that returns
  only `shuttleId` and a display label. Cleaner separation of concerns.

Option A is simpler; Option B is cleaner. Choose one and document it in the endpoint reference table.

---

### Caveat 5b — `"Cancelled"` breakdown resolution leaves the trip permanently in `IncidentActive`

**Problem:** `BreakdownService.ResolveBreakdownAsync` handles `resolutionType == "Cancelled"` with
only a comment: *"Trip status remains as-is (IncidentActive or as admin decides)"*. No status
transition is applied. The trip stays in `IncidentActive` indefinitely, and step 7's end-state
summary will show `"status": "IncidentActive"` rather than anything that indicates the trip
concluded.

**Fix (recommended for the demo):** Use `"Replaced"` as the default resolution type in step 6.
The `LockoutResult` from step 3 already carries the convoy list; pass the first shuttle in that
list as the `replacementShuttleId`. This transitions the trip to `"InTransit"` and produces a
clean end-state in step 7.

If `"Cancelled"` must be supported, `BreakdownService.ResolveBreakdownAsync` needs a status
transition added:

```csharp
else if (resolutionType == "Cancelled")
{
    var tripInstance = await _db.TripInstances.FindAsync(breakdownLog.TripInstanceId)
        ?? throw new KeyNotFoundException($"TripInstance {breakdownLog.TripInstanceId} not found.");
    tripInstance.Status = "Cancelled"; // or a defined terminal state
}
```

> **Note:** `"Cancelled"` is not currently a valid state in `TripStateMachineService.ValidTransitions`.
> If this path needs to be clean, add `("IncidentActive", "Cancelled")` to the valid transitions
> set and handle the status timestamp. For the demo, just use `"Replaced"` and skip this issue.

---

### Caveat 6 — Seed students may already have bookings on repeated demo runs

**Problem:** If the live demo is run more than once in the same dev environment (or the seeder is
run after the demo), `IBookingService.CreateBookingAsync` may return a conflict or create duplicate
bookings for the same student on the same trip.

**Fix:** Add a `POST /api/v1/demo/reset` endpoint to `DemoController` that deletes all demo-seeded
data (bookings, trip instances, breakdown logs, allocations created by the seeder). This makes the
demo repeatable without wiping the entire database.

Alternatively, `SeedBookingsAsync` can guard idempotently:

```csharp
// Skip if student already has a booking on this trip
var existing = await db.Bookings
    .AnyAsync(b => b.StudentId == studentId && b.TripInstanceId == tripInstanceId);
if (!existing)
    await _bookingService.CreateBookingAsync(studentId, tripInstanceId);
```

---

### Caveat 7 — `seed-history` will time out for large day counts

**Problem:** Synchronous processing of 30+ days involves hundreds of DB writes per day. A 90-day
seed at, say, 10 trips/day = ~900 trip insertions + bookings + allocations. Default Kestrel and
reverse-proxy timeouts (typically 30–120 seconds) will kill the request.

**Fix:** Apply a hard rule in the controller:

- **≤ 30 days:** process synchronously, return final summary.
- **> 30 days:** fire-and-forget into a `Task.Run` (or queue via `IBackgroundTaskQueue`), return
  `202 Accepted` immediately with a polling hint. Add a `GET /api/v1/demo/seed-status` endpoint
  that returns the current progress count from a shared in-memory state object.

The simplest acceptable implementation for a dev tool is:

```csharp
if (days > 30)
    return BadRequest("Maximum 30 days per request. Call in batches for larger ranges.");
```

Document whichever limit is chosen in `demo.html`'s seed history section.
