import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:nmu_shuttle_app/core/auth/auth_service.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';

/// Marshal-specific navigation drawer with only marshal-relevant screens.
class MarshalDrawer extends StatelessWidget {
  const MarshalDrawer({super.key, this.onLogout});

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
                    'NMU Shuttle — Marshal',
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
                    title: const Text('Dashboard'),
                    onTap: () {
                      Navigator.pop(context);
                      context.go('/marshal/home');
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
