import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/core/api/dio_client.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

// ─── Models ──────────────────────────────────────────────────────────────────

/// Represents a user returned from the API.
class UserModel {
  const UserModel({
    required this.id,
    required this.identifier,
    required this.firstName,
    required this.lastName,
    required this.email,
    required this.role,
    required this.isActive,
  });

  final int id;
  final String identifier;
  final String firstName;
  final String lastName;
  final String email;
  final String role;
  final bool isActive;

  String get fullName => '$firstName $lastName';

  factory UserModel.fromJson(Map<String, dynamic> json) {
    return UserModel(
      id: json['id'] as int,
      identifier: json['identifier'] as String? ?? '',
      firstName: json['firstName'] as String? ?? '',
      lastName: json['lastName'] as String? ?? '',
      email: json['email'] as String? ?? '',
      role: json['type'] as String? ?? json['role'] as String? ?? '',
      isActive: json['isActive'] as bool? ?? true,
    );
  }
}

/// Result from bulk CSV upload.
class BulkUploadResult {
  const BulkUploadResult({
    required this.successCount,
    required this.skippedRows,
  });

  final int successCount;
  final List<String> skippedRows;

  factory BulkUploadResult.fromJson(Map<String, dynamic> json) {
    // Backend returns { "created": int, "skipped": [{ "row": int, "reason": string }] }
    final skipped = (json['skipped'] as List<dynamic>?)
            ?.map((e) {
              if (e is Map<String, dynamic>) {
                return 'Row ${e['row']}: ${e['reason']}';
              }
              return e.toString();
            })
            .toList() ??
        [];

    return BulkUploadResult(
      successCount: json['created'] as int? ?? json['successCount'] as int? ?? 0,
      skippedRows: skipped,
    );
  }
}

// ─── State ───────────────────────────────────────────────────────────────────

enum UsersStatus { idle, loading, success, error }

class UsersState {
  const UsersState({
    this.status = UsersStatus.idle,
    this.users = const [],
    this.errorMessage,
  });

  final UsersStatus status;
  final List<UserModel> users;
  final String? errorMessage;

  UsersState copyWith({
    UsersStatus? status,
    List<UserModel>? users,
    String? errorMessage,
  }) {
    return UsersState(
      status: status ?? this.status,
      users: users ?? this.users,
      errorMessage: errorMessage,
    );
  }
}

// ─── Notifier ────────────────────────────────────────────────────────────────

class UsersNotifier extends StateNotifier<UsersState> {
  UsersNotifier(this._dioClient) : super(const UsersState());

  final DioClient _dioClient;

  /// Fetch all users from the API.
  Future<void> fetchUsers() async {
    state = state.copyWith(status: UsersStatus.loading);
    try {
      final response = await _dioClient.dio.get('/admin/users');
      final List<dynamic> data = response.data is List
          ? response.data as List<dynamic>
          : (response.data['users'] as List<dynamic>?) ?? [];
      final users = data.map((e) => UserModel.fromJson(e as Map<String, dynamic>)).toList();
      state = UsersState(status: UsersStatus.success, users: users);
    } on DioException catch (e) {
      state = UsersState(
        status: UsersStatus.error,
        errorMessage: e.response?.data?['message']?.toString() ??
            'Failed to load users',
      );
    } on Exception catch (_) {
      state = const UsersState(
        status: UsersStatus.error,
        errorMessage: 'Failed to load users',
      );
    }
  }

  /// Add a new user.
  Future<bool> addUser({
    required String role,
    required String identifier,
    required String firstName,
    required String lastName,
    required String email,
  }) async {
    try {
      await _dioClient.dio.post('/admin/users', data: {
        'type': role,
        if (role == 'Student') 'studentNumber': identifier,
        if (role == 'Marshal' || role == 'Admin') 'staffNumber': identifier,
        'firstName': firstName,
        'lastName': lastName,
        'email': email,
      });
      await fetchUsers();
      return true;
    } on DioException catch (e) {
      debugPrint('Add user error: ${e.response?.data}');
      return false;
    }
  }

  /// Update an existing user.
  Future<bool> updateUser({
    required int id,
    required String role,
    required String identifier,
    required String firstName,
    required String lastName,
    required String email,
    bool? isActive,
  }) async {
    try {
      await _dioClient.dio.put('/admin/users/$id?type=$role', data: {
        if (role == 'Student') 'studentNumber': identifier,
        if (role == 'Marshal' || role == 'Admin') 'staffNumber': identifier,
        'firstName': firstName,
        'lastName': lastName,
        'email': email,
        if (isActive != null) 'isActive': isActive,
      });
      await fetchUsers();
      return true;
    } on DioException catch (e) {
      debugPrint('Update user error: ${e.response?.data}');
      return false;
    }
  }

  /// Bulk upload users via CSV file bytes.
  Future<BulkUploadResult?> uploadCsv(Uint8List fileBytes, String fileName) async {
    try {
      final formData = FormData.fromMap({
        'file': MultipartFile.fromBytes(fileBytes, filename: fileName),
      });
      final response = await _dioClient.dio.post(
        '/admin/users/bulk',
        data: formData,
        options: Options(contentType: 'multipart/form-data'),
      );
      final result = BulkUploadResult.fromJson(
          response.data as Map<String, dynamic>);
      await fetchUsers();
      return result;
    } on DioException catch (e) {
      debugPrint('CSV upload error: ${e.response?.data}');
      return null;
    }
  }
}

// ─── Providers ───────────────────────────────────────────────────────────────

final usersProvider =
    StateNotifierProvider<UsersNotifier, UsersState>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return UsersNotifier(dioClient);
});
