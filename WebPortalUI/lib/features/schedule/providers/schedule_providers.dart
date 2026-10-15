import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/core/api/dio_client.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

// ─── Models ──────────────────────────────────────────────────────────────────

/// Represents a schedule time entry.
class ScheduleTimeModel {
  const ScheduleTimeModel({
    required this.id,
    required this.departureTime,
    required this.direction,
    required this.scheduleType,
    required this.campusId,
    required this.stopId,
    this.campusName,
    this.stopName,
  });

  final int id;
  final String departureTime;
  final String direction;
  final String scheduleType;
  final int campusId;
  final int stopId;
  final String? campusName;
  final String? stopName;

  factory ScheduleTimeModel.fromJson(Map<String, dynamic> json) {
    return ScheduleTimeModel(
      id: json['scheduleTimeId'] as int? ?? json['id'] as int,
      departureTime: json['departureTime'] as String? ?? '',
      direction: json['direction'] as String? ?? '',
      scheduleType: json['scheduleType'] as String? ?? '',
      campusId: json['campusId'] as int? ?? 0,
      stopId: json['stopId'] as int? ?? 0,
      campusName: json['campusName'] as String?,
      stopName: json['stopName'] as String?,
    );
  }
}

/// Represents a calendar exception entry.
class CalendarExceptionModel {
  const CalendarExceptionModel({
    required this.id,
    required this.targetDate,
    required this.exceptionType,
    required this.description,
  });

  final int id;
  final String targetDate;
  final String exceptionType;
  final String description;

  factory CalendarExceptionModel.fromJson(Map<String, dynamic> json) {
    return CalendarExceptionModel(
      id: json['calendarExceptionId'] as int? ?? json['id'] as int,
      targetDate: json['targetDate'] as String? ?? '',
      exceptionType: json['exceptionType'] as String? ?? '',
      description: json['description'] as String? ?? '',
    );
  }
}

/// Represents a priority schedule entry.
class PriorityScheduleModel {
  const PriorityScheduleModel({
    required this.id,
    required this.studentId,
    required this.stopId,
    required this.scheduleTimeId,
    required this.dayOfWeek,
    required this.isActive,
    this.studentNumber,
    this.studentName,
    this.stopName,
  });

  final int id;
  final int studentId;
  final int stopId;
  final int scheduleTimeId;
  final String dayOfWeek;
  final bool isActive;
  final String? studentNumber;
  final String? studentName;
  final String? stopName;

  factory PriorityScheduleModel.fromJson(Map<String, dynamic> json) {
    return PriorityScheduleModel(
      id: json['priorityScheduleId'] as int? ?? json['id'] as int,
      studentId: json['studentId'] as int? ?? 0,
      stopId: json['stopId'] as int? ?? 0,
      scheduleTimeId: json['scheduleTimeId'] as int? ?? 0,
      dayOfWeek: json['dayOfWeek'] as String? ?? '',
      isActive: json['isActive'] as bool? ?? true,
      studentNumber: json['studentNumber'] as String?,
      studentName: json['studentName'] as String?,
      stopName: json['stopName'] as String?,
    );
  }
}

// ─── Schedule Times State & Notifier ─────────────────────────────────────────

enum ScheduleStatus { idle, loading, success, error }

class ScheduleTimesState {
  const ScheduleTimesState({
    this.status = ScheduleStatus.idle,
    this.items = const [],
    this.errorMessage,
  });

  final ScheduleStatus status;
  final List<ScheduleTimeModel> items;
  final String? errorMessage;

  ScheduleTimesState copyWith({
    ScheduleStatus? status,
    List<ScheduleTimeModel>? items,
    String? errorMessage,
  }) {
    return ScheduleTimesState(
      status: status ?? this.status,
      items: items ?? this.items,
      errorMessage: errorMessage,
    );
  }
}

class ScheduleTimesNotifier extends StateNotifier<ScheduleTimesState> {
  ScheduleTimesNotifier(this._dioClient) : super(const ScheduleTimesState());

  final DioClient _dioClient;

