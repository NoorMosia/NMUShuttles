# Tech Stack

## Backend

- **Runtime**: .NET 8 (ASP.NET Core Web API)
- **ORM**: Entity Framework Core (Code-First migrations)
- **Database**: Azure SQL Server (SQL Server)
- **Auth**: JWT Bearer tokens (3 roles: Student, Marshal, Admin)
- **Push notifications**: Firebase Cloud Messaging (FCM)
- **Background services**: IHostedService (Rolling Injection Job + Lockout Gate)
- **Password hashing**: BCrypt (cost factor 12) for admin passwords
- **OTP**: 5-digit codes, 5-minute expiry, hashed storage

## Frontend

- **Mobile app**: Flutter (Android + iOS) — serves both Student and Marshal roles (role-switched via JWT claims)
- **Admin portal**: Flutter Web
- **UI design system**: Material Design 3 (native Flutter Material widgets, no Tailwind)
- **Typography**: Hanken Grotesk font family (via Google Fonts package)
- **Color system**: Material You token-based (primary gold #ffcc00, surface whites/warm neutrals) — implemented with Flutter's `ColorScheme.fromSeed` or custom `ThemeData`
- **Icons**: Material Symbols Outlined (via Flutter's built-in Icons or material_symbols package)
- **Styling**: Native Flutter widgets and theming only — no CSS frameworks. The HTML prototypes in this repo use Tailwind for visual reference but the actual Flutter implementation must use Flutter's native styling (BoxDecoration, ThemeData, etc.)

## Infrastructure

- **Cloud**: Azure (SQL Database, Communication Services)
- **Email**: Azure Communication Services (Email) — used exclusively for OTP delivery to students and marshals
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
# Restore NuGet packages
dotnet restore

# Run migrations
dotnet ef database update

# Add a new migration
dotnet ef migrations add <MigrationName>

# Run the API in development
dotnet run

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
| AZURE_COMM_CONNECTION_STRING | Azure Communication Services connection string |
| AZURE_COMM_SENDER_ADDRESS | Verified sender email (e.g., DoNotReply@your-domain.azurecomm.net) |
