import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/student/models/booking_model.dart';
import 'package:nmu_shuttle_app/features/student/repositories/booking_repository.dart';

/// Provides the BookingRepository instance.
final bookingRepositoryProvider = Provider<BookingRepository>((ref) {
  return BookingRepository(dioClient: ref.watch(dioClientProvider));
});

/// Fetches all bookings for the authenticated student.
final bookingsProvider = FutureProvider<List<BookingModel>>((ref) async {
  final repository = ref.watch(bookingRepositoryProvider);
  return repository.getBookings();
});

/// Derived provider: upcoming bookings.
/// Bookings where tripDate >= today AND bookingStatus != 'Cancelled'.
final upcomingBookingsProvider = Provider<AsyncValue<List<BookingModel>>>((ref) {
  return ref.watch(bookingsProvider).whenData((bookings) {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    return bookings.where((b) {
      final tripDay = DateTime(b.tripDate.year, b.tripDate.month, b.tripDate.day);
      return !tripDay.isBefore(today) && b.bookingStatus != 'Cancelled';
    }).toList()
      ..sort((a, b) => a.tripDate.compareTo(b.tripDate));
  });
});

/// Derived provider: past bookings.
/// Bookings where tripDate < today OR bookingStatus == 'Cancelled'.
final pastBookingsProvider = Provider<AsyncValue<List<BookingModel>>>((ref) {
  return ref.watch(bookingsProvider).whenData((bookings) {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    return bookings.where((b) {
      final tripDay = DateTime(b.tripDate.year, b.tripDate.month, b.tripDate.day);
      return tripDay.isBefore(today) || b.bookingStatus == 'Cancelled';
    }).toList()
      ..sort((a, b) => b.tripDate.compareTo(a.tripDate));
  });
});

/// Fetches a single booking by ID.
final bookingDetailProvider =
    FutureProvider.family<BookingModel, int>((ref, bookingId) async {
  final repository = ref.watch(bookingRepositoryProvider);
  return repository.getBookingById(bookingId);
});
