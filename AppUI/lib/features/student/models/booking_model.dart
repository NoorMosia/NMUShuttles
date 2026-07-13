/// Model representing a student booking from the API.
class BookingModel {
  const BookingModel({
    required this.bookingId,
    required this.bookingStatus,
    required this.bookingType,
    this.qrCodeToken,
    required this.tripDate,
    required this.departureTime,
    required this.direction,
    required this.stopName,
    required this.campusName,
    required this.boardedStatus,
    required this.createdAt,
  });

  final int bookingId;
  final String bookingStatus;
  final String bookingType;
  final String? qrCodeToken;
  final DateTime tripDate;
  final String departureTime;
  final String direction;
  final String stopName;
  final String campusName;
  final String boardedStatus;
  final DateTime createdAt;

  factory BookingModel.fromJson(Map<String, dynamic> json) {
    return BookingModel(
      bookingId: json['bookingId'] as int,
      bookingStatus: json['bookingStatus'] as String,
      bookingType: json['bookingType'] as String? ?? 'Standard',
      qrCodeToken: json['qrCodeToken'] as String?,
      tripDate: DateTime.parse(json['tripDate'] as String),
      departureTime: json['departureTime'] as String,
      direction: json['direction'] as String,
      stopName: json['stopName'] as String,
      campusName: json['campusName'] as String,
      boardedStatus: json['boardedStatus'] as String? ?? 'NotBoarded',
      createdAt: DateTime.parse(json['createdAt'] as String),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'bookingId': bookingId,
      'bookingStatus': bookingStatus,
      'bookingType': bookingType,
      'qrCodeToken': qrCodeToken,
      'tripDate': tripDate.toIso8601String(),
      'departureTime': departureTime,
      'direction': direction,
      'stopName': stopName,
      'campusName': campusName,
      'boardedStatus': boardedStatus,
      'createdAt': createdAt.toIso8601String(),
    };
  }
}
