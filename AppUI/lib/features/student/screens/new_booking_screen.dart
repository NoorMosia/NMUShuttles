import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/student/models/trip_model.dart';
import 'package:nmu_shuttle_app/features/student/providers/booking_providers.dart';
import 'package:nmu_shuttle_app/features/student/providers/trip_providers.dart';
import 'package:nmu_shuttle_app/shared/widgets/app_drawer.dart';
import 'package:nmu_shuttle_app/shared/widgets/back_button.dart';

/// New booking screen with direction, stop search, and capacity display.
class NewBookingScreen extends ConsumerStatefulWidget {
  const NewBookingScreen({super.key});

  @override
  ConsumerState<NewBookingScreen> createState() => _NewBookingScreenState();
}

class _NewBookingScreenState extends ConsumerState<NewBookingScreen> {
  String _direction = 'ToCampus';
  DateTime _selectedDate = DateTime.now();
  StopModel? _selectedStop;
  String _searchQuery = '';
  bool _isBooking = false;

  final TextEditingController _searchController = TextEditingController();

  /// Available dates: today through today + 3.
  List<DateTime> get _availableDates {
    final today = DateTime.now();
    return List.generate(
      4,
      (i) => DateTime(today.year, today.month, today.day + i),
    );
  }

  String _formatDateChip(DateTime date) {
    final now = DateTime.now();
    final today = DateTime(now.year, now.month, now.day);
    final diff = date.difference(today).inDays;
    if (diff == 0) return 'Today';
    if (diff == 1) return 'Tomorrow';
    return DateFormat('EEE d').format(date);
  }

  TripQuery? get _currentQuery {
    if (_selectedStop == null) return null;
    return TripQuery(
      date: DateFormat('yyyy-MM-dd').format(_selectedDate),
      stopId: _selectedStop!.stopId,
      direction: _direction,
    );
  }

