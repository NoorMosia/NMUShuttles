import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';
import 'package:nmu_shuttle_admin/features/stops/providers/stops_providers.dart';
import 'package:nmu_shuttle_admin/features/stops/providers/default_convoy_provider.dart';
import 'package:nmu_shuttle_admin/features/trips/providers/trips_providers.dart';

/// Detail screen for a single Stop, showing metadata, default convoy panel,
/// and a button to view trips for this stop.
class StopDetailScreen extends ConsumerStatefulWidget {
  const StopDetailScreen({super.key, required this.stopId});

  final int stopId;

  @override
  ConsumerState<StopDetailScreen> createState() => _StopDetailScreenState();
}

class _StopDetailScreenState extends ConsumerState<StopDetailScreen> {
  static const Color _navy = Color(0xFF132E51);
  static const Color _gold = Color(0xFFFFCC00);

  @override
  void initState() {
    super.initState();
    Future.microtask(() => ref.read(stopsProvider.notifier).fetchStops());
  }

  @override
  Widget build(BuildContext context) {
    final stopsState = ref.watch(stopsProvider);
    final stop = stopsState.stops.cast<Stop?>().firstWhere(
          (s) => s?.id == widget.stopId,
          orElse: () => null,
        );

    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Back navigation + title
          Row(
            children: [
              IconButton(
                onPressed: () => context.go('/stops'),
                icon: const Icon(Icons.arrow_back),
                tooltip: 'Back to Stops',
                color: _navy,
              ),
              const SizedBox(width: 8),
              Text(
                stop?.stopName ?? 'Stop Details',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 22,
                  fontWeight: FontWeight.bold,
                  color: _navy,
                ),
              ),
              const Spacer(),
              if (stop != null)
                FilledButton.icon(
                  onPressed: () => context.go('/stops/${widget.stopId}/trips'),
                  icon: const Icon(Icons.trip_origin_outlined, size: 18),
                  label: const Text('View Trips'),
                  style: FilledButton.styleFrom(
                    backgroundColor: _navy,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                  ),
                ),
            ],
          ),
          const SizedBox(height: 20),

