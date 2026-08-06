import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

// ─── Models ──────────────────────────────────────────────────────────────────

class TripInstance {
  const TripInstance({
    required this.id,
    required this.tripDate,
    required this.departureTime,
    required this.direction,
    required this.stopName,
    required this.campusName,
    required this.status,
    this.passengerCount = 0,
    this.convoyAllocations = const [],
    this.breakdownLogs = const [],
  });

  final int id;
  final String tripDate;
  final String departureTime;
  final String direction;
  final String stopName;
  final String campusName;
  final String status;
  final int passengerCount;
  final List<ConvoyAllocation> convoyAllocations;
  final List<BreakdownLog> breakdownLogs;

  factory TripInstance.fromJson(Map<String, dynamic> json) {
    return TripInstance(
      id: json['tripInstanceId'] as int? ?? json['id'] as int,
      tripDate: json['tripDate'] as String,
      departureTime: json['departureTime'] as String,
      direction: json['direction'] as String,
      stopName: json['stopName'] as String? ?? '',
      campusName: json['campusName'] as String? ?? '',
      status: json['status'] as String,
      passengerCount: json['passengerCount'] as int? ?? 0,
      convoyAllocations: (json['convoyAllocations'] as List?)
              ?.map(
                  (e) => ConvoyAllocation.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
      breakdownLogs: (json['breakdownLogs'] as List?)
              ?.map((e) => BreakdownLog.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

class ConvoyAllocation {
  const ConvoyAllocation({
    required this.id,
    required this.shuttleId,
    required this.licensePlate,
    required this.capacity,
    required this.orderPosition,
    required this.isOperational,
    this.isAccessibleVehicle = false,
  });

  final int id;
  final int shuttleId;
  final String licensePlate;
  final int capacity;
  final int orderPosition;
  final bool isOperational;
  final bool isAccessibleVehicle;

  factory ConvoyAllocation.fromJson(Map<String, dynamic> json) {
    return ConvoyAllocation(
      id: json['id'] as int,
      shuttleId: json['shuttleId'] as int,
      licensePlate: json['licensePlate'] as String? ?? '',
      capacity: json['capacity'] as int? ?? 0,
      orderPosition: json['orderPosition'] as int? ?? 0,
      isOperational: json['isOperational'] as bool? ?? true,
      isAccessibleVehicle: json['isAccessibleVehicle'] as bool? ?? false,
    );
  }
}

class BreakdownLog {
  const BreakdownLog({
    required this.id,
    required this.shuttleId,
    required this.licensePlate,
    this.resolutionType,
    this.replacementShuttleId,
    this.resolvedAt,
  });

  final int id;
  final int shuttleId;
  final String licensePlate;
  final String? resolutionType;
  final int? replacementShuttleId;
  final String? resolvedAt;

  bool get isResolved => resolvedAt != null;

  factory BreakdownLog.fromJson(Map<String, dynamic> json) {
    return BreakdownLog(
      id: json['id'] as int,
      shuttleId: json['shuttleId'] as int,
      licensePlate: json['licensePlate'] as String? ?? '',
      resolutionType: json['resolutionType'] as String?,
      replacementShuttleId: json['replacementShuttleId'] as int?,
      resolvedAt: json['resolvedAt'] as String?,
    );
  }
}

class ShuttleOption {
  const ShuttleOption({
    required this.id,
    required this.licensePlate,
    required this.capacity,
    required this.isAccessibleVehicle,
  });

  final int id;
  final String licensePlate;
  final int capacity;
  final bool isAccessibleVehicle;

  factory ShuttleOption.fromJson(Map<String, dynamic> json) {
    return ShuttleOption(
      id: json['shuttleId'] as int? ?? json['id'] as int? ?? 0,
      licensePlate: json['licensePlate'] as String? ?? '',
      capacity: json['capacity'] as int? ?? 0,
      isAccessibleVehicle: json['isAccessibleVehicle'] as bool? ?? false,
    );
  }
}

// ─── Query Parameters ────────────────────────────────────────────────────────

class TripQueryParams {
  const TripQueryParams({this.date, this.status});

  final String? date;
  final String? status;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is TripQueryParams &&
          runtimeType == other.runtimeType &&
          date == other.date &&
          status == other.status;

  @override
  int get hashCode => date.hashCode ^ status.hashCode;
}

// ─── Providers ───────────────────────────────────────────────────────────────

/// Fetches trips filtered by date and/or status.
final tripsProvider =
    FutureProvider.family<List<TripInstance>, TripQueryParams>(
        (ref, params) async {
  final dioClient = ref.watch(dioClientProvider);
  final queryParameters = <String, dynamic>{};
  if (params.date != null && params.date!.isNotEmpty) {
    queryParameters['date'] = params.date;
  }
  if (params.status != null && params.status!.isNotEmpty) {
    queryParameters['status'] = params.status;
  }

  final response = await dioClient.dio.get(
    '/admin/trips',
    queryParameters: queryParameters,
  );

  final list = response.data as List;
  return list
      .map((e) => TripInstance.fromJson(e as Map<String, dynamic>))
      .toList();
});

/// Fetches available shuttles for assignment.
final availableShuttlesProvider =
    FutureProvider<List<ShuttleOption>>((ref) async {
  final dioClient = ref.watch(dioClientProvider);
  final response = await dioClient.dio.get('/admin/shuttles/active');
  final list = response.data as List;
  return list
      .map((e) => ShuttleOption.fromJson(e as Map<String, dynamic>))
      .toList();
});

// ─── Notifier for Trip Actions ───────────────────────────────────────────────

class TripActionsNotifier extends StateNotifier<AsyncValue<void>> {
  TripActionsNotifier(this._ref) : super(const AsyncValue.data(null));

  final Ref _ref;

  /// Assign a shuttle to a trip.
  Future<bool> assignShuttle(int tripId, int shuttleId) async {
    state = const AsyncValue.loading();
    try {
      final dioClient = _ref.read(dioClientProvider);
      await dioClient.dio.post(
        '/admin/trips/$tripId/shuttles',
        data: {'shuttleId': shuttleId},
      );
      state = const AsyncValue.data(null);
      return true;
    } catch (e, st) {
      state = AsyncValue.error(e, st);
      return false;
    }
  }

  /// Update trip status.
  Future<bool> updateStatus(int tripId, String status) async {
    state = const AsyncValue.loading();
    try {
      final dioClient = _ref.read(dioClientProvider);
      await dioClient.dio.patch(
        '/admin/trips/$tripId/status',
        data: {'status': status},
      );
      state = const AsyncValue.data(null);
      return true;
    } catch (e, st) {
      state = AsyncValue.error(e, st);
      return false;
    }
  }

  /// Resolve a breakdown.
  Future<bool> resolveBreakdown({
    required int tripId,
    required int breakdownLogId,
    required String resolutionType,
    int? replacementShuttleId,
  }) async {
    state = const AsyncValue.loading();
    try {
      final dioClient = _ref.read(dioClientProvider);
      final data = <String, dynamic>{
        'resolutionType': resolutionType,
      };
      if (replacementShuttleId != null) {
        data['replacementShuttleId'] = replacementShuttleId;
      }
      await dioClient.dio.post(
        '/trips/$tripId/breakdown/$breakdownLogId/resolve',
        data: data,
      );
      state = const AsyncValue.data(null);
      return true;
    } catch (e, st) {
      state = AsyncValue.error(e, st);
      return false;
    }
  }
}

final tripActionsProvider =
    StateNotifierProvider<TripActionsNotifier, AsyncValue<void>>((ref) {
  return TripActionsNotifier(ref);
});

/// Helper to format today's date.
String formatToday() => DateFormat('yyyy-MM-dd').format(DateTime.now());
