import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_app/core/auth/auth_service.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/student/models/report_model.dart';
import 'package:nmu_shuttle_app/features/student/providers/profile_providers.dart';
import 'package:nmu_shuttle_app/features/student/screens/report_detail_screen.dart';
import 'package:nmu_shuttle_app/features/student/screens/report_form_screen.dart';
import 'package:nmu_shuttle_app/shared/widgets/app_drawer.dart';
import 'package:nmu_shuttle_app/shared/widgets/hamburger_menu_button.dart';

/// Profile screen displaying student info, reports list, and logout.
class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(studentProfileProvider);
    final reportsAsync = ref.watch(reportsProvider);

    return Scaffold(
      drawer: AppDrawer(
        onLogout: () => _handleLogout(context),
      ),
      body: Stack(
        children: [
          RefreshIndicator(
            onRefresh: () async {
              ref.invalidate(reportsProvider);
              ref.invalidate(studentProfileProvider);
            },
            child: ListView(
              padding: EdgeInsets.only(
                top: MediaQuery.of(context).padding.top + 70,
                left: 16,
                right: 16,
                bottom: 100,
              ),
              children: [
                // ─── Profile Card ─────────────────────────────
                _buildProfileCard(context, profileAsync),
                const SizedBox(height: 24),
                // ─── My Reports Section ───────────────────────
                Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    Text(
                      'My Reports',
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(
                            fontWeight: FontWeight.bold,
                            color: AppTheme.navy,
                          ),
                    ),
                    TextButton.icon(
                      onPressed: () => _navigateToReportForm(context),
                      icon: const Icon(Icons.add, size: 18),
                      label: const Text('New'),
                    ),
                  ],
                ),
                const SizedBox(height: 8),
                _buildReportsList(context, reportsAsync),
                const SizedBox(height: 24),
                // ─── Logout Button ────────────────────────────
                OutlinedButton.icon(
                  onPressed: () => _handleLogout(context),
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
          // Floating hamburger menu
          const HamburgerMenuButton(),
        ],
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => _navigateToReportForm(context),
        child: const Icon(Icons.report_outlined),
      ),
    );
  }

  Widget _buildProfileCard(
    BuildContext context,
    AsyncValue<StudentProfile?> profileAsync,
  ) {
    return profileAsync.when(
      loading: () => const Card(
        child: Padding(
          padding: EdgeInsets.all(24),
          child: Center(child: CircularProgressIndicator()),
        ),
      ),
      error: (_, __) => Card(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Center(
            child: Text(
              'Unable to load profile',
              style: Theme.of(context).textTheme.bodyLarge,
            ),
          ),
        ),
      ),
      data: (profile) {
        if (profile == null) {
          return Card(
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Center(
                child: Text(
                  'Profile not available',
                  style: Theme.of(context).textTheme.bodyLarge,
                ),
              ),
            ),
          );
        }
        return Card(
          elevation: 2,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          child: Padding(
            padding: const EdgeInsets.all(20),
            child: Row(
              children: [
                // Avatar placeholder
                CircleAvatar(
                  radius: 36,
                  backgroundColor: AppTheme.navy,
                  child: Text(
                    '${profile.firstName.isNotEmpty ? profile.firstName[0] : ''}${profile.lastName.isNotEmpty ? profile.lastName[0] : ''}',
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
                        '${profile.firstName} ${profile.lastName}',
                        style:
                            Theme.of(context).textTheme.titleMedium?.copyWith(
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
                            profile.studentNumber,
                            style: Theme.of(context)
                                .textTheme
                                .bodyMedium
                                ?.copyWith(color: Colors.grey.shade700),
                          ),
                        ],
                      ),
                      const SizedBox(height: 2),
                      Row(
                        children: [
                          const Icon(Icons.email_outlined,
                              size: 16, color: Colors.grey),
                          const SizedBox(width: 4),
                          Flexible(
                            child: Text(
                              profile.email,
                              style: Theme.of(context)
                                  .textTheme
                                  .bodyMedium
                                  ?.copyWith(color: Colors.grey.shade700),
                              overflow: TextOverflow.ellipsis,
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }

  Widget _buildReportsList(
    BuildContext context,
    AsyncValue<List<ReportModel>> reportsAsync,
  ) {
    return reportsAsync.when(
      loading: () => const Center(
        child: Padding(
          padding: EdgeInsets.all(24),
          child: CircularProgressIndicator(),
        ),
      ),
      error: (error, _) => Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: Column(
            children: [
              const Icon(Icons.error_outline, size: 40, color: Colors.red),
              const SizedBox(height: 8),
              Text(
                'Failed to load reports',
                style: Theme.of(context).textTheme.bodyLarge,
              ),
            ],
          ),
        ),
      ),
      data: (reports) {
        if (reports.isEmpty) {
          return Center(
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                children: [
                  Icon(Icons.description_outlined,
                      size: 48, color: Colors.grey.shade400),
                  const SizedBox(height: 12),
                  Text(
                    'No reports filed yet',
                    style: Theme.of(context).textTheme.bodyLarge?.copyWith(
                          color: Colors.grey.shade600,
                        ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Tap the button to file a new report',
                    style: Theme.of(context).textTheme.bodySmall?.copyWith(
                          color: Colors.grey.shade500,
                        ),
                  ),
                ],
              ),
            ),
          );
        }
        return Column(
          children: reports
              .map((report) => _ReportCard(
                    report: report,
                    onTap: () => _navigateToReportDetail(context, report),
                  ))
              .toList(),
        );
      },
    );
  }

  void _navigateToReportForm(BuildContext context) {
    Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => const ReportFormScreen()),
    );
  }

  void _navigateToReportDetail(BuildContext context, ReportModel report) {
    Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => ReportDetailScreen(report: report)),
    );
  }

  Future<void> _handleLogout(BuildContext context) async {
    await AuthService().deleteToken();
    if (context.mounted) {
      context.go('/login');
    }
  }
}

/// Individual report card widget.
class _ReportCard extends StatelessWidget {
  const _ReportCard({required this.report, required this.onTap});

  final ReportModel report;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: ListTile(
        onTap: onTap,
        contentPadding:
            const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
        leading: _buildCategoryIcon(),
        title: Text(
          _formatCategory(report.category),
          style: const TextStyle(fontWeight: FontWeight.w600),
        ),
        subtitle: Text(
          DateFormat('dd MMM yyyy, HH:mm').format(report.createdAt.toLocal()),
          style: Theme.of(context).textTheme.bodySmall,
        ),
        trailing: _StatusBadge(status: report.status),
      ),
    );
  }

  Widget _buildCategoryIcon() {
    IconData icon;
    switch (report.category) {
      case 'DriverBehavior':
        icon = Icons.person_outline;
        break;
      case 'TimetableIssue':
        icon = Icons.schedule;
        break;
      case 'VehicleQuality':
        icon = Icons.directions_bus_outlined;
        break;
      default:
        icon = Icons.report_outlined;
    }
    return CircleAvatar(
      backgroundColor: AppTheme.navy.withValues(alpha: 0.1),
      child: Icon(icon, color: AppTheme.navy, size: 20),
    );
  }

  String _formatCategory(String category) {
    // Insert spaces before capital letters for display
    return category.replaceAllMapped(
      RegExp(r'([a-z])([A-Z])'),
      (match) => '${match.group(1)} ${match.group(2)}',
    );
  }
}

/// Status badge widget with color coding.
class _StatusBadge extends StatelessWidget {
  const _StatusBadge({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    Color bgColor;
    Color textColor;
    switch (status) {
      case 'Submitted':
        bgColor = Colors.blue.shade50;
        textColor = Colors.blue.shade700;
        break;
      case 'UnderReview':
        bgColor = Colors.orange.shade50;
        textColor = Colors.orange.shade700;
        break;
      case 'Resolved':
        bgColor = Colors.green.shade50;
        textColor = Colors.green.shade700;
        break;
      default:
        bgColor = Colors.grey.shade100;
        textColor = Colors.grey.shade700;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: bgColor,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        _formatStatus(status),
        style: TextStyle(
          color: textColor,
          fontSize: 12,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }

  String _formatStatus(String status) {
    return status.replaceAllMapped(
      RegExp(r'([a-z])([A-Z])'),
      (match) => '${match.group(1)} ${match.group(2)}',
    );
  }
}