  Future<void> _confirmBooking(TripModel trip) async {
    setState(() => _isBooking = true);
    try {
      final repository = ref.read(bookingRepositoryProvider);
      final booking = await repository.createBooking(trip.tripInstanceId);
      // Invalidate bookings cache so home screen refreshes
      ref.invalidate(bookingsProvider);
      if (mounted) {
        context.go('/student/booking/${booking.bookingId}');
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Booking failed: ${e.toString()}'),
            backgroundColor: Colors.red,
          ),
        );
      }
    } finally {
      if (mounted) setState(() => _isBooking = false);
    }
  }

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final stopsAsync = ref.watch(stopsProvider);

    return Scaffold(
      drawer: AppDrawer(
        onLogout: () => context.go('/login'),
      ),
      body: Stack(
        children: [
          SafeArea(
            child: Column(
              children: [
                const SizedBox(height: 60),
                // Title
                Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16),
                  child: Align(
                    alignment: Alignment.centerLeft,
                    child: Text(
                      'New Booking',
                      style:
                          Theme.of(context).textTheme.headlineSmall?.copyWith(
                                fontWeight: FontWeight.bold,
                                color: AppTheme.navy,
                              ),
                    ),
                  ),
                ),
                const SizedBox(height: 20),
                Expanded(
                  child: SingleChildScrollView(
                    padding: const EdgeInsets.symmetric(horizontal: 16),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        // Direction selector
                        _buildSectionLabel('Direction'),
                        const SizedBox(height: 8),
                        SizedBox(
                          width: double.infinity,
                          child: SegmentedButton<String>(
                            segments: const [
                              ButtonSegment(
                                value: 'ToCampus',
                                label: Text('To Campus'),
                                icon: Icon(Icons.arrow_forward),
                              ),
                              ButtonSegment(
                                value: 'FromCampus',
                                label: Text('From Campus'),
                                icon: Icon(Icons.arrow_back),
                              ),
                            ],
                            selected: {_direction},
                            onSelectionChanged: (selection) {
                              setState(() => _direction = selection.first);
                            },
                            style: SegmentedButton.styleFrom(
                              selectedBackgroundColor: AppTheme.primaryGold,
                              selectedForegroundColor: AppTheme.navy,
                            ),
                          ),
                        ),
                        const SizedBox(height: 20),

                        // Stop search
                        _buildSectionLabel('Stop'),
                        const SizedBox(height: 8),
                        _buildStopSearch(stopsAsync),
                        const SizedBox(height: 20),

                        // Date picker (4-day horizontal chips)
                        _buildSectionLabel('Date'),
                        const SizedBox(height: 8),
                        _buildDateChips(),
                        const SizedBox(height: 20),

                        // Departure time list
                        _buildSectionLabel('Available Trips'),
                        const SizedBox(height: 8),
                        _buildTripsList(),
                        const SizedBox(height: 80),
                      ],
                    ),
                  ),
                ),
              ],
            ),
          ),
          const FloatingBackButton(),
        ],
      ),
    );
  }

  Widget _buildSectionLabel(String label) {
    return Text(
      label,
      style: Theme.of(context).textTheme.titleSmall?.copyWith(
            fontWeight: FontWeight.w600,
            color: AppTheme.navy,
          ),
    );
  }

  Widget _buildStopSearch(AsyncValue<List<StopModel>> stopsAsync) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        TextField(
          controller: _searchController,
          decoration: InputDecoration(
            hintText: 'Search by stop name or address...',
            prefixIcon: const Icon(Icons.search),
            border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12),
            ),
            contentPadding:
                const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
          ),
          onChanged: (value) => setState(() => _searchQuery = value),
        ),
        if (_searchQuery.isNotEmpty)
          stopsAsync.when(
            loading: () => const Padding(
              padding: EdgeInsets.all(8),
              child: Center(child: CircularProgressIndicator()),
            ),
            error: (_, __) => const Padding(
              padding: EdgeInsets.all(8),
              child: Text('Failed to load stops'),
            ),
            data: (stops) {
              final filtered = stops.where((stop) {
                final query = _searchQuery.toLowerCase();
                return stop.stopName.toLowerCase().contains(query) ||
                    stop.fullAddress.toLowerCase().contains(query);
              }).toList();

              if (filtered.isEmpty) {
                return const Padding(
                  padding: EdgeInsets.all(8),
                  child: Text('No stops found'),
                );
              }

              return Container(
                margin: const EdgeInsets.only(top: 4),
                constraints: const BoxConstraints(maxHeight: 200),
                decoration: BoxDecoration(
                  border: Border.all(color: Colors.grey.shade300),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: Material(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(12),
                  clipBehavior: Clip.antiAlias,
                  child: ListView.separated(
                  shrinkWrap: true,
                  padding: EdgeInsets.zero,
                  itemCount: filtered.length,
                  separatorBuilder: (_, __) =>
                      Divider(height: 1, color: Colors.grey.shade200),
                  itemBuilder: (context, index) {
                    final stop = filtered[index];
                    final isSelected =
                        _selectedStop?.stopId == stop.stopId;
                    return ListTile(
                      dense: true,
                      selected: isSelected,
                      selectedTileColor: AppTheme.primaryGold.withOpacity(0.1),
                      leading: const Icon(Icons.location_on_outlined, size: 20),
                      title: Text(stop.stopName,
                          style: const TextStyle(fontSize: 14)),
                      subtitle: Text(stop.fullAddress,
                          style: TextStyle(
                              fontSize: 12, color: Colors.grey.shade600)),
                      onTap: () {
                        setState(() {
                          _selectedStop = stop;
                          _searchController.text = stop.stopName;
                          _searchQuery = '';
                        });
                      },
                    );
                  },
                ),
                ),
              );
            },
          ),
        if (_selectedStop != null && _searchQuery.isEmpty)
          Padding(
            padding: const EdgeInsets.only(top: 8),
            child: Chip(
              avatar: const Icon(Icons.location_on, size: 18),
              label: Text(_selectedStop!.stopName),
              onDeleted: () => setState(() {
                _selectedStop = null;
                _searchController.clear();
              }),
            ),
          ),
      ],
    );
  }

  Widget _buildDateChips() {
    return SizedBox(
      height: 44,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        itemCount: _availableDates.length,
        separatorBuilder: (_, __) => const SizedBox(width: 8),
        itemBuilder: (context, index) {
          final date = _availableDates[index];
          final isSelected = _selectedDate.year == date.year &&
              _selectedDate.month == date.month &&
              _selectedDate.day == date.day;
          return ChoiceChip(
            label: Text(_formatDateChip(date)),
            selected: isSelected,
            selectedColor: AppTheme.primaryGold,
            labelStyle: TextStyle(
              color: isSelected ? AppTheme.navy : Colors.grey.shade700,
              fontWeight: isSelected ? FontWeight.w600 : FontWeight.normal,
            ),
            onSelected: (_) => setState(() => _selectedDate = date),
          );
        },
      ),
    );
  }

  Widget _buildTripsList() {
    final query = _currentQuery;
    if (query == null) {
      return Container(
        padding: const EdgeInsets.all(24),
        alignment: Alignment.center,
        child: Text(
          'Select a stop to see available trips',
          style: TextStyle(color: Colors.grey.shade500),
        ),
      );
    }

    final tripsAsync = ref.watch(tripAvailabilityProvider(query));

    return tripsAsync.when(
      loading: () => const Padding(
        padding: EdgeInsets.all(24),
        child: Center(child: CircularProgressIndicator()),
      ),
      error: (error, _) => Padding(
        padding: const EdgeInsets.all(24),
        child: Center(
          child: Column(
            children: [
              const Icon(Icons.error_outline, color: Colors.red),
              const SizedBox(height: 8),
              Text('Failed to load trips: $error'),
            ],
          ),
        ),
      ),
      data: (trips) {
        // Filter out trips whose departure time has already passed (today only)
        final now = DateTime.now();
        final isToday = _selectedDate.year == now.year &&
            _selectedDate.month == now.month &&
            _selectedDate.day == now.day;

        final visibleTrips = isToday
            ? trips.where((trip) {
                final parts = trip.departureTime.split(':');
                if (parts.length < 2) return true;
                final tripHour = int.tryParse(parts[0]) ?? 0;
                final tripMinute = int.tryParse(parts[1]) ?? 0;
                return tripHour > now.hour ||
                    (tripHour == now.hour && tripMinute > now.minute);
              }).toList()
            : trips;

        if (visibleTrips.isEmpty) {
          return Container(
            padding: const EdgeInsets.all(24),
            alignment: Alignment.center,
            child: Column(
              children: [
                Icon(Icons.directions_bus_outlined,
                    size: 48, color: Colors.grey.shade400),
                const SizedBox(height: 8),
                Text(
                  'No trips available for this selection',
                  style: TextStyle(color: Colors.grey.shade600),
                ),
              ],
            ),
          );
        }

        return ListView.builder(
          shrinkWrap: true,
          physics: const NeverScrollableScrollPhysics(),
          itemCount: visibleTrips.length,
          itemBuilder: (context, index) =>
              _TripCard(
                trip: visibleTrips[index],
                isBooking: _isBooking,
                onConfirm: () => _confirmBooking(visibleTrips[index]),
              ),
        );
      },
    );
  }
}

