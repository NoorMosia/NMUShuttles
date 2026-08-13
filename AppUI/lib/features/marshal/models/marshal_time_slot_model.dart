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