          if (stopsState.isLoading)
            const Expanded(
              child: Center(child: CircularProgressIndicator(strokeWidth: 3)),
            )
          else if (stop == null)
            Expanded(
              child: Center(
                child: Text(
                  'Stop not found',
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 14,
                    color: Colors.grey.shade600,
                  ),
                ),
              ),
            )
          else
            Expanded(
              child: SingleChildScrollView(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // Stop metadata card
                    _buildMetadataCard(stop),
                    const SizedBox(height: 24),
                    // Default Convoy panel placeholder (Task 4.2)
                    _buildDefaultConvoySection(),
                  ],
                ),
              ),
            ),
        ],
      ),
    );
  }

  Widget _buildMetadataCard(Stop stop) {
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
          Text(
            'Stop Information',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 16,
              fontWeight: FontWeight.w600,
              color: _navy,
            ),
          ),
          const SizedBox(height: 16),
          _infoRow('Name', stop.stopName),
          _infoRow('Address', stop.fullAddress),
          _infoRow('GPS', stop.gpsCoordinates),
        ],
      ),
    );
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
              style: GoogleFonts.hankenGrotesk(
                fontSize: 13,
                color: _navy,
              ),
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildDefaultConvoySection() {
    final convoyAsync = ref.watch(defaultConvoyProvider(widget.stopId));

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
                'Default Convoy',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 16,
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
              const Spacer(),
              FilledButton.icon(
                onPressed: () => _showAddShuttleDialog(),
                icon: const Icon(Icons.add, size: 16),
                label: const Text('Add Shuttle'),
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
          const SizedBox(height: 12),
          // Advisory banner
          Container(
            width: double.infinity,
            padding: const EdgeInsets.all(12),
            decoration: BoxDecoration(
              color: _gold.withValues(alpha: 0.15),
              borderRadius: BorderRadius.circular(8),
              border: Border.all(color: _gold.withValues(alpha: 0.4)),
            ),
            child: Row(
              children: [
                Icon(Icons.info_outline, size: 16, color: Colors.amber.shade800),
                const SizedBox(width: 8),
                Expanded(
                  child: Text(
                    'Changes apply to trips created from tonight onwards. '
                    'Trips for the next 3 days already have their convoy — edit those from the trip detail screen.',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 12,
                      color: Colors.amber.shade900,
                    ),
                  ),
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          // Convoy list
          convoyAsync.when(
            loading: () => const Center(
              child: Padding(
                padding: EdgeInsets.all(20),
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
            ),
            error: (error, _) => Center(
              child: Text(
                'Failed to load convoy',
                style: GoogleFonts.hankenGrotesk(color: Colors.red.shade600),
              ),
            ),
            data: (entries) => entries.isEmpty
                ? Center(
                    child: Padding(
                      padding: const EdgeInsets.all(20),
                      child: Text(
                        'No shuttles assigned to default convoy',
                        style: GoogleFonts.hankenGrotesk(
                          fontSize: 13,
                          color: Colors.grey.shade500,
                        ),
                      ),
                    ),
                  )
                : _buildConvoyList(entries),
          ),
        ],
      ),
    );
  }

  Widget _buildConvoyList(List<DefaultConvoyEntry> entries) {
    return ReorderableListView.builder(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      itemCount: entries.length,
      onReorder: (oldIndex, newIndex) => _handleReorder(entries, oldIndex, newIndex),
      itemBuilder: (context, index) {
        final entry = entries[index];
        return ListTile(
          key: ValueKey(entry.shuttleId),
          leading: CircleAvatar(
            radius: 16,
            backgroundColor: _navy.withValues(alpha: 0.1),
            child: Text(
              '${entry.orderPosition}',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 12,
                fontWeight: FontWeight.w700,
                color: _navy,
              ),
            ),
          ),
          title: Row(
            children: [
              Text(
                entry.licensePlate,
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 13,
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
              if (entry.isAccessibleVehicle) ...[
                const SizedBox(width: 8),
                Icon(Icons.accessible, size: 16, color: Colors.blue.shade700),
              ],
            ],
          ),
          subtitle: Text(
            'Capacity: ${entry.capacity}',
            style: GoogleFonts.hankenGrotesk(fontSize: 12, color: Colors.grey.shade600),
          ),
          trailing: IconButton(
            icon: Icon(Icons.remove_circle_outline, color: Colors.red.shade600, size: 20),
            tooltip: 'Remove from convoy',
            onPressed: () => _removeShuttle(entry.shuttleId),
          ),
        );
      },
    );
  }

  Future<void> _handleReorder(List<DefaultConvoyEntry> entries, int oldIndex, int newIndex) async {
    if (newIndex > oldIndex) newIndex--;
    final reordered = List<DefaultConvoyEntry>.from(entries);
    final item = reordered.removeAt(oldIndex);
    reordered.insert(newIndex, item);

    final orderedIds = reordered.map((e) => e.shuttleId).toList();

    try {
      await ref.read(defaultConvoyActionsProvider).reorder(widget.stopId, orderedIds);
      ref.invalidate(defaultConvoyProvider(widget.stopId));
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Failed to reorder: ${_extractError(e)}'),
            backgroundColor: Colors.red.shade600,
          ),
        );
      }
    }
  }

  Future<void> _removeShuttle(int shuttleId) async {
    try {
      await ref.read(defaultConvoyActionsProvider).removeShuttle(widget.stopId, shuttleId);
      ref.invalidate(defaultConvoyProvider(widget.stopId));
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Failed to remove: ${_extractError(e)}'),
            backgroundColor: Colors.red.shade600,
          ),
        );
      }
    }
  }

  Future<void> _showAddShuttleDialog() async {
    // Fetch shuttles not assigned to any stop's default convoy
    List<ShuttleOption> available;
    try {
      final dioClient = ref.read(dioClientProvider);
      final response = await dioClient.dio.get('/admin/shuttles/available');
      final list = response.data as List;
      available = list.map((e) => ShuttleOption.fromJson(e as Map<String, dynamic>)).toList();
    } catch (e) {
      available = [];
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Failed to load shuttles: $e'),
            backgroundColor: Colors.red.shade600,
          ),
        );
      }
      return;
    }

    if (!mounted) return;

    final selected = await showDialog<int>(
      context: context,
      builder: (ctx) => _AddShuttleToConvoyDialog(shuttles: available),
    );

    if (selected != null) {
      try {
        await ref.read(defaultConvoyActionsProvider).addShuttle(widget.stopId, selected);
        ref.invalidate(defaultConvoyProvider(widget.stopId));
      } catch (e) {
        if (mounted) {
          ScaffoldMessenger.of(context).showSnackBar(
            SnackBar(
              content: Text('Failed to add shuttle: ${_extractError(e)}'),
              backgroundColor: Colors.red.shade600,
            ),
          );
        }
      }
    }
  }

  String _extractError(Object error) {
    if (error is DioException) {
      final data = error.response?.data;
      if (data is Map<String, dynamic> && data.containsKey('message')) {
        return data['message'].toString();
      }
    }
    return error.toString();
  }
}


// ─── Add Shuttle to Convoy Dialog ────────────────────────────────────────────

class _AddShuttleToConvoyDialog extends StatefulWidget {
  const _AddShuttleToConvoyDialog({required this.shuttles});

  final List<ShuttleOption> shuttles;

  @override
  State<_AddShuttleToConvoyDialog> createState() =>
      _AddShuttleToConvoyDialogState();
}

class _AddShuttleToConvoyDialogState extends State<_AddShuttleToConvoyDialog> {
  static const Color _navy = Color(0xFF132E51);
  int? _selectedId;

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
      title: Row(
        children: [
          const Icon(Icons.directions_bus, color: _navy),
          const SizedBox(width: 8),
          Text(
            'Add Shuttle to Default Convoy',
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
        child: widget.shuttles.isEmpty
            ? Text(
                'No active shuttles available',
                style: GoogleFonts.hankenGrotesk(color: Colors.grey),
              )
            : Column(
                mainAxisSize: MainAxisSize.min,
                children: widget.shuttles.map((shuttle) {
                  final isSelected = _selectedId == shuttle.id;
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
                    onTap: () => setState(() => _selectedId = shuttle.id),
                  );
                }).toList(),
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
          onPressed: _selectedId != null
              ? () => Navigator.of(context).pop(_selectedId)
              : null,
          style: FilledButton.styleFrom(
            backgroundColor: _navy,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8),
            ),
          ),
          child: Text(
            'Add',
            style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w600),
          ),
        ),
      ],
    );
  }
}
