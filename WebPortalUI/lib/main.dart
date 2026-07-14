import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/core/router/admin_router.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const ProviderScope(child: AdminApp()));
}

class AdminApp extends ConsumerWidget {
  const AdminApp({super.key});

  static const Color primaryGold = Color(0xFFFFCC00);
  static const Color navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(adminRouterProvider);

    return MaterialApp.router(
      title: 'NMU Shuttle Admin',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        brightness: Brightness.light,
        colorScheme: ColorScheme.light(
          primary: primaryGold,
          onPrimary: navy,
          secondary: navy,
          onSecondary: Colors.white,
          surface: Colors.white,
          onSurface: navy,
          error: Colors.red.shade700,
          onError: Colors.white,
        ),
        textTheme: GoogleFonts.hankenGroteskTextTheme(),
        scaffoldBackgroundColor: const Color(0xFFF5F5F5),
        drawerTheme: const DrawerThemeData(
          backgroundColor: Colors.white,
        ),
        navigationDrawerTheme: NavigationDrawerThemeData(
          backgroundColor: Colors.white,
          indicatorColor: primaryGold.withOpacity(0.2),
          labelTextStyle: WidgetStateProperty.resolveWith((states) {
            if (states.contains(WidgetState.selected)) {
              return GoogleFonts.hankenGrotesk(
                color: navy,
                fontWeight: FontWeight.w600,
              );
            }
            return GoogleFonts.hankenGrotesk(color: navy);
          }),
        ),
      ),
      routerConfig: router,
    );
  }
}
