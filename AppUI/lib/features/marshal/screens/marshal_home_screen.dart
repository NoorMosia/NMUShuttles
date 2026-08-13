import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/shared/widgets/marshal_drawer.dart';
import 'package:nmu_shuttle_app/features/marshal/models/marshal_time_slot_model.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/marshal_home_provider.dart';

/// Marshal Home screen displaying assigned location, next time slot,
/// remaining schedule (grouped by time), and action buttons.
class MarshalHomeScreen extends ConsumerWidget {
  const MarshalHomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(marshalHomeProvider);

    return Scaffold(
      drawer: MarshalDrawer(
        onLogout: () => context.go('/login'),
      ),
      body: Builder(
        builder: (scaffoldContext) => Stack(
          children: [
            RefreshIndicator(
              onRefresh: () =>
                  ref.read(marshalHomeProvider.notifier).refresh(),
              child: _buildBody(scaffoldContext, state),
            ),
            // Floating hamburger + Dashboard label
            Positioned(
              top: MediaQuery.of(scaffoldContext).padding.top + 12,
              left: 16,
              child: Row(
                children: [
                  Material(
                    color: Colors.white,
                    elevation: 0,
                    borderRadius: BorderRadius.circular(8),
                    child: InkWell(
                      borderRadius: BorderRadius.circular(8),
                      onTap: () => Scaffold.of(scaffoldContext).openDrawer(),
                      child: const Padding(
                        padding: EdgeInsets.all(10),
                        child: Icon(
                          Icons.menu,
                          color: AppTheme.navy,
                          size: 24,
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(width: 6),
                  Material(
                    color: Colors.white,
                    elevation: 0,
                    borderRadius: BorderRadius.circular(8),
                    child: const Padding(
                      padding: EdgeInsets.symmetric(horizontal: 10, vertical: 10),
                      child: Text(
                        'Dashboard',
                        style: TextStyle(
                          color: AppTheme.navy,
                          fontWeight: FontWeight.w600,
                          fontSize: 18,
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildBody(BuildContext context, MarshalHomeState state) {
    if (state.isLoading && state.timeSlots.isEmpty) {
      return const Center(
        child: CircularProgressIndicator(color: AppTheme.primaryGold),
      );
    }

    if (state.error != null && state.timeSlots.isEmpty) {
      return ListView(
        children: [
          SizedBox(height: MediaQuery.of(context).size.height * 0.35),
          Center(
            child: Column(
              children: [
                const Icon(Icons.error_outline,
                    size: 48, color: AppTheme.navy),
                const SizedBox(height: 12),
                Text(
                  state.error!,
                  style: const TextStyle(color: AppTheme.navy, fontSize: 16),
                  textAlign: TextAlign.center,
                ),
              ],
            ),
          ),
        ],
      );
    }

    return ListView(
      padding: EdgeInsets.only(
        top: MediaQuery.of(context).padding.top + 64,
        left: 20,
        right: 20,
        bottom: 100,
      ),
      children: [
        // --- Assigned Location Header ---
        _buildLocationHeader(context, state),
        const SizedBox(height: 12),

        // --- Get Directions ---
        _buildDirectionsButton(state),
        const SizedBox(height: 24),

        // --- Next Time Slot Card ---
        if (state.nextTimeSlot != null)
          _buildNextTimeSlotCard(context, state.nextTimeSlot!),
        if (state.nextTimeSlot == null) _buildNoTripsCard(context),
        const SizedBox(height: 24),

        // --- Remaining Schedule ---
        if (state.remainingSlots.isNotEmpty) ...[
          const Text(
            'Remaining Schedule',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w600,
              color: AppTheme.navy,
            ),
          ),
          const SizedBox(height: 12),
          ...state.remainingSlots
              .map((slot) => _buildScheduleItem(context, slot)),
        ],

        const SizedBox(height: 24),
      ],
    );
  }

  Widget _buildLocationHeader(BuildContext context, MarshalHomeState state) {
    final locationName = state.assignment?.locationName ?? 'Loading...';
    final locationType = state.assignment?.isCampusAssigned == true
        ? 'Campus Station — From Campus'
        : 'Stop Station — To Campus';

    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppTheme.navy,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Row(
        children: [
          Container(
            padding: const EdgeInsets.all(10),
            decoration: BoxDecoration(
              color: AppTheme.primaryGold,
              borderRadius: BorderRadius.circular(8),
            ),
            child: const Icon(
              Icons.location_on,
              color: AppTheme.navy,
              size: 24,
            ),
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  locationType,
                  style: const TextStyle(
                    color: Colors.white70,
                    fontSize: 13,
                  ),
                ),
                const SizedBox(height: 4),
                Text(
                  locationName,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 18,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildNextTimeSlotCard(
      BuildContext context, MarshalTimeSlotModel slot) {
    // Show date if not today
    final isToday = _isToday(slot.tripDate);
    final dateLabel = isToday ? 'Today' : slot.tripDate;

    return GestureDetector(
      onTap: () => _navigateToCheckIn(context, slot),
      child: Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          color: AppTheme.primaryGold,
          borderRadius: BorderRadius.circular(16),
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Text(
                      'Next Boarding',
                      style: TextStyle(
                        color: AppTheme.navy,
                        fontSize: 14,
                        fontWeight: FontWeight.w600,
                      ),
                    ),
                    if (!isToday) ...[
                      const SizedBox(width: 8),
                      Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 8, vertical: 3),
                        decoration: BoxDecoration(
                          color: AppTheme.navy.withValues(alpha: 0.15),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(
                          dateLabel,
                          style: const TextStyle(
                            color: AppTheme.navy,
                            fontSize: 11,
                            fontWeight: FontWeight.w600,
                          ),
                        ),
                      ),
                    ],
                  ],
                ),
                Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    if (slot.isScanningOpen)
                      Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 8, vertical: 3),
                        decoration: BoxDecoration(
                          color: Colors.green.shade700,
                          borderRadius: BorderRadius.circular(10),
                        ),
                        child: const Text(
                          'SCAN OPEN',
                          style: TextStyle(
                            color: Colors.white,
                            fontSize: 10,
                            fontWeight: FontWeight.w700,
                          ),
                        ),
                      ),
                    const SizedBox(width: 6),
                    _buildStatusChip(slot.status),
                  ],
                ),
              ],
            ),
            const SizedBox(height: 12),
            Text(
              slot.displayTime,
              style: const TextStyle(
                color: AppTheme.navy,
                fontSize: 36,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 8),
            Row(
              children: [
                _buildDirectionChip(slot.direction),
                const Spacer(),
                Icon(Icons.people, color: AppTheme.navy.withValues(alpha: 0.7), size: 18),
                const SizedBox(width: 4),
                Text(
                  '${slot.boardedCount}/${slot.confirmedCount}',
                  style: TextStyle(
                    color: AppTheme.navy.withValues(alpha: 0.8),
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const SizedBox(width: 12),
                Icon(Icons.directions_bus, color: AppTheme.navy.withValues(alpha: 0.7), size: 18),
                const SizedBox(width: 4),
                Text(
                  '${slot.stopCount} stops',
                  style: TextStyle(
                    color: AppTheme.navy.withValues(alpha: 0.8),
                    fontSize: 14,
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildNoTripsCard(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.grey.shade100,
        borderRadius: BorderRadius.circular(16),
      ),
      child: const Column(
        children: [
          Icon(Icons.check_circle_outline, color: AppTheme.navy, size: 48),
          SizedBox(height: 12),
          Text(
            'No upcoming trips',
            style: TextStyle(
              color: AppTheme.navy,
              fontSize: 16,
              fontWeight: FontWeight.w600,
            ),
          ),
          SizedBox(height: 4),
          Text(
            'All trips for today are completed.',
            style: TextStyle(color: Colors.grey, fontSize: 14),
          ),
        ],
      ),
    );
  }

  Widget _buildScheduleItem(BuildContext context, MarshalTimeSlotModel slot) {
    final isToday = _isToday(slot.tripDate);

    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: InkWell(
        borderRadius: BorderRadius.circular(10),
        onTap: () => _navigateToCheckIn(context, slot),
        child: Container(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
          decoration: BoxDecoration(
            color: Colors.grey.shade50,
            borderRadius: BorderRadius.circular(10),
            border: Border.all(color: Colors.grey.shade200),
          ),
          child: Row(
            children: [
              Container(
                width: 4,
                height: 44,
                decoration: BoxDecoration(
                  color: slot.direction == 'FromCampus'
                      ? AppTheme.primaryGold
                      : AppTheme.navy,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              const SizedBox(width: 14),
              // Time and direction
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Text(
                          slot.displayTime,
                          style: const TextStyle(
                            color: AppTheme.navy,
                            fontSize: 18,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                        if (!isToday) ...[
                          const SizedBox(width: 8),
                          Text(
                            slot.tripDate,
                            style: TextStyle(
                              color: Colors.grey.shade500,
                              fontSize: 12,
                            ),
                          ),
                        ],
                      ],
                    ),
                    const SizedBox(height: 2),
                    Text(
                      slot.directionLabel,
                      style: TextStyle(
                        color: Colors.grey.shade600,
                        fontSize: 12,
                      ),
                    ),
                  ],
                ),
              ),
              // Passenger count
              Text(
                '${slot.boardedCount}/${slot.confirmedCount}',
                style: TextStyle(
                  color: Colors.grey.shade700,
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                ),
              ),
              const SizedBox(width: 12),
              _buildStatusChip(slot.status),
              const SizedBox(width: 8),
              const Icon(Icons.chevron_right, color: Colors.grey),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildDirectionChip(String direction) {
    final isFrom = direction == 'FromCampus';
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: AppTheme.navy.withValues(alpha: 0.15),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(
            isFrom ? Icons.arrow_forward : Icons.arrow_back,
            size: 14,
            color: AppTheme.navy,
          ),
          const SizedBox(width: 4),
          Text(
            isFrom ? 'From Campus' : 'To Campus',
            style: const TextStyle(
              color: AppTheme.navy,
              fontSize: 12,
              fontWeight: FontWeight.w600,
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildStatusChip(String status) {
    Color backgroundColor;
    Color textColor;

    switch (status) {
      case 'Locked':
        backgroundColor = Colors.orange.shade100;
        textColor = Colors.orange.shade800;
        break;
      case 'In Transit':
        backgroundColor = Colors.blue.shade100;
        textColor = Colors.blue.shade800;
        break;
      case 'Incident Active':
        backgroundColor = Colors.red.shade100;
        textColor = Colors.red.shade800;
        break;
      case 'Completed':
        backgroundColor = Colors.green.shade100;
        textColor = Colors.green.shade800;
        break;
      default: // Scheduling
        backgroundColor = Colors.grey.shade100;
        textColor = Colors.grey.shade700;
    }

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: backgroundColor,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        status,
        style: TextStyle(
          color: textColor,
          fontSize: 12,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }

  Widget _buildDirectionsButton(MarshalHomeState state) {
    return SizedBox(
      width: double.infinity,
      height: 44,
      child: OutlinedButton.icon(
        onPressed: () => _launchDirections(state),
        style: OutlinedButton.styleFrom(
          foregroundColor: AppTheme.navy,
          side: const BorderSide(color: AppTheme.navy, width: 1.5),
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(14),
          ),
        ),
        icon: const Icon(Icons.directions, size: 20),
        label: const Text(
          'Get Directions',
          style: TextStyle(
            fontSize: 14,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),
    );
  }

  /// Navigates to the check-in screen for a time slot.
  void _navigateToCheckIn(BuildContext context, MarshalTimeSlotModel slot) {
    // Pass the time slot info as query parameters so the check-in screen
    // can load the passenger list for this slot.
    context.push(
      '/marshal/checkin'
      '?departureTime=${slot.departureTime}'
      '&direction=${slot.direction}'
      '&tripDate=${slot.tripDate}',
    );
  }

  /// Checks if the trip date string represents today.
  bool _isToday(String tripDate) {
    try {
      final date = DateTime.parse(tripDate);
      final now = DateTime.now();
      return date.year == now.year &&
          date.month == now.month &&
          date.day == now.day;
    } catch (_) {
      return true; // Assume today if parsing fails
    }
  }

  /// Launches Google Maps navigation to the marshal's assigned location.
  Future<void> _launchDirections(MarshalHomeState state) async {
    final gps = state.assignment?.gpsCoordinates;
    if (gps == null || gps.isEmpty) return;

    final uri = Uri.parse(
        'https://www.google.com/maps/dir/?api=1&destination=$gps&travelmode=driving');

    if (await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    }
  }
}
