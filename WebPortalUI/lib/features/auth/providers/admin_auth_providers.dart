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

/// State for the login flow.
enum LoginStatus { idle, loading, success, error }

class LoginState {
  const LoginState({
    this.status = LoginStatus.idle,
    this.errorMessage,
  });

  final LoginStatus status;
  final String? errorMessage;

  LoginState copyWith({
    LoginStatus? status,
    String? errorMessage,
  }) {
    return LoginState(
      status: status ?? this.status,
      errorMessage: errorMessage,
    );
  }
}

/// StateNotifier that manages the login flow.
class LoginNotifier extends StateNotifier<LoginState> {
  LoginNotifier(this._dioClient, this._authService)
      : super(const LoginState());

  final DioClient _dioClient;
  final AdminAuthService _authService;

  /// Attempt login with email and password.
  /// On 200: saves JWT token, returns true.
  /// On 401: sets generic error message, returns false.
  /// On network error: sets connection error message, returns false.
  Future<bool> login(String email, String password) async {
    state = const LoginState(status: LoginStatus.loading);

    try {
      final response = await _dioClient.dio.post(
        '/auth/admin/login',
        data: {
          'email': email,
          'password': password,
        },
      );

      if (response.statusCode == 200) {
        final token = response.data['token'] as String;
        await _authService.saveToken(token);
        state = const LoginState(status: LoginStatus.success);
        return true;
      }

      // Unexpected status code
      state = const LoginState(
        status: LoginStatus.error,
        errorMessage: 'Invalid credentials',
      );
      return false;
    } on DioException catch (e) {
      if (e.response?.statusCode == 401) {
        state = const LoginState(
          status: LoginStatus.error,
          errorMessage: 'Invalid credentials',
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

  /// Reset to idle state (e.g., when navigating away).
  void reset() {
    state = const LoginState();
  }
}

/// Provider for the LoginNotifier.
final loginProvider =
    StateNotifierProvider<LoginNotifier, LoginState>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  final authService = ref.watch(adminAuthServiceProvider);
  return LoginNotifier(dioClient, authService);
});
