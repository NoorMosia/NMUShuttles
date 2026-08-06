import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/core/api/dio_client.dart';
import 'package:nmu_shuttle_admin/core/auth/admin_auth_service.dart';

/// Singleton provider for the AdminAuthService.
final adminAuthServiceProvider = Provider<AdminAuthService>((ref) {
  return AdminAuthService();
});

/// Singleton provider for the DioClient.
final dioClientProvider = Provider<DioClient>((ref) {
  final authService = ref.watch(adminAuthServiceProvider);
  return DioClient(authService: authService);
});

/// State for the login flow (two-step OTP).
enum LoginStatus { idle, loading, otpSent, verifying, success, error }

class LoginState {
  const LoginState({
    this.status = LoginStatus.idle,
    this.errorMessage,
    this.email,
  });

  final LoginStatus status;
  final String? errorMessage;
  final String? email;

  LoginState copyWith({
    LoginStatus? status,
    String? errorMessage,
    String? email,
  }) {
    return LoginState(
      status: status ?? this.status,
      errorMessage: errorMessage,
      email: email ?? this.email,
    );
  }
}

/// StateNotifier that manages the two-step OTP login flow.
class LoginNotifier extends StateNotifier<LoginState> {
  LoginNotifier(this._dioClient, this._authService)
      : super(const LoginState());

  final DioClient _dioClient;
  final AdminAuthService _authService;

  /// Step 1: Request OTP for the given email.
  /// On 200: transitions to otpSent state, returns true.
  /// On 404: admin not found error, returns false.
  /// On network error: sets connection error message, returns false.
  Future<bool> requestOtp(String email) async {
    state = LoginState(status: LoginStatus.loading, email: email);

    try {
      final response = await _dioClient.dio.post(
        '/auth/admin/otp/request',
        data: {'email': email},
      );

      if (response.statusCode == 200) {
        state = LoginState(status: LoginStatus.otpSent, email: email);
        return true;
      }

      state = const LoginState(
        status: LoginStatus.error,
        errorMessage: 'Admin not found',
      );
      return false;
    } on DioException catch (e) {
      if (e.response?.statusCode == 404) {
        state = const LoginState(
          status: LoginStatus.error,
          errorMessage: 'Admin not found',
        );
      } else {
        state = const LoginState(
          status: LoginStatus.error,
          errorMessage: 'Unable to connect to server',
        );
      }
      return false;
    } on Exception catch (_) {
      state = const LoginState(
        status: LoginStatus.error,
        errorMessage: 'Unable to connect to server',
      );
      return false;
    }
  }

  /// Step 2: Verify OTP and obtain JWT.
  /// On 200: saves JWT token, returns true.
  /// On 401: invalid/expired OTP, returns false.
  /// On network error: sets connection error message, returns false.
  Future<bool> verifyOtp(String otp) async {
    final email = state.email;
    if (email == null) {
      state = const LoginState(
        status: LoginStatus.error,
        errorMessage: 'Email not set. Please start over.',
      );
      return false;
    }

    state = LoginState(status: LoginStatus.verifying, email: email);

    try {
      final response = await _dioClient.dio.post(
        '/auth/admin/otp/verify',
        data: {
          'email': email,
          'otp': otp,
        },
      );

      if (response.statusCode == 200) {
        final token = response.data['token'] as String;
        await _authService.saveToken(token);
        state = LoginState(status: LoginStatus.success, email: email);
        return true;
      }

      state = LoginState(
        status: LoginStatus.otpSent,
        email: email,
        errorMessage: 'Invalid or expired OTP',
      );
      return false;
    } on DioException catch (e) {
      if (e.response?.statusCode == 401) {
        state = LoginState(
          status: LoginStatus.otpSent,
          email: email,
          errorMessage: 'Invalid or expired OTP',
        );
      } else {
        state = LoginState(
          status: LoginStatus.otpSent,
          email: email,
          errorMessage: 'Unable to connect to server',
        );
      }
      return false;
    } on Exception catch (_) {
      state = LoginState(
        status: LoginStatus.otpSent,
        email: email,
        errorMessage: 'Unable to connect to server',
      );
      return false;
    }
  }

  /// Reset to idle state (e.g., when navigating away or going back).
  void reset() {
    state = const LoginState();
  }

  /// Go back to the email step from OTP step.
  void backToEmail() {
    state = LoginState(status: LoginStatus.idle, email: state.email);
  }
}

/// Provider for the LoginNotifier.
final loginProvider =
    StateNotifierProvider<LoginNotifier, LoginState>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  final authService = ref.watch(adminAuthServiceProvider);
  return LoginNotifier(dioClient, authService);
});
