import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

/// Model for a trip in the stop-centric view.
class StopTripEntry {
  const StopTripEntry({
    required this.tripInstanceId,
    required this.tripDate,
    required this.departureTime,
    required this.direction,
    required this.status,
    required this.shuttleCount,
    required this.totalCapacity,
    required this.confirmedCount,
    required this.hasEmptyConvoy,
  });

  final int tripInstanceId;
  final String tripDate;
  final String departureTime;
  final String direction;
  final String status;
  final int shuttleCount;
  final int totalCapacity;
  final int confirmedCount;
  final bool hasEmptyConvoy;

  factory StopTripEntry.fromJson(Map<String, dynamic> json) {
    return StopTripEntry(
      tripInstanceId: json['tripInstanceId'] as int? ?? 0,
      tripDate: json['tripDate'] as String? ?? '',
      departureTime: json['departureTime'] as String? ?? '',
      direction: json['direction'] as String? ?? '',
      status: json['status'] as String? ?? '',
      shuttleCount: json['shuttleCount'] as int? ?? 0,
      totalCapacity: json['totalCapacity'] as int? ?? 0,
      confirmedCount: json['confirmedCount'] as int? ?? 0,
      hasEmptyConvoy: json['hasEmptyConvoy'] as bool? ?? false,
    );
  }
}

/// Response model for the stop trips endpoint.
class StopTripsResponse {
  const StopTripsResponse({
    required this.stopId,
    required this.stopName,
    required this.trips,
  });

  final int stopId;
  final String stopName;
  final List<StopTripEntry> trips;

  factory StopTripsResponse.fromJson(Map<String, dynamic> json) {
    return StopTripsResponse(
      stopId: json['stopId'] as int? ?? 0,
      stopName: json['stopName'] as String? ?? '',
      trips: (json['trips'] as List?)
              ?.map((e) => StopTripEntry.fromJson(e as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

/// Fetches trips for a specific stop.
final stopTripsProvider =
    FutureProvider.autoDispose.family<StopTripsResponse, int>(
  (ref, stopId) async {
    final dioClient = ref.watch(dioClientProvider);
    final response = await dioClient.dio.get('/admin/stops/$stopId/trips');
    return StopTripsResponse.fromJson(response.data as Map<String, dynamic>);
  },
);
