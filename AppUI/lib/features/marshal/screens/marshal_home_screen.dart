import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:url_launcher/url_launcher.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/shared/widgets/app_drawer.dart';
import 'package:nmu_shuttle_app/shared/widgets/hamburger_menu_button.dart';
import 'package:nmu_shuttle_app/features/marshal/models/marshal_trip_model.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/marshal_home_provider.dart';

/// Marshal Home screen displaying assigned location, next boarding trip,
/// remaining schedule, QR scanner button, and directions button.
class MarshalHomeScreen extends ConsumerWidget {
  const MarshalHomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(marshalHomeProvider);

    return Scaffold(
      drawer: AppDrawer(
        onLogout: () => context.go('/login'),
      ),
      body: Stack(
        children: [
          RefreshIndicator(
            onRefresh: () =>
                ref.read(marshalHomeProvider.notifier).refresh(),
            child: _buildBody(context, state),
          ),
          const HamburgerMenuButton(),
        ],
      ),
    );
  }

  Widget _buildBody(BuildContext context, MarshalHomeState state) {
    if (state.isLoading && state.trips.isEmpty) {
      return const Center(
        child: CircularProgressIndicator(color: AppTheme.primaryGold),
      );
    }

    if (state.error != null && state.trips.isEmpty) {
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
        const SizedBox(height: 24),

        // --- Next Boarding Card ---
        if (state.nextBoardingTrip != null)
          _buildNextBoardingCard(context, state.nextBoardingTrip!),
        if (state.nextBoardingTrip == null) _buildNoTripsCard(context),
        const SizedBox(height: 24),

        // --- Remaining Schedule ---
        if (state.remainingTrips.isNotEmpty) ...[
          const Text(
            'Remaining Schedule',
            style: TextStyle(
              fontSize: 18,
              fontWeight: FontWeight.w600,
              color: AppTheme.navy,
            ),
          ),
          const SizedBox(height: 12),
          ...state.remainingTrips
              .map((trip) => _buildScheduleItem(context, trip)),
        ],

        const SizedBox(height: 24),

        // --- Action Buttons ---
        _buildActionButtons(context, state),
      ],
    );
  }

  Widget _buildLocationHeader(BuildContext context, MarshalHomeState state) {
    final locationName = state.assignment?.locationName ?? 'Loading...';
    final direction = state.assignment?.direction ?? '';
    final directionLabel = direction == 'FromCampus'
        ? 'From Campus'
        : direction == 'ToCampus'
            ? 'To Campus'
            : '';

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
                const Text(
                  'Your Station',
                  style: TextStyle(
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
                if (directionLabel.isNotEmpty)
                  Text(
                    directionLabel,
                    style: const TextStyle(
                      color: AppTheme.primaryGold,
                      fontSize: 13,
                      fontWeight: FontWeight.w500,
                    ),
                  ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildNextBoardingCard(
      BuildContext context, MarshalTripModel trip) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: AppTheme.primaryGold,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: AppTheme.primaryGold.withValues(alpha: 0.3),
            blurRadius: 12,
            offset: const Offset(0, 4),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'Next Boarding',
                style: TextStyle(
                  color: AppTheme.navy,
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                ),
              ),
              _buildStatusChip(trip.status),
            ],
          ),
          const SizedBox(height: 12),
          Text(
            trip.departureTime,
            style: const TextStyle(
              color: AppTheme.navy,
              fontSize: 36,
              fontWeight: FontWeight.bold,
            ),
          ),
          const SizedBox(height: 8),
          Row(
            children: [
              const Icon(Icons.directions_bus, color: AppTheme.navy, size: 18),
              const SizedBox(width: 6),
              Text(
                '${trip.stopName} → ${trip.campusName}',
                style: const TextStyle(
                  color: AppTheme.navy,
                  fontSize: 14,
                ),
              ),
            ],
          ),
        ],
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

  Widget _buildScheduleItem(BuildContext context, MarshalTripModel trip) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 8),
      child: InkWell(
        borderRadius: BorderRadius.circular(10),
        onTap: () => context.push('/marshal/passengers/${trip.tripInstanceId}'),
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
                height: 36,
                decoration: BoxDecoration(
                  color: AppTheme.primaryGold,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
              const SizedBox(width: 14),
              Text(
                trip.departureTime,
                style: const TextStyle(
                  color: AppTheme.navy,
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                ),
              ),
              const Spacer(),
              _buildStatusChip(trip.status),
              const SizedBox(width: 8),
              const Icon(Icons.chevron_right, color: Colors.grey),
            ],
          ),
        ),
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
      case 'InTransit':
        backgroundColor = Colors.blue.shade100;
        textColor = Colors.blue.shade800;
        break;
      case 'IncidentActive':
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
        _formatStatus(status),
        style: TextStyle(
          color: textColor,
          fontSize: 12,
          fontWeight: FontWeight.w600,
        ),
      ),
    );
  }

  String _formatStatus(String status) {
    switch (status) {
      case 'InTransit':
        return 'In Transit';
      case 'IncidentActive':
        return 'Incident';
      default:
        return status;
    }
  }

  Widget _buildActionButtons(BuildContext context, MarshalHomeState state) {
    return Column(
      children: [
        // QR Scanner Button — prominent
        SizedBox(
          width: double.infinity,
          height: 56,
          child: ElevatedButton.icon(
            onPressed: () {
              // Navigate to scan interface (passenger list with scan)
              final nextTrip = state.nextBoardingTrip;
              if (nextTrip != null) {
                context
                    .push('/marshal/passengers/${nextTrip.tripInstanceId}');
              }
            },
            style: ElevatedButton.styleFrom(
              backgroundColor: AppTheme.primaryGold,
              foregroundColor: AppTheme.navy,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(14),
              ),
              elevation: 2,
            ),
            icon: const Icon(Icons.qr_code_scanner, size: 28),
            label: const Text(
              'Scan QR Code',
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
          ),
        ),
        const SizedBox(height: 12),

        // Directions Button
        SizedBox(
          width: double.infinity,
          height: 48,
          child: OutlinedButton.icon(
            onPressed: () => _launchDirections(state),
            style: OutlinedButton.styleFrom(
              foregroundColor: AppTheme.navy,
              side: const BorderSide(color: AppTheme.navy, width: 1.5),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(14),
              ),
            ),
            icon: const Icon(Icons.directions, size: 22),
            label: const Text(
              'Get Directions',
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ),
      ],
    );
  }

  /// Launches Google Maps navigation to the marshal's assigned location.
  Future<void> _launchDirections(MarshalHomeState state) async {
    final gps = state.assignment?.gpsCoordinates;
    if (gps == null || gps.isEmpty) return;

    // GPS coordinates expected as "lat,lng" format
    final uri = Uri.parse(
        'https://www.google.com/maps/dir/?api=1&destination=$gps&travelmode=driving');

    if (await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    }
  }
}
