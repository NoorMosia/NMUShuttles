import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:nmu_shuttle_admin/core/auth/admin_auth_service.dart';

/// Configures and provides a singleton Dio HTTP client for the admin portal.
/// - Attaches JWT Bearer token to all requests via interceptor.
/// - Handles 401 responses by clearing the token and triggering redirect to login.
/// - Uses SharedPreferences for web-safe token storage.
class DioClient {
  DioClient({
    String? baseUrl,
    AdminAuthService? authService,
  })  : _authService = authService ?? AdminAuthService(),
        _dio = Dio(
          BaseOptions(
            baseUrl: baseUrl ??
                'https://juicy-dubiously-pushpin.ngrok-free.dev/api/v1',
            connectTimeout: const Duration(seconds: 30),
            receiveTimeout: const Duration(seconds: 30),
            headers: {
              'Content-Type': 'application/json',
              'Accept': 'application/json',
              'ngrok-skip-browser-warning': 'true',
            },
          ),
        ) {
    _dio.interceptors.addAll([
      _jwtInterceptor(),
      _errorInterceptor(),
      if (kDebugMode) _loggingInterceptor(),
    ]);
  }

  final Dio _dio;
  final AdminAuthService _authService;

  /// Callback invoked when a 401 is received, allowing the router to redirect.
  VoidCallback? onUnauthorized;

  /// Exposes the configured Dio instance for use by repository classes.
  Dio get dio => _dio;

  /// JWT interceptor: reads token from SharedPreferences and attaches it.
  InterceptorsWrapper _jwtInterceptor() {
    return InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await _authService.getToken();
        if (kDebugMode) {
          debugPrint(
              '[DIO] Token present: ${token != null && token.isNotEmpty}');
        }
        if (token != null && token.isNotEmpty) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        handler.next(options);
      },
    );
  }

  /// Error interceptor: handles 401 (unauthorized) by clearing token.
  /// Skips token clearing for auth endpoints (login, etc.)
  InterceptorsWrapper _errorInterceptor() {
    return InterceptorsWrapper(
      onError: (error, handler) async {
        if (error.response?.statusCode == 401) {
          // Don't clear token for login requests — those are expected 401s
          final path = error.requestOptions.path;
          if (!path.contains('/auth/')) {
            await _authService.deleteToken();
            onUnauthorized?.call();
          }
        }
        handler.next(error);
      },
    );
  }

  /// Logging interceptor for debug builds only.
  InterceptorsWrapper _loggingInterceptor() {
    return InterceptorsWrapper(
      onRequest: (options, handler) {
        debugPrint('[DIO] → ${options.method} ${options.uri}');
        handler.next(options);
      },
      onResponse: (response, handler) {
        debugPrint(
            '[DIO] ← ${response.statusCode} ${response.requestOptions.uri}');
        handler.next(response);
      },
      onError: (error, handler) {
        debugPrint('[DIO] ✗ ${error.response?.statusCode} ${error.message}');
        handler.next(error);
      },
    );
  }
}
