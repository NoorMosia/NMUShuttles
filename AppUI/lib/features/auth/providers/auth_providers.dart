import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/api/dio_client.dart';
import 'package:nmu_shuttle_app/core/auth/auth_service.dart';
import 'package:nmu_shuttle_app/core/notifications/push_notification_service.dart';
import 'package:nmu_shuttle_app/features/auth/repositories/auth_repository.dart';
import 'package:nmu_shuttle_app/features/auth/services/auth_api_service.dart';

// ─── Enums ─────────────────────────────────────────────────────

/// The two authentication steps on the login screen.
enum AuthStep { identifier, otp }

/// User role selection on the login screen.
enum AuthRole { student, marshal }

// ─── Auth State ────────────────────────────────────────────────

/// Immutable state for the auth flow.
class AuthState {
  const AuthState({
    this.isLoading = false,
    this.error,
    this.currentStep = AuthStep.identifier,
    this.role = AuthRole.student,
    this.authenticatedRole,
    this.devOtp,
  });

  final bool isLoading;
  final String? error;
  final AuthStep currentStep;
  final AuthRole role;

  /// Set after successful OTP verification; used for routing.
  final String? authenticatedRole;

  /// EVALUATION ONLY: the OTP returned by non-production backends so it can be
  /// shown on the confirm screen. Null in production (backend omits it).
  final String? devOtp;

  AuthState copyWith({
    bool? isLoading,
    String? error,
    bool clearError = false,
    AuthStep? currentStep,
    AuthRole? role,
    String? authenticatedRole,
    String? devOtp,
    bool clearDevOtp = false,
  }) {
    return AuthState(
      isLoading: isLoading ?? this.isLoading,
      error: clearError ? null : (error ?? this.error),
      currentStep: currentStep ?? this.currentStep,
      role: role ?? this.role,
      authenticatedRole: authenticatedRole ?? this.authenticatedRole,
      devOtp: clearDevOtp ? null : (devOtp ?? this.devOtp),
    );
  }
}

// ─── Auth Notifier ─────────────────────────────────────────────

/// StateNotifier managing the full auth flow:
/// 1. Request OTP (student or marshal)
/// 2. Verify OTP → store JWT → register device → return role
class AuthNotifier extends StateNotifier<AuthState> {
  AuthNotifier({
    required AuthRepository authRepository,
    required AuthService authService,
  })  : _authRepository = authRepository,
        _authService = authService,
        super(const AuthState());

  final AuthRepository _authRepository;
  final AuthService _authService;

  /// Update the selected role.
  void setRole(AuthRole role) {
    state = state.copyWith(role: role, clearError: true);
  }

  /// Reset back to the identifier step (e.g., when user wants to change number).
  void resetToIdentifier() {
    state = const AuthState();
  }

  /// Step 1: Request an OTP for the given identifier.
  Future<void> requestOtp(AuthRole role, String identifier) async {
    state = state.copyWith(isLoading: true, clearError: true, clearDevOtp: true);

    try {
      final String? devOtp;
      if (role == AuthRole.student) {
        devOtp = await _authRepository.requestStudentOtp(identifier);
      } else {
        devOtp = await _authRepository.requestMarshalOtp(identifier);
      }

      state = state.copyWith(
        isLoading: false,
        currentStep: AuthStep.otp,
        devOtp: devOtp,
        clearDevOtp: devOtp == null,
      );
    } on DioException catch (e) {
      state = state.copyWith(
        isLoading: false,
        error: _mapDioError(e),
      );
    } catch (e) {
      state = state.copyWith(
        isLoading: false,
        error: 'An unexpected error occurred. Please try again.',
      );
    }
  }

