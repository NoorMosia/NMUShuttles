# Tech Stack

## Backend

- **Runtime**: .NET 9 (ASP.NET Core Web API)
- **ORM**: Entity Framework Core (Code-First migrations)
- **Database**: Azure SQL Server (SQL Server)
- **Auth**: JWT Bearer tokens (3 roles: Student, Marshal, Admin)
- **Push notifications**: Firebase Cloud Messaging (FCM)
- **Background services**: IHostedService (Rolling Injection Job + Lockout Gate)
- **Password hashing**: BCrypt (cost factor 12) for admin passwords
- **OTP**: 5-digit codes, 5-minute expiry, hashed storage

## Frontend

- **Mobile app**: Flutter (Android only) — serves both Student and Marshal roles (role-switched via JWT claims). No iOS build target.
- **Admin portal**: Flutter Web
- **UI design system**: Material Design 3 (native Flutter Material widgets, no Tailwind)
- **Typography**: Hanken Grotesk font family (via Google Fonts package)
- **Color system**: University brand colors — primary gold `#ffcc00` and navy `#132e51`. Both light and dark themes supported, light mode is the default. Use `ColorScheme.fromSeed` or custom `ColorScheme` definitions in `ThemeData`.
- **Icons**: Material Symbols Outlined (via Flutter's built-in Icons or material_symbols package)
- **Styling**: Native Flutter widgets and theming only — no CSS frameworks. The HTML prototypes in this repo use Tailwind for visual reference but the actual Flutter implementation must use Flutter's native styling (BoxDecoration, ThemeData, etc.)
- **Navigation pattern**: No top AppBar. Use a floating hamburger icon (top-left) with a white square background and small border radius (`BorderRadius.circular(8)`). Tapping it opens a Drawer sidebar. The Logout button is pinned to the bottom of the sidebar (use a `Column` with `Spacer` or `mainAxisAlignment: end`).
- **Branding assets**:
  - QR code center image: `assets/logo.svg`
  - Main logo (website header + login pages): `assets/logos/1.png`

## Infrastructure

- **Cloud**: Azure (SQL Database)
- **Email**: Resend (https://resend.com) transactional email API — used exclusively for OTP delivery to students, marshals, and admins. Called server-side from the .NET API via a typed `HttpClient` (`ResendEmailService`), keyed by `Email:Resend:ApiKey`. Behind the `IEmailService` abstraction so the provider can be swapped without touching OTP logic.
- **Notifications**: Firebase Cloud Messaging (push notifications to devices)
- **Time zone**: SAST (UTC+2) for all scheduling logic

## API Conventions

- All routes prefixed `/api/v1`
- JWT Bearer auth on all routes except OTP request/verify and admin login
- Role guards: `[Student]`, `[Marshal]`, `[Admin]`
- RESTful design: GET for reads, POST for creates, PATCH for partial updates, DELETE for removal
- HTTP status codes: 200/201 success, 400 bad request (locked trip), 401 unauthorized, 404 not found, 409 conflict (duplicate), 422 unprocessable (validation)

## Database Conventions

- INT IDENTITY surrogate primary keys
- All timestamps stored as DATETIME2 in UTC
- NVARCHAR for strings with explicit max lengths
- CHECK constraints for enum-like columns (stored as strings, not numeric enums)
- Filtered unique indexes for soft-delete scenarios (e.g., active bookings only)
- DbContext name: `ShuttleDbContext`

## Common Commands

```bash
# Flutter commands (SDK not on PATH)
C:\Users\s229895735\Documents\flutter\bin\flutter.bat pub get
C:\Users\s229895735\Documents\flutter\bin\flutter.bat build apk
C:\Users\s229895735\Documents\flutter\bin\flutter.bat run

# .NET commands (SDK not on PATH)
$env:DOTNET_ROOT = "C:\Users\s229895735\Documents\dotnet-sdk-9.0.315-win-x64"
& "C:\Users\s229895735\Documents\dotnet-sdk-9.0.315-win-x64\dotnet.exe" build
& "C:\Users\s229895735\Documents\dotnet-sdk-9.0.315-win-x64\dotnet.exe" run

# Restore NuGet packages
cd Backend
dotnet restore

# Run migrations
dotnet ef database update --project NmuShuttle.Api

# Add a new migration
dotnet ef migrations add <MigrationName> --project NmuShuttle.Api

# Run the API in development
dotnet run --project NmuShuttle.Api

# Build
dotnet build

# Run tests
dotnet test
```

## Environment Variables

| Variable | Purpose |
|----------|---------|
| DB_CONNECTION_STRING | Azure SQL connection string |
| JWT_SECRET | JWT signing key (min 32 chars) |
| FCM_SERVICE_ACCOUNT_PATH | Path to Firebase service account JSON |
| OTP_EXPIRY_MINUTES | OTP lifetime (default: 5) |
| Email__Resend__ApiKey | Resend API key (secret — keep out of git; use user-secrets or host env var) |
| Email__Resend__FromAddress | Verified sender address (e.g., `onboarding@resend.dev` for testing, or `noreply@mandela.ac.za` once the domain is verified in Resend) |
| Email__Resend__FromName | Sender display name (default: `NMU Shuttle`) |

> Config binds via the standard .NET `Email:Resend:*` section. As environment variables use the double-underscore form (`Email__Resend__ApiKey`). For local dev, prefer user-secrets: `dotnet user-secrets set "Email:Resend:ApiKey" "<key>"` in the `NmuShuttle.Api` project. Never commit the key.

## Feature / Test Flags

| Config key | Purpose |
|------------|---------|
| `ScanWindow:OpenAllTodayTrips` | **TEST-ONLY.** When `true`, every one of *today's* not-yet-departed trips is open to QR scanning, bypassing the 35-minute pre-departure window (tomorrow-onward trips keep the normal rule). Set to `false` for production to enforce the strict 35-min window. Bound at startup via `IOptions<ScanWindowOptions>`, so an API restart is required after changing it. |

> ⚠️ `ScanWindow:OpenAllTodayTrips` must be `false` in production — it intentionally loosens who can be checked in early. Full reference and code touchpoints: `scanner.md` in the repo root.
