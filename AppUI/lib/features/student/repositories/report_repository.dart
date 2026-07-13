import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/core/api/dio_client.dart';
import 'package:nmu_shuttle_app/features/student/models/report_model.dart';

/// Repository for report-related API calls.
class ReportRepository {
  ReportRepository({required DioClient dioClient}) : _dioClient = dioClient;

  final DioClient _dioClient;

  /// Fetches all reports for the authenticated student.
  /// GET /api/v1/reports
  Future<List<ReportModel>> getReports() async {
    final response = await _dioClient.dio.get(ApiConstants.reports);
    final List<dynamic> data = response.data as List<dynamic>;
    return data
        .map((json) => ReportModel.fromJson(json as Map<String, dynamic>))
        .toList();
  }

  /// Creates a new report.
  /// POST /api/v1/reports
  Future<ReportModel> createReport({
    required String category,
    required String description,
    int? tripInstanceId,
  }) async {
    final response = await _dioClient.dio.post(
      ApiConstants.reports,
      data: {
        'category': category,
        'description': description,
        'tripInstanceId': tripInstanceId,
      },
    );
    return ReportModel.fromJson(response.data as Map<String, dynamic>);
  }
}
