import 'package:dio/dio.dart';
import 'package:nmu_shuttle_app/core/api/dio_client.dart';

/// API service for authentication endpoints.
/// Handles OTP request/verify for both Student and Marshal roles,
/// and device registration for push notifications.
class AuthApiService {
  AuthApiService({required DioClient dioClient}) : _dio = dioClient.dio;

  final Dio _dio;

  // ─── Student OTP ───────────────────────────────────────────────

  /// Request an OTP for a student.
  /// POST /auth/student/otp/request
  /// Body: { "studentNumber": "s312345678" }
  Future<void> requestStudentOtp(String studentNumber) async {
    await _dio.post(
      '/auth/student/otp/request',
      data: {'studentNumber': studentNumber},
    );
  }

  /// Verify a student's OTP and return auth response data.
  /// POST /auth/student/otp/verify
  /// Body: { "studentNumber": "s312345678", "otp": "12345" }
  Future<Map<String, dynamic>> verifyStudentOtp(
    String studentNumber,
    String otp,
  ) async {
    final response = await _dio.post(
      '/auth/student/otp/verify',
      data: {'studentNumber': studentNumber, 'otp': otp},
    );
    return response.data as Map<String, dynamic>;
  }

  // ─── Marshal OTP ───────────────────────────────────────────────

  /// Request an OTP for a marshal.
  /// POST /auth/marshal/otp/request
  /// Body: { "staffNumber": "1234567" }
  Future<void> requestMarshalOtp(String staffNumber) async {
    await _dio.post(
      '/auth/marshal/otp/request',
      data: {'staffNumber': staffNumber},
    );
  }

  /// Verify a marshal's OTP and return auth response data.
  /// POST /auth/marshal/otp/verify
  /// Body: { "staffNumber": "1234567", "otp": "12345" }
  Future<Map<String, dynamic>> verifyMarshalOtp(
    String staffNumber,
    String otp,
  ) async {
    final response = await _dio.post(
      '/auth/marshal/otp/verify',
      data: {'staffNumber': staffNumber, 'otp': otp},
    );
    return response.data as Map<String, dynamic>;
  }

  // ─── Device Registration ───────────────────────────────────────

  /// Register a device for push notifications.
  /// POST /auth/device/register
  /// Body: { "fcmToken": "...", "platform": "android" }
  Future<void> registerDevice(String fcmToken, String platform) async {
    await _dio.post(
      '/auth/device/register',
      data: {'fcmToken': fcmToken, 'platform': platform},
    );
  }
}
