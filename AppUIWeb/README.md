# NMU Shuttle — Web

Flutter **Web** variant of the NMU Shuttle app (Student + Marshal), running in a
browser. It is a thin shell that **reuses the mobile app's codebase** — screens,
routing, providers, repositories and models — from `../AppUI` via a path
dependency (`nmu_shuttle_app`). Only the startup wiring differs.

## How it relates to AppUI

| | AppUI (mobile) | AppUIWeb (this project) |
|---|---|---|
| Platform | Android | Web (browser) |
| Entry point | `AppUI/lib/main.dart` | `AppUIWeb/lib/main.dart` |
| Screens / routing / state | owns them | reused from AppUI |
| Native splash | yes (`flutter_native_splash`) | skipped |
| Firebase Cloud Messaging | initialised | **not** initialised (see below) |

Because the UI, routing and data layers live in `AppUI`, changes to screens or
business logic there are automatically picked up here — no copies to keep in
sync. The only files owned by this project are `lib/main.dart`, the `web/`
folder, `pubspec.yaml`, and a copy of `assets/` (the reused screens load assets
by plain path, so they must exist in this app's bundle).

## Push notifications on web

The web entry point intentionally does **not** call
`PushNotificationService.initialize()`. The service is designed to be safe when
uninitialised: `isAvailable` is `false`, `getToken()` returns `null`, and
`onTokenRefresh` is an empty stream. As a result:

- Login skips FCM device registration (it has no token to send).
- The alerts screen stays current by polling the API every 30s
  (`notificationsProvider`), not by push delivery.

Wiring real web push (service worker + VAPID key) is a separate, optional task.

## API base URL

The base URL comes from `EnvConfig` in AppUI (shared). It is currently set to
`useNgrok = true`, which points at the ngrok tunnel and works from a browser.
Note: the `useNgrok = false` fallback (`http://10.0.2.2:5050`) is the Android
emulator alias and is **not** valid in a browser — use `localhost` there if you
change it.

## Run / build

The Flutter SDK is not on PATH; use the full path:

```powershell
# Install dependencies
C:\Users\s229895735\Documents\flutter\bin\flutter.bat pub get

# Run in Chrome (dev)
C:\Users\s229895735\Documents\flutter\bin\flutter.bat run -d chrome

# Production build (output in build/web)
C:\Users\s229895735\Documents\flutter\bin\flutter.bat build web
```
