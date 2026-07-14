import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/student/models/report_model.dart';
import 'package:nmu_shuttle_app/features/student/repositories/report_repository.dart';

// ─── Student Profile ───────────────────────────────────────────

/// Model holding student profile info fetched from the API.
class StudentProfile {
  const StudentProfile({
    required this.firstName,
    required this.lastName,
    required this.studentNumber,
    required this.email,
    required this.isPriorityUser,
    required this.noShowCount,
    required this.cancelledCount,
    required this.strikePoints,
  });

  final String firstName;
  final String lastName;
  final String studentNumber;
  final String email;
  final bool isPriorityUser;
  final int noShowCount;
  final int cancelledCount;
  final int strikePoints;

  factory StudentProfile.fromJson(Map<String, dynamic> json) {
    return StudentProfile(
      firstName: json['firstName'] as String? ?? '',
      lastName: json['lastName'] as String? ?? '',
      studentNumber: json['studentNumber'] as String? ?? '',
      email: json['email'] as String? ?? '',
      isPriorityUser: json['isPriorityUser'] as bool? ?? false,
      noShowCount: json['noShowCount'] as int? ?? 0,
      cancelledCount: json['cancelledCount'] as int? ?? 0,
      strikePoints: json['strikePoints'] as int? ?? 0,
    );
  }
}

/// Provider that fetches the student profile from the API.
final studentProfileProvider = FutureProvider<StudentProfile?>((ref) async {
  final dioClient = ref.watch(dioClientProvider);

  try {
    final response = await dioClient.dio.get(ApiConstants.studentProfile);
    return StudentProfile.fromJson(response.data as Map<String, dynamic>);
  } catch (_) {
    return null;
  }
});

// ─── Reports ───────────────────────────────────────────────────

/// Provides the ReportRepository instance.
final reportRepositoryProvider = Provider<ReportRepository>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return ReportRepository(dioClient: dioClient);
});

/// FutureProvider that fetches reports, ordered by CreatedAt DESC.
final reportsProvider = FutureProvider<List<ReportModel>>((ref) async {
  final repo = ref.watch(reportRepositoryProvider);
  final reports = await repo.getReports();
  // Sort by createdAt descending
  reports.sort((a, b) => b.createdAt.compareTo(a.createdAt));
  return reports;
});
