# Flutter & Android Studio Setup

Guide for setting up Flutter with Android Studio on this machine. The Flutter SDK and Android SDK are not on PATH, so explicit paths are required.

---

## SDK Locations

| Tool | Path |
|------|------|
| Flutter SDK | `C:\Users\s229895735\Documents\flutter` |
| Android SDK | `C:\AndroidSDK` |
| .NET SDK | `C:\Users\s229895735\Documents\dotnet-sdk-9.0.315-win-x64` |

---

## 1. Tell Flutter Where the Android SDK Is

Flutter won't detect the emulator unless it knows the Android SDK path:

```powershell
C:\Users\s229895735\Documents\flutter\bin\flutter.bat config --android-sdk "C:\AndroidSDK"
```

Restart any open editors after running this.

---

## 2. Verify ADB Sees the Emulator

```powershell
& "C:\AndroidSDK\platform-tools\adb.exe" devices
```

Expected output:

```
List of devices attached
emulator-5554   device
```

If the emulator is running but not listed, restart the ADB server:

```powershell
& "C:\AndroidSDK\platform-tools\adb.exe" kill-server
& "C:\AndroidSDK\platform-tools\adb.exe" start-server
```

---

## 3. Verify Flutter Detects the Emulator

```powershell
C:\Users\s229895735\Documents\flutter\bin\flutter.bat devices
```

You should see something like:

```
sdk gphone64 x86 64 (mobile) • emulator-5554 • android-x64 • Android 14 (API 34) (emulator)
```

---

## 4. Run the App on the Emulator

```powershell
C:\Users\s229895735\Documents\flutter\bin\flutter.bat run -d emulator-5554
```

From the `AppUI` directory:

```powershell
cd C:\Users\s229895735\NMUShuttles\AppUI
C:\Users\s229895735\Documents\flutter\bin\flutter.bat run -d emulator-5554
```

---

## 5. Common Flutter Commands

Since the Flutter SDK is not on PATH, always use the full path:

```powershell
# Get dependencies
C:\Users\s229895735\Documents\flutter\bin\flutter.bat pub get

# Build APK
C:\Users\s229895735\Documents\flutter\bin\flutter.bat build apk

# Run on emulator
C:\Users\s229895735\Documents\flutter\bin\flutter.bat run -d emulator-5554

# Run on Chrome (web portal)
C:\Users\s229895735\Documents\flutter\bin\flutter.bat run -d chrome

# Flutter doctor (diagnose issues)
C:\Users\s229895735\Documents\flutter\bin\flutter.bat doctor

# List available emulators
C:\Users\s229895735\Documents\flutter\bin\flutter.bat emulators

# Launch an emulator
C:\Users\s229895735\Documents\flutter\bin\flutter.bat emulators --launch <emulator_id>
```

---

## 6. Troubleshooting

### Flutter can't find emulator

1. Confirm the emulator is running in Android Studio (Device Manager).
2. Run `adb devices` with full path to check connectivity.
3. Re-run `flutter config --android-sdk "C:\AndroidSDK"`.
4. Restart the editor / terminal.

### Port 5000 blocked (backend won't start)

Windows often reserves port 5000 (Hyper-V). The backend is configured to use port **5050** instead. If it still fails:

```powershell
# Check what's using a port
netstat -ano | findstr :5050

# Use a custom port at runtime
dotnet run --project NmuShuttle.Api --urls "http://localhost:5060"
```

Remember to update the Flutter app's base URL if you change the backend port.

### Emulator can't reach the backend

The Android emulator uses `10.0.2.2` to access the host machine's `localhost`. The app is configured to hit `http://10.0.2.2:5050/api/v1`. Make sure:

1. The backend is running on port 5050.
2. Windows Firewall isn't blocking the connection.
3. You're not using `localhost` in the mobile app (that refers to the emulator itself).

---

## 7. API Base URLs

| App | URL | Why |
|-----|-----|-----|
| Mobile (Android emulator) | `http://10.0.2.2:5050/api/v1` | `10.0.2.2` maps to host's localhost inside the emulator |
| Web Portal (Chrome) | `http://localhost:5050/api/v1` | Browser runs on host, direct localhost access |

These are configured in:
- `AppUI/lib/core/api/dio_client.dart`
- `AppUI/lib/core/api/api_constants.dart`
- `WebPortalUI/lib/core/api/dio_client.dart`
