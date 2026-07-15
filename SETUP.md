# NMU Shuttles — Local Development Setup Guide

Follow these steps after cloning the repository to run the project locally.

---

## Prerequisites

| Tool | Version | Notes |
|------|---------|-------|
| .NET SDK | 9.0+ | Download from https://dotnet.microsoft.com/download |
| Flutter SDK | 3.x | Download from https://flutter.dev/docs/get-started/install |
| Android Studio | Latest | For Android emulator (mobile app) |
| Chrome | Latest | For Flutter Web (admin portal) |
| Git | Latest | For cloning the repo |

---

## 1. Clone the Repository

```bash
git clone https://github.com/<your-org>/NMUShuttles.git
cd NMUShuttles
```

---

## 2. Azure SQL Firewall — Add Your IP

The database is hosted on Azure SQL. Before the backend can connect, **the project owner must add your public IP** to the Azure SQL firewall rules.

1. Find your public IP: visit https://whatismyip.com
2. Send your IP address to the project owner
3. The owner adds it in **Azure Portal → SQL Server (nmushuttles) → Networking → Firewall rules → Add client IP**

Without this step, the backend will fail to connect to the database.

---

## 3. Backend Setup (.NET API)

### Navigate to the backend folder:
```bash
cd Backend
```

### Restore NuGet packages:
```bash
dotnet restore
```

### Run database migrations (creates tables + seed data):
```bash
dotnet ef database update --project NmuShuttle.Api
```

> If `dotnet ef` is not installed, run: `dotnet tool install --global dotnet-ef`

### Start the API server:
```bash
dotnet run --project NmuShuttle.Api
```

The API runs at **http://localhost:5000**. Swagger UI is available at http://localhost:5000/swagger.

---

## 4. Mobile App Setup (Flutter — Android)

### Navigate to the app folder:
```bash
cd AppUI
```

### Get dependencies:
```bash
flutter pub get
```

### Run on Android emulator:
```bash
flutter run
```

> The app connects to `http://10.0.2.2:5000/api/v1` by default (Android emulator's alias for host machine's localhost). The backend must be running.

### Run on a physical device:
If using a physical phone on the same WiFi, change the base URL in `AppUI/lib/core/api/dio_client.dart`:
```dart
baseUrl: 'http://<YOUR_PC_LOCAL_IP>:5000/api/v1',
```
Find your local IP with `ipconfig` (look for your WiFi adapter's IPv4 address, e.g., `192.168.1.x`).

---

## 5. Admin Portal Setup (Flutter Web)

### Navigate to the web portal folder:
```bash
cd WebPortalUI
```

### Get dependencies:
```bash
flutter pub get
```

### Run in Chrome:
```bash
flutter run -d chrome
```

> The admin portal connects to `http://localhost:5000/api/v1` by default. The backend must be running.

---

## 6. Test Credentials

### Admin Portal Login:
| Email | Password |
|-------|----------|
| admin@mandela.ac.za | Admin@2025! |
| transport@mandela.ac.za | Transport@2025! |

### Student App Login (OTP):
Use any student email from the seed data (e.g., `s312847563@mandela.ac.za`). The OTP is sent via Azure Communication Services email. For local testing, check the API console logs — the OTP is logged in development mode.

### Marshal App Login (OTP):
Use any marshal email (e.g., `b.cele@mandela.ac.za`). Same OTP process as students.

---

## 7. Project Structure

```
NMUShuttles/
├── Backend/                 ← .NET 9 Web API
│   └── NmuShuttle.Api/      ← Main project
├── AppUI/                   ← Flutter mobile app (Android — Student + Marshal)
├── WebPortalUI/             ← Flutter Web admin portal
├── MySpecGuide/             ← Domain spec, seed data, UI references
└── assets/                  ← Shared logos and images
```

---

## 8. Common Issues

### "Cannot connect to database"
→ Your IP is not in the Azure SQL firewall. Ask the project owner to add it.

### "Connection refused" on mobile app
→ The backend isn't running, or you're not using `10.0.2.2` (emulator) / your PC's local IP (physical device).

### "flutter: command not found"
→ Flutter isn't on your PATH. Add it: https://flutter.dev/docs/get-started/install

### "dotnet: command not found"
→ .NET SDK isn't on your PATH. Add it or use the full path to the executable.

### Migrations fail with FK constraint errors
→ Drop the database and recreate:
```bash
dotnet ef database drop --project NmuShuttle.Api --force
dotnet ef database update --project NmuShuttle.Api
```

---

## 9. Environment Variables (Reference)

These are configured in `appsettings.Development.json` — no `.env` file needed for local dev:

| Variable | Purpose |
|----------|---------|
| ConnectionStrings:DefaultConnection | Azure SQL connection string |
| Jwt:Secret | JWT signing key (min 32 chars) |
| Firebase:ServiceAccountPath | Path to Firebase service account JSON (for push notifications) |
| Otp:ExpiryMinutes | OTP lifetime (default: 5) |

---

## 10. Running Without dotnet on PATH

If the .NET SDK is not on your system PATH (like on lab machines), use the full path:

```powershell
# Example with SDK at a custom location:
"C:\path\to\dotnet-sdk\dotnet.exe" restore
"C:\path\to\dotnet-sdk\dotnet.exe" ef database update --project NmuShuttle.Api
"C:\path\to\dotnet-sdk\dotnet.exe" run --project NmuShuttle.Api
```

Same for Flutter:
```powershell
"C:\path\to\flutter\bin\flutter.bat" pub get
"C:\path\to\flutter\bin\flutter.bat" run
```
