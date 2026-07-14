import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_admin/features/trips/providers/trips_providers.dart';

/// Trips management screen with filtering, detail expansion, and breakdown resolution.
class TripsScreen extends ConsumerStatefulWidget {
  const TripsScreen({super.key});

  @override
  ConsumerState<TripsScreen> createState() => _TripsScreenState();
}

class _TripsScreenState extends ConsumerState<TripsScreen> {
  static const Color _navy = Color(0xFF132E51);

  DateTime? _selectedDate;
  String? _selectedStatus;
  int? _expandedTripId;

  static const List<String> _statusOptions = [
    'Scheduling',
    'Locked',
    'InTransit',
    'Completed',
    'IncidentActive',
  ];

  String? get _formattedDate =>
      _selectedDate != null ? DateFormat('yyyy-MM-dd').format(_selectedDate!) : null;

  TripQueryParams get _queryParams =>
      TripQueryParams(date: _formattedDate, status: _selectedStatus);

  @override
  Widget build(BuildContext context) {
    final tripsAsync = ref.watch(tripsProvider(_queryParams));

    return Scaffold(
      backgroundColor: const Color(0xFFF5F5F5),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            _buildHeader(),
            const SizedBox(height: 20),
            _buildFilters(context),
            const SizedBox(height: 20),
            Expanded(child: _buildContent(tripsAsync)),
          ],
        ),
      ),
    );
  }

  Widget _buildHeader() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text(
          'Trips',
          style: GoogleFonts.hankenGrotesk(
            fontSize: 28,
            fontWeight: FontWeight.bold,
            color: _navy,
          ),
        ),
        const SizedBox(height: 4),
        Text(
          'View and manage generated trips, resolve breakdowns, and assign shuttles',
          style: GoogleFonts.hankenGrotesk(
            fontSize: 14,
            color: Colors.grey.shade600,
          ),
        ),
      ],
    );
  }

  Widget _buildFilters(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        children: [
          // Date picker
          _FilterChip(
            label: _selectedDate != null
                ? DateFormat('dd MMM yyyy').format(_selectedDate!)
                : 'Select Date',
            icon: Icons.calendar_today_outlined,
            onTap: () => _pickDate(context),
          ),
          const SizedBox(width: 12),
          // Status dropdown
          _buildStatusDropdown(),
          const SizedBox(width: 12),
          // Clear filters
          if (_selectedDate != null || _selectedStatus != null)
            TextButton.icon(
              onPressed: () {
                setState(() {
                  _selectedDate = null;
                  _selectedStatus = null;
                });
              },
              icon: const Icon(Icons.clear, size: 16),
              label: Text(
                'Clear',
                style: GoogleFonts.hankenGrotesk(fontSize: 13),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildStatusDropdown() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12),
      decoration: BoxDecoration(
        border: Border.all(color: _navy.withValues(alpha: 0.2)),
        borderRadius: BorderRadius.circular(8),
      ),
      child: DropdownButtonHideUnderline(
        child: DropdownButton<String?>(
          value: _selectedStatus,
          hint: Text(
            'All Statuses',
            style: GoogleFonts.hankenGrotesk(fontSize: 13, color: _navy),
          ),
          items: [
            DropdownMenuItem<String?>(
              value: null,
              child: Text(
                'All Statuses',
                style: GoogleFonts.hankenGrotesk(fontSize: 13, color: _navy),
              ),
            ),
            ..._statusOptions.map(
              (s) => DropdownMenuItem<String?>(
                value: s,
                child: Text(
                  s,
                  style: GoogleFonts.hankenGrotesk(fontSize: 13, color: _navy),
                ),
              ),
            ),
          ],
          onChanged: (value) => setState(() => _selectedStatus = value),
        ),
      ),
    );
  }

  Future<void> _pickDate(BuildContext context) async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate ?? DateTime.now(),
      firstDate: DateTime(2024),
      lastDate: DateTime(2030),
    );
    if (picked != null) {
      setState(() => _selectedDate = picked);
    }
  }

  Widget _buildContent(AsyncValue<List<TripInstance>> tripsAsync) {
    return tripsAsync.when(
      loading: () => const Center(
        child: CircularProgressIndicator(strokeWidth: 3),
      ),
      error: (error, _) => Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.error_outline, size: 48, color: Colors.red.shade300),
            const SizedBox(height: 12),
            Text(
              'Failed to load trips',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 14,
                color: Colors.grey.shade700,
              ),
            ),
            const SizedBox(height: 12),
            OutlinedButton.icon(
              onPressed: () => ref.invalidate(tripsProvider(_queryParams)),
              icon: const Icon(Icons.refresh),
              label: const Text('Retry'),
            ),
          ],
        ),
      ),
      data: (trips) => trips.isEmpty
          ? Center(
              child: Text(
                'No trips found for the selected filters',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 14,
                  color: Colors.grey.shade600,
                ),
              ),
            )
          : _buildTripTable(trips),
    );
  }

  Widget _buildTripTable(List<TripInstance> trips) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(12),
        child: SingleChildScrollView(
          child: Column(
            children: [
              // Table header
              Container(
                color: _navy.withValues(alpha: 0.05),
                padding: const EdgeInsets.symmetric(
                    vertical: 12, horizontal: 16),
                child: Row(
                  children: [
                    _headerCell('Trip Date', flex: 2),
                    _headerCell('Departure', flex: 2),
                    _headerCell('Direction', flex: 2),
                    _headerCell('Stop', flex: 3),
                    _headerCell('Campus', flex: 2),
                    _headerCell('Status', flex: 2),
                    _headerCell('Actions', flex: 2),
                  ],
                ),
              ),
              const Divider(height: 1),
              // Table rows
              ...trips.map((trip) => _buildTripRow(trip)),
            ],
          ),
        ),
      ),
    );
  }

  Widget _headerCell(String text, {int flex = 1}) {
    return Expanded(
      flex: flex,
      child: Text(
        text,
        style: GoogleFonts.hankenGrotesk(
          fontSize: 12,
          fontWeight: FontWeight.w600,
          color: _navy,
        ),
      ),
    );
  }

  Widget _buildTripRow(TripInstance trip) {
    final isExpanded = _expandedTripId == trip.id;

    return Column(
      children: [
        InkWell(
          onTap: () {
            setState(() {
              _expandedTripId = isExpanded ? null : trip.id;
            });
          },
          child: Container(
            padding: const EdgeInsets.symmetric(vertical: 12, horizontal: 16),
            decoration: BoxDecoration(
              color: isExpanded ? _navy.withValues(alpha: 0.02) : null,
              border: Border(
                bottom: BorderSide(color: Colors.grey.shade200),
              ),
            ),
            child: Row(
              children: [
                _dataCell(trip.tripDate, flex: 2),
                _dataCell(trip.departureTime, flex: 2),
                _dataCell(trip.direction, flex: 2),
                _dataCell(trip.stopName, flex: 3),
                _dataCell(trip.campusName, flex: 2),
                Expanded(
                  flex: 2,
                  child: _StatusChip(status: trip.status),
                ),
                Expanded(
                  flex: 2,
                  child: Row(
                    children: [
                      IconButton(
                        icon: Icon(
                          isExpanded
                              ? Icons.expand_less
                              : Icons.expand_more,
                          color: _navy,
                          size: 20,
                        ),
                        onPressed: () {
                          setState(() {
                            _expandedTripId = isExpanded ? null : trip.id;
                          });
                        },
                        tooltip: 'Details',
                      ),
                      IconButton(
                        icon: const Icon(
                          Icons.directions_bus_outlined,
                          color: _navy,
                          size: 20,
                        ),
                        onPressed: () => _showAssignShuttleDialog(trip),
                        tooltip: 'Assign Shuttle',
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ),
        ),
        if (isExpanded) _buildExpandedDetail(trip),
      ],
    );
  }

  Widget _dataCell(String text, {int flex = 1}) {
    return Expanded(
      flex: flex,
      child: Text(
        text,
        style: GoogleFonts.hankenGrotesk(fontSize: 13, color: _navy),
        overflow: TextOverflow.ellipsis,
      ),
    );
  }

  Widget _buildExpandedDetail(TripInstance trip) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: _navy.withValues(alpha: 0.02),
        border: Border(
          bottom: BorderSide(color: Colors.grey.shade200),
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Passenger count
          Row(
            children: [
              const Icon(Icons.people_outline, size: 18, color: _navy),
              const SizedBox(width: 8),
              Text(
                'Passengers: ${trip.passengerCount}',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 13,
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),

          // Convoy allocations
          if (trip.convoyAllocations.isNotEmpty) ...[
            Text(
              'Convoy Allocations',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 13,
                fontWeight: FontWeight.w600,
                color: _navy,
              ),
            ),
            const SizedBox(height: 8),
            _buildConvoyTable(trip.convoyAllocations),
            const SizedBox(height: 16),
          ],

          // Breakdown resolution (for IncidentActive trips)
          if (trip.status == 'IncidentActive' &&
              trip.breakdownLogs.isNotEmpty)
            _buildBreakdownResolution(trip),
        ],
      ),
    );
  }

  Widget _buildConvoyTable(List<ConvoyAllocation> allocations) {
    return Table(
      columnWidths: const {
        0: FlexColumnWidth(1),
        1: FlexColumnWidth(2),
        2: FlexColumnWidth(1.5),
        3: FlexColumnWidth(1.5),
        4: FlexColumnWidth(1.5),
      },
      border: TableBorder.all(color: Colors.grey.shade300, width: 0.5),
      children: [
        TableRow(
          decoration: BoxDecoration(color: _navy.withValues(alpha: 0.05)),
          children: [
            _tableHeader('Position'),
            _tableHeader('License Plate'),
            _tableHeader('Capacity'),
            _tableHeader('Accessible'),
            _tableHeader('Operational'),
          ],
        ),
        ...allocations.map((a) => TableRow(
              children: [
                _tableCell('${a.orderPosition}'),
                _tableCell(a.licensePlate),
                _tableCell('${a.capacity}'),
                _tableCell(a.isAccessibleVehicle ? 'Yes' : 'No'),
                _tableCellWidget(
                  Icon(
                    a.isOperational ? Icons.check_circle : Icons.cancel,
                    size: 16,
                    color: a.isOperational ? Colors.green : Colors.red,
                  ),
                ),
              ],
            )),
      ],
    );
  }

  Widget _tableHeader(String text) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 10),
      child: Text(
        text,
        style: GoogleFonts.hankenGrotesk(
          fontSize: 12,
          fontWeight: FontWeight.w600,
          color: _navy,
        ),
      ),
    );
  }

  Widget _tableCell(String text) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 10),
      child: Text(
        text,
        style: GoogleFonts.hankenGrotesk(fontSize: 12, color: _navy),
      ),
    );
  }

  Widget _tableCellWidget(Widget child) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6, horizontal: 10),
      child: child,
    );
  }

  Widget _buildBreakdownResolution(TripInstance trip) {
    final unresolvedLogs =
        trip.breakdownLogs.where((log) => !log.isResolved).toList();

    if (unresolvedLogs.isEmpty) return const SizedBox.shrink();

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        const Divider(),
        const SizedBox(height: 12),
        Row(
          children: [
            Icon(Icons.warning_amber_rounded,
                size: 18, color: Colors.orange.shade700),
            const SizedBox(width: 8),
            Text(
              'Breakdown Resolution Required',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 13,
                fontWeight: FontWeight.w600,
                color: Colors.orange.shade800,
              ),
            ),
          ],
        ),
        const SizedBox(height: 12),
        ...unresolvedLogs.map(
          (log) => _BreakdownResolutionCard(
            trip: trip,
            breakdownLog: log,
            onResolved: () =>
                ref.invalidate(tripsProvider(_queryParams)),
          ),
        ),
      ],
    );
  }

  void _showAssignShuttleDialog(TripInstance trip) {
    showDialog(
      context: context,
      builder: (ctx) => _AssignShuttleDialog(
        tripId: trip.id,
        onAssigned: () => ref.invalidate(tripsProvider(_queryParams)),
      ),
    );
  }
}

