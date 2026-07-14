/// Data models for the Operations Overview dashboard.

class StopDemand {
  const StopDemand({
    required this.stopId,
    required this.stopName,
    required this.confirmedBookingCount,
  });

  final int stopId;
  final String stopName;
  final int confirmedBookingCount;

  factory StopDemand.fromJson(Map<String, dynamic> json) {
    return StopDemand(
      stopId: json['stopId'] as int,
      stopName: json['stopName'] as String,
      confirmedBookingCount: json['confirmedBookingCount'] as int,
    );
  }
}

class ReportStatusSummary {
  const ReportStatusSummary({
    required this.submitted,
    required this.underReview,
    required this.resolved,
  });

  final int submitted;
  final int underReview;
  final int resolved;

  factory ReportStatusSummary.fromJson(Map<String, dynamic> json) {
    return ReportStatusSummary(
      submitted: json['submitted'] as int,
      underReview: json['underReview'] as int,
      resolved: json['resolved'] as int,
    );
  }
}

class BreakdownCancellationTally {
  const BreakdownCancellationTally({
    required this.totalBreakdowns,
    required this.totalCancellations,
  });

  final int totalBreakdowns;
  final int totalCancellations;

  factory BreakdownCancellationTally.fromJson(Map<String, dynamic> json) {
    return BreakdownCancellationTally(
      totalBreakdowns: json['totalBreakdowns'] as int,
      totalCancellations: json['totalCancellations'] as int,
    );
  }
}

class OverviewData {
  const OverviewData({
    required this.mostInDemandStops,
    required this.leastInDemandStops,
    required this.reportStatusSummary,
    required this.breakdownCancellationTally,
  });

  final List<StopDemand> mostInDemandStops;
  final List<StopDemand> leastInDemandStops;
  final ReportStatusSummary reportStatusSummary;
  final BreakdownCancellationTally breakdownCancellationTally;

  factory OverviewData.fromJson(Map<String, dynamic> json) {
    return OverviewData(
      mostInDemandStops: (json['mostInDemandStops'] as List)
          .map((e) => StopDemand.fromJson(e as Map<String, dynamic>))
          .toList(),
      leastInDemandStops: (json['leastInDemandStops'] as List)
          .map((e) => StopDemand.fromJson(e as Map<String, dynamic>))
          .toList(),
      reportStatusSummary: ReportStatusSummary.fromJson(
          json['reportStatusSummary'] as Map<String, dynamic>),
      breakdownCancellationTally: BreakdownCancellationTally.fromJson(
          json['breakdownCancellationTally'] as Map<String, dynamic>),
    );
  }
}

/// Models for the vendor reliability endpoint.

class ShuttleReliability {
  const ShuttleReliability({
    required this.shuttleId,
    required this.licensePlate,
    required this.breakdownCount,
    required this.cancellationCount,
  });

  final int shuttleId;
  final String licensePlate;
  final int breakdownCount;
  final int cancellationCount;

  factory ShuttleReliability.fromJson(Map<String, dynamic> json) {
    return ShuttleReliability(
      shuttleId: json['shuttleId'] as int,
      licensePlate: json['licensePlate'] as String,
      breakdownCount: json['breakdownCount'] as int,
      cancellationCount: json['cancellationCount'] as int,
    );
  }
}

class VendorReliability {
  const VendorReliability({
    required this.vendorCompanyName,
    required this.totalBreakdowns,
    required this.totalCancellations,
  });

  final String vendorCompanyName;
  final int totalBreakdowns;
  final int totalCancellations;

  factory VendorReliability.fromJson(Map<String, dynamic> json) {
    return VendorReliability(
      vendorCompanyName: json['vendorCompanyName'] as String,
      totalBreakdowns: json['totalBreakdowns'] as int,
      totalCancellations: json['totalCancellations'] as int,
    );
  }
}

class VendorReliabilityData {
  const VendorReliabilityData({
    required this.perShuttle,
    required this.perVendor,
  });

  final List<ShuttleReliability> perShuttle;
  final List<VendorReliability> perVendor;

  factory VendorReliabilityData.fromJson(Map<String, dynamic> json) {
    return VendorReliabilityData(
      perShuttle: (json['perShuttle'] as List)
          .map((e) => ShuttleReliability.fromJson(e as Map<String, dynamic>))
          .toList(),
      perVendor: (json['perVendor'] as List)
          .map((e) => VendorReliability.fromJson(e as Map<String, dynamic>))
          .toList(),
    );
  }
}
