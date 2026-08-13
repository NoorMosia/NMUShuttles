import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:nmu_shuttle_app/core/auth/auth_service.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/marshal_home_provider.dart';
import 'package:nmu_shuttle_app/shared/widgets/marshal_drawer.dart';

/// Marshal profile screen — mirrors the student profile flow.
/// Shows name, staff number, assigned station, and logout button.
class MarshalProfileScreen extends ConsumerWidget {
  const MarshalProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(marshalHomeProvider);
    final assignment = state.assignment;

    return Scaffold(
      drawer: MarshalDrawer(
        onLogout: () => context.go('/login'),
      ),
      body: Builder(
        builder: (scaffoldContext) => Stack(
          children: [
            RefreshIndicator(
              onRefresh: () =>
                  ref.read(marshalHomeProvider.notifier).refresh(),
              child: ListView(
                padding: EdgeInsets.only(
                  top: MediaQuery.of(scaffoldContext).padding.top + 70,
                  left: 16,
                  right: 16,
                  bottom: 100,
                ),
                children: [
                  // ─── Profile Card ─────────────────────────────
                  _buildProfileCard(context, assignment),
                  const SizedBox(height: 16),
                  // ─── Assignment Details ────────────────────────
                  _buildAssignmentCard(context, assignment),
                  const SizedBox(height: 24),
                  // ─── Logout Button ────────────────────────────
                  OutlinedButton.icon(
                    onPressed: () => _handleLogout(context, ref),
                    icon: const Icon(Icons.logout, color: Colors.red),
                    label: const Text(
                      'Logout',
                      style: TextStyle(color: Colors.red),
                    ),
                    style: OutlinedButton.styleFrom(
                      side: const BorderSide(color: Colors.red),
                      minimumSize: const Size(double.infinity, 48),
                      shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(12),
                      ),
                    ),
                  ),
                ],
              ),
            ),
            // Floating hamburger + Profile label
            Positioned(
              top: MediaQuery.of(scaffoldContext).padding.top + 12,
              left: 16,
              child: Row(
                children: [
                  Material(
                    color: Colors.white,
                    elevation: 0,
                    borderRadius: BorderRadius.circular(8),
                    child: InkWell(
                      borderRadius: BorderRadius.circular(8),
                      onTap: () =>
                          Scaffold.of(scaffoldContext).openDrawer(),
                      child: const Padding(
                        padding: EdgeInsets.all(10),
                        child: Icon(
                          Icons.menu,
                          color: AppTheme.navy,
                          size: 24,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(width: 6),
                  Material(
                    color: Colors.white,
                    elevation: 0,
                    borderRadius: BorderRadius.circular(8),
                    child: const Padding(
                      padding:
                          EdgeInsets.symmetric(horizontal: 10, vertical: 10),
                      child: Text(
                        'Profile',
                        style: TextStyle(
                          color: AppTheme.navy,
                          fontWeight: FontWeight.w600,
                          fontSize: 18,
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildProfileCard(BuildContext context, MarshalAssignment? assignment) {
    if (assignment == null) {
      return Container(
        padding: const EdgeInsets.all(24),
        decoration: BoxDecoration(
          color: Colors.white,
          borderRadius: BorderRadius.circular(16),
        ),
        child: const Center(child: CircularProgressIndicator()),
      );
    }

    final initials =
        '${(assignment.firstName ?? '').isNotEmpty ? assignment.firstName![0] : ''}'
        '${(assignment.lastName ?? '').isNotEmpty ? assignment.lastName![0] : ''}';

    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
      ),
      child: Row(
        children: [
          // Avatar
          CircleAvatar(
            radius: 36,
            backgroundColor: AppTheme.navy,
            child: Text(
              initials.toUpperCase(),
              style: const TextStyle(
                color: Colors.white,
                fontSize: 22,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
          const SizedBox(width: 16),
          // Info
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  assignment.fullName,
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: AppTheme.navy,
                      ),
                ),
                const SizedBox(height: 4),
                Row(
                  children: [
                    const Icon(Icons.badge_outlined,
                        size: 16, color: Colors.grey),
                    const SizedBox(width: 4),
                    Text(
                      assignment.staffNumber ?? '—',
                      style: Theme.of(context)
                          .textTheme
                          .bodyMedium
                          ?.copyWith(color: Colors.grey.shade700),
                    ),
                  ],
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildAssignmentCard(
      BuildContext context, MarshalAssignment? assignment) {
    if (assignment == null) return const SizedBox.shrink();

    final hasStop = assignment.assignedStopId != null;
    final hasCampus = assignment.assignedCampusId != null;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Padding(
          padding: const EdgeInsets.only(left: 4, bottom: 16),
          child: Text(
            'Assignments',
            style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  fontWeight: FontWeight.bold,
                  color: AppTheme.navy,
                ),
          ),
        ),
        if (hasStop) ...[
          _buildDetailRow(
            context,
            icon: Icons.location_on_outlined,
            label: 'Morning — To Campus',
            value: assignment.assignedStopName ?? 'Unknown Stop',
          ),
          const SizedBox(height: 14),
        ],
        if (hasCampus) ...[
          _buildDetailRow(
            context,
            icon: Icons.apartment_outlined,
            label: 'Afternoon — From Campus',
            value: assignment.assignedCampusName ?? 'Unknown Campus',
          ),
        ],
        if (!hasStop && !hasCampus)
          _buildDetailRow(
            context,
            icon: Icons.warning_amber_outlined,
            label: 'Status',
            value: 'No assignments',
          ),
      ],
    );
  }

  Widget _buildDetailRow(
    BuildContext context, {
    required IconData icon,
    required String label,
    required String value,
  }) {
    return Row(
      children: [
        Icon(icon, size: 20, color: Colors.grey.shade600),
        const SizedBox(width: 12),
        Expanded(
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                label,
                style: TextStyle(
                  color: Colors.grey.shade500,
                  fontSize: 12,
                ),
              ),
              const SizedBox(height: 2),
              Text(
                value,
                style: const TextStyle(
                  color: AppTheme.navy,
                  fontSize: 15,
                  fontWeight: FontWeight.w600,
                ),
              ),
            ],
          ),
        ),
      ],
    );
  }

  Future<void> _handleLogout(BuildContext context, WidgetRef ref) async {
    await AuthService().deleteToken();
    ref.invalidate(authStateProvider);
    ref.invalidate(marshalHomeProvider);

    if (context.mounted) {
      context.go('/login');
    }
  }
}
