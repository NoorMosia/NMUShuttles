import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/student/models/booking_model.dart';

/// Booking card widget showing trip details and status.
/// Displays direction, stop, campus, date, time, and a color-coded status badge.
/// Tapping navigates to the booking detail screen.
class BookingCard extends StatelessWidget {
  const BookingCard({super.key, required this.booking});

  final BookingModel booking;

  @override
  Widget build(BuildContext context) {
    final dateFormat = DateFormat('EEE, d MMM');
    final directionIcon =
        booking.direction == 'ToCampus' ? Icons.arrow_upward : Icons.arrow_downward;
    final directionLabel =
        booking.direction == 'ToCampus' ? 'To Campus' : 'From Campus';

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: 1,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: () => context.push('/student/booking/${booking.bookingId}'),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              // Direction icon
              Container(
                width: 44,
                height: 44,
                decoration: BoxDecoration(
                  color: AppTheme.navy.withValues(alpha: 0.1),
                  borderRadius: BorderRadius.circular(10),
                ),
                child: Icon(directionIcon, color: AppTheme.navy),
              ),
              const SizedBox(width: 12),
              // Details
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      booking.stopName,
                      style: Theme.of(context).textTheme.titleSmall?.copyWith(
                            fontWeight: FontWeight.w600,
                          ),
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                    const SizedBox(height: 2),
                    Text(
                      '${booking.campusName} • $directionLabel',
                      style: Theme.of(context).textTheme.bodySmall?.copyWith(
                            color: Colors.grey.shade600,
                          ),
                    ),
                    const SizedBox(height: 4),
                    Row(
                      children: [
                        Icon(Icons.calendar_today,
                            size: 14, color: Colors.grey.shade500),
                        const SizedBox(width: 4),
                        Text(
                          dateFormat.format(booking.tripDate),
                          style:
                              Theme.of(context).textTheme.bodySmall?.copyWith(
                                    color: Colors.grey.shade600,
                                  ),
                        ),
                        const SizedBox(width: 12),
                        Icon(Icons.access_time,
                            size: 14, color: Colors.grey.shade500),
                        const SizedBox(width: 4),
                        Text(
                          booking.departureTime,
                          style:
                              Theme.of(context).textTheme.bodySmall?.copyWith(
                                    color: Colors.grey.shade600,
                                  ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
              // Status chip
              _StatusChip(status: booking.bookingStatus),
            ],
          ),
        ),
      ),
    );
  }
}

/// Status chip with color-coded background.
/// Confirmed = green, WaitingList = orange, Cancelled = red.
class _StatusChip extends StatelessWidget {
  const _StatusChip({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    Color backgroundColor;
    Color textColor;
    String displayLabel;

    switch (status) {
      case 'Confirmed':
        backgroundColor = Colors.green.shade50;
        textColor = Colors.green.shade700;
        displayLabel = 'Confirmed';
        break;
      case 'WaitingList':
        backgroundColor = Colors.orange.shade50;
        textColor = Colors.orange.shade700;
        displayLabel = 'Waiting List';
        break;
      case 'Cancelled':
        backgroundColor = Colors.red.shade50;
        textColor = Colors.red.shade700;
        displayLabel = 'Cancelled';
        break;
      default:
        backgroundColor = Colors.grey.shade100;
        textColor = Colors.grey.shade700;
        displayLabel = status;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: BorderRadius.circular(20),
      ),
      child: Text(
        displayLabel,
        style: TextStyle(
          fontSize: 12,
          fontWeight: FontWeight.w500,
          color: textColor,
        ),
      ),
    );
  }
}
