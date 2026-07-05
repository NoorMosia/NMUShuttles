# Project Structure

## Current Repository Layout

This repository contains the domain specification, seed data, and UI prototypes for the NMU Shuttle Management system. The actual .NET backend and Flutter app code are to be implemented based on these artifacts.

```
/
├── .kiro/
│   ├── specs/nmu-shuttle-management/   # Spec documents (requirements, design, tasks)
│   └── steering/                       # AI steering rules (this folder)
├── seedData/                           # Reference seed data in markdown tables
│   ├── admins.md
│   ├── marshals.md
│   ├── offCampusShuttleStops.md
│   ├── onCampusShuttleStopsmd
│   ├── shuttleTimes.md
│   ├── shuttleVehicles.md
│   └── students.md
├── UI inspirations html files/         # Static HTML mockups (reference only, not production code)
│   ├── admin webapp/                   # Admin portal screen prototypes
│   │   ├── 1.auth/                     # Login screens
│   │   ├── 2.overview/                 # Dashboard views
│   │   ├── 3.stops/                    # Stop CRUD screens
│   │   ├── 4.shuttles/                # Shuttle management
│   │   ├── 4.users/                    # User management (single + bulk add)
│   │   └── 5.reports/                  # Report viewing
│   └── app/                            # Mobile app screen prototypes
│       ├── 1.auth/                     # OTP login flow
│       ├── 2.home/                     # Booking flow (form, details, upcoming)
│       ├── 3.Past/                     # Trip history
│       ├── 4.report/                   # Grievance reporting
│       ├── 5.Maps/                     # Map views, nearby stops, directions
│       ├── 6.alerts/                   # Notifications
│       ├── 7.profile/                  # User profile
│       └── 8.marshal/                  # Marshal check-in, scanning, passenger list
├── readme.md                           # Domain specification (entities, invariants, lifecycles)
├── firstSeed.md                        # Canonical seed data with all tables
├── MoreRules.md                        # Additional business rules and clarifications
├── appsettings.Development.json        # .NET config template
├── .env                                # Environment variable template
└── .gitignore                          # Ignores for .NET, Flutter, IDE, and OS files
```

## Planned Implementation Structure

The .gitignore reveals the target implementation will include:

- **.NET backend** — `bin/`, `obj/`, `*.sln`, NuGet packages
- **Flutter frontend** — `build/`, `.dart_tool/`, `.flutter-plugins`

## Key Reference Files

| File | Purpose |
|------|---------|
| `readme.md` | Authoritative domain model — all entities, attributes, relationships, and business invariants |
| `firstSeed.md` | Canonical seed data for development/testing |
| `MoreRules.md` | Supplementary business rules (marshal workflow, scan timing, shuttle ordering, priority placement) |
| `UI inspirations html files/` | Visual reference for building Flutter screens — not deployable code |

## Notes

- The HTML files use Tailwind CDN + Material Symbols — they are design references only, not part of the build pipeline.
- Seed data files in `/seedData/` are duplicates/subsets of `firstSeed.md` (the canonical source).
- When implementing, follow the schema and API design in `.kiro/specs/nmu-shuttle-management/design.md`.
