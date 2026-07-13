/// Model representing a notification from the API.
class NotificationModel {
  const NotificationModel({
    required this.notificationId,
    required this.title,
    required this.message,
    required this.isRead,
    this.relatedTripInstanceId,
    required this.createdAt,
  });

  final int notificationId;
  final String title;
  final String message;
  final bool isRead;
  final int? relatedTripInstanceId;
  final DateTime createdAt;

  factory NotificationModel.fromJson(Map<String, dynamic> json) {
    return NotificationModel(
      notificationId: json['notificationId'] as int,
      title: json['title'] as String,
      message: json['message'] as String,
      isRead: json['isRead'] as bool,
      relatedTripInstanceId: json['relatedTripInstanceId'] as int?,
      createdAt: DateTime.parse(json['createdAt'] as String),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'notificationId': notificationId,
      'title': title,
      'message': message,
      'isRead': isRead,
      'relatedTripInstanceId': relatedTripInstanceId,
      'createdAt': createdAt.toIso8601String(),
    };
  }

  /// Returns a copy with updated fields.
  NotificationModel copyWith({
    int? notificationId,
    String? title,
    String? message,
    bool? isRead,
    int? relatedTripInstanceId,
    DateTime? createdAt,
  }) {
    return NotificationModel(
      notificationId: notificationId ?? this.notificationId,
      title: title ?? this.title,
      message: message ?? this.message,
      isRead: isRead ?? this.isRead,
      relatedTripInstanceId:
          relatedTripInstanceId ?? this.relatedTripInstanceId,
      createdAt: createdAt ?? this.createdAt,
    );
  }
}
