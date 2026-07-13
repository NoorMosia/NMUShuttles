import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/api/dio_client.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/student/models/stop_model.dart';

/// Repository that fetches stops from the API.
class StopsRepository {
  StopsRepository({required DioClient dioClient}) : _dioClient = dioClient;

  final DioClient _dioClient;

  /// Fetches all active stops with GPS coordinates.
  /// GET /api/v1/stops
  Future<List<MapStopModel>> getAllStops() async {
    final response = await _dioClient.dio.get('/stops');
    final List<dynamic> data = response.data as List<dynamic>;
    return data
        .map((json) => MapStopModel.fromJson(json as Map<String, dynamic>))
        .where((stop) => stop.isActive)
        .toList();
  }
}

/// Provides the StopsRepository instance.
final stopsRepositoryProvider = Provider<StopsRepository>((ref) {
  return StopsRepository(dioClient: ref.watch(dioClientProvider));
});

/// Fetches all active stops with GPS coordinates for the map.
final mapStopsProvider = FutureProvider<List<MapStopModel>>((ref) async {
  final repository = ref.watch(stopsRepositoryProvider);
  return repository.getAllStops();
});
