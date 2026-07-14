import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:nmu_shuttle_app/core/auth/auth_service.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';

/// App navigation drawer. Logout button is pinned to the bottom.
class AppDrawer extends StatelessWidget {
  const AppDrawer({super.key, this.onLogout});

  final VoidCallback? onLogout;

  @override
  Widget build(BuildContext context) {
    return Drawer(
      child: SafeArea(
        child: Column(
          children: [
            // Drawer header with branding
            Container(
              width: double.infinity,
              padding: const EdgeInsets.symmetric(vertical: 24, horizontal: 16),
              decoration: const BoxDecoration(
                color: AppTheme.navy,
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Image.asset(
                    'assets/logos/1.png',
                    height: 48,
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'NMU Shuttle',
                    style: TextStyle(
                      color: Colors.white,
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),
            // Navigation items
            Expanded(
              child: ListView(
                padding: EdgeInsets.zero,
                children: [
                  ListTile(
                    leading: const Icon(Icons.home_outlined),
                    title: const Text('Home'),
                    onTap: () {
                      Navigator.pop(context);
                      context.go('/student/home');
                    },
                  ),
                  ListTile(
                    leading: const Icon(Icons.map_outlined),
                    title: const Text('Maps'),
                    onTap: () {
                      Navigator.pop(context);
                      context.go('/student/maps');
                    },
                  ),
                  ListTile(
                    leading: const Icon(Icons.notifications_outlined),
                    title: const Text('Alerts'),
                    onTap: () {
                      Navigator.pop(context);
                      context.go('/student/alerts');
                    },
                  ),
                  ListTile(
                    leading: const Icon(Icons.person_outlined),
                    title: const Text('Profile'),
                    onTap: () {
                      Navigator.pop(context);
                      context.go('/student/profile');
                    },
                  ),
                ],
              ),
            ),
            // Logout pinned to bottom
            const Divider(height: 1),
            ListTile(
              leading: const Icon(Icons.logout, color: Colors.red),
              title: const Text(
                'Logout',
                style: TextStyle(color: Colors.red),
              ),
              onTap: () async {
                await AuthService().deleteToken();
                onLogout?.call();
              },
            ),
          ],
        ),
      ),
    );
  }
}
