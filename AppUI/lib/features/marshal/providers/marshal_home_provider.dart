import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/api/dio_client.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/marshal/models/marshal_time_slot_model.dart';

/// Marshal assignment info retrieved from GET /marshal/me.
class MarshalAssignment {
  const MarshalAssignment({
    this.firstName,
    this.lastName,
    this.staffNumber,
    this.assignedCampusId,
    this.assignedCampusName,
    this.assignedStopId,
    this.assignedStopName,
    this.gpsCoordinates,
  });

  final String? firstName;
  final String? lastName;
  final String? staffNumber;
  final int? assignedCampusId;
  final String? assignedCampusName;
  final int? assignedStopId;
  final String? assignedStopName;
  final String? gpsCoordinates;

  /// Full display name.
  String get fullName {
    final first = firstName ?? '';
    final last = lastName ?? '';
    final name = '$first $last'.trim();
    return name.isNotEmpty ? name : 'Unknown';
  }

  /// The display name for the marshal's assigned location.
  String get locationName =>
      assignedCampusName ?? assignedStopName ?? 'Unknown Location';

  /// Whether this marshal is assigned to a campus (vs. a stop).
  bool get isCampusAssigned => assignedCampusId != null;
}

/// State for the Marshal Home screen.
class MarshalHomeState {
  const MarshalHomeState({
    this.assignment,
    this.timeSlots = const [],
    this.isLoading = false,
    this.error,
  });

  final MarshalAssignment? assignment;
  final List<MarshalTimeSlotModel> timeSlots;
  final bool isLoading;
  final String? error;

  /// Upcoming time slots (departure not yet passed, not Completed) sorted
  /// chronologically. A departed trip that is still "Locked" is excluded here
  /// so it can never be shown as the next boarding.
  List<MarshalTimeSlotModel> get activeSlots {
    final now = DateTime.now();
    final slots = timeSlots.where((t) => t.isUpcoming(now)).toList();
    slots.sort((a, b) {
      final da = a.departureDateTime;
      final db = b.departureDateTime;
      if (da == null || db == null) return 0;
      return da.compareTo(db);
    });
    return slots;
  }

  /// The next upcoming time slot (soonest upcoming, non-Completed).
  MarshalTimeSlotModel? get nextTimeSlot =>
      activeSlots.isNotEmpty ? activeSlots.first : null;

  /// Remaining time slots after the next one.
  List<MarshalTimeSlotModel> get remainingSlots =>
      activeSlots.length > 1 ? activeSlots.sublist(1) : [];

  MarshalHomeState copyWith({
    MarshalAssignment? assignment,
    List<MarshalTimeSlotModel>? timeSlots,
    bool? isLoading,
    String? error,
    bool clearError = false,
  }) {
    return MarshalHomeState(
      assignment: assignment ?? this.assignment,
      timeSlots: timeSlots ?? this.timeSlots,
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

  /// Loads the marshal's assignment and today's grouped time slots.
  Future<void> loadData() async {
    state = state.copyWith(isLoading: true, clearError: true);

    try {
      // Fetch marshal assignment info
      final assignment = await _fetchAssignment();
      if (assignment == null) {
        state = state.copyWith(
          isLoading: false,
          error: 'Could not determine marshal assignment.',
        );
        return;
      }

      // Fetch today's time slots from the new grouped endpoint
      final timeSlots = await _fetchTodayTimeSlots();

      state = state.copyWith(
        assignment: assignment,
        timeSlots: timeSlots,
        isLoading: false,
      );
    } catch (e) {
      state = state.copyWith(
        isLoading: false,
        error: 'Failed to load schedule. Pull to refresh.',
      );
    }
  }

  /// Fetches marshal assignment from GET /marshal/me.
  Future<MarshalAssignment?> _fetchAssignment() async {
    try {
      final response = await _dioClient.dio.get('/marshal/me');
      final data = response.data as Map<String, dynamic>;
      return MarshalAssignment(
        firstName: data['firstName'] as String?,
        lastName: data['lastName'] as String?,
        staffNumber: data['staffNumber'] as String?,
        assignedCampusId: _parseInt(data['assignedCampusId']),
        assignedCampusName: data['assignedCampusName'] as String?,
        assignedStopId: _parseInt(data['assignedStopId']),
        assignedStopName: data['assignedStopName'] as String?,
        gpsCoordinates: data['gpsCoordinates'] as String?,
      );
    } catch (e) {
      // Log so we can debug — don't swallow silently
      debugPrint('[MarshalHome] Failed to fetch assignment: $e');
      return null;
    }
  }

  /// Fetches today's grouped time slots from GET /marshal/trips/today.
  /// The backend handles filtering by marshal's assigned location and
  /// returns both directions (no direction assumption on the client).
  Future<List<MarshalTimeSlotModel>> _fetchTodayTimeSlots() async {
    final response = await _dioClient.dio.get('/marshal/trips/today');

    final List<dynamic> data = response.data as List<dynamic>;
    return data
        .map((json) =>
            MarshalTimeSlotModel.fromJson(json as Map<String, dynamic>))
        .toList();
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
    StateNotifierProvider.autoDispose<MarshalHomeNotifier, MarshalHomeState>((ref) {
  return MarshalHomeNotifier(
    dioClient: ref.watch(dioClientProvider),
  );
});
