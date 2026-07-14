import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_app/core/api/dio_client.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/marshal/models/marshal_trip_model.dart';

/// Marshal assignment info extracted from JWT claims.
class MarshalAssignment {
  const MarshalAssignment({
    this.assignedCampusId,
    this.assignedCampusName,
    this.assignedStopId,
    this.assignedStopName,
    this.gpsCoordinates,
  });

  final int? assignedCampusId;
  final String? assignedCampusName;
  final int? assignedStopId;
  final String? assignedStopName;
  final String? gpsCoordinates;

  /// The display name for the marshal's assigned location.
  String get locationName =>
      assignedCampusName ?? assignedStopName ?? 'Unknown Location';

  /// Whether this marshal is assigned to a campus (shows FromCampus trips).
  bool get isCampusAssigned => assignedCampusId != null;

  /// The expected direction for trips at this location.
  String get direction => isCampusAssigned ? 'FromCampus' : 'ToCampus';
}

/// State for the Marshal Home screen.
class MarshalHomeState {
  const MarshalHomeState({
    this.assignment,
    this.trips = const [],
    this.isLoading = false,
    this.error,
  });

  final MarshalAssignment? assignment;
  final List<MarshalTripModel> trips;
  final bool isLoading;
  final String? error;

  /// Active (non-Completed) trips sorted chronologically.
  List<MarshalTripModel> get activeTrips =>
      trips.where((t) => t.isActive).toList();

  /// The next boarding trip (soonest non-Completed trip).
  MarshalTripModel? get nextBoardingTrip =>
      activeTrips.isNotEmpty ? activeTrips.first : null;

  /// Remaining trips after the next boarding trip.
  List<MarshalTripModel> get remainingTrips =>
      activeTrips.length > 1 ? activeTrips.sublist(1) : [];

  MarshalHomeState copyWith({
    MarshalAssignment? assignment,
    List<MarshalTripModel>? trips,
    bool? isLoading,
    String? error,
    bool clearError = false,
  }) {
    return MarshalHomeState(
      assignment: assignment ?? this.assignment,
      trips: trips ?? this.trips,
      isLoading: isLoading ?? this.isLoading,
      error: clearError ? null : (error ?? this.error),
    );
  }
}

/// StateNotifier that manages the Marshal Home screen data.
class MarshalHomeNotifier extends StateNotifier<MarshalHomeState> {
  MarshalHomeNotifier({
    required DioClient dioClient,
  })  : _dioClient = dioClient,
        super(const MarshalHomeState()) {
    loadData();
  }

  final DioClient _dioClient;

  /// Loads the marshal's assignment and today's trips.
  Future<void> loadData() async {
    state = state.copyWith(isLoading: true, clearError: true);

    try {
      // Extract marshal assignment from JWT claims
      final assignment = await _getMarshalAssignment();
      if (assignment == null) {
        state = state.copyWith(
          isLoading: false,
          error: 'Could not determine marshal assignment.',
        );
        return;
      }

      // Fetch today's trips filtered by location and direction
      final trips = await _fetchTodayTrips(assignment);

      state = state.copyWith(
        assignment: assignment,
        trips: trips,
        isLoading: false,
      );
    } catch (e) {
      state = state.copyWith(
        isLoading: false,
        error: 'Failed to load schedule. Pull to refresh.',
      );
    }
  }

  /// Fetches the marshal assignment info from the API.
  Future<MarshalAssignment?> _getMarshalAssignment() async {
    return await _fetchAssignmentFromApi();
  }

  /// Fetches marshal assignment from the API as fallback.
  Future<MarshalAssignment?> _fetchAssignmentFromApi() async {
    try {
      final response = await _dioClient.dio.get('/marshal/me');
      final data = response.data as Map<String, dynamic>;
      return MarshalAssignment(
        assignedCampusId: _parseInt(data['assignedCampusId']),
        assignedCampusName: data['assignedCampusName'] as String?,
        assignedStopId: _parseInt(data['assignedStopId']),
        assignedStopName: data['assignedStopName'] as String?,
        gpsCoordinates: data['gpsCoordinates'] as String?,
      );
    } catch (_) {
      return null;
    }
  }

  /// Fetches today's trips filtered by the marshal's assigned location and direction.
  Future<List<MarshalTripModel>> _fetchTodayTrips(
      MarshalAssignment assignment) async {
    final today = DateFormat('yyyy-MM-dd').format(DateTime.now());

    final queryParams = <String, dynamic>{
      'date': today,
      'direction': assignment.direction,
    };

    if (assignment.isCampusAssigned) {
      queryParams['campusId'] = assignment.assignedCampusId;
    } else {
      queryParams['stopId'] = assignment.assignedStopId;
    }

    final response = await _dioClient.dio.get(
      '/trips',
      queryParameters: queryParams,
    );

    final List<dynamic> data = response.data as List<dynamic>;
    final trips = data
        .map((json) =>
            MarshalTripModel.fromJson(json as Map<String, dynamic>))
        .toList();

    // Sort by departure time chronologically
    trips.sort((a, b) => a.departureTime.compareTo(b.departureTime));

    return trips;
  }

  /// Refresh data (for pull-to-refresh).
  Future<void> refresh() => loadData();

  int? _parseInt(dynamic value) {
    if (value == null) return null;
    if (value is int) return value;
    if (value is String) return int.tryParse(value);
    return null;
  }
}

/// Provider for the MarshalHomeNotifier.
final marshalHomeProvider =
    StateNotifierProvider<MarshalHomeNotifier, MarshalHomeState>((ref) {
  return MarshalHomeNotifier(
    dioClient: ref.watch(dioClientProvider),
  );
});
