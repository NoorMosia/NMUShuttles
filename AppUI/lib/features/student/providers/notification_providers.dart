import 'dart:async';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/student/models/notification_model.dart';
import 'package:nmu_shuttle_app/features/student/repositories/notification_repository.dart';

/// Provides the NotificationRepository instance.
final notificationRepositoryProvider = Provider<NotificationRepository>((ref) {
  return NotificationRepository(dioClient: ref.watch(dioClientProvider));
});

/// Fetches all notifications ordered by CreatedAt DESC.
///
/// Implemented as a self-polling stream (fetch immediately, then every 30s) so
/// the alerts screen stays current even when a push can't be delivered (e.g.
/// stale/placeholder tokens or Firebase not configured). A delivered push still
/// triggers an immediate refresh via `ref.invalidate(notificationsProvider)`,
/// which restarts this stream. Pull-to-refresh and error-retry also invalidate.
final notificationsProvider =
    StreamProvider<List<NotificationModel>>((ref) {
  final repository = ref.watch(notificationRepositoryProvider);
  final controller = StreamController<List<NotificationModel>>();

  Future<void> fetch() async {
    try {
      final notifications = await repository.getNotifications();
      // Sort by createdAt descending (most recent first)
      notifications.sort((a, b) => b.createdAt.compareTo(a.createdAt));
      if (!controller.isClosed) {
        controller.add(notifications);
      }
    } catch (e) {
      if (!controller.isClosed) {
        controller.addError(e);
      }
    }
  }

  fetch();
  final timer = Timer.periodic(const Duration(seconds: 30), (_) => fetch());

  ref.onDispose(() {
    timer.cancel();
    controller.close();
  });

  return controller.stream;
});

/// Derived provider: count of unread notifications.
final unreadNotificationCountProvider = Provider<AsyncValue<int>>((ref) {
  return ref.watch(notificationsProvider).whenData(
        (notifications) =>
            notifications.where((n) => !n.isRead).length,
      );
});
