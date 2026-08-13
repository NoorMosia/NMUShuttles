import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';

// ─── Time Slot Passenger Model ─────────────────────────────────

class TimeSlotPassenger {
  const TimeSlotPassenger({
    required this.bookingId,
    required this.studentName,
    required this.studentNumber,
    required this.stopName,
    required this.boardedStatus,
    this.shuttleId,
    this.boardedAt,
  });

  final int bookingId;
  final String studentName;
  final String studentNumber;
  final String stopName;
  final String boardedStatus;
  final int? shuttleId;
  final DateTime? boardedAt;

  bool get isBoarded => boardedStatus == 'Boarded';

  factory TimeSlotPassenger.fromJson(Map<String, dynamic> json) {
    return TimeSlotPassenger(
      bookingId: json['bookingId'] as int,
      studentName: json['studentName'] as String? ?? '',
      studentNumber: json['studentNumber'] as String? ?? '',
      stopName: json['stopName'] as String? ?? '',
      boardedStatus: json['boardedStatus'] as String? ?? 'Pending',
      shuttleId: json['shuttleId'] as int?,
      boardedAt: json['boardedAt'] != null
          ? DateTime.tryParse(json['boardedAt'] as String)
          : null,
    );
  }
}

class TimeSlotPassengerList {
  const TimeSlotPassengerList({
    required this.departureTime,
    required this.direction,
    required this.tripDate,
    required this.totalConfirmed,
    required this.boardedCount,
    required this.pendingCount,
    required this.passengers,
  });

  final String departureTime;
  final String direction;
  final String tripDate;
  final int totalConfirmed;
  final int boardedCount;
  final int pendingCount;
  final List<TimeSlotPassenger> passengers;

  factory TimeSlotPassengerList.fromJson(Map<String, dynamic> json) {
    return TimeSlotPassengerList(
      departureTime: json['departureTime'] as String? ?? '',
      direction: json['direction'] as String? ?? '',
      tripDate: json['tripDate'] as String? ?? '',
      totalConfirmed: json['totalConfirmed'] as int? ?? 0,
      boardedCount: json['boardedCount'] as int? ?? 0,
      pendingCount: json['pendingCount'] as int? ?? 0,
      passengers: (json['passengers'] as List<dynamic>?)
              ?.map((p) =>
                  TimeSlotPassenger.fromJson(p as Map<String, dynamic>))
              .toList() ??
          [],
    );
  }
}

// ─── Request key for the provider ──────────────────────────────

class TimeSlotQuery {
  const TimeSlotQuery({
    required this.departureTime,
    required this.direction,
    required this.date,
    this.search,
  });

  final String departureTime;
  final String direction;
  final String date;
  final String? search;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is TimeSlotQuery &&
          departureTime == other.departureTime &&
          direction == other.direction &&
          date == other.date &&
          search == other.search;

  @override
  int get hashCode => Object.hash(departureTime, direction, date, search);
}

// ─── Provider (30s polling) ────────────────────────────────────

/// StreamProvider that fetches the time-slot passenger list every 30 seconds.
final timeSlotPassengersProvider = StreamProvider.autoDispose
    .family<TimeSlotPassengerList, TimeSlotQuery>((ref, query) {
  final dioClient = ref.watch(dioClientProvider);
  final controller = StreamController<TimeSlotPassengerList>();

  Future<void> fetch() async {
    try {
      final queryParams = <String, dynamic>{
        'departureTime': query.departureTime,
        'direction': query.direction,
        'date': query.date,
      };
      if (query.search != null && query.search!.isNotEmpty) {
        queryParams['search'] = query.search;
      }

      final response = await dioClient.dio.get(
        ApiConstants.marshalTimeSlotPassengers,
        queryParameters: queryParams,
      );
      final result = TimeSlotPassengerList.fromJson(
          response.data as Map<String, dynamic>);
      if (!controller.isClosed) {
        controller.add(result);
      }
    } on DioException catch (e) {
      if (!controller.isClosed) {
        controller.addError(e);
      }
    }
  }

  fetch();
  final timer = Timer.periodic(const Duration(seconds: 30), (_) => fetch());

  ref.onDispose(() {
    timer.cancel();
    controller.close();
  });

  return controller.stream;
});

// ─── Manual Check-In Provider ──────────────────────────────────

/// Submits a manual check-in by booking ID: POST /api/v1/checkin/manual
final manualCheckInProvider =
    FutureProvider.autoDispose.family<Map<String, dynamic>, int>(
  (ref, bookingId) async {
    final dioClient = ref.watch(dioClientProvider);
    final response = await dioClient.dio.post(
      ApiConstants.manualCheckIn,
      data: {'bookingId': bookingId},
    );
    return response.data as Map<String, dynamic>;
  },
);
