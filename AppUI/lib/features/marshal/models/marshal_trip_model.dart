/// Data class representing a trip instance for the Marshal Home display.
class MarshalTripModel {
  const MarshalTripModel({
    required this.tripInstanceId,
    required this.departureTime,
    required this.direction,
    required this.stopName,
    required this.campusName,
    required this.status,
  });

  final int tripInstanceId;
  final String departureTime;
  final String direction;
  final String stopName;
  final String campusName;
  final String status;

  factory MarshalTripModel.fromJson(Map<String, dynamic> json) {
    return MarshalTripModel(
      tripInstanceId: json['tripInstanceId'] as int,
      departureTime: json['departureTime'] as String,
      direction: json['direction'] as String,
      stopName: json['stopName'] as String? ?? '',
      campusName: json['campusName'] as String? ?? '',
      status: json['status'] as String? ?? 'Scheduling',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'tripInstanceId': tripInstanceId,
      'departureTime': departureTime,
      'direction': direction,
      'stopName': stopName,
      'campusName': campusName,
      'status': status,
    };
  }

  /// Whether this trip has not yet reached Completed status.
  bool get isActive => status != 'Completed';
}
