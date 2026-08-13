import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_app/core/auth/auth_service.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/student/models/report_model.dart';
import 'package:nmu_shuttle_app/features/student/providers/booking_providers.dart';
import 'package:nmu_shuttle_app/features/student/providers/notification_providers.dart';
import 'package:nmu_shuttle_app/features/student/providers/profile_providers.dart';
import 'package:nmu_shuttle_app/features/student/providers/trip_providers.dart';
import 'package:nmu_shuttle_app/features/student/screens/report_detail_screen.dart';
import 'package:nmu_shuttle_app/shared/widgets/student_shell.dart';

/// Profile screen displaying student info, strike points, reports list, and logout.
class ProfileScreen extends ConsumerWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final profileAsync = ref.watch(studentProfileProvider);
    final reportsAsync = ref.watch(reportsProvider);

    return Scaffold(
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
                const SizedBox(height: 16),
                // ─── Strike Points Section ────────────────────
                _buildStrikeSection(context, profileAsync),
                const SizedBox(height: 24),
                // ─── My Reports Section ───────────────────────
                Container(
                  padding: const EdgeInsets.only(top: 16),
                  decoration: BoxDecoration(
                    border: Border(
                      top: BorderSide(color: Colors.grey.shade300, width: 1),
                    ),
                  ),
                  child: Text(
                    'My Reports',
                    style: Theme.of(context).textTheme.titleLarge?.copyWith(
                          fontWeight: FontWeight.bold,
                          color: AppTheme.navy,
                        ),
                  ),
                ),
                const SizedBox(height: 8),
                _buildReportsList(context, reportsAsync),
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
          // Floating hamburger menu + Profile label
          Positioned(
            top: MediaQuery.of(context).padding.top + 12,
            left: 16,
            child: Row(
              children: [
                Material(
                  color: Colors.white,
                  elevation: 0,
                  borderRadius: BorderRadius.circular(8),
                  child: InkWell(
                    borderRadius: BorderRadius.circular(8),
                    onTap: () {
                      if (studentShellScaffoldKey.currentState?.hasDrawer ?? false) {
                        studentShellScaffoldKey.currentState!.openDrawer();
                      } else {
                        Scaffold.of(context).openDrawer();
                      }
                    },
                    child: const Padding(
                      padding: EdgeInsets.all(10),
                      child: Icon(
                        Icons.menu,
                        color: Color(0xFF132E51),
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
                    padding: EdgeInsets.symmetric(horizontal: 10, vertical: 10),
                    child: Text(
                      'Profile',
                      style: TextStyle(
                        color: Color(0xFF132E51),
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
    );
  }

  Widget _buildProfileCard(
    BuildContext context,
    AsyncValue<StudentProfile?> profileAsync,
  ) {
    return profileAsync.when(
      loading: () => const Card(
        elevation: 0,
        child: Padding(
          padding: EdgeInsets.all(24),
          child: Center(child: CircularProgressIndicator()),
        ),
      ),
      error: (_, __) => Card(
        elevation: 0,
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
            elevation: 0,
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
        return Container(
          padding: const EdgeInsets.all(20),
          decoration: BoxDecoration(
            color: Colors.white,
            border: Border.all(color: Colors.grey.shade200),
            borderRadius: BorderRadius.circular(16),
          ),
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
        );
      },
    );
  }

  Widget _buildStrikeSection(
    BuildContext context,
    AsyncValue<StudentProfile?> profileAsync,
  ) {
    return profileAsync.when(
      loading: () => const SizedBox.shrink(),
      error: (_, __) => const SizedBox.shrink(),
      data: (profile) {
        if (profile == null) return const SizedBox.shrink();

        final noShowCount = profile.noShowCount;
        final cancelledCount = profile.cancelledCount;
        final clampedStrikes = profile.strikePoints;
        const maxStrikes = 6;

        return Container(
          padding: const EdgeInsets.all(16),
          decoration: BoxDecoration(
            color: Colors.white,
            border: Border.all(color: Colors.grey.shade200),
            borderRadius: BorderRadius.circular(16),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Strike Points',
                style: Theme.of(context).textTheme.titleSmall?.copyWith(
                      fontWeight: FontWeight.bold,
                      color: AppTheme.navy,
                    ),
              ),
              const SizedBox(height: 12),
              // Strike progress bar
              ClipRRect(
                borderRadius: BorderRadius.circular(8),
                child: LinearProgressIndicator(
                  value: clampedStrikes / maxStrikes,
                  minHeight: 10,
                  backgroundColor: Colors.grey.shade200,
                  valueColor: AlwaysStoppedAnimation<Color>(
                    clampedStrikes >= maxStrikes
                        ? Colors.red
                        : clampedStrikes >= 4
                            ? Colors.orange
                            : AppTheme.primaryGold,
                  ),
                ),
              ),
              const SizedBox(height: 8),
              Text(
                '$clampedStrikes / $maxStrikes strike points',
                style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                      fontWeight: FontWeight.w600,
                      color: clampedStrikes >= maxStrikes
                          ? Colors.red
                          : AppTheme.navy,
                    ),
              ),
              const SizedBox(height: 12),
              // Breakdown row
              Row(
                children: [
                  _buildStrikeStat(
                    context,
                    icon: Icons.person_off_outlined,
                    label: 'No-shows',
                    count: noShowCount,
                    color: Colors.red.shade600,
                  ),
                  const SizedBox(width: 24),
                  _buildStrikeStat(
                    context,
                    icon: Icons.cancel_outlined,
                    label: 'Cancelled',
                    count: cancelledCount,
                    color: Colors.orange.shade700,
                  ),
                ],
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildStrikeStat(
    BuildContext context, {
    required IconData icon,
    required String label,
    required int count,
    required Color color,
  }) {
    return Row(
      children: [
        Icon(icon, size: 18, color: color),
        const SizedBox(width: 6),
        Text(
          '$count $label',
          style: Theme.of(context).textTheme.bodySmall?.copyWith(
                color: color,
                fontWeight: FontWeight.w500,
              ),
        ),
      ],
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

  void _navigateToReportDetail(BuildContext context, ReportModel report) {
    Navigator.of(context).push(
      MaterialPageRoute(builder: (_) => ReportDetailScreen(report: report)),
    );
  }

  Future<void> _handleLogout(BuildContext context, WidgetRef ref) async {
    await AuthService().deleteToken();

    // Invalidate all user-specific cached providers so the next user starts fresh
    ref.invalidate(studentProfileProvider);
    ref.invalidate(bookingsProvider);
    ref.invalidate(reportsProvider);
    ref.invalidate(notificationsProvider);
    ref.invalidate(tripAvailabilityProvider);
    ref.invalidate(stopsProvider);

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
      elevation: 0,
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
