import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/features/trips/providers/trips_providers.dart';
import 'package:nmu_shuttle_admin/features/trips/providers/trip_notes_provider.dart';

/// Detail screen for a single Trip, showing metadata, campus route,
/// convoy allocations, and trip notes.
class TripDetailScreen extends ConsumerStatefulWidget {
  const TripDetailScreen({super.key, required this.tripId});

  final int tripId;

  @override
  ConsumerState<TripDetailScreen> createState() => _TripDetailScreenState();
}

class _TripDetailScreenState extends ConsumerState<TripDetailScreen> {
  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    final tripsAsync = ref.watch(tripsProvider(const TripQueryParams()));

    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Back navigation + title
          Row(
            children: [
              IconButton(
                onPressed: () => context.go('/trips'),
                icon: const Icon(Icons.arrow_back),
                tooltip: 'Back to Trips',
                color: _navy,
              ),
              const SizedBox(width: 8),
              Text(
                'Trip Details',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 22,
                  fontWeight: FontWeight.bold,
                  color: _navy,
                ),
              ),
            ],
          ),
          const SizedBox(height: 20),
          Expanded(
            child: tripsAsync.when(
              loading: () => const Center(
                child: CircularProgressIndicator(strokeWidth: 3),
              ),
              error: (error, _) => Center(
                child: Text(
                  'Failed to load trip details',
                  style: GoogleFonts.hankenGrotesk(color: Colors.red),
                ),
              ),
              data: (trips) {
                final trip = trips.cast<TripInstance?>().firstWhere(
                      (t) => t?.id == widget.tripId,
                      orElse: () => null,
                    );
                if (trip == null) {
                  return Center(
                    child: Text(
                      'Trip not found',
                      style: GoogleFonts.hankenGrotesk(color: Colors.grey),
                    ),
                  );
                }
                return _buildTripContent(trip);
              },
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildTripContent(TripInstance trip) {
    return SingleChildScrollView(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Trip metadata card
          _buildMetadataCard(trip),
          const SizedBox(height: 24),
          // Convoy allocations
          _buildConvoySection(trip),
          const SizedBox(height: 24),
          // Trip Notes placeholder (Task 4.4)
          _buildNotesSection(trip),
        ],
      ),
    );
  }

  Widget _buildMetadataCard(TripInstance trip) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: const Color(0xFFE8ECF0)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Text(
                'Trip Information',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
              const Spacer(),
              _StatusBadge(status: trip.status),
            ],
          ),
          const SizedBox(height: 16),
          _infoRow('Date', trip.tripDate),
          _infoRow('Departure', trip.departureTime),
          _infoRow('Direction', trip.direction),
          _infoRow('Stop', trip.stopName),
          _infoRow('Campus', trip.campusName),
          _infoRow('Passengers', '${trip.passengerCount}'),
        ],
      ),
    );
  }

  Widget _buildConvoySection(TripInstance trip) {
    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: const Color(0xFFE8ECF0)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Text(
                'Convoy Allocations',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
              const Spacer(),
              FilledButton.icon(
                onPressed: () => _showAssignShuttleDialog(trip),
                icon: const Icon(Icons.add, size: 16),
                label: const Text('Assign Shuttle'),
                style: FilledButton.styleFrom(
                  backgroundColor: _navy,
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          if (trip.convoyAllocations.isEmpty)
            Center(
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Text(
                  'No shuttles assigned to this trip',
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 13,
                    color: Colors.grey.shade500,
                  ),
                ),
              ),
            )
          else
            _buildConvoyTable(trip.convoyAllocations),
        ],
      ),
    );
  }

  Widget _buildConvoyTable(List<ConvoyAllocation> allocations) {
    return Table(
      columnWidths: const {
        0: FlexColumnWidth(1),
        1: FlexColumnWidth(2.5),
        2: FlexColumnWidth(1.5),
        3: FlexColumnWidth(1.5),
        4: FlexColumnWidth(1.5),
      },
      border: TableBorder.all(color: Colors.grey.shade300, width: 0.5),
      children: [
        TableRow(
          decoration: BoxDecoration(color: _navy.withValues(alpha: 0.05)),
          children: [
            _tableHeader('Pos'),
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
                _tableCellIcon(a.isAccessibleVehicle
                    ? Icons.accessible
                    : Icons.remove, a.isAccessibleVehicle),
                _tableCellIcon(a.isOperational
                    ? Icons.check_circle
                    : Icons.cancel, a.isOperational),
              ],
            )),
      ],
    );
  }

  Widget _buildNotesSection(TripInstance trip) {
    final notesAsync = ref.watch(tripNotesProvider(widget.tripId));

    return Container(
      width: double.infinity,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: const Color(0xFFE8ECF0)),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Text(
                'Trip Notes',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
              const Spacer(),
              FilledButton.icon(
                onPressed: () => _showAddNoteDialog(trip),
                icon: const Icon(Icons.add, size: 16),
                label: const Text('Add Note'),
                style: FilledButton.styleFrom(
                  backgroundColor: _navy,
                  padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          notesAsync.when(
            loading: () => const Center(
              child: Padding(
                padding: EdgeInsets.all(20),
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
            ),
            error: (_, __) => Center(
              child: Text(
                'Failed to load notes',
                style: GoogleFonts.hankenGrotesk(color: Colors.red.shade600),
              ),
            ),
            data: (notes) => notes.isEmpty
                ? Center(
                    child: Padding(
                      padding: const EdgeInsets.all(20),
                      child: Text(
                        'No notes for this trip',
                        style: GoogleFonts.hankenGrotesk(
                          fontSize: 13,
                          color: Colors.grey.shade500,
                        ),
                      ),
                    ),
                  )
                : Column(
                    children: notes.map((note) => _buildNoteCard(note)).toList(),
                  ),
          ),
        ],
      ),
    );
  }

  Widget _buildNoteCard(TripNoteModel note) {
    final isResolution = note.isResolutionAction;

    return Container(
      width: double.infinity,
      margin: const EdgeInsets.only(bottom: 12),
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: isResolution ? Colors.green.shade50 : Colors.grey.shade50,
        borderRadius: BorderRadius.circular(8),
        border: Border.all(
          color: isResolution ? Colors.green.shade300 : Colors.grey.shade300,
        ),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 2),
                decoration: BoxDecoration(
                  color: _noteTypeColor(note.noteType).withValues(alpha: 0.15),
                  borderRadius: BorderRadius.circular(4),
                ),
                child: Text(
                  note.noteType,
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 11,
                    fontWeight: FontWeight.w600,
                    color: _noteTypeColor(note.noteType),
                  ),
                ),
              ),
              if (isResolution) ...[
                const SizedBox(width: 8),
                Icon(Icons.check_circle, size: 14, color: Colors.green.shade700),
                const SizedBox(width: 4),
                Text(
                  'Resolution',
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 11,
                    fontWeight: FontWeight.w600,
                    color: Colors.green.shade700,
                  ),
                ),
              ],
              const Spacer(),
              Text(
                note.createdAt.substring(0, 16).replaceFirst('T', ' '),
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 11,
                  color: Colors.grey.shade600,
                ),
              ),
            ],
          ),
          const SizedBox(height: 8),
          Text(
            note.body,
            style: GoogleFonts.hankenGrotesk(fontSize: 13, color: _navy),
          ),
          const SizedBox(height: 6),
          Text(
            '— ${note.createdByAdminName}',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 11,
              fontStyle: FontStyle.italic,
              color: Colors.grey.shade600,
            ),
          ),
        ],
      ),
    );
  }

  Color _noteTypeColor(String type) {
    return switch (type) {
      'Incident' => Colors.orange.shade700,
      'Replacement' => Colors.blue.shade700,
      'General' => Colors.grey.shade700,
      'NoShow' => Colors.red.shade700,
      _ => Colors.grey.shade700,
    };
  }

  Future<void> _showAddNoteDialog(TripInstance trip) async {
    final result = await showDialog<TripNoteModel?>(
      context: context,
      builder: (ctx) => _AddTripNoteDialog(
        tripId: widget.tripId,
        tripStatus: trip.status,
      ),
    );

    if (result != null) {
      ref.invalidate(tripNotesProvider(widget.tripId));

      if (result.isResolutionAction && mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              'Incident resolved. Notifications sent to ${result.notificationsSent ?? 0} passengers.',
              style: GoogleFonts.hankenGrotesk(),
            ),
            backgroundColor: Colors.green.shade600,
          ),
        );
        // Also refresh trips data
        ref.invalidate(tripsProvider(const TripQueryParams()));
      }
    }
  }

  Widget _infoRow(String label, String value) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 10),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SizedBox(
            width: 100,
            child: Text(
              label,
              style: GoogleFonts.hankenGrotesk(
                fontSize: 13,
                fontWeight: FontWeight.w600,
                color: Colors.grey.shade600,
              ),
            ),
          ),
          Expanded(
            child: Text(
              value,
              style: GoogleFonts.hankenGrotesk(fontSize: 13, color: _navy),
            ),
          ),
        ],
      ),
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

  Widget _tableCellIcon(IconData icon, bool positive) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 6, horizontal: 10),
      child: Icon(
        icon,
        size: 16,
        color: positive ? Colors.green : Colors.red.shade400,
      ),
    );
  }

  void _showAssignShuttleDialog(TripInstance trip) {
    showDialog(
      context: context,
      builder: (ctx) => _AssignShuttleDialog(
        tripId: trip.id,
        onAssigned: () => ref.invalidate(tripsProvider(const TripQueryParams())),
      ),
    );
  }
}

