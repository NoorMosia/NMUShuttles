import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

/// Model representing trip details linked to a report.
class TripDetails {
  const TripDetails({
    required this.tripInstanceId,
    required this.tripDate,
    required this.departureTime,
    required this.direction,
    required this.stopName,
    required this.tripStatus,
    this.shuttlePlate,
    this.vendorName,
  });

  final int tripInstanceId;
  final String tripDate;
  final String departureTime;
  final String direction;
  final String stopName;
  final String tripStatus;
  final String? shuttlePlate;
  final String? vendorName;

  factory TripDetails.fromJson(Map<String, dynamic> json) {
    return TripDetails(
      tripInstanceId: json['tripInstanceId'] as int? ?? 0,
      tripDate: json['tripDate'] as String? ?? '',
      departureTime: json['departureTime'] as String? ?? '',
      direction: json['direction'] as String? ?? '',
      stopName: json['stopName'] as String? ?? '',
      tripStatus: json['tripStatus'] as String? ?? '',
      shuttlePlate: json['shuttlePlate'] as String?,
      vendorName: json['vendorName'] as String?,
    );
  }
}

/// Model representing a student report.
class Report {
  const Report({
    required this.id,
    required this.category,
    required this.studentNumber,
    required this.submissionDate,
    required this.status,
    this.description,
    this.adminNotes,
    this.tripInstanceId,
    this.bookingId,
    this.tripDetails,
  });

  final int id;
  final String category;
  final String studentNumber;
  final String submissionDate;
  final String status;
  final String? description;
  final String? adminNotes;
  final int? tripInstanceId;
  final int? bookingId;
  final TripDetails? tripDetails;

  factory Report.fromJson(Map<String, dynamic> json) {
    return Report(
      id: json['id'] as int? ?? json['reportId'] as int? ?? 0,
      category: json['category'] as String? ?? '',
      studentNumber: json['studentNumber'] as String? ?? '',
      submissionDate: json['submissionDate'] as String? ?? '',
      status: json['status'] as String? ?? 'Submitted',
      description: json['description'] as String?,
      adminNotes: json['adminNotes'] as String?,
      tripInstanceId: json['tripInstanceId'] as int?,
      bookingId: json['bookingId'] as int?,
      tripDetails: json['tripDetails'] != null
          ? TripDetails.fromJson(json['tripDetails'] as Map<String, dynamic>)
          : null,
    );
  }
}

/// State for the reports list.
class ReportsState {
  const ReportsState({
    this.reports = const [],
    this.isLoading = false,
    this.errorMessage,
  });

  final List<Report> reports;
  final bool isLoading;
  final String? errorMessage;

  ReportsState copyWith({
    List<Report>? reports,
    bool? isLoading,
    String? errorMessage,
  }) {
    return ReportsState(
      reports: reports ?? this.reports,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage,
    );
  }
}

/// State for a single report detail.
class ReportDetailState {
  const ReportDetailState({
    this.report,
    this.isLoading = false,
    this.errorMessage,
  });

  final Report? report;
  final bool isLoading;
  final String? errorMessage;

  ReportDetailState copyWith({
    Report? report,
    bool? isLoading,
    String? errorMessage,
  }) {
    return ReportDetailState(
      report: report ?? this.report,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage,
    );
  }
}

/// StateNotifier managing reports list operations.
class ReportsNotifier extends StateNotifier<ReportsState> {
  ReportsNotifier(this._dio) : super(const ReportsState());

  final Dio _dio;

  /// Fetch all reports from the API.
  Future<void> fetchReports() async {
    state = state.copyWith(isLoading: true, errorMessage: null);
    try {
      final response = await _dio.get('/admin/reports');
      final List<dynamic> data = response.data is List
          ? response.data as List<dynamic>
          : (response.data['data'] as List<dynamic>? ?? []);
      final reports =
          data.map((e) => Report.fromJson(e as Map<String, dynamic>)).toList();
      state = state.copyWith(reports: reports, isLoading: false);
    } on DioException catch (e) {
      state = state.copyWith(
        isLoading: false,
        errorMessage: e.response?.data?['message']?.toString() ??
            'Failed to load reports',
      );
    }
  }
}

/// StateNotifier managing single report detail and status updates.
class ReportDetailNotifier extends StateNotifier<ReportDetailState> {
  ReportDetailNotifier(this._dio) : super(const ReportDetailState());

  final Dio _dio;

  /// Fetch a single report by ID.
  Future<void> fetchReport(int id) async {
    state = state.copyWith(isLoading: true, errorMessage: null);
    try {
      final response = await _dio.get('/admin/reports/$id');
      final data = response.data is Map<String, dynamic>
          ? response.data as Map<String, dynamic>
          : <String, dynamic>{};
      final report = Report.fromJson(data);
      state = state.copyWith(report: report, isLoading: false);
    } on DioException catch (e) {
      state = state.copyWith(
        isLoading: false,
        errorMessage: e.response?.data?['message']?.toString() ??
            'Failed to load report',
      );
    }
  }

  /// Update report status and admin notes via PATCH.
  /// Returns null on success or an error message.
  Future<String?> updateReportStatus({
    required int id,
    required String status,
    required String adminNotes,
  }) async {
    try {
      await _dio.patch('/admin/reports/$id', data: {
        'status': status,
        'adminNotes': adminNotes,
      });
      // Refresh the report after update
      await fetchReport(id);
      return null;
    } on DioException catch (e) {
      if (e.response?.statusCode == 422 || e.response?.statusCode == 400) {
        final data = e.response?.data;
        if (data is Map<String, dynamic>) {
          return data['message']?.toString() ?? 'Validation error';
        }
      }
      return 'Failed to update report status';
    }
  }
}

/// Provider for the ReportsNotifier (list).
final reportsProvider =
    StateNotifierProvider<ReportsNotifier, ReportsState>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return ReportsNotifier(dioClient.dio);
});

/// Provider for the ReportDetailNotifier (single report).
final reportDetailProvider =
    StateNotifierProvider<ReportDetailNotifier, ReportDetailState>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return ReportDetailNotifier(dioClient.dio);
});
