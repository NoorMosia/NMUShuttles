/// Model representing an available trip instance from the API.
class TripModel {
  const TripModel({
    required this.tripInstanceId,
    required this.departureTime,
    required this.direction,
    required this.stopName,
    required this.campusName,
    required this.tripDate,
    required this.status,
    required this.totalCapacity,
    required this.confirmedCount,
    required this.availableSeats,
  });

  final int tripInstanceId;
  final String departureTime;
  final String direction;
  final String stopName;
  final String campusName;
  final DateTime tripDate;
  final String status;
  final int totalCapacity;
  final int confirmedCount;
  final int availableSeats;

  factory TripModel.fromJson(Map<String, dynamic> json) {
    return TripModel(
      tripInstanceId: json['tripInstanceId'] as int,
      departureTime: json['departureTime'] as String,
      direction: json['direction'] as String,
      stopName: json['stopName'] as String? ?? '',
      campusName: json['campusName'] as String? ?? '',
      tripDate: DateTime.parse(json['tripDate'] as String),
      status: json['status'] as String? ?? 'Scheduling',
      totalCapacity: json['totalCapacity'] as int? ?? 0,
      confirmedCount: json['confirmedCount'] as int? ?? 0,
      availableSeats: json['availableSeats'] as int? ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'tripInstanceId': tripInstanceId,
      'departureTime': departureTime,
      'direction': direction,
      'stopName': stopName,
      'campusName': campusName,
      'tripDate': tripDate.toIso8601String(),
      'status': status,
      'totalCapacity': totalCapacity,
      'confirmedCount': confirmedCount,
      'availableSeats': availableSeats,
    };
  }
}

/// Model representing a stop for the stop search.
class StopModel {
  const StopModel({
    required this.stopId,
    required this.stopName,
    required this.fullAddress,
  });

  final int stopId;
  final String stopName;
  final String fullAddress;

  factory StopModel.fromJson(Map<String, dynamic> json) {
    return StopModel(
      stopId: json['stopId'] as int,
      stopName: json['stopName'] as String,
      fullAddress: json['fullAddress'] as String? ?? '',
    );
  }
}