  /// Step 2: Verify the OTP → store JWT → register device → resolve role.
  Future<void> verifyOtp(
    AuthRole role,
    String identifier,
    String otp,
  ) async {
    state = state.copyWith(isLoading: true, clearError: true);

    try {
      Map<String, dynamic> response;

      if (role == AuthRole.student) {
        response = await _authRepository.verifyStudentOtp(identifier, otp);
      } else {
        response = await _authRepository.verifyMarshalOtp(identifier, otp);
      }

      // Extract and store token
      final token = response['token'] as String;
      await _authService.saveToken(token);

      // Determine role from JWT claims
      final resolvedRole = await _authService.getUserRole() ?? 'Student';

      // Register device for push notifications (graceful failure)
      try {
        final fcmToken = await PushNotificationService.instance.getToken();
        if (fcmToken != null && fcmToken.isNotEmpty) {
          await _authRepository.registerDevice(fcmToken, 'android');
        } else {
          debugPrint('[Auth] No FCM token available; skipping device registration.');
        }
      } catch (e) {
        // Non-blocking — device registration failure shouldn't prevent login
        debugPrint('[Auth] Device registration failed: $e');
      }

      state = state.copyWith(
        isLoading: false,
        authenticatedRole: resolvedRole,
      );
    } on DioException catch (e) {
      state = state.copyWith(
        isLoading: false,
        error: _mapDioError(e),
      );
    } catch (e) {
      state = state.copyWith(
        isLoading: false,
        error: 'An unexpected error occurred. Please try again.',
      );
    }
  }

  /// Maps Dio error responses to user-friendly messages.
  String _mapDioError(DioException e) {
    final statusCode = e.response?.statusCode;
    switch (statusCode) {
      case 404:
        return 'User not found. Please check your number and try again.';
      case 422:
        return 'Invalid format. Please check your input.';
      case 401:
        return 'Invalid or expired OTP. Please try again.';
      case 429:
        return 'Too many attempts. Please wait before trying again.';
      default:
        return 'Connection error. Please check your internet and try again.';
    }
  }
}

// ─── Riverpod Providers ────────────────────────────────────────

/// Provides the DioClient singleton.
final dioClientProvider = Provider<DioClient>((ref) {
  return DioClient();
});

/// Provides the AuthService singleton.
final authServiceProvider = Provider<AuthService>((ref) {
  return AuthService();
});

/// Provides the AuthApiService.
final authApiServiceProvider = Provider<AuthApiService>((ref) {
  return AuthApiService(dioClient: ref.watch(dioClientProvider));
});

/// Provides the AuthRepository for data-layer API calls.
final authRepositoryProvider = Provider<AuthRepository>((ref) {
  return AuthRepository(dioClient: ref.watch(dioClientProvider));
});

/// Keeps the backend in sync when FCM rotates the device token.
/// Watch this provider once at app root so the subscription stays alive.
/// Re-registration only succeeds when a JWT is present (the Dio interceptor
/// attaches it); otherwise the backend returns 401 and we ignore it.
final fcmTokenRefreshProvider = Provider<void>((ref) {
  final authService = ref.watch(authServiceProvider);
  final authRepository = ref.watch(authRepositoryProvider);

  final subscription =
      PushNotificationService.instance.onTokenRefresh.listen((token) async {
    if (token.isEmpty) return;
    if (!await authService.isAuthenticated()) return;
    try {
      await authRepository.registerDevice(token, 'android');
      debugPrint('[FCM] Re-registered refreshed device token.');
    } catch (e) {
      debugPrint('[FCM] Token refresh re-registration failed: $e');
    }
  });

  ref.onDispose(subscription.cancel);
});

/// Provides the AuthNotifier StateNotifier + AuthState.
final authStateProvider =
    StateNotifierProvider<AuthNotifier, AuthState>((ref) {
  return AuthNotifier(
    authRepository: ref.watch(authRepositoryProvider),
    authService: ref.watch(authServiceProvider),
  );
});

/// FutureProvider that checks whether the user is currently authenticated
/// (valid, non-expired JWT stored on device).
final isAuthenticatedProvider = FutureProvider<bool>((ref) async {
  final authService = ref.watch(authServiceProvider);
  return authService.isAuthenticated();
});

/// FutureProvider that returns the current user's role from JWT claims.
/// Returns null if not authenticated or if role cannot be determined.
final userRoleProvider = FutureProvider<String?>((ref) async {
  final authService = ref.watch(authServiceProvider);
  return authService.getUserRole();
});
