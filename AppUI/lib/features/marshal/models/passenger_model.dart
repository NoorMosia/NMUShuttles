/// Model representing a passenger entry from the manifest API.
class PassengerModel {
  const PassengerModel({
    required this.bookingId,
    required this.studentName,
    required this.studentNumber,
    required this.boardedStatus,
    required this.bookingType,
  });

  final int bookingId;
  final String studentName;
  final String studentNumber;
  final String boardedStatus;
  final String bookingType;

  /// Whether this passenger has boarded.
  bool get isBoarded => boardedStatus == 'Boarded';

  factory PassengerModel.fromJson(Map<String, dynamic> json) {
    return PassengerModel(
      bookingId: json['bookingId'] as int,
      studentName: json['studentName'] as String,
      studentNumber: json['studentNumber'] as String,
      boardedStatus: json['boardedStatus'] as String? ?? 'Pending',
      bookingType: json['bookingType'] as String? ?? 'Standard',
    );
  }

  /// Returns a copy with the boardedStatus updated.
  PassengerModel copyWith({String? boardedStatus}) {
    return PassengerModel(
      bookingId: bookingId,
      studentName: studentName,
      studentNumber: studentNumber,
      boardedStatus: boardedStatus ?? this.boardedStatus,
      bookingType: bookingType,
    );
  }
}

/// Model representing the full passenger manifest response.
class ManifestModel {
  const ManifestModel({
    required this.tripInstanceId,
    required this.totalConfirmed,
    required this.boardedCount,
    required this.pendingCount,
    required this.passengers,
  });

  final int tripInstanceId;
  final int totalConfirmed;
  final int boardedCount;
  final int pendingCount;
  final List<PassengerModel> passengers;

  factory ManifestModel.fromJson(Map<String, dynamic> json) {
    final passengerList = (json['passengers'] as List<dynamic>?)
            ?.map((p) => PassengerModel.fromJson(p as Map<String, dynamic>))
            .toList() ??
        [];
    return ManifestModel(
      tripInstanceId: json['tripInstanceId'] as int,
      totalConfirmed: json['totalConfirmed'] as int? ?? 0,
      boardedCount: json['boardedCount'] as int? ?? 0,
      pendingCount: json['pendingCount'] as int? ?? 0,
      passengers: passengerList,
    );
  }

  /// Returns a copy with a specific passenger's status updated.
  ManifestModel withPassengerBoarded(int bookingId) {
    final updatedPassengers = passengers.map((p) {
      if (p.bookingId == bookingId) {
        return p.copyWith(boardedStatus: 'Boarded');
      }
      return p;
    }).toList();

    return ManifestModel(
      tripInstanceId: tripInstanceId,
      totalConfirmed: totalConfirmed,
      boardedCount: boardedCount + 1,
      pendingCount: pendingCount - 1,
      passengers: updatedPassengers,
    );
  }
}

/// Model representing the check-in result from POST /api/v1/checkin.
class CheckInResultModel {
  const CheckInResultModel({
    required this.bookingId,
    required this.studentName,
    required this.boardedStatus,
    this.boardedAt,
    required this.assignedShuttleId,
    required this.assignedShuttleLicensePlate,
    required this.orderPosition,
  });

  final int bookingId;
  final String studentName;
  final String boardedStatus;
  final DateTime? boardedAt;
  final int assignedShuttleId;
  final String assignedShuttleLicensePlate;
  final int orderPosition;

  factory CheckInResultModel.fromJson(Map<String, dynamic> json) {
    return CheckInResultModel(
      bookingId: json['bookingId'] as int,
      studentName: json['studentName'] as String,
      boardedStatus: json['boardedStatus'] as String,
      boardedAt: json['boardedAt'] != null
          ? DateTime.parse(json['boardedAt'] as String)
          : null,
      assignedShuttleId: json['assignedShuttleId'] as int,
      assignedShuttleLicensePlate:
          json['assignedShuttleLicensePlate'] as String,
      orderPosition: json['orderPosition'] as int,
    );
  }
}

/// Model representing the breakdown flag response.
class BreakdownResultModel {
  const BreakdownResultModel({
    required this.breakdownLogId,
    required this.newTotalCapacity,
    required this.status,
  });

  final int breakdownLogId;
  final int newTotalCapacity;
  final String status;

  factory BreakdownResultModel.fromJson(Map<String, dynamic> json) {
    return BreakdownResultModel(
      breakdownLogId: json['breakdownLogId'] as int,
      newTotalCapacity: json['newTotalCapacity'] as int,
      status: json['status'] as String,
    );
  }
}

/// Simple model for shuttle allocations shown in the breakdown dialog.
class ShuttleAllocationModel {
  const ShuttleAllocationModel({
    required this.shuttleId,
    required this.licensePlate,
    required this.orderPosition,
    required this.isOperational,
  });

  final int shuttleId;
  final String licensePlate;
  final int orderPosition;
  final bool isOperational;

  factory ShuttleAllocationModel.fromJson(Map<String, dynamic> json) {
    return ShuttleAllocationModel(
      shuttleId: json['shuttleId'] as int,
      licensePlate: json['licensePlate'] as String,
      orderPosition: json['orderPosition'] as int,
      isOperational: json['isOperational'] as bool? ?? true,
    );
  }
}
