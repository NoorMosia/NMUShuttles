import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/core/ui/error_display.dart';
import 'package:nmu_shuttle_app/features/student/models/notification_model.dart';
import 'package:nmu_shuttle_app/features/student/providers/notification_providers.dart';
import 'package:nmu_shuttle_app/features/student/screens/alert_detail_screen.dart';
import 'package:nmu_shuttle_app/features/student/widgets/date_separator.dart';
import 'package:nmu_shuttle_app/shared/widgets/student_shell.dart';

/// Alerts screen listing all notifications ordered by CreatedAt DESC.
/// Uses a floating hamburger menu (no AppBar) and pull-to-refresh.
class AlertsScreen extends ConsumerWidget {
  const AlertsScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final notificationsAsync = ref.watch(notificationsProvider);

    return Scaffold(
      body: Stack(
        children: [
          Column(
            children: [
              SizedBox(height: MediaQuery.of(context).padding.top + 60),
              const SizedBox(height: 16),
              // Notification list
              Expanded(
                child: notificationsAsync.when(
                  loading: () =>
                      const Center(child: CircularProgressIndicator()),
                  error: (error, _) => ErrorRetry(
                    error: error,
                    title: 'Failed to load notifications',
                    onRetry: () => ref.invalidate(notificationsProvider),
                  ),
                  data: (notifications) {
                    if (notifications.isEmpty) {
                      return const _EmptyState();
                    }
                    final grouped = _groupNotificationsByDate(notifications);
                    final dateKeys = grouped.keys.toList();
                    return RefreshIndicator(
                      onRefresh: () async {
                        ref.invalidate(notificationsProvider);
                        // Wait for the provider to finish refetching
                        await ref.read(notificationsProvider.future);
                      },
                      child: ListView.builder(
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        itemCount: dateKeys.length,
                        itemBuilder: (context, dateIndex) {
                          final date = dateKeys[dateIndex];
                          final dayNotifications = grouped[date]!;
                          return Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              DateSeparator(label: _formatDateLabel(date)),
                              ...dayNotifications.map(
                                (n) => _NotificationCard(notification: n),
                              ),
                            ],
                          );
                        },
                      ),
                    );
                  },
                ),
              ),
            ],
          ),
          // Floating hamburger menu + Notifications label
          Positioned(
            top: MediaQuery.of(context).padding.top + 12,
            left: 16,
            right: 16,
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
                Expanded(
                  child: Material(
                    color: Colors.white,
                    elevation: 0,
                    borderRadius: BorderRadius.circular(8),
                    child: const Padding(
                      padding: EdgeInsets.symmetric(horizontal: 10, vertical: 10),
                      child: Text(
                        'Notifications',
                        style: TextStyle(
                          color: Color(0xFF132E51),
                          fontWeight: FontWeight.w600,
                          fontSize: 18,
                        ),
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
}

/// Groups notifications by their [createdAt] date (date only, local time),
/// preserving the input order (newest first as returned by the API).
Map<DateTime, List<NotificationModel>> _groupNotificationsByDate(
    List<NotificationModel> notifications) {
  final Map<DateTime, List<NotificationModel>> grouped = {};
  for (final n in notifications) {
    final local = n.createdAt.toLocal();
    final dateKey = DateTime(local.year, local.month, local.day);
    grouped.putIfAbsent(dateKey, () => []).add(n);
  }
  return grouped;
}

/// Human-friendly label for a notification day group.
String _formatDateLabel(DateTime date) {
  final now = DateTime.now();
  final today = DateTime(now.year, now.month, now.day);
  final dateOnly = DateTime(date.year, date.month, date.day);
  final difference = dateOnly.difference(today).inDays;

  if (difference == 0) return 'Today';
  if (difference == -1) return 'Yesterday';
  return DateFormat('dd/MM').format(date);
}

/// A single notification card with unread indicator.
class _NotificationCard extends ConsumerWidget {
  const _NotificationCard({required this.notification});

  final NotificationModel notification;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final isUnread = !notification.isRead;

    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: Material(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        elevation: 0,
        child: InkWell(
          borderRadius: BorderRadius.circular(12),
          onTap: () => _onTap(context, ref),
          child: Container(
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(12),
              border: isUnread
                  ? const Border(
                      left: BorderSide(
                        color: AppTheme.primaryGold,
                        width: 4,
                      ),
                    )
                  : null,
            ),
            padding: const EdgeInsets.all(16),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Content
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        notification.title,
                        style: theme.textTheme.titleSmall?.copyWith(
                          fontWeight:
                              isUnread ? FontWeight.bold : FontWeight.w600,
                          color: AppTheme.navy,
                        ),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      const SizedBox(height: 4),
                      Text(
                        notification.message,
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: AppTheme.navy,
                        ),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      const SizedBox(height: 8),
                      Text(
                        _relativeTimestamp(notification.createdAt),
                        style: theme.textTheme.labelSmall?.copyWith(
                          color: Colors.grey.shade500,
                        ),
                      ),
                    ],
                  ),
                ),
                // Chevron
                Icon(
                  Icons.chevron_right,
                  color: Colors.grey.shade400,
                  size: 20,
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  void _onTap(BuildContext context, WidgetRef ref) {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => AlertDetailScreen(notification: notification),
      ),
    );
  }

  /// Formats a DateTime into a relative timestamp string.
  String _relativeTimestamp(DateTime dateTime) {
    final now = DateTime.now();
    final local = dateTime.toLocal();
    final diff = now.difference(local);

    if (diff.inMinutes < 1) return 'Just now';
    if (diff.inMinutes < 60) return '${diff.inMinutes} min ago';
    if (diff.inHours < 24) {
      final hours = diff.inHours;
      return '$hours ${hours == 1 ? 'hour' : 'hours'} ago';
    }
    if (diff.inDays == 1) return 'Yesterday';
    if (diff.inDays < 7) return '${diff.inDays} days ago';

    // Fallback to date
    return '${local.day}/${local.month}/${local.year}';
  }
}

/// Empty state widget shown when there are no notifications.
class _EmptyState extends StatelessWidget {
  const _EmptyState();

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Icon(
            Icons.notifications_none_outlined,
            size: 64,
            color: Colors.grey.shade400,
          ),
          const SizedBox(height: 16),
          Text(
            'No notifications yet',
            style: Theme.of(context).textTheme.titleMedium?.copyWith(
                  color: Colors.grey.shade600,
                ),
          ),
          const SizedBox(height: 8),
          Text(
            'You\'ll be notified about trip updates here',
            style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                  color: Colors.grey.shade500,
                ),
          ),
        ],
      ),
    );
  }
}

