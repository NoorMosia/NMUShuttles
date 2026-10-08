import 'package:firebase_core/firebase_core.dart';
import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_local_notifications/flutter_local_notifications.dart';

/// Top-level background handler. Must be a top-level or static function
/// annotated with `@pragma('vm:entry-point')` so it survives tree-shaking
/// and can run in a separate isolate when the app is terminated/backgrounded.
@pragma('vm:entry-point')
Future<void> firebaseMessagingBackgroundHandler(RemoteMessage message) async {
  // Firebase must be initialised in the background isolate before use.
  await Firebase.initializeApp();
  // The OS already displays the `notification` payload while backgrounded,
  // so there is nothing more to do here. Hook data-message handling in if needed.
  debugPrint('[FCM] Background message: ${message.messageId}');
}

/// Centralised Firebase Cloud Messaging client.
///
/// Responsibilities:
/// - Initialise Firebase + local-notifications channel.
/// - Request runtime notification permission (Android 13+ / iOS).
/// - Expose the current FCM device token for backend registration.
/// - Surface foreground messages as heads-up local notifications.
class PushNotificationService {
  PushNotificationService._();

  static final PushNotificationService instance = PushNotificationService._();

  final FirebaseMessaging _messaging = FirebaseMessaging.instance;
  final FlutterLocalNotificationsPlugin _localNotifications =
      FlutterLocalNotificationsPlugin();

  bool _initialised = false;

  /// Android notification channel used for foreground heads-up notifications.
  static const AndroidNotificationChannel _channel = AndroidNotificationChannel(
    'nmu_shuttle_default',
    'Shuttle Alerts',
    description: 'Trip updates, breakdowns and schedule changes.',
    importance: Importance.high,
  );

  /// Initialise Firebase, the local-notifications plugin and the FCM listeners.
  /// Safe to call multiple times — only the first call does work.
  Future<void> initialize() async {
    if (_initialised) return;

    await Firebase.initializeApp();

    // Register the background handler as early as possible.
    FirebaseMessaging.onBackgroundMessage(firebaseMessagingBackgroundHandler);

    await _setupLocalNotifications();
    await requestPermission();

    // Foreground messages: Android does not show them automatically, so we
    // render a heads-up local notification ourselves.
    FirebaseMessaging.onMessage.listen(_showForegroundNotification);

    _initialised = true;
  }

  /// Requests runtime permission to display notifications.
  Future<void> requestPermission() async {
    await _messaging.requestPermission(
      alert: true,
      badge: true,
      sound: true,
    );
  }

  /// Returns the current FCM registration token, or null if unavailable.
  Future<String?> getToken() async {
    try {
      return await _messaging.getToken();
    } catch (e) {
      debugPrint('[FCM] Failed to get token: $e');
      return null;
    }
  }

  /// Stream of token-refresh events. Backend re-registration should listen here.
  Stream<String> get onTokenRefresh => _messaging.onTokenRefresh;

  /// Deletes the device token locally (used on logout).
  Future<void> deleteToken() async {
    try {
      await _messaging.deleteToken();
    } catch (e) {
      debugPrint('[FCM] Failed to delete token: $e');
    }
  }

  Future<void> _setupLocalNotifications() async {
    const androidInit =
        AndroidInitializationSettings('@mipmap/ic_launcher');
    const initSettings = InitializationSettings(android: androidInit);

    await _localNotifications.initialize(initSettings);

    await _localNotifications
        .resolvePlatformSpecificImplementation<
            AndroidFlutterLocalNotificationsPlugin>()
        ?.createNotificationChannel(_channel);
  }

  void _showForegroundNotification(RemoteMessage message) {
    final notification = message.notification;
    if (notification == null) return;

    _localNotifications.show(
      notification.hashCode,
      notification.title,
      notification.body,
      NotificationDetails(
        android: AndroidNotificationDetails(
          _channel.id,
          _channel.name,
          channelDescription: _channel.description,
          importance: Importance.high,
          priority: Priority.high,
          icon: '@mipmap/ic_launcher',
        ),
      ),
    );
  }
}