// ─── Status Chip Widget ──────────────────────────────────────────────────────

class _StatusChip extends StatelessWidget {
  const _StatusChip({required this.status});
  final String status;

  @override
  Widget build(BuildContext context) {
    final (Color bg, Color fg) = _getStatusColors(status);
    return Align(
      alignment: Alignment.centerLeft,
      child: Container(
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
      ),
    );
  }

  static (Color, Color) _getStatusColors(String status) {
    return switch (status) {
      'Scheduling' => (Colors.blue.shade50, Colors.blue.shade700),
      'Locked' => (Colors.purple.shade50, Colors.purple.shade700),
      'InTransit' => (Colors.green.shade50, Colors.green.shade700),
      'Completed' => (Colors.grey.shade200, Colors.grey.shade700),
      'IncidentActive' => (Colors.orange.shade50, Colors.orange.shade800),
      _ => (Colors.grey.shade100, Colors.grey.shade600),
    };
  }
}

// ─── Filter Chip Widget ──────────────────────────────────────────────────────

class _FilterChip extends StatelessWidget {
  const _FilterChip({
    required this.label,
    required this.icon,
    required this.onTap,
  });

  final String label;
  final IconData icon;
  final VoidCallback onTap;

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    return InkWell(
      onTap: onTap,
      borderRadius: BorderRadius.circular(8),
      child: Container(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        decoration: BoxDecoration(
          border: Border.all(color: _navy.withValues(alpha: 0.2)),
          borderRadius: BorderRadius.circular(8),
        ),
        child: Row(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(icon, size: 16, color: _navy),
            const SizedBox(width: 8),
            Text(
              label,
              style: GoogleFonts.hankenGrotesk(
                fontSize: 13,
                fontWeight: FontWeight.w500,
                color: _navy,
              ),
            ),
          ],
        ),
      ),
    );
  }
}