// ─── Status Badge ────────────────────────────────────────────────────────────

class _StatusBadge extends StatelessWidget {
  const _StatusBadge({required this.status});
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
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 4),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        status,
        style: GoogleFonts.hankenGrotesk(
          fontSize: 12,
          fontWeight: FontWeight.w600,
          color: fg,
        ),
      ),
    );
  }
}

// ─── Assign Shuttle Dialog (reused pattern) ──────────────────────────────────

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
          data: (shuttles) => Column(
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
                title: Text(shuttle.licensePlate,
                    style: GoogleFonts.hankenGrotesk(
                        fontWeight: FontWeight.w600, color: _navy)),
                subtitle: Text(
                  'Capacity: ${shuttle.capacity}',
                  style: GoogleFonts.hankenGrotesk(fontSize: 12),
                ),
                trailing:
                    isSelected ? const Icon(Icons.check_circle, color: _navy) : null,
                onTap: () => setState(() => _selectedShuttleId = shuttle.id),
              );
            }).toList(),
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text('Cancel',
              style: GoogleFonts.hankenGrotesk(color: Colors.grey.shade700)),
        ),
        FilledButton(
          onPressed:
              _selectedShuttleId != null && !_isSubmitting ? _submit : null,
          style: FilledButton.styleFrom(
            backgroundColor: _navy,
            shape:
                RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
          ),
          child: _isSubmitting
              ? const SizedBox(
                  width: 16,
                  height: 16,
                  child:
                      CircularProgressIndicator(strokeWidth: 2, color: Colors.white),
                )
              : Text('Assign',
                  style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w600)),
        ),
      ],
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
    }
  }
}


