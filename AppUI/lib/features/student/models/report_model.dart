/// Model representing a student report from the API.
class ReportModel {
  const ReportModel({
    required this.reportId,
    required this.category,
    required this.description,
    required this.status,
    this.adminNotes,
    this.tripInstanceId,
    required this.createdAt,
    this.updatedAt,
  });

  final int reportId;
  final String category;
  final String description;
  final String status;
  final String? adminNotes;
  final int? tripInstanceId;
  final DateTime createdAt;
  final DateTime? updatedAt;

  factory ReportModel.fromJson(Map<String, dynamic> json) {
    return ReportModel(
      reportId: json['reportId'] as int,
      category: json['category'] as String,
      description: json['description'] as String,
      status: json['status'] as String,
      adminNotes: json['adminNotes'] as String?,
      tripInstanceId: json['tripInstanceId'] as int?,
      createdAt: DateTime.parse(json['createdAt'] as String),
      updatedAt: json['updatedAt'] != null
          ? DateTime.parse(json['updatedAt'] as String)
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'reportId': reportId,
      'category': category,
      'description': description,
      'status': status,
      'adminNotes': adminNotes,
      'tripInstanceId': tripInstanceId,
      'createdAt': createdAt.toIso8601String(),
      'updatedAt': updatedAt?.toIso8601String(),
    };
  }
}
