# NMU Shuttle Management Network

University transit platform for Nelson Mandela University (NMU) in Gqeberha, South Africa. Coordinates daily shuttle bookings between off-campus residential stops and university campuses.

## Core Capabilities

- **Student booking**: Students book seats on scheduled shuttle trips (up to 3 days ahead). QR-code-based boarding passes.
- **Priority accessibility**: Disability-cleared students get automatic pre-bookings via recurring priority schedules, assigned to front accessible shuttles.
- **Marshal check-in**: Field staff at boarding points verify passengers by scanning QR codes. Scanning opens 35 min before departure.
- **Admin portal**: Full CRUD over stops, shuttles, users, schedules, calendar exceptions. Vendor reliability analytics. Report management.
- **Automated operations**: Nightly rolling job creates trips 3 days ahead + injects priority bookings. Lockout gate fires at T-60 min to lock trips and promote waitlisted students.
- **Convoy model**: Multiple shuttles per trip, capacity calculated as sum of operational vehicles. Accessible shuttles at front of convoy (lowest OrderPosition).
- **Breakdown handling**: Marshal flags shuttle → Admin resolves (replace or cancel) → Push notification sent only after admin resolution.

## User Roles

| Role | Auth Method | Platform |
|------|-------------|----------|
| Student | OTP via university email | Flutter mobile (Android/iOS) |
| Marshal | OTP via university email | Flutter mobile (role-switched) |
| Admin | Email + password | Flutter web portal |

## Key Business Rules

- Trips are generated 3 days ahead at midnight (SAST), skipping Sundays, public holidays, and university recesses.
- Trip locks 60 minutes before departure — no new bookings or cancellations allowed after lock.
- Waitlist promotion runs at lock time (FIFO by CreatedAt).
- Students are assigned to shuttles at check-in time, filling front shuttle first.
- Breakdown/cancellation events tallied per shuttle and vendor for analytics.
- Stops have a FullAddress attribute for search by name or address.
