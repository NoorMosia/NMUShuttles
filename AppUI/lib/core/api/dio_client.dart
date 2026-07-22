import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:nmu_shuttle_app/core/auth/auth_service.dart';

/// Configures and provides a singleton Dio HTTP client.
/// - Attaches JWT Bearer token to all requests via interceptor.
/// - Handles 401 responses by clearing the token (triggers redirect to login).
/// - Configurable base URL (defaults to Android emulator localhost).
class DioClient {
  DioClient({
    String? baseUrl,
    AuthService? authService,
  })  : _authService = authService ?? AuthService(),
        _dio = Dio(
          BaseOptions(
            baseUrl: baseUrl ?? 'http://10.0.2.2:5000/api/v1',
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
  final AuthService _authService;

  /// Callback invoked when a 401 is received, allowing the router to redirect.
  VoidCallback? onUnauthorized;

  /// Exposes the configured Dio instance for use by repository classes.
  Dio get dio => _dio;

  /// JWT interceptor: reads token from secure storage and attaches it.
  InterceptorsWrapper _jwtInterceptor() {
    return InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await _authService.getToken();
        if (token != null && token.isNotEmpty) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        handler.next(options);
      },
    );
  }

  /// Error interceptor: handles 401 (unauthorized) and logs errors.
  InterceptorsWrapper _errorInterceptor() {
    return InterceptorsWrapper(
      onError: (error, handler) async {
        if (error.response?.statusCode == 401) {
          // Clear the stored token — session expired or invalid
          await _authService.deleteToken();
          onUnauthorized?.call();
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
