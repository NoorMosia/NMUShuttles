# Project Structure

## Current Repository Layout

This repository contains the domain specification, seed data, UI prototypes, and implementation code for the NMU Shuttle Management system.

```
/
├── .kiro/
│   ├── specs/nmu-shuttle-management/   # Spec documents (requirements, design, tasks)
│   └── steering/                       # AI steering rules (this folder)
├── Backend/                            # .NET 9 Web API (solution + project)
│   ├── NmuShuttle.sln                  # Solution file
│   └── NmuShuttle.Api/                 # ASP.NET Core Web API project
│       ├── Controllers/                # API controllers
│       ├── Controllers/Admin/          # Admin-only controllers
│       ├── Services/                   # Business logic services
│       ├── BackgroundServices/         # IHostedService implementations
│       ├── Data/                       # DbContext, seed data, migrations
│       ├── Models/                     # EF Core entity models
│       ├── DTOs/                       # Request/response DTOs
│       ├── Infrastructure/             # Cross-cutting concerns (exception handler, etc.)
│       └── Validators/                 # FluentValidation validators
├── AppUI/                              # Flutter mobile app (Android only — Student + Marshal)
├── WebPortalUI/                        # Flutter Web admin portal
├── assets/                             # Shared assets (logos, SVGs)
│   ├── logo.svg                        # QR code center image
│   └── logos/1.png                     # Main logo (website + login pages)
├── MySpecGuide/                        # Domain specification, seed data, and UI prototypes
│   ├── readme.md                       # Authoritative domain model
│   ├── firstSeed.md                    # Canonical seed data
│   ├── MoreRules.md                    # Additional business rules
│   ├── seedData/                       # Reference seed data in markdown tables
│   ├── SchoolScheduleFiles/            # University schedule PDFs
│   └── UI inspirations html files/     # Static HTML mockups (reference only)
├── appsettings.Development.json        # .NET config template
├── .env                                # Environment variable template
└── .gitignore                          # Ignores for .NET, Flutter, IDE, and OS files
```

## Key Reference Files

| File | Purpose |
|------|---------|
| `MySpecGuide/readme.md` | Authoritative domain model — all entities, attributes, relationships, and business invariants |
| `MySpecGuide/firstSeed.md` | Canonical seed data for development/testing |
| `MySpecGuide/MoreRules.md` | Supplementary business rules (marshal workflow, scan timing, shuttle ordering, priority placement) |
| `MySpecGuide/UI inspirations html files/` | Visual reference for building Flutter screens — not deployable code |

## Notes

- The HTML files use Tailwind CDN + Material Symbols — they are design references only, not part of the build pipeline.
- Seed data files in `MySpecGuide/seedData/` are duplicates/subsets of `firstSeed.md` (the canonical source).
- When implementing, follow the schema and API design in `.kiro/specs/nmu-shuttle-management/design.md`.