// ─── Assign Shuttle Dialog ───────────────────────────────────────────────────

class _AssignShuttleDialog extends ConsumerStatefulWidget {
  const _AssignShuttleDialog({
    required this.tripId,
    required this.onAssigned,
  });

  final int tripId;
  final VoidCallback onAssigned;

  @override
  ConsumerState<_AssignShuttleDialog> createState() =>
      _AssignShuttleDialogState();
}

class _AssignShuttleDialogState extends ConsumerState<_AssignShuttleDialog> {
  static const Color _navy = Color(0xFF132E51);

  int? _selectedShuttleId;
  bool _isSubmitting = false;

  @override
  Widget build(BuildContext context) {
    final shuttlesAsync = ref.watch(availableShuttlesProvider);

    return AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      title: Row(
        children: [
          const Icon(Icons.directions_bus, color: _navy),
          const SizedBox(width: 8),
          Text(
            'Assign Shuttle',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 18,
              fontWeight: FontWeight.bold,
              color: _navy,
            ),
          ),
        ],
      ),
      content: SizedBox(
        width: 400,
        child: shuttlesAsync.when(
          loading: () => const SizedBox(
            height: 100,
            child: Center(child: CircularProgressIndicator(strokeWidth: 2)),
          ),
          error: (_, __) => Text(
            'Failed to load shuttles',
            style: GoogleFonts.hankenGrotesk(color: Colors.red),
          ),
          data: (shuttles) => _buildShuttleList(shuttles),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text(
            'Cancel',
            style: GoogleFonts.hankenGrotesk(color: Colors.grey.shade700),
          ),
        ),
        FilledButton(
          onPressed:
              _selectedShuttleId != null && !_isSubmitting ? _submit : null,
          style: FilledButton.styleFrom(
            backgroundColor: _navy,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8),
            ),
          ),
          child: _isSubmitting
              ? const SizedBox(
                  width: 16,
                  height: 16,
                  child: CircularProgressIndicator(
                    strokeWidth: 2,
                    color: Colors.white,
                  ),
                )
              : Text(
                  'Assign',
                  style: GoogleFonts.hankenGrotesk(
                    fontWeight: FontWeight.w600,
                  ),
                ),
        ),
      ],
    );
  }

  Widget _buildShuttleList(List<ShuttleOption> shuttles) {
    if (shuttles.isEmpty) {
      return Text(
        'No shuttles available',
        style: GoogleFonts.hankenGrotesk(color: Colors.grey),
      );
    }
    return Column(
      mainAxisSize: MainAxisSize.min,
      children: shuttles.map((shuttle) {
        final isSelected = _selectedShuttleId == shuttle.id;
        return ListTile(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(8),
          ),
          selected: isSelected,
          selectedTileColor: _navy.withValues(alpha: 0.05),
          leading: Icon(
            shuttle.isAccessibleVehicle
                ? Icons.accessible
                : Icons.directions_bus_outlined,
            color: isSelected ? _navy : Colors.grey.shade600,
          ),
          title: Text(
            shuttle.licensePlate,
            style: GoogleFonts.hankenGrotesk(
              fontWeight: FontWeight.w600,
              color: _navy,
            ),
          ),
          subtitle: Text(
            'Capacity: ${shuttle.capacity}${shuttle.isAccessibleVehicle ? ' • Accessible' : ''}',
            style: GoogleFonts.hankenGrotesk(fontSize: 12),
          ),
          trailing: isSelected
              ? const Icon(Icons.check_circle, color: _navy)
              : null,
          onTap: () => setState(() => _selectedShuttleId = shuttle.id),
        );
      }).toList(),
    );
  }

  Future<void> _submit() async {
    if (_selectedShuttleId == null) return;
    setState(() => _isSubmitting = true);

    final success = await ref
        .read(tripActionsProvider.notifier)
        .assignShuttle(widget.tripId, _selectedShuttleId!);

    if (!mounted) return;
    setState(() => _isSubmitting = false);

    if (success) {
      widget.onAssigned();
      Navigator.of(context).pop();
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            'Shuttle assigned successfully',
            style: GoogleFonts.hankenGrotesk(),
          ),
          backgroundColor: Colors.green.shade600,
        ),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            'Failed to assign shuttle',
            style: GoogleFonts.hankenGrotesk(),
          ),
          backgroundColor: Colors.red.shade600,
        ),
      );
    }
  }
}

