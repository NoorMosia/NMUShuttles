import 'dart:convert';

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
  // Guard against duplicate-app errors if the default app already exists.
  if (Firebase.apps.isEmpty) {
    await Firebase.initializeApp();
  }
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

  final FlutterLocalNotificationsPlugin _localNotifications =
      FlutterLocalNotificationsPlugin();

  bool _initialised = false;

  /// Invoked when the user taps a notification (foreground, background, or
  /// cold-start). Receives the message's `data` payload so the app can
  /// deep-link. Set by the app layer via [setNotificationTapHandler]; kept as
  /// a callback so this service stays decoupled from the router.
  void Function(Map<String, dynamic> data)? _onNotificationTap;

  /// If a notification launched the app from a terminated state before the tap
  /// handler was registered, its data is held here and replayed once a handler
  /// is set.
  Map<String, dynamic>? _pendingTapData;

  /// Registers the handler used to route notification taps. Any tap captured
  /// before this was set (e.g. cold start) is replayed immediately.
  void setNotificationTapHandler(void Function(Map<String, dynamic> data) handler) {
    _onNotificationTap = handler;
    final pending = _pendingTapData;
    if (pending != null) {
      _pendingTapData = null;
      handler(pending);
    }
  }

  void _dispatchTap(Map<String, dynamic> data) {
    final handler = _onNotificationTap;
    if (handler != null) {
      handler(data);
    } else {
      // No handler yet (cold start before the router is ready) — hold it.
      _pendingTapData = data;
    }
  }

  /// True once Firebase + FCM have been initialised successfully. When false,
  /// all messaging accessors below are no-ops so a Firebase failure can never
  /// crash the UI (e.g. missing/stale native config in a dev build).
  bool get isAvailable => _initialised;

  /// Lazily resolved so we never touch `FirebaseMessaging.instance` (which
  /// throws `[core/no-app]`) before Firebase has actually been created.
  FirebaseMessaging get _messaging => FirebaseMessaging.instance;

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

    // Avoid a duplicate-app error if Firebase was already created elsewhere.
    if (Firebase.apps.isEmpty) {
      await Firebase.initializeApp();
    }

    // Register the background handler as early as possible.
    FirebaseMessaging.onBackgroundMessage(firebaseMessagingBackgroundHandler);

    await _setupLocalNotifications();
    await requestPermission();

    // Foreground messages: Android does not show them automatically, so we
    // render a heads-up local notification ourselves.
    FirebaseMessaging.onMessage.listen(_showForegroundNotification);

    // Tap on a notification while the app is backgrounded (but alive).
    FirebaseMessaging.onMessageOpenedApp.listen((message) {
      _dispatchTap(message.data);
    });

    // App launched from a terminated state by tapping a notification.
    final initialMessage = await _messaging.getInitialMessage();
    if (initialMessage != null) {
      _dispatchTap(initialMessage.data);
    }

    // Only mark available once Firebase + FCM wiring succeeded.
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
    if (!_initialised) return null;
    try {
      return await _messaging.getToken();
    } catch (e) {
      debugPrint('[FCM] Failed to get token: $e');
      return null;
    }
  }

  /// Stream of token-refresh events. Backend re-registration should listen here.
  /// Returns an empty stream when FCM is unavailable so callers never touch
  /// `FirebaseMessaging.instance` before Firebase is initialised.
  Stream<String> get onTokenRefresh =>
      _initialised ? _messaging.onTokenRefresh : const Stream<String>.empty();

  /// Deletes the device token locally (used on logout).
  Future<void> deleteToken() async {
    if (!_initialised) return;
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

    await _localNotifications.initialize(
      initSettings,
      // Tap on a foreground notification we rendered ourselves. The FCM data
      // payload is JSON-encoded into the notification payload below.
      onDidReceiveNotificationResponse: (response) {
        final payload = response.payload;
        if (payload == null || payload.isEmpty) return;
        try {
          final decoded = jsonDecode(payload) as Map<String, dynamic>;
          _dispatchTap(decoded);
        } catch (e) {
          debugPrint('[FCM] Failed to decode notification payload: $e');
        }
      },
    );

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
      // Carry the FCM data payload so a tap can deep-link, matching the
      // background/cold-start paths handled via FirebaseMessaging streams.
      payload: message.data.isEmpty ? null : jsonEncode(message.data),
    );
  }
}
