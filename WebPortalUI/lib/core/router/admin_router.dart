import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:nmu_shuttle_admin/core/auth/admin_auth_service.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';
import 'package:nmu_shuttle_admin/features/auth/screens/admin_login_screen.dart';
import 'package:nmu_shuttle_admin/features/overview/screens/overview_screen.dart';
import 'package:nmu_shuttle_admin/features/overview/screens/detailed_analytics_screen.dart';
import 'package:nmu_shuttle_admin/features/reports/screens/reports_screen.dart';
import 'package:nmu_shuttle_admin/features/schedule/screens/schedule_screen.dart';
import 'package:nmu_shuttle_admin/features/schedule/screens/calendar_exceptions_screen.dart';
import 'package:nmu_shuttle_admin/features/schedule/screens/priority_schedules_screen.dart';
import 'package:nmu_shuttle_admin/features/shuttles/screens/shuttles_screen.dart';
import 'package:nmu_shuttle_admin/features/stops/screens/stops_screen.dart';
import 'package:nmu_shuttle_admin/features/stops/screens/stop_detail_screen.dart';
import 'package:nmu_shuttle_admin/features/stops/screens/stop_trips_screen.dart';
import 'package:nmu_shuttle_admin/features/trips/screens/trips_screen.dart';
import 'package:nmu_shuttle_admin/features/trips/screens/trip_detail_screen.dart';
import 'package:nmu_shuttle_admin/features/users/screens/users_screen.dart';
import 'package:nmu_shuttle_admin/features/vendors/screens/vendors_screen.dart';
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
            path: '/stops/:id',
            builder: (context, state) {
              final id = int.parse(state.pathParameters['id']!);
              return StopDetailScreen(stopId: id);
            },
          ),
          GoRoute(
            path: '/stops/:id/trips',
            builder: (context, state) {
              final id = int.parse(state.pathParameters['id']!);
              return StopTripsScreen(stopId: id);
            },
          ),
          GoRoute(
            path: '/shuttles',
            builder: (context, state) => const ShuttlesScreen(),
          ),
          GoRoute(
            path: '/vendors',
            builder: (context, state) => const VendorsScreen(),
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
            builder: (context, state) => const CalendarExceptionsScreen(),
          ),
          GoRoute(
            path: '/priority-schedules',
            builder: (context, state) => const PrioritySchedulesScreen(),
          ),
          GoRoute(
            path: '/trips',
            builder: (context, state) => const TripsScreen(),
          ),
          GoRoute(
            path: '/trips/:id',
            builder: (context, state) {
              final id = int.parse(state.pathParameters['id']!);
              return TripDetailScreen(tripId: id);
            },
          ),
          GoRoute(
            path: '/reports',
            builder: (context, state) => const ReportsScreen(),
          ),
          GoRoute(
            path: '/analytics/detailed',
            builder: (context, state) => const DetailedAnalyticsScreen(),
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
