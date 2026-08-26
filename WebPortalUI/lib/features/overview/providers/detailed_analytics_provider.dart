import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';
import 'package:nmu_shuttle_admin/features/overview/models/detailed_analytics_model.dart';

/// Provider that fetches detailed analytics data.
final detailedAnalyticsProvider =
    FutureProvider<DetailedAnalyticsData>((ref) async {
  final dioClient = ref.watch(dioClientProvider);
  final response = await dioClient.dio.get('/admin/analytics/detailed');
  return DetailedAnalyticsData.fromJson(response.data as Map<String, dynamic>);
});
