# NMU Shuttle Management Network — Development Cost Estimate

## Project Scope Summary

This estimate covers the full NMU Shuttle Management platform as currently specified:

| Component | Complexity |
|-----------|-----------|
| .NET 9 Backend API | ~45 endpoints, 15 database entities, 2 background services, JWT auth, FCM push notifications, Azure Email OTP |
| Flutter Mobile App (Android) | ~13 screens, QR code generation/scanning, maps integration, real-time trip data, role-switched (Student + Marshal) |
| Flutter Web Admin Portal | ~8 screens, CRUD for all entities, CSV upload, analytics dashboard, incident resolution |
| Infrastructure | Azure SQL, Azure Communication Services, Firebase Cloud Messaging, ngrok/deployment |
| Database | 15 tables, migrations, seed data, complex business rules (convoy model, lockout gate, waitlist promotion) |

### Feature Breakdown

| Feature Area | Effort Level |
|---|---|
| OTP authentication (Student + Marshal + Admin) | Medium |
| Booking system (create, cancel, capacity enforcement, waitlist) | High |
| Rolling 4-day trip injection (background service) | High |
| Lockout gate + waitlist promotion (background service) | High |
| QR code check-in with convoy sequential fill | High |
| Vehicle breakdown → admin resolution → push notification | High |
| Priority schedule management + auto pre-booking | Medium |
| Admin CRUD (stops, shuttles, users, schedules, calendar exceptions) | Medium |
| Bulk CSV user upload with validation | Medium |
| Push notifications via FCM | Medium |
| Maps integration with GPS directions | Medium |
| Student reports/grievances | Low-Medium |
| Admin analytics dashboard (demand, vendor reliability) | Medium |
| Trip state machine enforcement | Medium |

**Estimated total development effort:** 2,500 – 4,000 person-hours depending on team seniority and process overhead.

---

## South African Market Rates (2025–2026)

Rates are based on published data from OfferZen, Clutch.co, Specno, and other SA industry sources.

| Developer Level | Hourly Rate (ZAR) | Monthly Salary (ZAR) |
|---|---|---|
| Junior Developer | R300 – R500/hr | R22,000 – R30,000 |
| Mid-level Developer | R500 – R800/hr | R35,000 – R50,000 |
| Senior Developer | R800 – R1,200/hr | R55,000 – R85,000 |
| Lead/Architect | R1,200 – R1,800/hr | R80,000 – R110,000 |
| UI/UX Designer | R400 – R900/hr | R30,000 – R55,000 |
| Project Manager | R600 – R1,200/hr | R45,000 – R75,000 |
| QA/Tester | R350 – R700/hr | R25,000 – R45,000 |

**Note:** Hourly rates represent what companies *charge clients*, not employee salaries. Companies apply a 2x–3.5x multiplier on salaries to cover overheads, profit, tooling, and management.

---

## Cost Estimates by Company Tier

### Tier 1: Startup / Small Agency (2–8 people)
**Examples:** Small Flutter shops, freelance teams, early-stage dev agencies

| Item | Estimate |
|---|---|
| Team | 2–3 developers, 1 designer (part-time), no dedicated PM |
| Hourly rate charged | R350 – R600/hr |
| Timeline | 5–7 months |
| Process | Lean, minimal documentation, direct client communication |

| | Lowest | Average | Highest |
|---|---|---|---|
| **Total Quote** | **R650,000** | **R950,000** | **R1,300,000** |

**What you get:** Functional system with all core features. Less polish on edge cases, minimal automated testing, thinner documentation. May cut corners on background service robustness or analytics depth.

**Risks:** Key-person dependency, limited capacity for parallel work, potential timeline slippage if a dev leaves.

---

### Tier 2: Mid-size Agency (15–80 people)
**Examples:** Specno (Cape Town), Retro Rabbit (Pretoria), Korbitec, Polymorph, Synthesis

| Item | Estimate |
|---|---|
| Team | 3–5 developers, 1 designer, 1 QA, 1 PM/Scrum Master |
| Hourly rate charged | R700 – R1,100/hr |
| Timeline | 4–6 months |
| Process | Agile sprints, design system, CI/CD, code reviews, staging environments |

| | Lowest | Average | Highest |
|---|---|---|---|
| **Total Quote** | **R1,400,000** | **R2,100,000** | **R2,800,000** |

**What you get:** Professionally built system with proper architecture, automated tests, deployment pipelines, documentation, handover support. Well-defined UX. Analytics dashboard built to spec.

**Industry reference:** Specno publicly states R740k for a basic MVP (4 months) and R1.4m+ for complex builds (6–8 months). This project exceeds their "complex" threshold due to the three client surfaces, background services, and integrations.

---

### Tier 3: Large / Enterprise Consultancy (100+ people)
**Examples:** DVT, Entelect, BBD, Accenture South Africa, Deloitte Digital