/// Card displaying a single trip with departure time, capacity, and action button.
class _TripCard extends StatelessWidget {
  const _TripCard({
    required this.trip,
    required this.isBooking,
    required this.onConfirm,
  });

  final TripModel trip;
  final bool isBooking;
  final VoidCallback onConfirm;

  @override
  Widget build(BuildContext context) {
    final hasSeats = trip.availableSeats > 0;

    return Card(
      margin: const EdgeInsets.only(bottom: 12),
      elevation: 0,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Row(
          children: [
            // Departure time
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
              decoration: BoxDecoration(
                color: AppTheme.navy.withOpacity(0.1),
                borderRadius: BorderRadius.circular(8),
              ),
              child: Column(
                children: [
                  const Icon(Icons.access_time, size: 18, color: AppTheme.navy),
                  const SizedBox(height: 4),
                  Text(
                    trip.departureTime,
                    style: const TextStyle(
                      fontWeight: FontWeight.bold,
                      color: AppTheme.navy,
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(width: 16),
            // Trip info
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    trip.campusName,
                    style: Theme.of(context).textTheme.titleSmall?.copyWith(
                          fontWeight: FontWeight.w600,
                        ),
                  ),
                  const SizedBox(height: 4),
                  Row(
                    children: [
                      Icon(
                        Icons.event_seat,
                        size: 16,
                        color: hasSeats
                            ? Colors.green.shade600
                            : Colors.red.shade600,
                      ),
                      const SizedBox(width: 4),
                      Text(
                        hasSeats
                            ? '${trip.availableSeats} seats available'
                            : 'Full — waitlist available',
                        style: TextStyle(
                          fontSize: 13,
                          color: hasSeats
                              ? Colors.green.shade600
                              : Colors.red.shade600,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
            // Action button
            SizedBox(
              height: 36,
              child: ElevatedButton(
                onPressed: isBooking ? null : onConfirm,
                style: ElevatedButton.styleFrom(
                  backgroundColor:
                      hasSeats ? AppTheme.primaryGold : Colors.orange.shade100,
                  foregroundColor:
                      hasSeats ? AppTheme.navy : Colors.orange.shade800,
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                  padding: const EdgeInsets.symmetric(horizontal: 12),
                ),
                child: Text(
                  hasSeats ? 'Confirm' : 'Waitlist',
                  style: const TextStyle(fontSize: 13, fontWeight: FontWeight.w600),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
