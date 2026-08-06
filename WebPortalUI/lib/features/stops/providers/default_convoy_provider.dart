import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

/// Model for a default convoy entry.
class DefaultConvoyEntry {
  const DefaultConvoyEntry({
    required this.stopShuttleDefaultId,
    required this.stopId,
    required this.shuttleId,
    required this.licensePlate,
    required this.isAccessibleVehicle,
    required this.capacity,
    required this.orderPosition,
  });

  final int stopShuttleDefaultId;
  final int stopId;
  final int shuttleId;
  final String licensePlate;
  final bool isAccessibleVehicle;
  final int capacity;
  final int orderPosition;

  factory DefaultConvoyEntry.fromJson(Map<String, dynamic> json) {
    return DefaultConvoyEntry(
      stopShuttleDefaultId: json['stopShuttleDefaultId'] as int? ?? 0,
      stopId: json['stopId'] as int? ?? 0,
      shuttleId: json['shuttleId'] as int? ?? 0,
      licensePlate: json['licensePlate'] as String? ?? '',
      isAccessibleVehicle: json['isAccessibleVehicle'] as bool? ?? false,
      capacity: json['capacity'] as int? ?? 0,
      orderPosition: json['orderPosition'] as int? ?? 0,
    );
  }
}

/// Fetches the default convoy for a stop.
final defaultConvoyProvider =
    FutureProvider.autoDispose.family<List<DefaultConvoyEntry>, int>(
  (ref, stopId) async {
    final dioClient = ref.watch(dioClientProvider);
    final response =
        await dioClient.dio.get('/admin/stops/$stopId/default-convoy');
    final list = response.data as List;
    return list
        .map((e) => DefaultConvoyEntry.fromJson(e as Map<String, dynamic>))
        .toList();
  },
);

/// Actions for managing the default convoy.
class DefaultConvoyActions {
  DefaultConvoyActions(this._dio);
  final Dio _dio;

  Future<void> addShuttle(int stopId, int shuttleId) async {
    await _dio.post('/admin/stops/$stopId/default-convoy', data: {
      'shuttleId': shuttleId,
    });
  }

  Future<void> removeShuttle(int stopId, int shuttleId) async {
    await _dio.delete('/admin/stops/$stopId/default-convoy/$shuttleId');
  }

  Future<void> reorder(int stopId, List<int> orderedShuttleIds) async {
    await _dio.patch('/admin/stops/$stopId/default-convoy/reorder', data: {
      'orderedShuttleIds': orderedShuttleIds,
    });
  }
}

final defaultConvoyActionsProvider = Provider<DefaultConvoyActions>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return DefaultConvoyActions(dioClient.dio);
});


/// Fetches active shuttles NOT assigned to any stop's default convoy.
final availableConvoyShuttlesProvider =
    FutureProvider.autoDispose<List<DefaultConvoyEntry>>((ref) async {
  final dioClient = ref.watch(dioClientProvider);
  final response = await dioClient.dio.get('/admin/shuttles/available');
  final list = response.data as List;
  return list
      .map((e) => DefaultConvoyEntry.fromJson(e as Map<String, dynamic>))
      .toList();
});