  Future<void> fetchScheduleTimes() async {
    state = state.copyWith(status: ScheduleStatus.loading);
    try {
      final response = await _dioClient.dio.get('/admin/schedule-times');
      final List<dynamic> data = response.data is List
          ? response.data as List<dynamic>
          : (response.data['scheduleTimes'] as List<dynamic>?) ?? [];
      final items = data
          .map((e) => ScheduleTimeModel.fromJson(e as Map<String, dynamic>))
          .toList();
      state = ScheduleTimesState(status: ScheduleStatus.success, items: items);
    } on DioException catch (e) {
      state = ScheduleTimesState(
        status: ScheduleStatus.error,
        errorMessage: e.response?.data?['message']?.toString() ??
            'Failed to load schedule times',
      );
    } on Exception catch (_) {
      state = const ScheduleTimesState(
        status: ScheduleStatus.error,
        errorMessage: 'Failed to load schedule times',
      );
    }
  }

  Future<bool> addScheduleTime({
    required String departureTime,
    required String direction,
    required String scheduleType,
    required int stopId,
    bool requiresScanning = true,
  }) async {
    try {
      await _dioClient.dio.post('/admin/schedule-times', data: {
        'departureTime': departureTime,
        'direction': direction,
        'scheduleType': scheduleType,
        'stopId': stopId,
        'requiresScanning': requiresScanning,
      });
      await fetchScheduleTimes();
      return true;
    } on DioException catch (e) {
      debugPrint('Add schedule time error: ${e.response?.data}');
      return false;
    }
  }

  Future<bool> updateScheduleTime({
    required int id,
    required String departureTime,
    required String direction,
    required String scheduleType,
    required int stopId,
    bool requiresScanning = true,
  }) async {
    try {
      await _dioClient.dio.put('/admin/schedule-times/$id', data: {
        'departureTime': departureTime,
        'direction': direction,
        'scheduleType': scheduleType,
        'stopId': stopId,
        'requiresScanning': requiresScanning,
      });
      await fetchScheduleTimes();
      return true;
    } on DioException catch (e) {
      debugPrint('Update schedule time error: ${e.response?.data}');
      return false;
    }
  }

  Future<bool> deleteScheduleTime(int id) async {
    try {
      await _dioClient.dio.delete('/admin/schedule-times/$id');
      await fetchScheduleTimes();
      return true;
    } on DioException catch (e) {
      debugPrint('Delete schedule time error: ${e.response?.data}');
      return false;
    }
  }
}

// ─── Calendar Exceptions State & Notifier ────────────────────────────────────

class CalendarExceptionsState {
  const CalendarExceptionsState({
    this.status = ScheduleStatus.idle,
    this.items = const [],
    this.errorMessage,
  });

  final ScheduleStatus status;
  final List<CalendarExceptionModel> items;
  final String? errorMessage;

  CalendarExceptionsState copyWith({
    ScheduleStatus? status,
    List<CalendarExceptionModel>? items,
    String? errorMessage,
  }) {
    return CalendarExceptionsState(
      status: status ?? this.status,
      items: items ?? this.items,
      errorMessage: errorMessage,
    );
  }
}

class CalendarExceptionsNotifier extends StateNotifier<CalendarExceptionsState> {
  CalendarExceptionsNotifier(this._dioClient)
      : super(const CalendarExceptionsState());

  final DioClient _dioClient;

  Future<void> fetchCalendarExceptions() async {
    state = state.copyWith(status: ScheduleStatus.loading);
    try {
      final response = await _dioClient.dio.get('/admin/calendar-exceptions');
      final List<dynamic> data = response.data is List
          ? response.data as List<dynamic>
          : (response.data['calendarExceptions'] as List<dynamic>?) ?? [];
      final items = data
          .map((e) =>
              CalendarExceptionModel.fromJson(e as Map<String, dynamic>))
          .toList();
      // Sort by date
      items.sort((a, b) => a.targetDate.compareTo(b.targetDate));
      state = CalendarExceptionsState(
          status: ScheduleStatus.success, items: items);
    } on DioException catch (e) {
      state = CalendarExceptionsState(
        status: ScheduleStatus.error,
        errorMessage: e.response?.data?['message']?.toString() ??
            'Failed to load calendar exceptions',
      );
    } on Exception catch (_) {
      state = const CalendarExceptionsState(
        status: ScheduleStatus.error,
        errorMessage: 'Failed to load calendar exceptions',
      );
    }
  }

