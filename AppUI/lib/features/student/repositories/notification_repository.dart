import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/core/api/dio_client.dart';
import 'package:nmu_shuttle_app/features/student/models/notification_model.dart';

/// Repository for notification-related API calls.
class NotificationRepository {
  NotificationRepository({required DioClient dioClient})
      : _dioClient = dioClient;

  final DioClient _dioClient;

  /// Fetches all notifications for the authenticated user.
  /// GET /api/v1/notifications
  Future<List<NotificationModel>> getNotifications() async {
    final response = await _dioClient.dio.get(ApiConstants.notifications);
    final List<dynamic> data = response.data as List<dynamic>;
    return data
        .map((json) =>
            NotificationModel.fromJson(json as Map<String, dynamic>))
        .toList();
  }

  /// Marks a notification as read.
  /// PATCH /api/v1/notifications/{id}/read
  Future<void> markAsRead(int id) async {
    await _dioClient.dio.patch('${ApiConstants.notifications}/$id/read');
  }
}
