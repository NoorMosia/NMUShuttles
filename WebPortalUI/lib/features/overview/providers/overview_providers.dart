import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';
import 'package:nmu_shuttle_admin/features/overview/models/overview_model.dart';

/// Provider that fetches the overview analytics data.
final overviewProvider = FutureProvider<OverviewData>((ref) async {
  final dioClient = ref.watch(dioClientProvider);
  final response = await dioClient.dio.get('/admin/analytics/overview');
  return OverviewData.fromJson(response.data as Map<String, dynamic>);
});

/// Provider family that fetches vendor reliability data for a given date range.
final vendorReliabilityProvider =
    FutureProvider.family<VendorReliabilityData, DateRangeParams>(
        (ref, params) async {
  final dioClient = ref.watch(dioClientProvider);
  final response = await dioClient.dio.get(
    '/admin/analytics/vendor-reliability',
    queryParameters: {
      'fromDate': params.fromDate,
      'toDate': params.toDate,
    },
  );
  return VendorReliabilityData.fromJson(response.data as Map<String, dynamic>);
});

/// Simple parameter class for date range queries.
class DateRangeParams {
  const DateRangeParams({required this.fromDate, required this.toDate});

  final String fromDate;
  final String toDate;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is DateRangeParams &&
          runtimeType == other.runtimeType &&
          fromDate == other.fromDate &&
          toDate == other.toDate;

  @override
  int get hashCode => fromDate.hashCode ^ toDate.hashCode;
}
