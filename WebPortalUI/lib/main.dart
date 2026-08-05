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
          primary: navy,
          onPrimary: Colors.white,
          secondary: primaryGold,
          onSecondary: navy,
          surface: Colors.white,
          onSurface: navy,
          surfaceContainerHighest: const Color(0xFFF8F9FB),
          error: Colors.red.shade700,
          onError: Colors.white,
          outline: const Color(0xFFE8ECF0),
        ),
        textTheme: GoogleFonts.hankenGroteskTextTheme(),
        scaffoldBackgroundColor: const Color(0xFFF8F9FB),
        cardTheme: CardThemeData(
          elevation: 0,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(12),
            side: const BorderSide(color: Color(0xFFE8ECF0)),
          ),
          color: Colors.white,
        ),
        dividerTheme: const DividerThemeData(
          color: Color(0xFFE8ECF0),
          thickness: 1,
          space: 1,
        ),
        filledButtonTheme: FilledButtonThemeData(
          style: FilledButton.styleFrom(
            backgroundColor: navy,
            foregroundColor: Colors.white,
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
            textStyle: GoogleFonts.hankenGrotesk(
              fontWeight: FontWeight.w600,
              fontSize: 14,
            ),
          ),
        ),
        outlinedButtonTheme: OutlinedButtonThemeData(
          style: OutlinedButton.styleFrom(
            foregroundColor: navy,
            side: const BorderSide(color: Color(0xFFE8ECF0)),
            padding: const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
            textStyle: GoogleFonts.hankenGrotesk(
              fontWeight: FontWeight.w600,
              fontSize: 14,
            ),
          ),
        ),
        inputDecorationTheme: InputDecorationTheme(
          filled: true,
          fillColor: const Color(0xFFF8F9FB),
          border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(10),
            borderSide: const BorderSide(color: Color(0xFFE8ECF0)),
          ),
          enabledBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(10),
            borderSide: const BorderSide(color: Color(0xFFE8ECF0)),
          ),
          focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(10),
            borderSide: BorderSide(color: navy, width: 1.5),
          ),
          contentPadding:
              const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
          labelStyle: GoogleFonts.hankenGrotesk(
            fontSize: 14,
            color: const Color(0xFF5A6B7B),
          ),
        ),
        dataTableTheme: DataTableThemeData(
          headingTextStyle: GoogleFonts.hankenGrotesk(
            fontSize: 11,
            fontWeight: FontWeight.w600,
            letterSpacing: 0.5,
            color: const Color(0xFF9CA3AF),
          ),
          dataTextStyle: GoogleFonts.hankenGrotesk(
            fontSize: 14,
            color: navy,
          ),
          headingRowColor:
              WidgetStateProperty.all(const Color(0xFFF8F9FB)),
          dataRowColor: WidgetStateProperty.all(Colors.transparent),
        ),
        dialogTheme: DialogThemeData(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          titleTextStyle: GoogleFonts.hankenGrotesk(
            fontSize: 18,
            fontWeight: FontWeight.w600,
            color: navy,
          ),
        ),
      ),
      routerConfig: router,
    );
  }
}
