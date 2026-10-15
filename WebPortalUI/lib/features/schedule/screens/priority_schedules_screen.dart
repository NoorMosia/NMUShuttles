import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/core/validators.dart';
import 'package:nmu_shuttle_admin/features/schedule/providers/schedule_providers.dart';

/// Standalone screen for managing Priority Schedules.
class PrioritySchedulesScreen extends ConsumerStatefulWidget {
  const PrioritySchedulesScreen({super.key});

  @override
  ConsumerState<PrioritySchedulesScreen> createState() =>
      _PrioritySchedulesScreenState();
}

class _PrioritySchedulesScreenState
    extends ConsumerState<PrioritySchedulesScreen> {
  static const Color _navy = Color(0xFF132E51);

  @override
  void initState() {
    super.initState();
    Future.microtask(() {
      ref.read(prioritySchedulesProvider.notifier).fetchPrioritySchedules();
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(prioritySchedulesProvider);

    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FB),
      body: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Priority Schedules',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 22,
                fontWeight: FontWeight.w700,
                color: _navy,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              'Manage recurring priority placements for disability-cleared students',
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
                  onPressed: () => _showCreatePriorityDialog(context, ref),
                  icon: const Icon(Icons.add, size: 18),
                  label: const Text('Add Priority Schedule'),
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
      BuildContext context, WidgetRef ref, PrioritySchedulesState state) {
    switch (state.status) {
      case ScheduleStatus.idle:
      case ScheduleStatus.loading:
        return const Center(child: CircularProgressIndicator(strokeWidth: 3));
      case ScheduleStatus.error:
        return _ErrorPanel(
          message: state.errorMessage ?? 'An error occurred',
          onRetry: () => ref
              .read(prioritySchedulesProvider.notifier)
              .fetchPrioritySchedules(),
        );
      case ScheduleStatus.success:
        if (state.items.isEmpty) {
          return Center(
            child: Text(
              'No priority schedules found',
              style: GoogleFonts.hankenGrotesk(
                  fontSize: 14, color: const Color(0xFF5A6B7B)),
            ),
          );
        }
        return _buildTable(context, ref, state.items);
    }
  }

  Widget _buildTable(
      BuildContext context, WidgetRef ref, List<PriorityScheduleModel> items) {
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
              DataColumn(label: Text('STUDENT NUMBER')),
              DataColumn(label: Text('STUDENT NAME')),
              DataColumn(label: Text('STOP')),
              DataColumn(label: Text('DAY OF WEEK')),
              DataColumn(label: Text('STATUS')),
              DataColumn(label: Text('ACTIONS')),
            ],
            rows: items.map((item) {
              return DataRow(cells: [
                DataCell(Text(item.studentNumber ?? '${item.studentId}')),
                DataCell(Text(item.studentName ?? '-')),
                DataCell(Text(item.stopName ?? '${item.stopId}')),
                DataCell(Text(item.dayOfWeek)),
                DataCell(_ActiveStatusChip(isActive: item.isActive)),
                DataCell(
                  item.isActive
                      ? IconButton(
                          icon: Icon(Icons.block_outlined,
                              size: 18, color: Colors.orange.shade700),
                          tooltip: 'Deactivate',
                          onPressed: () =>
                              _confirmDeactivate(context, ref, item),
                        )
                      : const SizedBox.shrink(),
                ),
              ]);
            }).toList(),
          ),
        ),
      ),
    );
  }

  void _showCreatePriorityDialog(BuildContext context, WidgetRef ref) {
    showDialog(
      context: context,
      builder: (ctx) => _PriorityScheduleFormDialog(
        onSubmit: (studentId, stopId, scheduleTimeId, dayOfWeek) {
          ref.read(prioritySchedulesProvider.notifier).addPrioritySchedule(
                studentId: studentId,
                stopId: stopId,
                scheduleTimeId: scheduleTimeId,
                dayOfWeek: dayOfWeek,
              );
        },
      ),
    );
  }

  void _confirmDeactivate(
      BuildContext context, WidgetRef ref, PriorityScheduleModel item) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(
          'Deactivate Priority Schedule',
          style: GoogleFonts.hankenGrotesk(
            fontWeight: FontWeight.w600,
            color: _navy,
          ),
        ),
        content: Text(
          'Are you sure you want to deactivate this priority schedule for student ${item.studentNumber ?? item.studentId}?',
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
                  .read(prioritySchedulesProvider.notifier)
                  .deactivatePrioritySchedule(item.id);
              Navigator.of(ctx).pop();
            },
            style: FilledButton.styleFrom(
              backgroundColor: Colors.orange.shade700,
            ),
            child: const Text('Deactivate'),
          ),
        ],
      ),
    );
  }
}

