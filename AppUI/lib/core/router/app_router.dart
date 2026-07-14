import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:nmu_shuttle_app/core/auth/auth_service.dart';
import 'package:nmu_shuttle_app/features/auth/screens/login_screen.dart';
import 'package:nmu_shuttle_app/features/student/screens/student_home_screen.dart';
import 'package:nmu_shuttle_app/features/student/screens/new_booking_screen.dart';
import 'package:nmu_shuttle_app/features/student/screens/booking_detail_screen.dart';
import 'package:nmu_shuttle_app/features/student/screens/maps_screen.dart';
import 'package:nmu_shuttle_app/features/student/screens/alerts_screen.dart';
import 'package:nmu_shuttle_app/features/student/screens/profile_screen.dart';
import 'package:nmu_shuttle_app/features/student/screens/report_form_screen.dart';
import 'package:nmu_shuttle_app/features/marshal/screens/marshal_home_screen.dart';
import 'package:nmu_shuttle_app/features/marshal/screens/passenger_list_screen.dart';
import 'package:nmu_shuttle_app/shared/widgets/student_shell.dart';

/// Application router configuration using go_router.
/// Uses ShellRoute to keep the bottom navigation bar persistent across student screens.
class AppRouter {
  AppRouter({AuthService? authService})
      : _authService = authService ?? AuthService();

  final AuthService _authService;

  late final GoRouter router = GoRouter(
    initialLocation: '/login',
    redirect: _globalRedirect,
    routes: [
      GoRoute(
        path: '/login',
        name: 'login',
        builder: (context, state) => const LoginScreen(),
      ),

      // ─── Student Shell (persistent bottom nav) ───────────────
      ShellRoute(
        builder: (context, state, child) => StudentShell(child: child),
        routes: [
          GoRoute(
            path: '/student/home',
            name: 'studentHome',
            builder: (context, state) => const StudentHomeScreen(),
          ),
          GoRoute(
            path: '/student/maps',
            name: 'maps',
            builder: (context, state) => const MapsScreen(),
          ),
          GoRoute(
            path: '/student/alerts',
            name: 'alerts',
            builder: (context, state) => const AlertsScreen(),
          ),
          GoRoute(
            path: '/student/profile',
            name: 'profile',
            builder: (context, state) => const ProfileScreen(),
          ),
        ],
      ),

      // ─── Student routes outside the shell (no bottom nav) ────
      GoRoute(
        path: '/student/booking/new',
        name: 'newBooking',
        builder: (context, state) => const NewBookingScreen(),
      ),
      GoRoute(
        path: '/student/booking/:id',
        name: 'bookingDetail',
        builder: (context, state) {
          final id = state.pathParameters['id']!;
          return BookingDetailScreen(bookingId: id);
        },
      ),
      GoRoute(
        path: '/student/report/new',
        name: 'newReport',
        builder: (context, state) => const ReportFormScreen(),
      ),

      // ─── Marshal routes ──────────────────────────────────────
      GoRoute(
        path: '/marshal/home',
        name: 'marshalHome',
        builder: (context, state) => const MarshalHomeScreen(),
      ),
      GoRoute(
        path: '/marshal/passengers/:tripId',
        name: 'passengerList',
        builder: (context, state) {
          final tripId = state.pathParameters['tripId']!;
          return PassengerListScreen(tripId: tripId);
        },
      ),
    ],
  );

  /// Global redirect: if not authenticated, force to /login.
  /// If authenticated and on /login, redirect to appropriate home.
  /// If marshal tries to access student routes, redirect to marshal home.
  Future<String?> _globalRedirect(
    BuildContext context,
    GoRouterState state,
  ) async {
    final isAuthenticated = await _authService.isAuthenticated();
    final isOnLogin = state.matchedLocation == '/login';

    if (!isAuthenticated && !isOnLogin) {
      return '/login';
    }

    if (isAuthenticated && isOnLogin) {
      final role = await _authService.getUserRole();
      if (role == 'Marshal') {
        return '/marshal/home';
      }
      return '/student/home';
    }

    // Prevent marshals from accessing student routes
    if (isAuthenticated && state.matchedLocation.startsWith('/student')) {
      final role = await _authService.getUserRole();
      if (role == 'Marshal') {
        return '/marshal/home';
      }
    }

    // Prevent students from accessing marshal routes
    if (isAuthenticated && state.matchedLocation.startsWith('/marshal')) {
      final role = await _authService.getUserRole();
      if (role == 'Student') {
        return '/student/home';
      }
    }

    return null;
  }
}