  Future<bool> addCalendarException({
    required String targetDate,
    required String exceptionType,
    required String description,
  }) async {
    try {
      await _dioClient.dio.post('/admin/calendar-exceptions', data: {
        'targetDate': targetDate,
        'exceptionType': exceptionType,
        'description': description,
      });
      await fetchCalendarExceptions();
      return true;
    } on DioException catch (e) {
      debugPrint('Add calendar exception error: ${e.response?.data}');
      return false;
    }
  }

  Future<bool> deleteCalendarException(int id) async {
    try {
      await _dioClient.dio.delete('/admin/calendar-exceptions/$id');
      await fetchCalendarExceptions();
      return true;
    } on DioException catch (e) {
      debugPrint('Delete calendar exception error: ${e.response?.data}');
      return false;
    }
  }
}

// ─── Priority Schedules State & Notifier ─────────────────────────────────────

class PrioritySchedulesState {
  const PrioritySchedulesState({
    this.status = ScheduleStatus.idle,
    this.items = const [],
    this.errorMessage,
  });

  final ScheduleStatus status;
  final List<PriorityScheduleModel> items;
  final String? errorMessage;

  PrioritySchedulesState copyWith({
    ScheduleStatus? status,
    List<PriorityScheduleModel>? items,
    String? errorMessage,
  }) {
    return PrioritySchedulesState(
      status: status ?? this.status,
      items: items ?? this.items,
      errorMessage: errorMessage,
    );
  }
}

class PrioritySchedulesNotifier extends StateNotifier<PrioritySchedulesState> {
  PrioritySchedulesNotifier(this._dioClient)
      : super(const PrioritySchedulesState());

  final DioClient _dioClient;

  Future<void> fetchPrioritySchedules() async {
    state = state.copyWith(status: ScheduleStatus.loading);
    try {
      final response = await _dioClient.dio.get('/admin/priority-schedules');
      final List<dynamic> data = response.data is List
          ? response.data as List<dynamic>
          : (response.data['prioritySchedules'] as List<dynamic>?) ?? [];
      final items = data
          .map((e) =>
              PriorityScheduleModel.fromJson(e as Map<String, dynamic>))
          .toList();
      state = PrioritySchedulesState(
          status: ScheduleStatus.success, items: items);
    } on DioException catch (e) {
      state = PrioritySchedulesState(
        status: ScheduleStatus.error,
        errorMessage: e.response?.data?['message']?.toString() ??
            'Failed to load priority schedules',
      );
    } on Exception catch (_) {
      state = const PrioritySchedulesState(
        status: ScheduleStatus.error,
        errorMessage: 'Failed to load priority schedules',
      );
    }
  }

  Future<bool> addPrioritySchedule({
    required int studentId,
    required int stopId,
    required int scheduleTimeId,
    required String dayOfWeek,
  }) async {
    try {
      await _dioClient.dio.post('/admin/priority-schedules', data: {
        'studentId': studentId,
        'stopId': stopId,
        'scheduleTimeId': scheduleTimeId,
        'dayOfWeek': dayOfWeek,
      });
      await fetchPrioritySchedules();
      return true;
    } on DioException catch (e) {
      debugPrint('Add priority schedule error: ${e.response?.data}');
      return false;
    }
  }

  Future<bool> deactivatePrioritySchedule(int id) async {
    try {
      await _dioClient.dio.patch('/admin/priority-schedules/$id', data: {
        'isActive': false,
      });
      await fetchPrioritySchedules();
      return true;
    } on DioException catch (e) {
      debugPrint('Deactivate priority schedule error: ${e.response?.data}');
      return false;
    }
  }
}

// ─── Providers ───────────────────────────────────────────────────────────────

final scheduleTimesProvider =
    StateNotifierProvider<ScheduleTimesNotifier, ScheduleTimesState>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return ScheduleTimesNotifier(dioClient);
});

final calendarExceptionsProvider = StateNotifierProvider<
    CalendarExceptionsNotifier, CalendarExceptionsState>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return CalendarExceptionsNotifier(dioClient);
});

final prioritySchedulesProvider = StateNotifierProvider<
    PrioritySchedulesNotifier, PrioritySchedulesState>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return PrioritySchedulesNotifier(dioClient);
});

