import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_native_splash/flutter_native_splash.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/router/app_router.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';

void main() {
  final widgetsBinding = WidgetsFlutterBinding.ensureInitialized();
  FlutterNativeSplash.preserve(widgetsBinding: widgetsBinding);

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

class NmuShuttleApp extends StatefulWidget {
  const NmuShuttleApp({super.key});

  @override
  State<NmuShuttleApp> createState() => _NmuShuttleAppState();
}

class _NmuShuttleAppState extends State<NmuShuttleApp> {
  late final AppRouter _appRouter;

  @override
  void initState() {
    super.initState();
    _appRouter = AppRouter();
    FlutterNativeSplash.remove();
  }

  // @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
      title: 'NMU Shuttle',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.lightTheme,
      darkTheme: AppTheme.darkTheme,
      themeMode: ThemeMode.light,
      routerConfig: _appRouter.router,
      // Global: tapping anywhere outside a form input dismisses the keyboard.
      builder: (context, child) {
        return GestureDetector(
          behavior: HitTestBehavior.translucent,
          onTap: () => FocusManager.instance.primaryFocus?.unfocus(),
          child: child,
        );
      },
    );
  }
}
