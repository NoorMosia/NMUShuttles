# Web Portal — Edge Cases To Handle

Reference list of edge cases found across the Flutter Web admin portal (`WebPortalUI`).
Organized by feature area, with cross-cutting/domain issues at the end. Checkboxes so you
can track what's been addressed.

Priority key: **[P1]** breaks core flow / data integrity / security · **[P2]** confusing or lost feedback · **[P3]** polish / minor.

---

## Core Infrastructure

### API client (`core/api/dio_client.dart`)
- [ ] **[P1]** Base URL hardcoded to an ngrok dev tunnel — not environment-driven. Breaks in any other environment. Make it config/env-based.
- [ ] **[P2]** No global handling for `403`, `500`, `502/503`, timeouts, or `connectionError`. Only `401` is handled globally; every screen handles the rest inconsistently.
- [ ] **[P3]** No retry/backoff or offline detection.

### Auth service (`core/auth/admin_auth_service.dart`)
- [ ] **[P2]** Token expiry only checked at route-redirect and on 401. A token that expires while the user sits on a page isn't detected until the next API call.

### Router (`core/router/admin_router.dart`)
- [ ] **[P1]** No role guard — router only checks `isAuthenticated()`, never the JWT role claim. A Student/Marshal token could load admin routes client-side (backend still enforces, but client should guard too).
- [ ] **[P1]** Route params use `int.parse(...)` for `/stops/:id`, `/stops/:id/trips`, `/trips/:id`. A non-numeric id in the URL throws `FormatException` and crashes the route builder. Use `int.tryParse` + fallback/404.
- [ ] **[P3]** `redirect` builds a fresh `AdminAuthService()` on every navigation; brief flash possible (no loading gate).

---

## Auth / Login (`features/auth`)
- [ ] **[P2]** No resend-OTP button and no OTP expiry countdown (5-min expiry not surfaced). User must go "Back to email" to re-request.
- [ ] **[P2]** No rate-limit / lockout feedback (HTTP 429 collapses into "Unable to connect to server").
- [ ] **[P2]** `verifyOtp` reads `response.data['token']` with no null/shape check — a 200 with a missing/renamed token field throws and shows a misleading "Unable to connect to server".
- [ ] **[P3]** Email not lowercased/normalized before submit (backend must handle case).

---

## Stops (`features/stops`)

### List / Create / Edit / Delete (`stops_screen.dart`)
- [ ] **[P2]** Duplicate stop name relies entirely on backend 409; no client pre-check and message surfacing depends on backend shape.
- [ ] **[P1]** Delete of a referenced stop (used by schedule times, priority schedules, default convoy, existing trips) has no client-side warning — only whatever the backend returns in a SnackBar.
- [ ] **[P2]** GPS parsing: existing coordinates split naively on `,`. A stored value without a comma silently leaves longitude empty.
- [ ] **[P2]** create/update only special-case 422/409; any other failure shows generic "Failed to create/update stop" (no network vs 500 distinction).
- [ ] **[P3]** Dialog `TextEditingController`s never disposed — leak on each open.

### Stop detail — marshal assign + default convoy (`stop_detail_screen.dart`)
- [ ] **[P1]** `stopMarshalProvider` swallows all errors and returns null — a network failure looks identical to "No marshal assigned".
- [ ] **[P2]** Convoy reorder isn't truly optimistic: list reorders locally, then on failure shows a SnackBar but the visual order already changed until refetch; if refetch also fails, stale order shown.
- [ ] **[P1]** Accessible-shuttle-at-front invariant not enforced client-side — admin can drag a non-accessible shuttle to position 0. Relies on backend.
- [ ] **[P2]** `_showAddShuttleDialog` shows raw `error.toString()` instead of `_extractError` — leaks raw Dio string.
- [ ] **[P2]** No concurrent-edit protection (no ETag/version) — two admins editing the same convoy clobber each other (last-write-wins).
- [ ] **[P3]** Deep-link to a deleted/edited stop id still fires marshal/convoy sub-fetches against that id.

### Stop trips (`stop_trips_screen.dart`)
- [ ] **[P3]** Generic "Failed to load trips" with no Retry button (inconsistent with trips_screen which has one).

---

## Shuttles (`features/shuttles`)
- [ ] **[P2]** Capacity slider caps at 80 but the text field only validates `> 0` (digits-only, no max). A typed value above 80 is submitted while the slider clamps display — boundary mismatch.
- [ ] **[P2]** Duplicate license plate relies on backend 409.
- [ ] **[P2]** Vendor dropdown depends on `vendorsProvider`; if vendors fail to load, dropdown is empty and shuttle can't be created — no explanation of why.
- [ ] **[P1]** Editing a shuttle allocated to locked/in-transit trips has no guard; capacity change can desync trip capacity math.
- [ ] **[P3]** Reactivate has no confirmation (deactivate does).
- [ ] **[P3]** Dialog controllers not disposed.

