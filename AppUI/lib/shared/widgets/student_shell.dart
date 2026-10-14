import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/student/providers/notification_providers.dart';
import 'package:nmu_shuttle_app/shared/widgets/app_drawer.dart';

/// Global key for the student shell scaffold, allowing child screens
/// to open the drawer that overlays the entire screen including bottom nav.
final studentShellScaffoldKey = GlobalKey<ScaffoldState>();

/// Shell widget that wraps student screens with a persistent bottom navigation bar.
/// The body content changes based on the current route, but the nav bar stays visible.
class StudentShell extends ConsumerWidget {
  const StudentShell({super.key, required this.child});

  final Widget child;

  static const _tabs = [
    '/student/home',
    '/student/maps',
    '/student/alerts',
    '/student/profile',
  ];

  int _currentIndex(BuildContext context) {
    final location = GoRouterState.of(context).matchedLocation;
    for (int i = 0; i < _tabs.length; i++) {
      if (location.startsWith(_tabs[i])) return i;
    }
    return 0;
  }

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final currentIndex = _currentIndex(context);
    final unreadCountAsync = ref.watch(unreadNotificationCountProvider);
    final unreadCount = unreadCountAsync.valueOrNull ?? 0;

    return Scaffold(
      key: studentShellScaffoldKey,
      drawer: AppDrawer(
        onLogout: () => context.go('/login'),
      ),
      body: child,
      bottomNavigationBar: NavigationBar(
        selectedIndex: currentIndex,
        onDestinationSelected: (index) {
          if (index != currentIndex) {
            context.go(_tabs[index]);
          }
        },
        destinations: [
          const NavigationDestination(
            icon: Icon(Icons.home_outlined),
            selectedIcon: Icon(Icons.home),
            label: 'Home',
          ),
          const NavigationDestination(
            icon: Icon(Icons.map_outlined),
            selectedIcon: Icon(Icons.map),
            label: 'Maps',
          ),
          NavigationDestination(
            icon: unreadCount > 0
                ? Badge(
                    backgroundColor: AppTheme.primaryGold,
                    textColor: AppTheme.navy,
                    label: Text('$unreadCount'),
                    child: const Icon(Icons.notifications_outlined),
                  )
                : const Icon(Icons.notifications_outlined),
            selectedIcon: unreadCount > 0
                ? Badge(
                    backgroundColor: AppTheme.primaryGold,
                    textColor: AppTheme.navy,
                    label: Text('$unreadCount'),
                    child: const Icon(Icons.notifications),
                  )
                : const Icon(Icons.notifications),
            label: 'Alerts',
          ),
          const NavigationDestination(
            icon: Icon(Icons.person_outlined),
            selectedIcon: Icon(Icons.person),
            label: 'Profile',
          ),
        ],
      ),
    );
  }
}