// ─── Form Dialog ─────────────────────────────────────────────────────────────

class _PriorityScheduleFormDialog extends StatefulWidget {
  const _PriorityScheduleFormDialog({required this.onSubmit});

  final void Function(
      int studentId, int stopId, int scheduleTimeId, String dayOfWeek) onSubmit;

  @override
  State<_PriorityScheduleFormDialog> createState() =>
      _PriorityScheduleFormDialogState();
}

class _PriorityScheduleFormDialogState
    extends State<_PriorityScheduleFormDialog> {
  static const Color _navy = Color(0xFF132E51);

  final _formKey = GlobalKey<FormState>();
  final _studentIdController = TextEditingController();
  final _stopIdController = TextEditingController();
  final _scheduleTimeIdController = TextEditingController();
  String _dayOfWeek = 'Monday';

  static const List<String> _daysOfWeek = [
    'Monday',
    'Tuesday',
    'Wednesday',
    'Thursday',
    'Friday',
    'Saturday',
  ];

  @override
  void dispose() {
    _studentIdController.dispose();
    _stopIdController.dispose();
    _scheduleTimeIdController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(
        'Add Priority Schedule',
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
                controller: _studentIdController,
                decoration: InputDecoration(
                  labelText: 'Student ID',
                  labelStyle: GoogleFonts.hankenGrotesk(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                  prefixIcon: const Icon(Icons.person_outlined),
                ),
                style: GoogleFonts.hankenGrotesk(),
                keyboardType: TextInputType.number,
                inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                validator: (v) =>
                    Validators.positiveInt(v, fieldName: 'Student ID'),
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _stopIdController,
                decoration: InputDecoration(
                  labelText: 'Stop ID',
                  labelStyle: GoogleFonts.hankenGrotesk(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                  prefixIcon: const Icon(Icons.location_on_outlined),
                ),
                style: GoogleFonts.hankenGrotesk(),
                keyboardType: TextInputType.number,
                inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                validator: (v) =>
                    Validators.positiveInt(v, fieldName: 'Stop ID'),
              ),
              const SizedBox(height: 16),
              TextFormField(
                controller: _scheduleTimeIdController,
                decoration: InputDecoration(
                  labelText: 'Schedule Time ID',
                  labelStyle: GoogleFonts.hankenGrotesk(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                  prefixIcon: const Icon(Icons.schedule_outlined),
                ),
                style: GoogleFonts.hankenGrotesk(),
                keyboardType: TextInputType.number,
                inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                validator: (v) =>
                    Validators.positiveInt(v, fieldName: 'Schedule Time ID'),
              ),
              const SizedBox(height: 16),
              DropdownButtonFormField<String>(
                initialValue: _dayOfWeek,
                decoration: InputDecoration(
                  labelText: 'Day of Week',
                  labelStyle: GoogleFonts.hankenGrotesk(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
                style: GoogleFonts.hankenGrotesk(color: _navy),
                items: _daysOfWeek
                    .map((d) => DropdownMenuItem(value: d, child: Text(d)))
                    .toList(),
                onChanged: (v) =>
                    setState(() => _dayOfWeek = v ?? 'Monday'),
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

  void _submit() {
    if (!_formKey.currentState!.validate()) return;
    final studentId = int.tryParse(_studentIdController.text) ?? 0;
    final stopId = int.tryParse(_stopIdController.text) ?? 0;
    final scheduleTimeId = int.tryParse(_scheduleTimeIdController.text) ?? 0;
    widget.onSubmit(studentId, stopId, scheduleTimeId, _dayOfWeek);
    Navigator.of(context).pop();
  }
}

// ─── Chip Widgets ────────────────────────────────────────────────────────────

class _ActiveStatusChip extends StatelessWidget {
  const _ActiveStatusChip({required this.isActive});
  final bool isActive;

  @override
  Widget build(BuildContext context) {
    final color = isActive ? Colors.green.shade700 : Colors.grey.shade600;
    final label = isActive ? 'Active' : 'Inactive';
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