// ─── Breakdown Resolution Card ───────────────────────────────────────────────

class _BreakdownResolutionCard extends ConsumerStatefulWidget {
  const _BreakdownResolutionCard({
    required this.trip,
    required this.breakdownLog,
    required this.onResolved,
  });

  final TripInstance trip;
  final BreakdownLog breakdownLog;
  final VoidCallback onResolved;

  @override
  ConsumerState<_BreakdownResolutionCard> createState() =>
      _BreakdownResolutionCardState();
}

class _BreakdownResolutionCardState
    extends ConsumerState<_BreakdownResolutionCard> {
  static const Color _navy = Color(0xFF132E51);

  String _resolutionType = 'Replaced';
  int? _replacementShuttleId;
  bool _isSubmitting = false;

  @override
  Widget build(BuildContext context) {
    final shuttlesAsync = ref.watch(availableShuttlesProvider);

    return Container(
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(color: Colors.orange.shade200),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(Icons.build, size: 16, color: Colors.orange),
              const SizedBox(width: 8),
              Text(
                'Shuttle: ${widget.breakdownLog.licensePlate}',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 13,
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          // Resolution type selector
          Row(
            children: [
              Text(
                'Resolution:',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 12,
                  fontWeight: FontWeight.w500,
                  color: _navy,
                ),
              ),
              const SizedBox(width: 12),
              ChoiceChip(
                label: Text(
                  'Replace Shuttle',
                  style: GoogleFonts.hankenGrotesk(fontSize: 12),
                ),
                selected: _resolutionType == 'Replaced',
                onSelected: (_) =>
                    setState(() => _resolutionType = 'Replaced'),
                selectedColor: const Color(0xFFFFCC00).withValues(alpha: 0.3),
              ),
              const SizedBox(width: 8),
              ChoiceChip(
                label: Text(
                  'Cancel',
                  style: GoogleFonts.hankenGrotesk(fontSize: 12),
                ),
                selected: _resolutionType == 'Cancelled',
                onSelected: (_) =>
                    setState(() => _resolutionType = 'Cancelled'),
                selectedColor: Colors.red.shade100,
              ),
            ],
          ),
          const SizedBox(height: 12),

          // Replacement shuttle dropdown (only for Replace)
          if (_resolutionType == 'Replaced')
            shuttlesAsync.when(
              loading: () => const LinearProgressIndicator(),
              error: (_, __) => Text(
                'Could not load shuttles',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 12,
                  color: Colors.red,
                ),
              ),
              data: (shuttles) => _buildReplacementDropdown(shuttles),
            ),

          const SizedBox(height: 12),
          // Submit button
          Align(
            alignment: Alignment.centerRight,
            child: FilledButton.icon(
              onPressed: _canSubmit && !_isSubmitting ? _submitResolution : null,
              icon: _isSubmitting
                  ? const SizedBox(
                      width: 14,
                      height: 14,
                      child: CircularProgressIndicator(
                        strokeWidth: 2,
                        color: Colors.white,
                      ),
                    )
                  : const Icon(Icons.check, size: 16),
              label: Text(
                'Resolve',
                style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w600),
              ),
              style: FilledButton.styleFrom(
                backgroundColor: _navy,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
              ),
            ),
          ),
        ],
      ),
    );
  }

  bool get _canSubmit {
    if (_resolutionType == 'Replaced' && _replacementShuttleId == null) {
      return false;
    }
    return true;
  }

  Widget _buildReplacementDropdown(List<ShuttleOption> shuttles) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12),
      decoration: BoxDecoration(
        border: Border.all(color: _navy.withValues(alpha: 0.2)),
        borderRadius: BorderRadius.circular(8),
      ),
      child: DropdownButtonHideUnderline(
        child: DropdownButton<int?>(
          isExpanded: true,
          value: _replacementShuttleId,
          hint: Text(
            'Select replacement shuttle',
            style: GoogleFonts.hankenGrotesk(fontSize: 13, color: _navy),
          ),
          items: shuttles.map((s) {
            return DropdownMenuItem<int?>(
              value: s.id,
              child: Text(
                '${s.licensePlate} (Cap: ${s.capacity})${s.isAccessibleVehicle ? ' ♿' : ''}',
                style: GoogleFonts.hankenGrotesk(fontSize: 13, color: _navy),
              ),
            );
          }).toList(),
          onChanged: (value) =>
              setState(() => _replacementShuttleId = value),
        ),
      ),
    );
  }

  Future<void> _submitResolution() async {
    setState(() => _isSubmitting = true);

    final success = await ref
        .read(tripActionsProvider.notifier)
        .resolveBreakdown(
          tripId: widget.trip.id,
          breakdownLogId: widget.breakdownLog.id,
          resolutionType: _resolutionType,
          replacementShuttleId:
              _resolutionType == 'Replaced' ? _replacementShuttleId : null,
        );

    if (!mounted) return;
    setState(() => _isSubmitting = false);

    if (success) {
      widget.onResolved();
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            'Breakdown resolved successfully',
            style: GoogleFonts.hankenGrotesk(),
          ),
          backgroundColor: Colors.green.shade600,
        ),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            'Failed to resolve breakdown',
            style: GoogleFonts.hankenGrotesk(),
          ),
          backgroundColor: Colors.red.shade600,
        ),
      );
    }
  }
}
