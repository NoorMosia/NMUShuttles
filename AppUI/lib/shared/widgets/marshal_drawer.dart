import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_app/core/auth/auth_service.dart';
import 'package:nmu_shuttle_app/core/notifications/push_notification_service.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/marshal_home_provider.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/passenger_list_provider.dart';
import 'package:nmu_shuttle_app/features/student/providers/notification_providers.dart';

/// Modern minimalistic marshal navigation drawer.
/// Clean white surface, subtle active indicator, logout pinned to bottom.
class MarshalDrawer extends ConsumerWidget {
  const MarshalDrawer({super.key, this.onLogout});

  final VoidCallback? onLogout;

  static const Color _border = Color(0xFFE8ECF0);

  static const List<_NavItem> _navItems = [
    _NavItem(label: 'Dashboard', icon: Icons.home_outlined, path: '/marshal/home'),
    _NavItem(label: 'Alerts', icon: Icons.notifications_outlined, path: '/marshal/alerts'),
    _NavItem(label: 'Profile', icon: Icons.person_outlined, path: '/marshal/profile'),
  ];

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final currentPath = GoRouterState.of(context).matchedLocation;

    return Drawer(
      backgroundColor: Colors.white,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.only(
          topRight: Radius.circular(16),
          bottomRight: Radius.circular(16),
        ),
      ),
      child: SafeArea(
        child: Padding(
          padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              const SizedBox(height: 24),
              // ─── Header ───
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 14),
                child: Row(
                  children: [
                    Container(
                      width: 12,
                      height: 12,
                      decoration: const BoxDecoration(
                        color: AppTheme.primaryGold,
                        shape: BoxShape.circle,
                      ),
                    ),
                    const SizedBox(width: 12),
                    Text(
                      'LATT mobile',
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 20,
                        fontWeight: FontWeight.w700,
                        color: AppTheme.navy,
                      ),
                    ),
                    const SizedBox(width: 8),
                    Container(
                      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
                      decoration: BoxDecoration(
                        color: AppTheme.navy.withOpacity(0.08),
                        borderRadius: BorderRadius.circular(6),
                      ),
                      child: Text(
                        'Marshal',
                        style: GoogleFonts.hankenGrotesk(
                          fontSize: 12,
                          fontWeight: FontWeight.w600,
                          color: AppTheme.navy,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 28),
              const Divider(height: 1, color: _border),
              const SizedBox(height: 16),

              // ─── Navigation items ───
              Expanded(
                child: ListView.separated(
                  itemCount: _navItems.length,
                  padding: EdgeInsets.zero,
                  separatorBuilder: (_, __) => const SizedBox(height: 10),
                  itemBuilder: (context, index) {
                    final item = _navItems[index];
                    final isSelected = currentPath.startsWith(item.path);
                    return _DrawerNavTile(
                      item: item,
                      isSelected: isSelected,
                      onTap: () {
                        Navigator.pop(context);
                        context.go(item.path);
                      },
                    );
                  },
                ),
              ),

              // ─── Logout pinned to bottom ───
              const Divider(height: 1, color: _border),
              const SizedBox(height: 8),
              _DrawerNavTile(
                item: const _NavItem(
                  label: 'Sign Out',
                  icon: Icons.logout_outlined,
                  path: '',
                ),
                isSelected: false,
                isDestructive: true,
                onTap: () async {
                  await PushNotificationService.instance.deleteToken();
                  await AuthService().deleteToken();
                  ref.invalidate(authStateProvider);
                  ref.invalidate(marshalHomeProvider);
                  ref.invalidate(passengerManifestProvider);
                  ref.invalidate(checkInProvider);
                  ref.invalidate(flagBreakdownProvider);
                  ref.invalidate(tripShuttlesProvider);
                  ref.invalidate(notificationsProvider);
                  onLogout?.call();
                },
              ),
            ],
          ),
        ),
      ),
    );
  }
}

// ─── Single navigation tile ──────────────────────────────────────────────────

class _DrawerNavTile extends StatelessWidget {
  const _DrawerNavTile({
    required this.item,
    required this.isSelected,
    required this.onTap,
    this.isDestructive = false,
  });

  final _NavItem item;
  final bool isSelected;
  final bool isDestructive;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    final Color iconColor;
    final Color textColor;

    if (isDestructive) {
      iconColor = Colors.red.shade600;
      textColor = Colors.red.shade600;
    } else if (isSelected) {
      iconColor = AppTheme.navy;
      textColor = AppTheme.navy;
    } else {
      iconColor = const Color(0xFF5A6B7B);
      textColor = const Color(0xFF5A6B7B);
    }

    return InkWell(
      borderRadius: BorderRadius.circular(12),
      onTap: onTap,
      child: Container(
        height: 56,
        padding: const EdgeInsets.symmetric(horizontal: 14),
        decoration: BoxDecoration(
          color: isSelected ? const Color(0xFFFEF9C3) : Colors.transparent,
          borderRadius: BorderRadius.circular(12),
        ),
        child: Row(
          children: [
            if (isSelected) ...[
              Container(
                width: 3.5,
                height: 22,
                decoration: BoxDecoration(
                  color: AppTheme.primaryGold,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              const SizedBox(width: 12),
            ],
            Icon(item.icon, size: 24, color: iconColor),
            const SizedBox(width: 16),
            Text(
              item.label,
              style: GoogleFonts.hankenGrotesk(
                fontSize: 16,
                fontWeight: isSelected ? FontWeight.w600 : FontWeight.w500,
                color: textColor,
              ),
            ),
          ],
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