// ─── Add Trip Note Dialog ────────────────────────────────────────────────────

class _AddTripNoteDialog extends ConsumerStatefulWidget {
  const _AddTripNoteDialog({required this.tripId, required this.tripStatus});

  final int tripId;
  final String tripStatus;

  @override
  ConsumerState<_AddTripNoteDialog> createState() => _AddTripNoteDialogState();
}

class _AddTripNoteDialogState extends ConsumerState<_AddTripNoteDialog> {
  static const Color _navy = Color(0xFF132E51);

  final _bodyController = TextEditingController();
  String _noteType = 'General';
  bool _isSubmitting = false;

  static const List<String> _noteTypes = [
    'General',
    'Incident',
    'Replacement',
    'NoShow',
  ];

  bool get _isResolutionType =>
      (_noteType == 'Incident' || _noteType == 'Replacement') &&
      widget.tripStatus == 'IncidentActive';

  @override
  void dispose() {
    _bodyController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      title: Row(
        children: [
          const Icon(Icons.note_add, color: _navy),
          const SizedBox(width: 8),
          Text(
            'Add Trip Note',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 18,
              fontWeight: FontWeight.bold,
              color: _navy,
            ),
          ),
        ],
      ),
      content: SizedBox(
        width: 500,
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Note type dropdown
            DropdownButtonFormField<String>(
              value: _noteType,
              decoration: InputDecoration(
                labelText: 'Note Type',
                labelStyle: GoogleFonts.hankenGrotesk(),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
              ),
              style: GoogleFonts.hankenGrotesk(color: _navy),
              items: _noteTypes
                  .map((t) => DropdownMenuItem(value: t, child: Text(t)))
                  .toList(),
              onChanged: (v) => setState(() => _noteType = v ?? 'General'),
            ),
            const SizedBox(height: 16),
            // Body text field
            TextFormField(
              controller: _bodyController,
              decoration: InputDecoration(
                labelText: 'Note Body',
                labelStyle: GoogleFonts.hankenGrotesk(),
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
                alignLabelWithHint: true,
              ),
              style: GoogleFonts.hankenGrotesk(),
              maxLines: 5,
              minLines: 3,
            ),
            if (_isResolutionType) ...[
              const SizedBox(height: 12),
              Container(
                padding: const EdgeInsets.all(10),
                decoration: BoxDecoration(
                  color: Colors.orange.shade50,
                  borderRadius: BorderRadius.circular(8),
                  border: Border.all(color: Colors.orange.shade300),
                ),
                child: Row(
                  children: [
                    Icon(Icons.warning_amber_rounded,
                        size: 16, color: Colors.orange.shade700),
                    const SizedBox(width: 8),
                    Expanded(
                      child: Text(
                        'This note will resolve the active incident and send '
                        'push notifications to all confirmed passengers.',
                        style: GoogleFonts.hankenGrotesk(
                          fontSize: 12,
                          color: Colors.orange.shade800,
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ],
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text('Cancel',
              style: GoogleFonts.hankenGrotesk(color: Colors.grey.shade700)),
        ),
        FilledButton(
          onPressed: !_isSubmitting ? _submit : null,
          style: FilledButton.styleFrom(
            backgroundColor: _isResolutionType ? Colors.orange.shade700 : _navy,
            shape:
                RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
          ),
          child: _isSubmitting
              ? const SizedBox(
                  width: 16,
                  height: 16,
                  child: CircularProgressIndicator(
                      strokeWidth: 2, color: Colors.white),
                )
              : Text(
                  _isResolutionType ? 'Resolve & Notify' : 'Save Note',
                  style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w600),
                ),
        ),
      ],
    );
  }

  Future<void> _submit() async {
    final body = _bodyController.text.trim();
    if (body.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Note body cannot be empty',
              style: GoogleFonts.hankenGrotesk()),
          backgroundColor: Colors.red.shade600,
        ),
      );
      return;
    }

