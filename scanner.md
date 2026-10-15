# Scanner / Scan-Window Switch

Quick reference for the marshal QR scan-window behavior and the test-only switch that controls it.

## The switch

**Config key:** `ScanWindow:OpenAllTodayTrips`

**Where it lives:** `Backend/NmuShuttle.Api/appsettings.json`

```json
"ScanWindow": {
  "OpenAllTodayTrips": true
}
```

- `true`  → **TESTING.** Every one of *today's* trips that hasn't departed yet is open to scanning right now. Tomorrow-onward trips keep the normal rule.
- `false` → **PRODUCTION.** Strict rule for all trips: only the single next upcoming trip is open early; every other future trip opens 35 minutes before departure.

> ⚠️ **Flip this to `false` before production.** It intentionally loosens who can be checked in early.

> Changing `appsettings.json` requires an **API restart** — the value is bound at startup via `IOptions`.

## What the rule actually does

| Trip | Flag `true` (testing) | Flag `false` (production) |
|------|-----------------------|---------------------------|
| Today, not yet departed | **Open** | Open only if it's the next trip, else T‑35 min |
| Today, already departed | Closed ("already departed") | Closed ("already departed") |
| Tomorrow onward | Next trip open early; rest at T‑35 min | Next trip open early; rest at T‑35 min |

## Code touchpoints (if you need to change the logic)

- **Flag definition:** `Backend/NmuShuttle.Api/Infrastructure/ScanWindowOptions.cs`
- **Registration/binding:** `Backend/NmuShuttle.Api/Program.cs` (`Configure<ScanWindowOptions>`, near the Check-In Service registration)
- **Authoritative gate (returns 422 if closed):** `Backend/NmuShuttle.Api/Services/CheckInService.cs` → `ValidateScanWindowAsync`
- **Marshal UI flag (`IsScanningOpen`):** `Backend/NmuShuttle.Api/Controllers/MarshalTripsController.cs` → `GetTodayTimeSlots`

Both call sites honor the flag so the marshal app's "scanning open" badge and the real check-in gate stay in agreement. If you change the rule, change both.

## Standard scan window

The normal pre-departure scan window is **35 minutes** (`ScanWindowMinutes` in both files above). Trips lock at **T‑60 min** (separate concern — see `LockoutGateService`).
