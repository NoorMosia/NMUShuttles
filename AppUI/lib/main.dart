import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/router/app_router.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
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
    );
  }
}
