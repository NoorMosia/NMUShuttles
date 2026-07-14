import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/core/auth/admin_auth_service.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';
import 'package:nmu_shuttle_admin/features/auth/screens/admin_login_screen.dart';
import 'package:nmu_shuttle_admin/features/overview/screens/overview_screen.dart';
import 'package:nmu_shuttle_admin/features/reports/screens/reports_screen.dart';
import 'package:nmu_shuttle_admin/features/schedule/screens/schedule_screen.dart';
import 'package:nmu_shuttle_admin/features/shuttles/screens/shuttles_screen.dart';
import 'package:nmu_shuttle_admin/features/stops/screens/stops_screen.dart';
import 'package:nmu_shuttle_admin/features/trips/screens/trips_screen.dart';
import 'package:nmu_shuttle_admin/features/users/screens/users_screen.dart';
import 'package:nmu_shuttle_admin/shared/widgets/admin_scaffold.dart';

final adminRouterProvider = Provider<GoRouter>((ref) {
  final dioClient = ref.watch(dioClientProvider);

  final router = GoRouter(
    initialLocation: '/overview',
    redirect: (context, state) async {
      final authService = AdminAuthService();
      final isLoggedIn = await authService.isAuthenticated();
      final isLoginRoute = state.matchedLocation == '/login';

      if (!isLoggedIn && !isLoginRoute) {
        return '/login';
      }

      if (isLoggedIn && isLoginRoute) {
        return '/overview';
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/login',
        builder: (context, state) => const AdminLoginScreen(),
      ),
      ShellRoute(
        builder: (context, state, child) {
          return AdminScaffold(
            currentPath: state.matchedLocation,
            child: child,
          );
        },
        routes: [
          GoRoute(
            path: '/overview',
            builder: (context, state) => const OverviewScreen(),
          ),
          GoRoute(
            path: '/stops',
            builder: (context, state) => const StopsScreen(),
          ),
          GoRoute(
            path: '/shuttles',
            builder: (context, state) => const ShuttlesScreen(),
          ),
          GoRoute(
            path: '/users',
            builder: (context, state) => const UsersScreen(),
          ),
          GoRoute(
            path: '/schedule',
            builder: (context, state) => const ScheduleScreen(),
          ),
          GoRoute(
            path: '/calendar-exceptions',
            builder: (context, state) => const ScheduleScreen(),
          ),
          GoRoute(
            path: '/priority-schedules',
            builder: (context, state) => const ScheduleScreen(),
          ),
          GoRoute(
            path: '/trips',
            builder: (context, state) => const TripsScreen(),
          ),
          GoRoute(
            path: '/reports',
            builder: (context, state) => const ReportsScreen(),
          ),
          GoRoute(
            path: '/analytics/detailed',
            builder: (context, state) => const _DetailedAnalyticsPlaceholder(),
          ),
        ],
      ),
    ],
  );

  // Wire up the DioClient 401 handler to redirect to login with expired flag.
  dioClient.onUnauthorized = () {
    router.go('/login?expired=true');
  };

  return router;
});


/// Placeholder for the Detailed Analytics page.
class _DetailedAnalyticsPlaceholder extends StatelessWidget {
  const _DetailedAnalyticsPlaceholder();

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              IconButton(
                onPressed: () => context.go('/overview'),
                icon: const Icon(Icons.arrow_back),
                tooltip: 'Back to Overview',
                color: const Color(0xFF132E51),
              ),
              const SizedBox(width: 8),
              Text(
                'Detailed Analytics',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 28,
                  fontWeight: FontWeight.bold,
                  color: const Color(0xFF132E51),
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            'Detailed analytics view coming soon',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 14,
              color: Colors.grey.shade600,
            ),
          ),
          const SizedBox(height: 32),
          const Expanded(
            child: Center(
              child: Text('Detailed analytics content coming soon'),
            ),
          ),
        ],
      ),
    );
  }
}
