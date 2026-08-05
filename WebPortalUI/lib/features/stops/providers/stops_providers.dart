import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

/// Model representing a shuttle stop.
class Stop {
  const Stop({
    required this.id,
    required this.stopName,
    required this.street,
    required this.suburb,
    required this.city,
    required this.postalCode,
    required this.fullAddress,
    required this.gpsCoordinates,
  });

  final int id;
  final String stopName;
  final String street;
  final String suburb;
  final String city;
  final String postalCode;
  final String fullAddress;
  final String gpsCoordinates;

  factory Stop.fromJson(Map<String, dynamic> json) {
    return Stop(
      id: json['stopId'] as int? ?? json['id'] as int? ?? 0,
      stopName: json['stopName'] as String? ?? '',
      street: json['street'] as String? ?? '',
      suburb: json['suburb'] as String? ?? '',
      city: json['city'] as String? ?? '',
      postalCode: json['postalCode'] as String? ?? '',
      fullAddress: json['fullAddress'] as String? ?? '',
      gpsCoordinates: json['gpsCoordinates'] as String? ?? '',
    );
  }
}

/// State for the stops list.
class StopsState {
  const StopsState({
    this.stops = const [],
    this.isLoading = false,
    this.errorMessage,
  });

  final List<Stop> stops;
  final bool isLoading;
  final String? errorMessage;

  StopsState copyWith({
    List<Stop>? stops,
    bool? isLoading,
    String? errorMessage,
  }) {
    return StopsState(
      stops: stops ?? this.stops,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage,
    );
  }
}

/// StateNotifier managing CRUD operations for stops.
class StopsNotifier extends StateNotifier<StopsState> {
  StopsNotifier(this._dio) : super(const StopsState());

  final Dio _dio;

  /// Fetch all stops from the API.
  Future<void> fetchStops() async {
    state = state.copyWith(isLoading: true, errorMessage: null);
    try {
      final response = await _dio.get('/admin/stops');
      final List<dynamic> data = response.data is List
          ? response.data as List<dynamic>
          : (response.data['data'] as List<dynamic>? ?? []);
      final stops = data.map((e) => Stop.fromJson(e as Map<String, dynamic>)).toList();
      state = state.copyWith(stops: stops, isLoading: false);
    } on DioException catch (e) {
      state = state.copyWith(
        isLoading: false,
        errorMessage: e.response?.data?['message']?.toString() ?? 'Failed to load stops',
      );
    }
  }

  /// Create a new stop. Returns null on success or an error message.
  Future<String?> createStop({
    required String stopName,
    required String street,
    required String suburb,
    required String city,
    required String postalCode,
    required String gpsCoordinates,
  }) async {
    try {
      await _dio.post('/admin/stops', data: {
        'stopName': stopName,
        'street': street,
        'suburb': suburb,
        'city': city,
        'postalCode': postalCode,
        'gpsCoordinates': gpsCoordinates,
      });
      await fetchStops();
      return null;
    } on DioException catch (e) {
      if (e.response?.statusCode == 422 || e.response?.statusCode == 409) {
        return _extractError(e.response);
      }
      return 'Failed to create stop';
    }
  }

  /// Update an existing stop. Returns null on success or an error message.
  Future<String?> updateStop({
    required int id,
    required String stopName,
    required String street,
    required String suburb,
    required String city,
    required String postalCode,
    required String gpsCoordinates,
  }) async {
    try {
      await _dio.put('/admin/stops/$id', data: {
        'stopName': stopName,
        'street': street,
        'suburb': suburb,
        'city': city,
        'postalCode': postalCode,
        'gpsCoordinates': gpsCoordinates,
      });
      await fetchStops();
      return null;
    } on DioException catch (e) {
      if (e.response?.statusCode == 422 || e.response?.statusCode == 409) {
        return _extractError(e.response);
      }
      return 'Failed to update stop';
    }
  }

  /// Delete a stop by ID. Returns null on success or an error message.
  Future<String?> deleteStop(int id) async {
    try {
      await _dio.delete('/admin/stops/$id');
      await fetchStops();
      return null;
    } on DioException catch (e) {
      return e.response?.data?['message']?.toString() ?? 'Failed to delete stop';
    }
  }

  String _extractError(Response? response) {
    if (response?.data is Map) {
      final data = response!.data as Map<String, dynamic>;
      if (data.containsKey('errors')) {
        final errors = data['errors'];
        if (errors is Map) {
          return errors.values
              .expand((v) => v is List ? v : [v])
              .join(', ');
        }
      }
      if (data.containsKey('message')) {
        return data['message'].toString();
      }
    }
    return 'Validation error';
  }
}

/// Provider for the StopsNotifier.
final stopsProvider = StateNotifierProvider<StopsNotifier, StopsState>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return StopsNotifier(dioClient.dio);
});
