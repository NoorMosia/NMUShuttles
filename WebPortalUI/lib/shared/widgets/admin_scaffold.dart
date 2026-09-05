import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/core/auth/admin_auth_service.dart';

/// Option B layout: top bar + collapsible/sticky sidebar.
/// Sidebar starts collapsed (icons only) and can be pinned open via toggle.
class AdminScaffold extends StatefulWidget {
  const AdminScaffold({
    super.key,
    required this.currentPath,
    required this.child,
  });

  final String currentPath;
  final Widget child;

  @override
  State<AdminScaffold> createState() => _AdminScaffoldState();
}

class _AdminScaffoldState extends State<AdminScaffold>
    with SingleTickerProviderStateMixin {
  static const Color _navy = Color(0xFF132E51);
  static const Color _gold = Color(0xFFFFCC00);
  static const Color _border = Color(0xFFE8ECF0);
  static const Color _bg = Color(0xFFF8F9FB);
  static const Color _textSecondary = Color(0xFF5A6B7B);
  static const Color _textTertiary = Color(0xFF9CA3AF);

  static const double _collapsedWidth = 68.0;
  static const double _expandedWidth = 240.0;
  static const double _topBarHeight = 56.0;
  static const Duration _animDuration = Duration(milliseconds: 250);

  bool _isExpanded = false;

  late final AnimationController _animController;
  late final Animation<double> _widthAnimation;

  static const List<_NavItem> _navItems = [
    _NavItem(label: 'Overview', icon: Icons.dashboard_outlined, path: '/overview'),
    _NavItem(label: 'Shuttle Stops', icon: Icons.location_on_outlined, path: '/stops'),
    _NavItem(label: 'Shuttles', icon: Icons.directions_bus_outlined, path: '/shuttles'),
    _NavItem(label: 'Vendors', icon: Icons.business_outlined, path: '/vendors'),
    _NavItem(label: 'Users', icon: Icons.people_outlined, path: '/users'),
    _NavItem(label: 'Schedule', icon: Icons.schedule_outlined, path: '/schedule'),
    _NavItem(label: 'Calendar', icon: Icons.calendar_month_outlined, path: '/calendar-exceptions'),
    _NavItem(label: 'Priority', icon: Icons.accessibility_new_outlined, path: '/priority-schedules'),
    _NavItem(label: 'Trips', icon: Icons.trip_origin_outlined, path: '/trips'),
    _NavItem(label: 'Reports', icon: Icons.bar_chart_outlined, path: '/reports'),
  ];

  @override
  void initState() {
    super.initState();
    _animController = AnimationController(
      vsync: this,
      duration: _animDuration,
    );
    _widthAnimation = Tween<double>(
      begin: _collapsedWidth,
      end: _expandedWidth,
    ).animate(CurvedAnimation(
      parent: _animController,
      curve: Curves.easeInOut,
    ));
  }

  @override
  void dispose() {
    _animController.dispose();
    super.dispose();
  }

  void _toggleSidebar() {
    setState(() {
      _isExpanded = !_isExpanded;
      if (_isExpanded) {
        _animController.forward();
      } else {
        _animController.reverse();
      }
    });
  }

  int get _selectedIndex {
    final path = widget.currentPath;
    // Use startsWith matching so /stops/1 highlights the /stops nav item
    final index = _navItems.indexWhere((item) =>
        path == item.path || (item.path != '/overview' && path.startsWith(item.path)));
    return index >= 0 ? index : 0;
  }

  String get _currentPageTitle {
    final index = _selectedIndex;
    return _navItems[index].label;
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: _bg,
      body: Column(
        children: [
          // ─── TOP BAR ───
          _buildTopBar(context),
          // ─── BODY: sidebar + content ───
          Expanded(
            child: Row(
              children: [
                _buildSidebar(context),
                Expanded(child: widget.child),
              ],
            ),
          ),
        ],
      ),
    );
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // TOP BAR
  // ═══════════════════════════════════════════════════════════════════════════
  Widget _buildTopBar(BuildContext context) {
    return Container(
      height: _topBarHeight,
      decoration: const BoxDecoration(
        color: Colors.white,
        border: Border(bottom: BorderSide(color: _border)),
      ),
      padding: const EdgeInsets.symmetric(horizontal: 16),
      child: Row(
        children: [
          // Toggle button
          _TopBarIconButton(
            icon: _isExpanded ? Icons.menu_open : Icons.menu,
            tooltip: _isExpanded ? 'Collapse sidebar' : 'Expand sidebar',
            onTap: _toggleSidebar,
          ),
          const SizedBox(width: 16),
          // Brand
          Container(
            width: 8,
            height: 8,
            decoration: const BoxDecoration(
              color: _gold,
              shape: BoxShape.circle,
            ),
          ),
          const SizedBox(width: 8),
          Text(
            'NMU Shuttles',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 16,
              fontWeight: FontWeight.w700,
              color: _navy,
            ),
          ),
          const Spacer(),
          // Breadcrumb
          Text(
            'Admin',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 13,
              color: _textTertiary,
            ),
          ),
          const Padding(
            padding: EdgeInsets.symmetric(horizontal: 4),
            child: Icon(Icons.chevron_right, size: 16, color: _textTertiary),
          ),
          Text(
            _currentPageTitle,
            style: GoogleFonts.hankenGrotesk(
              fontSize: 13,
              fontWeight: FontWeight.w500,
              color: _navy,
            ),
          ),
          const SizedBox(width: 20),
          // User pill
          Container(
            padding: const EdgeInsets.only(left: 4, right: 12, top: 4, bottom: 4),
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(24),
              border: Border.all(color: _border),
            ),
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                Container(
                  width: 28,
                  height: 28,
                  decoration: const BoxDecoration(
                    color: _gold,
                    shape: BoxShape.circle,
                  ),
                  alignment: Alignment.center,
                  child: Text(
                    'A',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 12,
                      fontWeight: FontWeight.w700,
                      color: _navy,
                    ),
                  ),
                ),
                const SizedBox(width: 8),
                Text(
                  'Admin',
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 13,
                    fontWeight: FontWeight.w500,
                    color: _navy,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  // ═══════════════════════════════════════════════════════════════════════════
  // SIDEBAR
  // ═══════════════════════════════════════════════════════════════════════════
  Widget _buildSidebar(BuildContext context) {
    return AnimatedBuilder(
      animation: _widthAnimation,
      builder: (context, child) {
        return Container(
          width: _widthAnimation.value,
          decoration: const BoxDecoration(
            color: Colors.white,
            border: Border(right: BorderSide(color: _border)),
          ),
          child: Column(
            children: [
              const SizedBox(height: 12),
              // Nav items
              Expanded(
                child: ListView.builder(
                  itemCount: _navItems.length,
                  padding: const EdgeInsets.symmetric(horizontal: 8),
                  itemBuilder: (context, index) {
                    return _buildNavItem(context, index);
                  },
                ),
              ),
              // Logout
              const Divider(),
              Padding(
                padding: const EdgeInsets.fromLTRB(8, 4, 8, 12),
                child: _buildLogoutItem(context),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildNavItem(BuildContext context, int index) {
    final item = _navItems[index];
    final isSelected = index == _selectedIndex;
    final showLabels = _widthAnimation.value > _collapsedWidth + 40;

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 1),
      child: Tooltip(
        message: showLabels ? '' : item.label,
        preferBelow: false,
        waitDuration: const Duration(milliseconds: 400),
        child: InkWell(
          borderRadius: BorderRadius.circular(10),
          onTap: () {
            if (!isSelected) {
              context.go(item.path);
            }
          },
          child: Container(
            height: 40,
            padding: EdgeInsets.symmetric(
              horizontal: showLabels ? 12 : 0,
            ),
            decoration: BoxDecoration(
              color: isSelected ? _gold : Colors.transparent,
              borderRadius: BorderRadius.circular(10),
            ),
            clipBehavior: Clip.hardEdge,
            alignment: showLabels ? Alignment.centerLeft : Alignment.center,
            child: Row(
              mainAxisSize: MainAxisSize.min,
              children: [
                if (isSelected && showLabels) ...[
                  Container(
                    width: 3,
                    height: 20,
                    decoration: BoxDecoration(
                      color: _navy,
                      borderRadius: BorderRadius.circular(2),
                    ),
                  ),
                  const SizedBox(width: 10),
                ],
                Icon(
                  item.icon,
                  size: 20,
                  color: isSelected ? _navy : _textSecondary,
                ),
                if (showLabels) ...[
                  const SizedBox(width: 14),
                  Flexible(
                    child: Text(
                      item.label,
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 14,
                        fontWeight:
                            isSelected ? FontWeight.w600 : FontWeight.w500,
                        color: isSelected ? _navy : _textSecondary,
                      ),
                      overflow: TextOverflow.ellipsis,
                      maxLines: 1,
                    ),
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildLogoutItem(BuildContext context) {
    final showLabels = _widthAnimation.value > _collapsedWidth + 40;

    return Tooltip(
      message: showLabels ? '' : 'Sign out',
      preferBelow: false,
      child: InkWell(
        borderRadius: BorderRadius.circular(10),
        onTap: () async {
          final authService = AdminAuthService();
          await authService.deleteToken();
          if (context.mounted) {
            context.go('/login');
          }
        },
        child: Container(
          height: 40,
          padding: EdgeInsets.symmetric(
            horizontal: showLabels ? 12 : 0,
          ),
          clipBehavior: Clip.hardEdge,
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(10),
          ),
          alignment: showLabels ? Alignment.centerLeft : Alignment.center,
          child: Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Icon(
                Icons.logout_outlined,
                size: 20,
                color: Colors.red.shade600,
              ),
              if (showLabels) ...[
                const SizedBox(width: 14),
                Flexible(
                  child: Text(
                    'Sign Out',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 14,
                      fontWeight: FontWeight.w500,
                      color: Colors.red.shade600,
                    ),
                    overflow: TextOverflow.ellipsis,
                    maxLines: 1,
                  ),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}

// ─── Top bar icon button ─────────────────────────────────────────────────────

class _TopBarIconButton extends StatelessWidget {
  const _TopBarIconButton({
    required this.icon,
    required this.tooltip,
    required this.onTap,
  });

  final IconData icon;
  final String tooltip;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Tooltip(
      message: tooltip,
      child: Material(
        color: Colors.transparent,
        borderRadius: BorderRadius.circular(8),
        child: InkWell(
          borderRadius: BorderRadius.circular(8),
          onTap: onTap,
          child: Container(
            width: 36,
            height: 36,
            alignment: Alignment.center,
            child: Icon(icon, size: 22, color: const Color(0xFF5A6B7B)),
          ),
        ),
      ),
    );
  }
}

// ─── Nav item model ──────────────────────────────────────────────────────────

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
