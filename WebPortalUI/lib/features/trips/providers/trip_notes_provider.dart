import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

/// Model for a trip note.
class TripNoteModel {
  const TripNoteModel({
    required this.tripNoteId,
    required this.tripInstanceId,
    required this.noteType,
    required this.body,
    required this.isResolutionAction,
    required this.createdByAdminId,
    required this.createdByAdminName,
    required this.createdAt,
    this.notificationsSent,
  });

  final int tripNoteId;
  final int tripInstanceId;
  final String noteType;
  final String body;
  final bool isResolutionAction;
  final int createdByAdminId;
  final String createdByAdminName;
  final String createdAt;
  final int? notificationsSent;

  factory TripNoteModel.fromJson(Map<String, dynamic> json) {
    return TripNoteModel(
      tripNoteId: json['tripNoteId'] as int? ?? 0,
      tripInstanceId: json['tripInstanceId'] as int? ?? 0,
      noteType: json['noteType'] as String? ?? '',
      body: json['body'] as String? ?? '',
      isResolutionAction: json['isResolutionAction'] as bool? ?? false,
      createdByAdminId: json['createdByAdminId'] as int? ?? 0,
      createdByAdminName: json['createdByAdminName'] as String? ?? '',
      createdAt: json['createdAt'] as String? ?? '',
      notificationsSent: json['notificationsSent'] as int?,
    );
  }
}

/// Fetches trip notes for a given trip.
final tripNotesProvider =
    FutureProvider.autoDispose.family<List<TripNoteModel>, int>(
  (ref, tripId) async {
    final dioClient = ref.watch(dioClientProvider);
    final response = await dioClient.dio.get('/admin/trips/$tripId/notes');
    final list = response.data as List;
    return list
        .map((e) => TripNoteModel.fromJson(e as Map<String, dynamic>))
        .toList();
  },
);

/// Actions for creating trip notes.
class TripNoteActions {
  TripNoteActions(this._dio);
  final Dio _dio;

  /// Creates a trip note. Returns the created note response.
  Future<TripNoteModel> createNote({
    required int tripId,
    required String noteType,
    required String body,
    required bool resolveIncident,
    int? replacementShuttleId,
  }) async {
    final data = <String, dynamic>{
      'noteType': noteType,
      'body': body,
      'resolveIncident': resolveIncident,
    };
    if (replacementShuttleId != null) {
      data['replacementShuttleId'] = replacementShuttleId;
    }
    final response = await _dio.post('/admin/trips/$tripId/notes', data: data);
    return TripNoteModel.fromJson(response.data as Map<String, dynamic>);
  }
}

final tripNoteActionsProvider = Provider<TripNoteActions>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return TripNoteActions(dioClient.dio);
});
