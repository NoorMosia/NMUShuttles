import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/features/schedule/providers/schedule_providers.dart';
import 'package:nmu_shuttle_admin/features/stops/providers/stops_providers.dart';

/// Schedule Times management screen (standalone, no tabs).
class ScheduleScreen extends ConsumerStatefulWidget {
  const ScheduleScreen({super.key});

  @override
  ConsumerState<ScheduleScreen> createState() => _ScheduleScreenState();
}

class _ScheduleScreenState extends ConsumerState<ScheduleScreen> {
  static const Color _navy = Color(0xFF132E51);

  @override
  void initState() {
    super.initState();
    Future.microtask(() {
      ref.read(scheduleTimesProvider.notifier).fetchScheduleTimes();
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(scheduleTimesProvider);

    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FB),
      body: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Schedule Times',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 22,
                fontWeight: FontWeight.w700,
                color: _navy,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              'Manage departure times, directions, and schedule types for shuttle routes',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 14,
                color: const Color(0xFF5A6B7B),
              ),
            ),
            const SizedBox(height: 24),
            // Action bar
            Row(
              mainAxisAlignment: MainAxisAlignment.end,
              children: [
                FilledButton.icon(
                  onPressed: () => _showCreateScheduleTimeDialog(context, ref),
                  icon: const Icon(Icons.add, size: 18),
                  label: const Text('Add Schedule Time'),
                ),
              ],
            ),
            const SizedBox(height: 16),
            // Content
            Expanded(child: _buildContent(context, ref, state)),
          ],
        ),
      ),
    );
  }

  Widget _buildContent(
      BuildContext context, WidgetRef ref, ScheduleTimesState state) {
    switch (state.status) {
      case ScheduleStatus.idle:
      case ScheduleStatus.loading:
        return const Center(child: CircularProgressIndicator(strokeWidth: 3));
      case ScheduleStatus.error:
        return _ErrorPanel(
          message: state.errorMessage ?? 'An error occurred',
          onRetry: () =>
              ref.read(scheduleTimesProvider.notifier).fetchScheduleTimes(),
        );
      case ScheduleStatus.success:
        if (state.items.isEmpty) {
          return Center(
            child: Text(
              'No schedule times found',
              style: GoogleFonts.hankenGrotesk(
                  fontSize: 14, color: const Color(0xFF5A6B7B)),
            ),
          );
        }
        return _buildTable(context, ref, state.items);
    }
  }

  Widget _buildTable(
      BuildContext context, WidgetRef ref, List<ScheduleTimeModel> items) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: const Color(0xFFE8ECF0)),
      ),
      child: SingleChildScrollView(
        child: SizedBox(
          width: double.infinity,
          child: DataTable(
            columnSpacing: 24,
            columns: const [
              DataColumn(label: Text('DEPARTURE TIME')),
              DataColumn(label: Text('DIRECTION')),
              DataColumn(label: Text('SCHEDULE TYPE')),
              DataColumn(label: Text('STOP')),
              DataColumn(label: Text('ACTIONS')),
            ],
            rows: items.map((item) {
              final stopLabel = (item.stopName != null && item.stopName!.isNotEmpty)
                  ? item.stopName!
                  : 'Stop ${item.stopId}';
              return DataRow(cells: [
                DataCell(Text(item.departureTime)),
                DataCell(_DirectionChip(direction: item.direction)),
                DataCell(_ScheduleTypeChip(type: item.scheduleType)),
                DataCell(Text(stopLabel)),
                DataCell(Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    IconButton(
                      icon: Icon(Icons.edit_outlined,
                          size: 18, color: Colors.blue.shade700),
                      tooltip: 'Edit',
                      onPressed: () =>
                          _showEditScheduleTimeDialog(context, ref, item),
                    ),
                    IconButton(
                      icon: Icon(Icons.delete_outlined,
                          size: 18, color: Colors.red.shade700),
                      tooltip: 'Delete',
                      onPressed: () => _confirmDelete(context, ref, item),
                    ),
                  ],
                )),
              ]);
            }).toList(),
          ),
        ),
      ),
    );
  }

  void _showCreateScheduleTimeDialog(BuildContext context, WidgetRef ref) {
    showDialog(
      context: context,
      builder: (ctx) => _ScheduleTimeFormDialog(
        title: 'Add Schedule Time',
        onSubmit: (departureTime, direction, scheduleType, stopId, requiresScanning) {
          ref.read(scheduleTimesProvider.notifier).addScheduleTime(
                departureTime: departureTime,
                direction: direction,
                scheduleType: scheduleType,
                stopId: stopId,
                requiresScanning: requiresScanning,
              );
        },
      ),
    );
  }

  void _showEditScheduleTimeDialog(
      BuildContext context, WidgetRef ref, ScheduleTimeModel item) {
    showDialog(
      context: context,
      builder: (ctx) => _ScheduleTimeFormDialog(
        title: 'Edit Schedule Time',
        initialDepartureTime: item.departureTime,
        initialDirection: item.direction,
        initialScheduleType: item.scheduleType,
        initialStopId: item.stopId,
        onSubmit: (departureTime, direction, scheduleType, stopId, requiresScanning) {
          ref.read(scheduleTimesProvider.notifier).updateScheduleTime(
                id: item.id,
                departureTime: departureTime,
                direction: direction,
                scheduleType: scheduleType,
                stopId: stopId,
                requiresScanning: requiresScanning,
              );
        },
      ),
    );
  }

  void _confirmDelete(
      BuildContext context, WidgetRef ref, ScheduleTimeModel item) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(
          'Delete Schedule Time',
          style: GoogleFonts.hankenGrotesk(
            fontWeight: FontWeight.w600,
            color: _navy,
          ),
        ),
        content: Text(
          'Are you sure you want to delete the ${item.departureTime} schedule time?',
          style: GoogleFonts.hankenGrotesk(),
        ),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(ctx).pop(),
            child: const Text('Cancel'),
          ),
          FilledButton(
            onPressed: () {
              ref
                  .read(scheduleTimesProvider.notifier)
                  .deleteScheduleTime(item.id);
              Navigator.of(ctx).pop();
            },
            style: FilledButton.styleFrom(
              backgroundColor: Colors.red.shade700,
            ),
            child: const Text('Delete'),
          ),
        ],
      ),
    );
  }
}

