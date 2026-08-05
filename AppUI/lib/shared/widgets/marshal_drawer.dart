import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:nmu_shuttle_app/core/auth/auth_service.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/marshal_home_provider.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/passenger_list_provider.dart';

/// Marshal-specific navigation drawer with only marshal-relevant screens.
class MarshalDrawer extends ConsumerWidget {
  const MarshalDrawer({super.key, this.onLogout});

  final VoidCallback? onLogout;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
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
                // Delete token from secure storage
                await AuthService().deleteToken();
                
                // Invalidate all marshal-specific data providers so they don't cache the old user's data
                ref.invalidate(marshalHomeProvider);
                ref.invalidate(passengerManifestProvider);
                ref.invalidate(checkInProvider);
                ref.invalidate(flagBreakdownProvider);
                ref.invalidate(tripShuttlesProvider);
                
                // Navigate to login
                onLogout?.call();
              },
            ),
          ],
        ),
      ),
    );
  }
}
