import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_web_plugins/url_strategy.dart';
import 'package:nmu_shuttle_app/core/router/app_router.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';

/// Web entry point for the NMU Shuttle app (Student + Marshal).
///
/// This reuses the mobile app's router, theme, screens, providers and
/// repositories via the `nmu_shuttle_app` path dependency. The only
/// difference from the mobile `main.dart` is startup wiring:
///
/// - No `flutter_native_splash` (mobile-only plugin).
/// - No Firebase / push-notification init. `PushNotificationService` is left
///   uninitialised on web, so it reports `isAvailable == false`, returns a
///   null FCM token and an empty token-refresh stream. The in-app alerts list
///   still works because it polls the API every 30s rather than relying on
///   push delivery.
void main() {
  WidgetsFlutterBinding.ensureInitialized();

  // Use hash-based URLs (/#/student/maps) on the web. This keeps go_router
  // deep links working on static hosts like GitHub Pages that have no
  // server-side rewrite to index.html, so a hard refresh won't 404.
  // Vercel can use path URLs via a rewrite (see vercel.json); if you deploy
  // only to Vercel you may remove this line.
  setUrlStrategy(const HashUrlStrategy());

  runApp(
    const ProviderScope(
      child: NmuShuttleWebApp(),
    ),
  );
}

class NmuShuttleWebApp extends StatefulWidget {
  const NmuShuttleWebApp({super.key});

  @override
  State<NmuShuttleWebApp> createState() => _NmuShuttleWebAppState();
}

class _NmuShuttleWebAppState extends State<NmuShuttleWebApp> {
  final AppRouter _appRouter = AppRouter();

  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: 'NMU Shuttle',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.lightTheme,
      darkTheme: AppTheme.darkTheme,
      themeMode: ThemeMode.light,
      routerConfig: _appRouter.router,
    );
  }
}
