/// Data class representing a grouped time slot for the Marshal dashboard.
/// Aggregates all trips (across stops) that share the same departure time and direction.
class MarshalTimeSlotModel {
  const MarshalTimeSlotModel({
    required this.departureTime,
    required this.direction,
    required this.tripDate,
    required this.status,
    required this.totalCapacity,
    required this.confirmedCount,
    required this.availableSeats,
    required this.boardedCount,
    required this.stopCount,
    required this.isScanningOpen,
    required this.tripInstanceIds,
  });

  final String departureTime;
  final String direction;
  final String tripDate;
  final String status;
  final int totalCapacity;
  final int confirmedCount;
  final int availableSeats;
  final int boardedCount;
  final int stopCount;
  final bool isScanningOpen;
  final List<int> tripInstanceIds;

  /// Whether this time slot has not yet reached Completed status.
  bool get isActive => status != 'Completed';

  /// The slot's departure as a local DateTime, or null if it can't be parsed.
  /// tripDate is "yyyy-MM-dd" and departureTime is "HH:mm[:ss]" in SAST
  /// (the device is assumed to be on SAST, matching the deployment region).
  DateTime? get departureDateTime {
    try {
      final datePart = DateTime.parse(tripDate); // yyyy-MM-dd
      final t = departureTime.length >= 5 ? departureTime : '00:00';
      final hh = int.parse(t.substring(0, 2));
      final mm = int.parse(t.substring(3, 5));
      return DateTime(datePart.year, datePart.month, datePart.day, hh, mm);
    } catch (_) {
      return null;
    }
  }

  /// Whether this slot is still upcoming (departure instant has not passed) and
  /// not already completed. This is what makes a slot eligible to be shown as
  /// the marshal's "next boarding" so a departed-but-still-Locked trip drops off.
  bool isUpcoming([DateTime? now]) {
    if (status == 'Completed') return false;
    final dep = departureDateTime;
    if (dep == null) return true; // if unparseable, don't hide it
    return !dep.isBefore(now ?? DateTime.now());
  }

  /// Display-friendly direction label.
  String get directionLabel {
    switch (direction) {
      case 'FromCampus':
        return 'From Campus';
      case 'ToCampus':
        return 'To Campus';
      default:
        return direction;
    }
  }

  /// Formatted departure time for display (e.g., "12:30").
  String get displayTime {
    // The API returns TimeOnly as "HH:mm:ss" — trim seconds for display.
    if (departureTime.length >= 5) {
      return departureTime.substring(0, 5);
    }
    return departureTime;
  }

  factory MarshalTimeSlotModel.fromJson(Map<String, dynamic> json) {
    return MarshalTimeSlotModel(
      departureTime: json['departureTime'] as String? ?? '',
      direction: json['direction'] as String? ?? '',
      tripDate: json['tripDate'] as String? ?? '',
      status: json['status'] as String? ?? 'Scheduling',
      totalCapacity: json['totalCapacity'] as int? ?? 0,
      confirmedCount: json['confirmedCount'] as int? ?? 0,
      availableSeats: json['availableSeats'] as int? ?? 0,
      boardedCount: json['boardedCount'] as int? ?? 0,
      stopCount: json['stopCount'] as int? ?? 0,
      isScanningOpen: json['isScanningOpen'] as bool? ?? false,
      tripInstanceIds: (json['tripInstanceIds'] as List<dynamic>?)
              ?.map((e) => e as int)
              .toList() ??
          [],
    );
  }
}
