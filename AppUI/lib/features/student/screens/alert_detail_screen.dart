import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/student/models/notification_model.dart';
import 'package:nmu_shuttle_app/features/student/providers/notification_providers.dart';

/// Detail screen showing the full title and message of a notification.
/// Marks the notification as read on open (fire-and-forget).
class AlertDetailScreen extends ConsumerStatefulWidget {
  const AlertDetailScreen({super.key, required this.notification});

  final NotificationModel notification;

  @override
  ConsumerState<AlertDetailScreen> createState() => _AlertDetailScreenState();
}

class _AlertDetailScreenState extends ConsumerState<AlertDetailScreen> {
  @override
  void initState() {
    super.initState();
    // Fire-and-forget: mark as read when opened
    if (!widget.notification.isRead) {
      ref
          .read(notificationRepositoryProvider)
          .markAsRead(widget.notification.notificationId)
          .then((_) {
        // Invalidate to refresh the list and unread count
        ref.invalidate(notificationsProvider);
      }).catchError((_) {
        // Silently ignore — best effort
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final notification = widget.notification;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Notification'),
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: () => Navigator.of(context).pop(),
        ),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Title
            Text(
              notification.title,
              style: theme.textTheme.headlineSmall?.copyWith(
                fontWeight: FontWeight.bold,
                color: AppTheme.navy,
              ),
            ),
            const SizedBox(height: 8),
            // Timestamp
            Text(
              _formatFullDate(notification.createdAt),
              style: theme.textTheme.bodySmall?.copyWith(
                color: Colors.grey.shade600,
              ),
            ),
            const Divider(height: 32),
            // Message body
            Text(
              notification.message,
              style: theme.textTheme.bodyLarge?.copyWith(
                height: 1.5,
              ),
            ),
          ],
        ),
      ),
    );
  }

  String _formatFullDate(DateTime dateTime) {
    final local = dateTime.toLocal();
    final months = [
      'January', 'February', 'March', 'April', 'May', 'June',
      'July', 'August', 'September', 'October', 'November', 'December',
    ];
    final hour = local.hour.toString().padLeft(2, '0');
    final minute = local.minute.toString().padLeft(2, '0');
    return '${local.day} ${months[local.month - 1]} ${local.year} at $hour:$minute';
  }
}
