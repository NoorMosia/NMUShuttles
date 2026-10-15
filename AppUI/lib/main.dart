import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_native_splash/flutter_native_splash.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/notifications/push_notification_service.dart';
import 'package:nmu_shuttle_app/core/router/app_router.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/student/providers/notification_providers.dart';

Future<void> main() async {
  final widgetsBinding = WidgetsFlutterBinding.ensureInitialized();
  FlutterNativeSplash.preserve(widgetsBinding: widgetsBinding);

  // Initialise Firebase + push notifications. Failures must not block startup
  // (e.g. missing google-services.json in a dev build).
  try {
    await PushNotificationService.instance.initialize();
  } catch (e) {
    debugPrint('[FCM] Push notification init failed: $e');
  }

  // White status bar background with dark icons
  SystemChrome.setSystemUIOverlayStyle(const SystemUiOverlayStyle(
    statusBarColor: Colors.white,
    statusBarIconBrightness: Brightness.dark,
    statusBarBrightness: Brightness.light,
  ));

  runApp(
    const ProviderScope(
      child: NmuShuttleApp(),
    ),
  );
}

class NmuShuttleApp extends ConsumerStatefulWidget {
  const NmuShuttleApp({super.key});

  @override
  ConsumerState<NmuShuttleApp> createState() => _NmuShuttleAppState();
}

class _NmuShuttleAppState extends ConsumerState<NmuShuttleApp> {
  late final AppRouter _appRouter;

  @override
  void initState() {
    super.initState();
    _appRouter = AppRouter();
    FlutterNativeSplash.remove();

    // Route notification taps (foreground, background, or cold-start) to the
    // relevant alerts screen, refreshing the list first so it reflects the new
    // notification. The data payload is set by the backend.
    PushNotificationService.instance.setNotificationTapHandler((data) {
      ref.invalidate(notificationsProvider);
      final ownerType = data['ownerType']?.toString();
      final location =
          ownerType == 'Marshal' ? '/marshal/alerts' : '/student/alerts';
      _appRouter.router.go(location);
    });

    // Refresh the notifications list whenever a push arrives in the foreground,
    // so the alerts screen updates live instead of needing a pull-to-refresh.
    PushNotificationService.instance.setMessageReceivedHandler(() {
      ref.invalidate(notificationsProvider);
    });
  }

  @override
  Widget build(BuildContext context) {
    // Keep the FCM token-refresh subscription alive for the app's lifetime.
    ref.watch(fcmTokenRefreshProvider);

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