    // If this is a resolution action, show confirmation dialog
    if (_isResolutionType) {
      final confirmed = await showDialog<bool>(
        context: context,
        builder: (ctx) => AlertDialog(
          title: Text('Confirm Resolution',
              style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600, color: _navy)),
          content: Text(
            'This note will resolve the active incident and send push '
            'notifications to all confirmed passengers. Proceed?',
            style: GoogleFonts.hankenGrotesk(),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(ctx).pop(false),
              child: const Text('Cancel'),
            ),
            FilledButton(
              onPressed: () => Navigator.of(ctx).pop(true),
              style: FilledButton.styleFrom(
                backgroundColor: Colors.orange.shade700,
              ),
              child: const Text('Resolve & Notify'),
            ),
          ],
        ),
      );
      if (confirmed != true) return;
    }

    setState(() => _isSubmitting = true);

    try {
      final result = await ref.read(tripNoteActionsProvider).createNote(
            tripId: widget.tripId,
            noteType: _noteType,
            body: body,
            resolveIncident: _isResolutionType,
          );

      if (mounted) {
        Navigator.of(context).pop(result);
      }
    } on DioException catch (e) {
      if (mounted) {
        setState(() => _isSubmitting = false);
        final msg = (e.response?.data is Map)
            ? (e.response!.data as Map)['message']?.toString() ?? 'Failed'
            : 'Failed to create note';
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(msg, style: GoogleFonts.hankenGrotesk()),
            backgroundColor: Colors.red.shade600,
          ),
        );
      }
    }
  }
}
