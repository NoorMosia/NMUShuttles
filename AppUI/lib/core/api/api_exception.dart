import 'package:dio/dio.dart';

/// A user-friendly exception that wraps [DioException] and extracts
/// the meaningful error message from RFC 7807 ProblemDetails responses.
///
/// Usage:
/// ```dart
/// try {
///   await dio.post('/bookings', data: {...});
/// } on DioException catch (e) {
///   throw ApiException.fromDioException(e);
/// }
/// ```
class ApiException implements Exception {
  /// The HTTP status code (e.g. 409, 400, 404).
  final int? statusCode;

  /// A short title (e.g. "Conflict", "Bad Request").
  final String? title;

  /// The user-friendly detail message from the API (e.g. "You already have a
  /// booking to campus for this day").
  final String message;

  /// The original [DioException], if available.
  final DioException? originalException;

  const ApiException({
    this.statusCode,
    this.title,
    required this.message,
    this.originalException,
  });

  /// Creates an [ApiException] from a [DioException] by parsing the response
  /// body for ProblemDetails fields (`detail`, `title`, `message`).
  ///
  /// Falls back to a generic message when the response body can't be parsed.
  factory ApiException.fromDioException(DioException exception) {
    final response = exception.response;
    final statusCode = response?.statusCode;

    // Try to extract structured error info from the response body
    String? detail;
    String? title;

    final data = response?.data;
    if (data is Map<String, dynamic>) {
      // RFC 7807 ProblemDetails: prefer "detail", then "message", then "title"
      detail = data['detail'] as String? ??
          data['message'] as String? ??
          data['title'] as String?;
      title = data['title'] as String?;
    } else if (data is String && data.isNotEmpty) {
      detail = data;
    }

    // Build a user-friendly fallback if nothing was extracted
    final message = detail ?? _fallbackMessage(statusCode, exception);

    return ApiException(
      statusCode: statusCode,
      title: title,
      message: message,
      originalException: exception,
    );
  }

  /// Returns a generic user-facing message based on the HTTP status code.
  static String _fallbackMessage(int? statusCode, DioException exception) {
    if (exception.type == DioExceptionType.connectionTimeout ||
        exception.type == DioExceptionType.receiveTimeout ||
        exception.type == DioExceptionType.sendTimeout) {
      return 'Connection timed out. Please check your internet and try again.';
    }

    if (exception.type == DioExceptionType.connectionError) {
      return 'Unable to connect to the server. Please check your internet connection.';
    }

    switch (statusCode) {
      case 400:
        return 'Invalid request. Please check your input and try again.';
      case 401:
        return 'Your session has expired. Please log in again.';
      case 403:
        return 'You don\'t have permission to perform this action.';
      case 404:
        return 'The requested resource was not found.';
      case 409:
        return 'A conflict occurred. You may have already performed this action.';
      case 422:
        return 'The provided data is invalid. Please check and try again.';
      case 500:
        return 'Something went wrong on our end. Please try again later.';
      default:
        return 'Something went wrong. Please try again.';
    }
  }

  /// Whether this is a conflict error (HTTP 409) — typically a duplicate action.
  bool get isConflict => statusCode == 409;

  /// Whether this is a "locked trip" or bad request error (HTTP 400).
  bool get isBadRequest => statusCode == 400;

  /// Whether this is a not found error (HTTP 404).
  bool get isNotFound => statusCode == 404;

  /// Whether this is a network/connectivity issue (no response received).
  bool get isNetworkError =>
      originalException?.type == DioExceptionType.connectionTimeout ||
      originalException?.type == DioExceptionType.receiveTimeout ||
      originalException?.type == DioExceptionType.connectionError;

  @override
  String toString() => message;
}
