import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/student/models/notification_model.dart';
import 'package:nmu_shuttle_app/features/student/repositories/notification_repository.dart';

/// Provides the NotificationRepository instance.
final notificationRepositoryProvider = Provider<NotificationRepository>((ref) {
  return NotificationRepository(dioClient: ref.watch(dioClientProvider));
});

/// Fetches all notifications ordered by CreatedAt DESC.
final notificationsProvider =
    FutureProvider<List<NotificationModel>>((ref) async {
  final repository = ref.watch(notificationRepositoryProvider);
  final notifications = await repository.getNotifications();
  // Sort by createdAt descending (most recent first)
  notifications.sort((a, b) => b.createdAt.compareTo(a.createdAt));
  return notifications;
});

/// Derived provider: count of unread notifications.
final unreadNotificationCountProvider = Provider<AsyncValue<int>>((ref) {
  return ref.watch(notificationsProvider).whenData(
        (notifications) =>
            notifications.where((n) => !n.isRead).length,
      );
});
