import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';

// ─── Convoy Status Model ───────────────────────────────────────

/// Model for a shuttle in the convoy status response.
class ConvoyShuttle {
  const ConvoyShuttle({
    required this.allocationId,
    required this.orderPosition,
    required this.shuttleId,
    required this.licensePlate,
    required this.isAccessibleVehicle,
    required this.capacity,
    required this.boardedCount,
    required this.isOperational,
  });

  final int allocationId;
  final int orderPosition;
  final int shuttleId;
  final String licensePlate;
  final bool isAccessibleVehicle;
  final int capacity;
  final int boardedCount;
  final bool isOperational;

  factory ConvoyShuttle.fromJson(Map<String, dynamic> json) {
    return ConvoyShuttle(
      allocationId: json['allocationId'] as int? ?? 0,
      orderPosition: json['orderPosition'] as int? ?? 0,
      shuttleId: json['shuttleId'] as int? ?? 0,
      licensePlate: json['licensePlate'] as String? ?? '',
      isAccessibleVehicle: json['isAccessibleVehicle'] as bool? ?? false,
      capacity: json['capacity'] as int? ?? 0,
      boardedCount: json['boardedCount'] as int? ?? 0,
      isOperational: json['isOperational'] as bool? ?? true,
    );
  }
}

/// Full convoy status response model.
class ConvoyStatus {
  const ConvoyStatus({
    required this.tripInstanceId,
    required this.requiresScanning,
    required this.shuttles,
  });

  final int tripInstanceId;
  final bool requiresScanning;
  final List<ConvoyShuttle> shuttles;

  factory ConvoyStatus.fromJson(Map<String, dynamic> json) {
    return ConvoyStatus(
      tripInstanceId: json['tripInstanceId'] as int? ?? 0,
      requiresScanning: json['requiresScanning'] as bool? ?? true,
      shuttles: (json['shuttles'] as List?)
              ?.map((e) => ConvoyShuttle.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

// ─── Convoy Provider (30s polling) ─────────────────────────────

/// StreamProvider that fetches convoy status every 30 seconds.
/// Automatically disposes when the screen is no longer visible.
final convoyStatusProvider =
    StreamProvider.autoDispose.family<ConvoyStatus, int>((ref, tripId) {
  final dioClient = ref.watch(dioClientProvider);

  final controller = StreamController<ConvoyStatus>();

  Future<void> fetch() async {
    try {
      final response = await dioClient.dio.get(
        '${ApiConstants.trips}/$tripId/convoy',
      );
      final status =
          ConvoyStatus.fromJson(response.data as Map<String, dynamic>);
      if (!controller.isClosed) {
        controller.add(status);
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
