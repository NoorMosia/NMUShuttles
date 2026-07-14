import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

/// Model representing a shuttle vehicle.
class Shuttle {
  const Shuttle({
    required this.id,
    required this.licensePlate,
    required this.capacity,
    required this.vendorCompanyName,
    required this.isAccessibleVehicle,
  });

  final int id;
  final String licensePlate;
  final int capacity;
  final String vendorCompanyName;
  final bool isAccessibleVehicle;

  factory Shuttle.fromJson(Map<String, dynamic> json) {
    return Shuttle(
      id: json['shuttleId'] as int? ?? json['id'] as int? ?? 0,
      licensePlate: json['licensePlate'] as String? ?? '',
      capacity: json['capacity'] as int? ?? 0,
      vendorCompanyName: json['vendorCompanyName'] as String? ?? '',
      isAccessibleVehicle: json['isAccessibleVehicle'] as bool? ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'licensePlate': licensePlate,
      'capacity': capacity,
      'vendorCompanyName': vendorCompanyName,
      'isAccessibleVehicle': isAccessibleVehicle,
    };
  }
}

/// State for the shuttles list.
class ShuttlesState {
  const ShuttlesState({
    this.shuttles = const [],
    this.isLoading = false,
    this.errorMessage,
  });

  final List<Shuttle> shuttles;
  final bool isLoading;
  final String? errorMessage;

  ShuttlesState copyWith({
    List<Shuttle>? shuttles,
    bool? isLoading,
    String? errorMessage,
  }) {
    return ShuttlesState(
      shuttles: shuttles ?? this.shuttles,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage,
    );
  }
}

/// StateNotifier managing CRUD operations for shuttles.
class ShuttlesNotifier extends StateNotifier<ShuttlesState> {
  ShuttlesNotifier(this._dio) : super(const ShuttlesState());

  final Dio _dio;

  /// Fetch all shuttles from the API.
  Future<void> fetchShuttles() async {
    state = state.copyWith(isLoading: true, errorMessage: null);
    try {
      final response = await _dio.get('/admin/shuttles');
      final List<dynamic> data = response.data is List
          ? response.data as List<dynamic>
          : (response.data['data'] as List<dynamic>? ?? []);
      final shuttles = data.map((e) => Shuttle.fromJson(e as Map<String, dynamic>)).toList();
      state = state.copyWith(shuttles: shuttles, isLoading: false);
    } on DioException catch (e) {
      state = state.copyWith(
        isLoading: false,
        errorMessage: e.response?.data?['message']?.toString() ?? 'Failed to load shuttles',
      );
    }
  }

  /// Create a new shuttle. Returns null on success or an error message.
  Future<String?> createShuttle({
    required String licensePlate,
    required int capacity,
    required String vendorCompanyName,
    required bool isAccessibleVehicle,
  }) async {
    try {
      await _dio.post('/admin/shuttles', data: {
        'licensePlate': licensePlate,
        'capacity': capacity,
        'vendorCompanyName': vendorCompanyName,
        'isAccessibleVehicle': isAccessibleVehicle,
      });
      await fetchShuttles();
      return null;
    } on DioException catch (e) {
      if (e.response?.statusCode == 422 || e.response?.statusCode == 409) {
        return _extractError(e.response);
      }
      return 'Failed to create shuttle';
    }
  }

  /// Update an existing shuttle. Returns null on success or an error message.
  Future<String?> updateShuttle({
    required int id,
    required String licensePlate,
    required int capacity,
    required String vendorCompanyName,
    required bool isAccessibleVehicle,
  }) async {
    try {
      await _dio.put('/admin/shuttles/$id', data: {
        'licensePlate': licensePlate,
        'capacity': capacity,
        'vendorCompanyName': vendorCompanyName,
        'isAccessibleVehicle': isAccessibleVehicle,
      });
      await fetchShuttles();
      return null;
    } on DioException catch (e) {
      if (e.response?.statusCode == 422 || e.response?.statusCode == 409) {
        return _extractError(e.response);
      }
      return 'Failed to update shuttle';
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

/// Provider for the ShuttlesNotifier.
final shuttlesProvider = StateNotifierProvider<ShuttlesNotifier, ShuttlesState>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return ShuttlesNotifier(dioClient.dio);
});