---

## Users (`features/users`)
- [ ] **[P1]** `addUser`/`updateUser` swallow the backend error (`debugPrint` + `return false`); screen shows generic "Operation failed." The real reason (409 duplicate student number, 422 validation, email conflict) is never surfaced. **Biggest gap here.**
- [ ] **[P2]** Duplicate identifier / duplicate email — no client detection, collapses to generic failure.
- [ ] **[P1]** CSV parsing uses naive `line.split(',')` — breaks on quoted fields containing commas; no BOM/quoted-header handling. Only checks too-few-columns; extra columns silently ignored.
- [ ] **[P2]** CSV role values not validated client-side against allowed roles before upload (typos/`Admin` pass through).
- [ ] **[P2]** Toggling active reuses `updateUser` sending full identifier/email; if derived-email logic differs from stored value, a toggle may rewrite fields.
- [ ] **[P2]** `_toggleUserActive` shows no error feedback (ignores the returned bool).
- [ ] **[P2]** Editing identifier is allowed and re-validated; changing a student number can collide (only backend catches).

---

## Vendors (`features/vendors`)
- [ ] **[P2]** Contact email never validated for format — invalid string sent as-is.
- [ ] **[P2]** Phone formatter forces exactly a 3-3-4 / 10-digit SA format; international or shorter numbers can't be entered.
- [ ] **[P2]** Delete relies on backend to reject when shuttles are attached — `shuttleCount` is available on the model but not used to disable the delete button / pre-check.
- [ ] **[P2]** `updateVendor` omits null fields from PATCH, so an email/phone can be changed but not cleared back to empty.
- [ ] **[P3]** Dialog controllers not disposed.

---

## Trips (`features/trips`)
- [ ] **[P1]** Trip-lock rule (T-60 min) invisible client-side. Assign-Shuttle and Add-Note shown regardless of status. Assigning to a Locked/InTransit/Completed trip is attempted and only backend rejects it.
- [ ] **[P1]** The specific "trip is locked" 400 message is never shown — `assignShuttle`/`updateStatus` failures show a hardcoded generic SnackBar.
- [ ] **[P1]** Two different breakdown-resolution paths coexist and look inconsistent:
  - `trips_screen` `_BreakdownResolutionCard` → `POST /trips/:tripId/breakdown/:breakdownLogId/resolve` (**path lacks the `/admin` prefix used elsewhere — likely a bug**).
  - `trip_detail_screen` add-note with `resolveIncident:true` → `POST /admin/trips/:tripId/notes`.
  - Only the notes path reports `notificationsSent`.
- [ ] **[P2]** Replacement-shuttle dropdown lists all active shuttles — no accessibility parity check (accessible replaced by non-accessible allowed) and no filtering of shuttles already in the convoy (can double-assign).
- [ ] **[P2]** `trip_detail_screen` fetches with `TripQueryParams()` (no date filter) then `.firstWhere(id)`. If the trip isn't in that list it shows "Trip not found" even though it exists. Needs a dedicated single-trip GET.
- [ ] **[P2]** Stale state: `_isResolutionType` requires status `IncidentActive`; if status changed server-side since load, client sends `resolveIncident:true` against a non-incident trip.
- [ ] **[P3]** Add-note body has no `maxLength`.
- [ ] **[P2]** Detail convoy table is read-only — can add but not reorder/remove shuttles from a specific trip's convoy.
- [ ] **[P2]** Concurrent breakdown resolution — two admins can resolve the same `breakdownLogId`; no idempotency/version guard.

---

## Schedule (`features/schedule`)

### Schedule times (`schedule_screen.dart`)
- [ ] **[P1]** Campus ID and Stop ID are free-text integer inputs, not dropdowns. `int.tryParse ?? 0` sends `0` or a bad FK on wrong input; only backend rejects.
- [ ] **[P1]** add/edit/delete swallow errors AND the screen ignores the returned bool — `_submit` calls the notifier then unconditionally `Navigator.pop()`. Failures close the dialog with no feedback and a stale-looking list.
- [ ] **[P1]** `requiresScanning` is hardcoded to `true` in initState on edit — editing a scan-disabled schedule time silently re-enables scanning. **Real bug.**
- [ ] **[P2]** No duplicate detection (same time/direction/stop) client-side.

