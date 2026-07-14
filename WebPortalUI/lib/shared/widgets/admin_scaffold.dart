import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/core/auth/admin_auth_service.dart';

/// Persistent sidebar navigation layout for the admin portal.
/// Displays a fixed NavigationDrawer on the left with the body content to the right.
class AdminScaffold extends StatelessWidget {
  const AdminScaffold({
    super.key,
    required this.currentPath,
    required this.child,
  });

  final String currentPath;
  final Widget child;

  static const Color _navy = Color(0xFF132E51);
  static const Color _gold = Color(0xFFFFCC00);

  static const List<_NavItem> _navItems = [
    _NavItem(label: 'Overview', icon: Icons.dashboard_outlined, path: '/overview'),
    _NavItem(label: 'Shuttle Stops', icon: Icons.location_on_outlined, path: '/stops'),
    _NavItem(label: 'Shuttles', icon: Icons.directions_bus_outlined, path: '/shuttles'),
    _NavItem(label: 'Users', icon: Icons.people_outlined, path: '/users'),
    _NavItem(label: 'Schedule', icon: Icons.schedule_outlined, path: '/schedule'),
    _NavItem(label: 'Calendar', icon: Icons.calendar_month_outlined, path: '/calendar-exceptions'),
    _NavItem(label: 'Priority', icon: Icons.accessibility_new_outlined, path: '/priority-schedules'),
    _NavItem(label: 'Trips', icon: Icons.trip_origin_outlined, path: '/trips'),
    _NavItem(label: 'Reports', icon: Icons.bar_chart_outlined, path: '/reports'),
  ];

  int get _selectedIndex {
    final index = _navItems.indexWhere((item) => item.path == currentPath);
    return index >= 0 ? index : 0;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Row(
        children: [
          // Persistent sidebar with explicit constraints
          Container(
            width: 260,
            constraints: const BoxConstraints(maxWidth: 260),
            child: _buildSidebar(context),
          ),
          // Vertical divider
          const VerticalDivider(width: 1, thickness: 1),
          // Body content
          Expanded(
            child: child,
          ),
        ],
      ),
    );
  }

  Widget _buildSidebar(BuildContext context) {
    return Material(
      color: Colors.white,
      child: Column(
        children: [
          // Logo header
          Padding(
            padding: const EdgeInsets.symmetric(vertical: 24, horizontal: 16),
            child: Image.asset(
              'assets/logos/1.png',
              height: 48,
              fit: BoxFit.contain,
              errorBuilder: (context, error, stackTrace) {
                return Text(
                  'NMU Shuttles',
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 20,
                    fontWeight: FontWeight.bold,
                    color: _navy,
                  ),
                );
              },
            ),
          ),
          const Divider(height: 1),
          const SizedBox(height: 8),
          // Navigation items
          Expanded(
            child: ListView.builder(
              itemCount: _navItems.length,
              padding: const EdgeInsets.symmetric(horizontal: 12),
              itemBuilder: (context, index) {
                final item = _navItems[index];
                final isSelected = index == _selectedIndex;

                return Padding(
                  padding: const EdgeInsets.symmetric(vertical: 2),
                  child: ListTile(
                    dense: true,
                    leading: Icon(
                      item.icon,
                      size: 20,
                      color: isSelected ? _navy : _navy,
                    ),
                    title: Text(
                      item.label,
                      style: GoogleFonts.hankenGrotesk(
                        fontWeight: isSelected ? FontWeight.w600 : FontWeight.w400,
                        color: isSelected ? _navy : _navy,
                        fontSize: 14,
                      ),
                    ),
                    selected: isSelected,
                    selectedTileColor: _gold,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                    onTap: () {
                      if (!isSelected) {
                        context.go(item.path);
                      }
                    },
                  ),
                );
              },
            ),
          ),
          // Logout button pinned to bottom
          const Divider(height: 1),
          Padding(
            padding: const EdgeInsets.all(12),
            child: ListTile(
              leading: Icon(Icons.logout_outlined, color: Colors.red.shade600),
              title: Text(
                'Logout',
                style: GoogleFonts.hankenGrotesk(
                  color: Colors.red.shade600,
                  fontWeight: FontWeight.w500,
                  fontSize: 14,
                ),
              ),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
              onTap: () async {
                final authService = AdminAuthService();
                await authService.deleteToken();
                if (context.mounted) {
                  context.go('/login');
                }
              },
            ),
          ),
        ],
      ),
    );
  }
}

/// Internal model for navigation items.
class _NavItem {
  const _NavItem({
    required this.label,
    required this.icon,
    required this.path,
  });

  final String label;
  final IconData icon;
  final String path;
}
