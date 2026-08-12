import 'package:nmu_shuttle_app/core/config/env_config.dart';

/// API route constants for the NMU Shuttle backend.
class ApiConstants {
  ApiConstants._();

  // Base URL — driven by EnvConfig.useNgrok
  static const String defaultBaseUrl = EnvConfig.baseUrl;

  // Auth endpoints
  static const String studentOtpRequest = '/auth/student/otp/request';
  static const String studentOtpVerify = '/auth/student/otp/verify';
  static const String marshalOtpRequest = '/auth/marshal/otp/request';
  static const String marshalOtpVerify = '/auth/marshal/otp/verify';
  static const String deviceRegister = '/auth/device/register';

  // Stop endpoints
  static const String stops = '/stops';

  // Booking endpoints
  static const String trips = '/trips';
  static const String bookings = '/bookings';

  // Check-in endpoints
  static const String checkIn = '/checkin';

  // Notification endpoints
  static const String notifications = '/notifications';

  // Report endpoints
  static const String reports = '/reports';

  // Student profile endpoint
  static const String studentProfile = '/students/me';
}
