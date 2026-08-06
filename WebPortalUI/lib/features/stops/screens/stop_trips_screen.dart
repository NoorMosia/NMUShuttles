import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/features/stops/providers/stop_trips_provider.dart';

/// Stop-centric trip view showing all upcoming trips for a specific stop
/// with convoy summary, filterable by date and status.
class StopTripsScreen extends ConsumerWidget {
  const StopTripsScreen({super.key, required this.stopId});

  final int stopId;

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final tripsAsync = ref.watch(stopTripsProvider(stopId));

    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Back navigation
          Row(
            children: [
              IconButton(
                onPressed: () => context.go('/stops/$stopId'),
                icon: const Icon(Icons.arrow_back),
                tooltip: 'Back to Stop',
                color: _navy,
              ),
              const SizedBox(width: 8),
              tripsAsync.when(
                data: (data) => Text(
                  'Trips — ${data.stopName}',
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 22,
                    fontWeight: FontWeight.bold,
                    color: _navy,
                  ),
                ),
                loading: () => Text(
                  'Trips',
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 22,
                    fontWeight: FontWeight.bold,
                    color: _navy,
                  ),
                ),
                error: (_, __) => Text(
                  'Trips',
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 22,
                    fontWeight: FontWeight.bold,
                    color: _navy,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 20),
          // Content
          Expanded(
            child: tripsAsync.when(
              loading: () => const Center(
                child: CircularProgressIndicator(strokeWidth: 3),
              ),
              error: (error, _) => Center(
                child: Text(
                  'Failed to load trips',
                  style: GoogleFonts.hankenGrotesk(color: Colors.red),
                ),
              ),
              data: (data) => data.trips.isEmpty
                  ? Center(
                      child: Text(
                        'No upcoming trips for this stop',
                        style: GoogleFonts.hankenGrotesk(
                          fontSize: 14,
                          color: Colors.grey.shade600,
                        ),
                      ),
                    )
                  : _buildTripsList(context, data.trips),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTripsList(BuildContext context, List<StopTripEntry> trips) {
    return ListView.builder(
      itemCount: trips.length,
      itemBuilder: (context, index) {
        final trip = trips[index];
        return _TripCard(trip: trip);
      },
    );
  }
}

class _TripCard extends StatelessWidget {
  const _TripCard({required this.trip});

  final StopTripEntry trip;
  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(10),
        side: BorderSide(
          color: trip.hasEmptyConvoy
              ? Colors.orange.shade300
              : const Color(0xFFE8ECF0),
        ),
      ),
      child: InkWell(
        borderRadius: BorderRadius.circular(10),
        onTap: () => context.go('/trips/${trip.tripInstanceId}'),
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Row(
            children: [
              // Time + direction
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    trip.departureTime,
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 16,
                      fontWeight: FontWeight.w700,
                      color: _navy,
                    ),
                  ),
                  const SizedBox(height: 2),
                  Text(
                    '${trip.tripDate} • ${trip.direction}',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 12,
                      color: Colors.grey.shade600,
                    ),
                  ),
                ],
              ),
              const SizedBox(width: 24),
              // Status badge
              _StatusChip(status: trip.status),
              const Spacer(),
              // Capacity info
              Column(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Text(
                    '${trip.confirmedCount}/${trip.totalCapacity}',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                      color: _navy,
                    ),
                  ),
                  Text(
                    '${trip.shuttleCount} shuttle${trip.shuttleCount != 1 ? 's' : ''}',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 11,
                      color: Colors.grey.shade600,
                    ),
                  ),
                ],
              ),
              if (trip.hasEmptyConvoy) ...[
                const SizedBox(width: 12),
                Tooltip(
                  message: 'Empty convoy — no shuttles assigned',
                  child: Icon(Icons.warning_amber_rounded,
                      color: Colors.orange.shade700, size: 20),
                ),
              ],
              const SizedBox(width: 8),
              Icon(Icons.chevron_right, color: Colors.grey.shade400, size: 20),
            ],
          ),
        ),
      ),
    );
  }
}

class _StatusChip extends StatelessWidget {
  const _StatusChip({required this.status});
  final String status;

  @override
  Widget build(BuildContext context) {
    final (Color bg, Color fg) = switch (status) {
      'Scheduling' => (Colors.blue.shade50, Colors.blue.shade700),
      'Locked' => (Colors.purple.shade50, Colors.purple.shade700),
      'InTransit' => (Colors.green.shade50, Colors.green.shade700),
      'Completed' => (Colors.grey.shade200, Colors.grey.shade700),
      'IncidentActive' => (Colors.orange.shade50, Colors.orange.shade800),
      _ => (Colors.grey.shade100, Colors.grey.shade600),
    };

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        status,
        style: GoogleFonts.hankenGrotesk(
          fontSize: 11,
          fontWeight: FontWeight.w600,
          color: fg,
        ),
      ),
    );
  }
}