// ─── Form Dialog ─────────────────────────────────────────────────────────────

class _ScheduleTimeFormDialog extends ConsumerStatefulWidget {
  const _ScheduleTimeFormDialog({
    required this.title,
    required this.onSubmit,
    this.initialDepartureTime,
    this.initialDirection,
    this.initialScheduleType,
    this.initialStopId,
  });

  final String title;
  final void Function(String departureTime, String direction,
      String scheduleType, int stopId, bool requiresScanning) onSubmit;
  final String? initialDepartureTime;
  final String? initialDirection;
  final String? initialScheduleType;
  final int? initialStopId;

  @override
  ConsumerState<_ScheduleTimeFormDialog> createState() =>
      _ScheduleTimeFormDialogState();
}

class _ScheduleTimeFormDialogState
    extends ConsumerState<_ScheduleTimeFormDialog> {
  static const Color _navy = Color(0xFF132E51);

  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _departureTimeController;
  int? _stopId;
  late String _direction;
  late String _scheduleType;
  late bool _requiresScanning;

  @override
  void initState() {
    super.initState();
    _departureTimeController =
        TextEditingController(text: widget.initialDepartureTime ?? '');
    _stopId = widget.initialStopId;
    _direction = widget.initialDirection ?? 'ToCampus';
    _scheduleType = widget.initialScheduleType ?? 'Weekday';
    _requiresScanning = true;
    // Ensure the stops list is loaded for the stop dropdown.
    Future.microtask(() {
      final stopsState = ref.read(stopsProvider);
      if (stopsState.stops.isEmpty) {
        ref.read(stopsProvider.notifier).fetchStops();
      }
    });
  }

  @override
  void dispose() {
    _departureTimeController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(
        widget.title,
        style: GoogleFonts.hankenGrotesk(
          fontWeight: FontWeight.w600,
          color: _navy,
        ),
      ),
      content: SizedBox(
        width: 420,
        child: Form(
          key: _formKey,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextFormField(
                controller: _departureTimeController,
                decoration: InputDecoration(
                  labelText: 'Departure Time (HH:mm)',
                  labelStyle: GoogleFonts.hankenGrotesk(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                  prefixIcon: const Icon(Icons.access_time),
                ),
                style: GoogleFonts.hankenGrotesk(),
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Required' : null,
                onTap: () async {
                  final time = await showTimePicker(
                    context: context,
                    initialTime: TimeOfDay.now(),
                  );
                  if (time != null) {
                    _departureTimeController.text =
                        '${time.hour.toString().padLeft(2, '0')}:${time.minute.toString().padLeft(2, '0')}';
                  }
                },
                readOnly: true,
              ),
              const SizedBox(height: 16),
              DropdownButtonFormField<String>(
                initialValue: _direction,
                decoration: InputDecoration(
                  labelText: 'Direction',
                  labelStyle: GoogleFonts.hankenGrotesk(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
                style: GoogleFonts.hankenGrotesk(color: _navy),
                items: const [
                  DropdownMenuItem(
                      value: 'ToCampus', child: Text('To Campus')),
                  DropdownMenuItem(
                      value: 'FromCampus', child: Text('From Campus')),
                ],
                onChanged: (v) =>
                    setState(() => _direction = v ?? 'ToCampus'),
              ),
              const SizedBox(height: 16),
              DropdownButtonFormField<String>(
                initialValue: _scheduleType,
                decoration: InputDecoration(
                  labelText: 'Schedule Type',
                  labelStyle: GoogleFonts.hankenGrotesk(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
                style: GoogleFonts.hankenGrotesk(color: _navy),
                items: const [
                  DropdownMenuItem(value: 'Weekday', child: Text('Weekday')),
                  DropdownMenuItem(
                      value: 'Saturday', child: Text('Saturday')),
                ],
                onChanged: (v) =>
                    setState(() => _scheduleType = v ?? 'Weekday'),
              ),
              const SizedBox(height: 16),
              _buildStopDropdown(),
              const SizedBox(height: 16),
              // Requires Scanning toggle
              SwitchListTile(
                title: Text(
                  'Requires QR Scanning',
                  style: GoogleFonts.hankenGrotesk(fontSize: 14, color: _navy),
                ),
                subtitle: Text(
                  'Disable for open-boarding trips (e.g. late-morning ToCampus)',
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 12,
                    color: const Color(0xFF5A6B7B),
                  ),
                ),
                value: _requiresScanning,
                onChanged: (v) => setState(() => _requiresScanning = v),
                activeColor: _navy,
                contentPadding: EdgeInsets.zero,
              ),
            ],
          ),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.of(context).pop(),
          child: Text('Cancel', style: GoogleFonts.hankenGrotesk()),
        ),
        FilledButton(
          onPressed: _submit,
          child: Text('Save', style: GoogleFonts.hankenGrotesk()),
        ),
      ],
    );
  }

  Widget _buildStopDropdown() {
    final stopsState = ref.watch(stopsProvider);
    if (stopsState.isLoading && stopsState.stops.isEmpty) {
      return _dropdownPlaceholder(
        label: 'Stop',
        icon: Icons.location_on_outlined,
        child: const SizedBox(
          height: 16,
          width: 16,
          child: CircularProgressIndicator(strokeWidth: 2),
        ),
      );
    }
    final stops = stopsState.stops;
    final hasSelected = stops.any((s) => s.id == _stopId);
    return DropdownButtonFormField<int>(
      initialValue: hasSelected ? _stopId : null,
      isExpanded: true,
      decoration: InputDecoration(
        labelText: 'Stop',
        labelStyle: GoogleFonts.hankenGrotesk(),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
        ),
        prefixIcon: const Icon(Icons.location_on_outlined),
      ),
      style: GoogleFonts.hankenGrotesk(color: _navy),
      items: stops
          .map((s) => DropdownMenuItem<int>(
                value: s.id,
                child: Text(s.stopName),
              ))
          .toList(),
      onChanged: (v) => setState(() => _stopId = v),
      validator: (v) => v == null ? 'Please select a stop' : null,
    );
  }

  /// A disabled, outlined field used while dropdown data is loading or failed,
  /// keeping the dialog layout stable.
  Widget _dropdownPlaceholder({
    required String label,
    required IconData icon,
    required Widget child,
  }) {
    return InputDecorator(
      decoration: InputDecoration(
        labelText: label,
        labelStyle: GoogleFonts.hankenGrotesk(),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(10),
        ),
        prefixIcon: Icon(icon),
      ),
      child: Align(alignment: Alignment.centerLeft, child: child),
    );
  }

  void _submit() {
    if (!_formKey.currentState!.validate()) return;
    widget.onSubmit(
      _departureTimeController.text,
      _direction,
      _scheduleType,
      _stopId!,
      _requiresScanning,
    );
    Navigator.of(context).pop();
  }
}

