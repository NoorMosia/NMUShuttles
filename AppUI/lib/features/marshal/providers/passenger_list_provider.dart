import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/marshal/models/passenger_model.dart';

// ─── Passenger Manifest Provider (30s polling) ─────────────────

/// StreamProvider that fetches the passenger manifest every 30 seconds.
/// Automatically disposes when the screen is no longer visible.
final passengerManifestProvider =
    StreamProvider.autoDispose.family<ManifestModel, int>((ref, tripId) {
  final dioClient = ref.watch(dioClientProvider);

  // Immediately fetch once, then repeat every 30 seconds.
  final controller = StreamController<ManifestModel>();

  Future<void> fetch() async {
    try {
      final response = await dioClient.dio.get(
        '${ApiConstants.trips}/$tripId/passengers',
      );
      final manifest =
          ManifestModel.fromJson(response.data as Map<String, dynamic>);
      if (!controller.isClosed) {
        controller.add(manifest);
      }
    } on DioException catch (e) {
      if (!controller.isClosed) {
        controller.addError(e);
      }
    }
  }

  // Initial fetch
  fetch();

  // Poll every 30 seconds
  final timer = Timer.periodic(const Duration(seconds: 30), (_) => fetch());

  ref.onDispose(() {
    timer.cancel();
    controller.close();
  });

  return controller.stream;
});

// ─── Check-In Provider ─────────────────────────────────────────

/// Submits a QR code check-in: POST /api/v1/checkin
/// Returns a CheckInResultModel on success, throws DioException on failure.
final checkInProvider =
    FutureProvider.autoDispose.family<CheckInResultModel, String>(
  (ref, qrCodeToken) async {
    final dioClient = ref.watch(dioClientProvider);
    final response = await dioClient.dio.post(
      ApiConstants.checkIn,
      data: {'qrCodeToken': qrCodeToken},
    );
    return CheckInResultModel.fromJson(response.data as Map<String, dynamic>);
  },
);

// ─── Breakdown Flag Provider ───────────────────────────────────

/// Request object for flagging a breakdown.
class BreakdownRequest {
  const BreakdownRequest({required this.tripId, required this.shuttleId});
  final int tripId;
  final int shuttleId;
}

/// Flags a shuttle breakdown: POST /api/v1/trips/{tripId}/breakdown
final flagBreakdownProvider =
    FutureProvider.autoDispose.family<BreakdownResultModel, BreakdownRequest>(
  (ref, request) async {
    final dioClient = ref.watch(dioClientProvider);
    final response = await dioClient.dio.post(
      '${ApiConstants.trips}/${request.tripId}/breakdown',
      data: {'shuttleId': request.shuttleId},
    );
    return BreakdownResultModel.fromJson(
        response.data as Map<String, dynamic>);
  },
);

// ─── Trip Shuttles Provider ────────────────────────────────────

/// Fetches the shuttle allocations for a trip (used in breakdown dialog).
/// GET /api/v1/trips/{tripId}/shuttles
final tripShuttlesProvider = FutureProvider.autoDispose
    .family<List<ShuttleAllocationModel>, int>((ref, tripId) async {
  final dioClient = ref.watch(dioClientProvider);
  final response = await dioClient.dio.get(
    '${ApiConstants.trips}/$tripId/shuttles',
  );
  final List<dynamic> data = response.data as List<dynamic>;
  return data
      .map((json) =>
          ShuttleAllocationModel.fromJson(json as Map<String, dynamic>))
      .toList();
});
