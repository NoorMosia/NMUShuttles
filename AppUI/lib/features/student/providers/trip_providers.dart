import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/student/models/trip_model.dart';
import 'package:nmu_shuttle_app/features/student/repositories/trip_repository.dart';

/// Provides the TripRepository instance.
final tripRepositoryProvider = Provider<TripRepository>((ref) {
  return TripRepository(dioClient: ref.watch(dioClientProvider));
});

/// Query parameters for fetching trip availability.
class TripQuery {
  const TripQuery({
    required this.date,
    this.stopId,
    this.direction,
  });

  final String date;
  final int? stopId;
  final String? direction;

  @override
  bool operator ==(Object other) =>
      identical(this, other) ||
      other is TripQuery &&
          runtimeType == other.runtimeType &&
          date == other.date &&
          stopId == other.stopId &&
          direction == other.direction;

  @override
  int get hashCode => Object.hash(date, stopId, direction);
}

/// Fetches available trips based on query params (date, stopId, direction).
final tripAvailabilityProvider =
    FutureProvider.family<List<TripModel>, TripQuery>((ref, query) async {
  final repository = ref.watch(tripRepositoryProvider);
  return repository.getTrips(
    date: query.date,
    stopId: query.stopId,
    direction: query.direction,
  );
});

/// Fetches all stops for the stop search field.
final stopsProvider = FutureProvider<List<StopModel>>((ref) async {
  final repository = ref.watch(tripRepositoryProvider);
  return repository.getStops();
});
