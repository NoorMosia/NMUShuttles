import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/student/models/booking_model.dart';
import 'package:nmu_shuttle_app/features/student/repositories/booking_repository.dart';

/// Provides the BookingRepository instance.
final bookingRepositoryProvider = Provider<BookingRepository>((ref) {
  return BookingRepository(dioClient: ref.watch(dioClientProvider));
});

/// A "refresh token" provider used solely for invalidation.
/// When external code calls `ref.invalidate(bookingsProvider)`, this triggers
/// both [upcomingBookingsProvider] and [pastBookingsProvider] to refetch.
final bookingsProvider = StateProvider<int>((ref) => 0);

/// Fetches upcoming bookings from the server (tripDate >= today, not cancelled).
/// Watches [bookingsProvider] so that invalidating it triggers a refetch.
final upcomingBookingsProvider =
    FutureProvider<List<BookingModel>>((ref) async {
  // Depend on the refresh token so invalidation cascades.
  ref.watch(bookingsProvider);
  final repository = ref.watch(bookingRepositoryProvider);
  final result = await repository.getBookings(
    filter: 'upcoming',
    page: 1,
    pageSize: 50,
  );
  return result.items;
});

/// Fetches past bookings from the server (tripDate < today or cancelled).
/// Watches [bookingsProvider] so that invalidating it triggers a refetch.
final pastBookingsProvider = FutureProvider<List<BookingModel>>((ref) async {
  // Depend on the refresh token so invalidation cascades.
  ref.watch(bookingsProvider);
  final repository = ref.watch(bookingRepositoryProvider);
  final result = await repository.getBookings(
    filter: 'past',
    page: 1,
    pageSize: 20,
  );
  return result.items;
});

/// Fetches a single booking by ID.
final bookingDetailProvider =
    FutureProvider.family<BookingModel, int>((ref, bookingId) async {
  final repository = ref.watch(bookingRepositoryProvider);
  return repository.getBookingById(bookingId);
});
