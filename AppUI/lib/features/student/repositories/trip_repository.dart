import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/core/api/dio_client.dart';
import 'package:nmu_shuttle_app/features/student/models/trip_model.dart';

/// Repository for trip-related API calls.
class TripRepository {
  TripRepository({required DioClient dioClient}) : _dioClient = dioClient;

  final DioClient _dioClient;

  /// Fetches available trips with optional filters.
  /// GET /api/v1/trips?date=X&stopId=Y&direction=Z
  Future<List<TripModel>> getTrips({
    String? date,
    int? stopId,
    String? direction,
  }) async {
    final queryParams = <String, dynamic>{};
    if (date != null) queryParams['date'] = date;
    if (stopId != null) queryParams['stopId'] = stopId;
    if (direction != null) queryParams['direction'] = direction;

    final response = await _dioClient.dio.get(
      ApiConstants.trips,
      queryParameters: queryParams,
    );
    final List<dynamic> data = response.data as List<dynamic>;
    return data
        .map((json) => TripModel.fromJson(json as Map<String, dynamic>))
        .toList();
  }

  /// Fetches all stops for the stop search.
  /// GET /api/v1/stops
  Future<List<StopModel>> getStops() async {
    final response = await _dioClient.dio.get('/stops');
    final List<dynamic> data = response.data as List<dynamic>;
    return data
        .map((json) => StopModel.fromJson(json as Map<String, dynamic>))
        .toList();
  }
}
