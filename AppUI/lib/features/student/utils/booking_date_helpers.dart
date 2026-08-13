import 'package:intl/intl.dart';
import 'package:nmu_shuttle_app/features/student/models/booking_model.dart';

/// Groups a list of bookings by their [tripDate] (date only, ignoring time).
/// Returns an ordered map where keys are dates and values are the bookings
/// for that date. Order is preserved from the input list.
Map<DateTime, List<BookingModel>> groupBookingsByDate(
    List<BookingModel> bookings) {
  final Map<DateTime, List<BookingModel>> grouped = {};

  for (final booking in bookings) {
    final dateKey = DateTime(
      booking.tripDate.year,
      booking.tripDate.month,
      booking.tripDate.day,
    );
    grouped.putIfAbsent(dateKey, () => []).add(booking);
  }

  return grouped;
}

/// Returns a human-friendly label for a date:
/// - "Today" if it matches today's date
/// - "Tomorrow" if it matches tomorrow's date
/// - "Yesterday" if it matches yesterday's date
/// - Otherwise "dd/MM" (e.g. "15/08")
String formatDateLabel(DateTime date) {
  final now = DateTime.now();
  final today = DateTime(now.year, now.month, now.day);
  final dateOnly = DateTime(date.year, date.month, date.day);

  final difference = dateOnly.difference(today).inDays;

  if (difference == 0) return 'Today';
  if (difference == 1) return 'Tomorrow';
  if (difference == -1) return 'Yesterday';

  return DateFormat('dd/MM').format(date);
}