| Item | Estimate |
|---|---|
| Team | 5–8 developers, 2 designers, 2 QA, 1 PM, 1 architect, 1 BA |
| Hourly rate charged | R1,100 – R1,800/hr |
| Timeline | 4–8 months |
| Process | Full enterprise methodology, extensive documentation, security audits, accessibility compliance, SLA-backed support, formal change control |

| | Lowest | Average | Highest |
|---|---|---|---|
| **Total Quote** | **R2,800,000** | **R3,800,000** | **R5,500,000** |

**What you get:** Enterprise-grade delivery with formal architecture review, security penetration testing, load testing, comprehensive documentation, training materials, 12-month warranty, and SLA-backed maintenance. Often includes a discovery phase billed separately (R150k–R400k).

**Why so expensive:** Large firms bill overhead for account management, compliance processes, governance meetings, and multi-layered QA. Universities often pay this premium because procurement processes favour established vendors with proven track records and formal SLAs.

---

## Summary Comparison Table

| Company Tier | Lowest | Average | Highest |
|---|---|---|---|
| Startup / Small Agency | R650,000 | R950,000 | R1,300,000 |
| Mid-size Agency | R1,400,000 | R2,100,000 | R2,800,000 |
| Large Enterprise Consultancy | R2,800,000 | R3,800,000 | R5,500,000 |

---

## Additional Costs Not Included Above

These are typically quoted separately or as ongoing costs:

| Item | Estimated Annual Cost |
|---|---|
| Azure SQL Database (Basic/Standard tier) | R12,000 – R60,000/year |
| Azure Communication Services (email OTP) | R2,000 – R10,000/year (volume-dependent) |
| Firebase (FCM is free, but Blaze plan for scale) | R0 – R5,000/year |
| Domain + SSL | R500 – R2,000/year |
| Ongoing maintenance (15–20% of build cost) | R100,000 – R1,000,000/year |
| App Store submission (Google Play) | R450 once-off |
| ngrok (paid plan for stable URLs during dev) | R1,500/year |

---

## Timeline Estimates

| Company Tier | Discovery | Design | Development | Testing & QA | Deployment |
|---|---|---|---|---|---|
| Startup | 1–2 weeks | 2–3 weeks | 14–20 weeks | 2–3 weeks | 1 week |
| Mid-size | 2–3 weeks | 3–4 weeks | 12–16 weeks | 3–4 weeks | 1–2 weeks |
| Enterprise | 3–6 weeks | 4–6 weeks | 12–20 weeks | 4–6 weeks | 2–4 weeks |

---

## What Makes This Project "Medium-High" Complexity

1. **Three client surfaces** — mobile app (dual-role), web admin portal, and backend API
2. **Two background services** — time-triggered automation (rolling injection + lockout gate)
3. **Complex business logic** — convoy capacity model, sequential fill, waitlist promotion, trip state machine
4. **Three external integrations** — Azure Communication Services, Firebase Cloud Messaging, Google Maps
5. **Role-based access** — three distinct roles with different auth flows (OTP vs password)
6. **Real-time capacity management** — dynamic recalculation on breakdown/replacement events
7. **QR code system** — generation, display, scanning, and verification
8. **Accessibility requirements** — priority pre-booking, accessible shuttle ordering

---

## Recommendation

For a university with budget constraints, a **mid-size agency** offers the best balance of quality, process rigour, and value. Expect to pay **R1.5m – R2.5m** for a production-ready system with proper testing, documentation, and a deployment pipeline.

If budget is the primary concern, a capable **startup agency** with Flutter + .NET experience can deliver a functional system for **R800k – R1.2m**, but expect to invest more in post-delivery hardening and documentation.

If the university's procurement process requires enterprise-grade SLAs, formal governance, and vendor risk mitigation, budget **R3m – R4.5m** for a large consultancy.

---

## Sources

- [Specno — App development costs in South Africa](https://specno.com/blog/app-cost-south-africa) (2023)
- [Zealousys — Mobile App Development Cost in South Africa 2026](https://zealousys.com/blog/mobile-app-development-costs-in-south-africa/) (2025)
- [OfferZen — Software Developer Salary Trends](https://www.offerzen.com/blog/software-developer-salary-south-africa) (2026)
- [Clutch.co — Top Flutter Developers in South Africa](https://clutch.co/za/developers/flutter) (2026)
- [Clutch.co — Top Software Developers for Educational Institutions in SA](https://clutch.co/za/developers/education-industry) (2026)
- [WeAreDevelopers — Software Developer Salary in South Africa](https://wearedevelopers.com/magazine/software-developer-salary-in-south-africa) (2025)
- [Trio.dev — Software Developer Salary in South Africa](https://trio.dev/developer-salary-in-south-africa/) (2025)
- [Indeed — DVT Developer Salaries](https://za.indeed.com/cmp/Dvt/salaries) (2025)

*Content was rephrased for compliance with licensing restrictions. All figures represent publicly available market data.*

---

*Report generated: July 2026*
*Project: NMU Shuttle Management Network*
*Currency: South African Rand (ZAR)*