// ─── Chip Widgets ────────────────────────────────────────────────────────────

class _DirectionChip extends StatelessWidget {
  const _DirectionChip({required this.direction});
  final String direction;

  @override
  Widget build(BuildContext context) {
    final isToCampus = direction == 'ToCampus';
    final color = isToCampus ? Colors.blue.shade700 : Colors.purple.shade700;
    final label = isToCampus ? 'To Campus' : 'From Campus';
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(6),
      ),
      child: Text(
        label,
        style: GoogleFonts.hankenGrotesk(
          fontSize: 12,
          fontWeight: FontWeight.w600,
          color: color,
        ),
      ),
    );
  }
}

class _ScheduleTypeChip extends StatelessWidget {
  const _ScheduleTypeChip({required this.type});
  final String type;

  @override
  Widget build(BuildContext context) {
    final isWeekday = type == 'Weekday';
    final color = isWeekday ? Colors.teal.shade700 : Colors.orange.shade700;
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(6),
      ),
      child: Text(
        type,
        style: GoogleFonts.hankenGrotesk(
          fontSize: 12,
          fontWeight: FontWeight.w600,
          color: color,
        ),
      ),
    );
  }
}

// ─── Error Panel ─────────────────────────────────────────────────────────────

class _ErrorPanel extends StatelessWidget {
  const _ErrorPanel({required this.message, required this.onRetry});
  final String message;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.symmetric(vertical: 40),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.error_outline, size: 48, color: Colors.red.shade300),
            const SizedBox(height: 12),
            Text(
              message,
              style: GoogleFonts.hankenGrotesk(
                fontSize: 14,
                color: const Color(0xFF5A6B7B),
              ),
            ),
            const SizedBox(height: 12),
            OutlinedButton.icon(
              onPressed: onRetry,
              icon: const Icon(Icons.refresh),
              label: const Text('Retry'),
            ),
          ],
        ),
      ),
    );
  }
}
