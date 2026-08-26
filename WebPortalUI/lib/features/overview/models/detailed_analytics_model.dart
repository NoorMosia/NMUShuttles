/// Data models for the Detailed Analytics page.

class StopBookingDemand {
  const StopBookingDemand({
    required this.stopId,
    required this.stopName,
    required this.totalBookings,
    required this.confirmedBookings,
    required this.waitlistedBookings,
  });

  final int stopId;
  final String stopName;
  final int totalBookings;
  final int confirmedBookings;
  final int waitlistedBookings;

  factory StopBookingDemand.fromJson(Map<String, dynamic> json) {
    return StopBookingDemand(
      stopId: json['stopId'] as int,
      stopName: json['stopName'] as String,
      totalBookings: json['totalBookings'] as int,
      confirmedBookings: json['confirmedBookings'] as int,
      waitlistedBookings: json['waitlistedBookings'] as int,
    );
  }
}

class OverallBookingStats {
  const OverallBookingStats({
    required this.total,
    required this.confirmed,
    required this.waitlisted,
    required this.cancelled,
  });

  final int total;
  final int confirmed;
  final int waitlisted;
  final int cancelled;

  factory OverallBookingStats.fromJson(Map<String, dynamic> json) {
    return OverallBookingStats(
      total: json['total'] as int,
      confirmed: json['confirmed'] as int,
      waitlisted: json['waitlisted'] as int,
      cancelled: json['cancelled'] as int,
    );
  }
}

class DetailedAnalyticsData {
  const DetailedAnalyticsData({
    required this.perStopDemand,
    required this.overallStats,
  });

  final List<StopBookingDemand> perStopDemand;
  final OverallBookingStats overallStats;

  factory DetailedAnalyticsData.fromJson(Map<String, dynamic> json) {
    return DetailedAnalyticsData(
      perStopDemand: (json['perStopDemand'] as List)
          .map((e) => StopBookingDemand.fromJson(e as Map<String, dynamic>))
          .toList(),
      overallStats: OverallBookingStats.fromJson(
          json['overallStats'] as Map<String, dynamic>),
    );
  }
}
