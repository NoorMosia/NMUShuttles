import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/student/models/notification_model.dart';
import 'package:nmu_shuttle_app/features/student/providers/notification_providers.dart';
import 'package:nmu_shuttle_app/features/student/screens/alert_detail_screen.dart';
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
                  error: (error, _) => _ErrorState(
                    onRetry: () => ref.invalidate(notificationsProvider),
                  ),
                  data: (notifications) {
                    if (notifications.isEmpty) {
                      return const _EmptyState();
                    }
                    return RefreshIndicator(
                      onRefresh: () async {
                        ref.invalidate(notificationsProvider);
                        // Wait for the provider to finish refetching
                        await ref.read(notificationsProvider.future);
                      },
                      child: ListView.builder(
                        padding: const EdgeInsets.symmetric(horizontal: 16),
                        itemCount: notifications.length,
                        itemBuilder: (context, index) => _NotificationCard(
                          notification: notifications[index],
                        ),
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
                      'Notifications',
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
        color: isUnread ? AppTheme.primaryGold.withValues(alpha: 0.05) : Colors.white,
        borderRadius: BorderRadius.circular(12),
        elevation: 0,
        child: InkWell(
          borderRadius: BorderRadius.circular(12),
          onTap: () => _onTap(context, ref),
          child: Container(
            decoration: BoxDecoration(
              borderRadius: BorderRadius.circular(12),
              border: Border(
                left: BorderSide(
                  color: isUnread ? AppTheme.primaryGold : Colors.grey.shade300,
                  width: isUnread ? 4 : 2,
                ),
              ),
            ),
            padding: const EdgeInsets.all(16),
            child: Row(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                // Unread indicator dot
                if (isUnread)
                  Container(
                    margin: const EdgeInsets.only(top: 4, right: 12),
                    width: 10,
                    height: 10,
                    decoration: const BoxDecoration(
                      color: AppTheme.primaryGold,
                      shape: BoxShape.circle,
                    ),
                  )
                else
                  const SizedBox(width: 22),
                // Content
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        notification.title,
                        style: theme.textTheme.titleSmall?.copyWith(
                          fontWeight:
                              isUnread ? FontWeight.bold : FontWeight.w500,
                          color: isUnread
                              ? AppTheme.navy
                              : Colors.grey.shade700,
                        ),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                      ),
                      const SizedBox(height: 4),
                      Text(
                        notification.message,
                        style: theme.textTheme.bodySmall?.copyWith(
                          color: isUnread
                              ? Colors.grey.shade700
                              : Colors.grey.shade500,
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

/// Error state widget with retry button.
class _ErrorState extends StatelessWidget {
  const _ErrorState({required this.onRetry});

  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          const Icon(Icons.error_outline, size: 48, color: Colors.red),
          const SizedBox(height: 12),
          Text(
            'Failed to load notifications',
            style: Theme.of(context).textTheme.bodyLarge,
          ),
          const SizedBox(height: 8),
          TextButton(
            onPressed: onRetry,
            child: const Text('Retry'),
          ),
        ],
      ),
    );
  }
}