### Calendar exceptions (`calendar_exceptions_screen.dart`)
- [ ] **[P2]** Same swallow-and-close pattern — `addCalendarException` bool ignored; a duplicate-date 409 or failure yields no feedback.
- [ ] **[P1]** No range support for `UniversityRecess` — recesses span multiple days but the form only allows a single `targetDate`. Admin must add one exception per day. Significant given the "skip recesses" trip-generation rule.
- [ ] **[P3]** Deleting an exception that trips were already generated/skipped for has no cascade explanation.

### Priority schedules (`priority_schedules_screen.dart`)
- [ ] **[P1]** Student ID, Stop ID, Schedule Time ID are ALL raw integer text fields. No lookups, no verification the student is disability-cleared, no verification the stop/time combination is valid. Worst offender for raw-ID entry.
- [ ] **[P1]** Same swallow-and-close pattern — `addPrioritySchedule` bool ignored; dialog pops regardless. Duplicate (same student/day/time) surfaces no error.
- [ ] **[P2]** Deactivate confirmation exists but no error feedback if the PATCH fails.
- [ ] **[P3]** No way to reactivate a deactivated priority schedule from the UI.

---

## Reports (`features/reports`)
- [ ] **[P2]** `ref.listen` pre-fill keys on `prev?.report?.id != next.report?.id` — reopening the same report may not re-run pre-fill, so the form can retain previously edited (unsaved) status/notes.
- [ ] **[P3]** Admin can submit an update with empty admin notes; no confirmation that resolving is final.
- [ ] **[P2]** No optimistic-lock/concurrent-edit protection (two admins updating the same report).
- [ ] **[P2]** "Submitted" status not selectable — a report can't be moved back to Submitted; on load "Submitted" defaults to "UnderReview".
- [ ] **[P3]** Linked trip shown as `#id` text only — not navigable.

---

## Overview & Detailed Analytics (`features/overview`)
- [ ] **[P2]** Date range has no `fromDate <= toDate` validation — two independent pickers allow From after To, producing an invalid/empty range with only a generic "No data" message.
- [ ] **[P3]** Date pickers allow 2020–2030 with no cap at "today" — future ranges are queryable.
- [ ] **[P3]** If all counts are 0, progress bars render empty with no "no activity" nuance.
- [ ] **[P3]** No auto-refresh and no snapshot timestamp shown.

---

## Cross-Cutting / Domain-Rule Edge Cases (systemic)

1. [ ] **[P1]** **Trip lock at T-60 min is invisible client-side.** No screen disables booking/assignment/cancellation based on `Locked`/`InTransit`/`Completed` or computed departure-minus-60. Resulting 400 "locked trip" messages are masked by generic strings.
2. [ ] **[P1]** **Backend error messages swallowed in many notifiers** (Users, Schedule Times, Calendar Exceptions, Priority Schedules) — 409/422/400 distinctions lost exactly where they matter (duplicate identifiers/schedules, locked trips).
3. [ ] **[P2]** **"Delete/deactivate entity referenced elsewhere" inconsistently handled.** Vendors warn in dialog (best); Stops rely on backend SnackBar; Shuttles soft-delete (safe). No screen pre-checks references even when data is available (e.g., vendor `shuttleCount`).
4. [ ] **[P2]** **Soft-delete + filtered-unique-index scenarios** (reactivating a shuttle whose plate now collides, re-adding a deactivated priority schedule, re-creating a stop with a soft-deleted name) are entirely backend-dependent; no client awareness.
5. [ ] **[P1]** **Raw numeric FK entry** (Schedule Times: Campus/Stop ID; Priority Schedules: Student/Stop/ScheduleTime ID) — no dropdowns, no existence validation, `int.tryParse ?? 0` masks bad input as `0`.
6. [ ] **[P1]** **Convoy/accessibility invariants** (accessible shuttle at front / lowest OrderPosition; capacity = sum of operational vehicles) not enforced in reorder UI or replacement-shuttle picker.
7. [ ] **[P2]** **Concurrent edit / optimistic locking absent everywhere** — no ETags/version fields; all mutations last-write-wins.
8. [ ] **[P1]** **Route param `int.parse` crashes** on malformed URLs; **no client-side role guard** on admin routes.
9. [ ] **[P3]** **Dialog-scoped `TextEditingController`s not disposed** across Stops/Shuttles/Vendors add/edit dialogs (memory leak on repeated opens).
10. [ ] **[P1]** **`stopMarshalProvider` conflates fetch-failure with "no marshal"** (returns null on any error).
11. [ ] **[P1]** **Priority schedules never verify the disability-clearance precondition** client-side, and there's no student/schedule lookup.
12. [ ] **[P1]** **Calendar exceptions can't express multi-day recesses** in a single entry, despite recess being a first-class trip-generation skip condition.
