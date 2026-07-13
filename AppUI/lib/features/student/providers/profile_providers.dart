import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:jwt_decoder/jwt_decoder.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/student/models/report_model.dart';
import 'package:nmu_shuttle_app/features/student/repositories/report_repository.dart';

// ─── Student Profile ───────────────────────────────────────────

/// Simple model holding student profile info extracted from JWT claims.
class StudentProfile {
  const StudentProfile({
    required this.firstName,
    required this.lastName,
    required this.studentNumber,
    required this.email,
  });

  final String firstName;
  final String lastName;
  final String studentNumber;
  final String email;
}

/// Provider that extracts student profile information from the stored JWT.
final studentProfileProvider = FutureProvider<StudentProfile?>((ref) async {
  final authService = ref.watch(authServiceProvider);
  final token = await authService.getToken();
  if (token == null || token.isEmpty) return null;

  try {
    final claims = JwtDecoder.decode(token);

    final firstName = claims['firstName'] as String? ??
        claims['given_name'] as String? ??
        'Student';
    final lastName = claims['lastName'] as String? ??
        claims['family_name'] as String? ??
        '';
    final studentNumber = claims['studentNumber'] as String? ??
        claims['sub'] as String? ??
        '';
    final email = claims['email'] as String? ??
        claims['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress']
            as String? ??
        '';

    return StudentProfile(
      firstName: firstName,
      lastName: lastName,
      studentNumber: studentNumber,
      email: email,
    );
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
