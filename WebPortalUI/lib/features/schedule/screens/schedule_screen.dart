import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_admin/features/schedule/providers/schedule_providers.dart';

/// Schedule management screen with 3 tabs:
/// Schedule Times, Calendar Exceptions, Priority Schedules.
class ScheduleScreen extends ConsumerStatefulWidget {
  const ScheduleScreen({super.key});

  @override
  ConsumerState<ScheduleScreen> createState() => _ScheduleScreenState();
}

class _ScheduleScreenState extends ConsumerState<ScheduleScreen>
    with SingleTickerProviderStateMixin {
  static const Color _navy = Color(0xFF132E51);
  static const Color _gold = Color(0xFFFFCC00);

  late TabController _tabController;

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 3, vsync: this);
    // Fetch data on load
    Future.microtask(() {
      ref.read(scheduleTimesProvider.notifier).fetchScheduleTimes();
      ref.read(calendarExceptionsProvider.notifier).fetchCalendarExceptions();
      ref.read(prioritySchedulesProvider.notifier).fetchPrioritySchedules();
    });
  }

  @override
  void dispose() {
    _tabController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF5F5F5),
      body: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Schedule',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 28,
                fontWeight: FontWeight.bold,
                color: _navy,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              'Manage shuttle schedules, calendar exceptions, and priority placements',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 14,
                color: Colors.grey.shade600,
              ),
            ),
            const SizedBox(height: 24),
            // Tab bar
            Container(
              decoration: BoxDecoration(
                color: Colors.white,
                borderRadius: BorderRadius.circular(12),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.05),
                    blurRadius: 10,
                    offset: const Offset(0, 2),
                  ),
                ],
              ),
              child: TabBar(
                controller: _tabController,
                labelColor: _navy,
                unselectedLabelColor: Colors.grey.shade500,
                indicatorColor: _gold,
                indicatorWeight: 3,
                labelStyle: GoogleFonts.hankenGrotesk(
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                ),
                unselectedLabelStyle: GoogleFonts.hankenGrotesk(
                  fontSize: 14,
                  fontWeight: FontWeight.w500,
                ),
                tabs: const [
                  Tab(text: 'Schedule Times'),
                  Tab(text: 'Calendar Exceptions'),
                  Tab(text: 'Priority Schedules'),
                ],
              ),
            ),
            const SizedBox(height: 16),
            // Tab views
            Expanded(
              child: TabBarView(
                controller: _tabController,
                children: const [
                  _ScheduleTimesTab(),
                  _CalendarExceptionsTab(),
                  _PrioritySchedulesTab(),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}


// ─── Schedule Times Tab ──────────────────────────────────────────────────────

class _ScheduleTimesTab extends ConsumerWidget {
  const _ScheduleTimesTab();

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(scheduleTimesProvider);

    return Column(
      children: [
        // Action bar
        Row(
          mainAxisAlignment: MainAxisAlignment.end,
          children: [
            FilledButton.icon(
              onPressed: () => _showCreateScheduleTimeDialog(context, ref),
              icon: const Icon(Icons.add, size: 18),
              label: const Text('Add Schedule Time'),
              style: FilledButton.styleFrom(
                backgroundColor: _navy,
                foregroundColor: Colors.white,
                padding:
                    const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
                textStyle: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600,
                  fontSize: 13,
                ),
              ),
            ),
          ],
        ),
        const SizedBox(height: 16),
        // Content
        Expanded(
          child: _buildContent(context, ref, state),
        ),
      ],
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
                  fontSize: 14, color: Colors.grey.shade600),
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
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: SingleChildScrollView(
        child: SizedBox(
          width: double.infinity,
          child: DataTable(
            headingRowColor:
                WidgetStateProperty.all(_navy.withValues(alpha: 0.05)),
            dataRowColor: WidgetStateProperty.resolveWith<Color?>(
              (states) {
                if (states.contains(WidgetState.hovered)) {
                  return Colors.grey.shade200;
                }
                return null;
              },
            ),
            headingTextStyle: GoogleFonts.hankenGrotesk(
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: _navy,
            ),
            dataTextStyle: GoogleFonts.hankenGrotesk(
              fontSize: 13,
              color: _navy,
            ),
            columnSpacing: 24,
            columns: const [
              DataColumn(label: Text('Departure Time')),
              DataColumn(label: Text('Direction')),
              DataColumn(label: Text('Schedule Type')),
              DataColumn(label: Text('Campus ID')),
              DataColumn(label: Text('Stop ID')),
              DataColumn(label: Text('Actions')),
            ],
            rows: items.map((item) {
              return DataRow(cells: [
                DataCell(Text(item.departureTime)),
                DataCell(_DirectionChip(direction: item.direction)),
                DataCell(_ScheduleTypeChip(type: item.scheduleType)),
                DataCell(Text('${item.campusId}')),
                DataCell(Text('${item.stopId}')),
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
                      onPressed: () =>
                          _confirmDelete(context, ref, item),
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
        onSubmit: (departureTime, direction, scheduleType, campusId, stopId) {
          ref.read(scheduleTimesProvider.notifier).addScheduleTime(
                departureTime: departureTime,
                direction: direction,
                scheduleType: scheduleType,
                campusId: campusId,
                stopId: stopId,
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
        initialCampusId: item.campusId,
        initialStopId: item.stopId,
        onSubmit: (departureTime, direction, scheduleType, campusId, stopId) {
          ref.read(scheduleTimesProvider.notifier).updateScheduleTime(
                id: item.id,
                departureTime: departureTime,
                direction: direction,
                scheduleType: scheduleType,
                campusId: campusId,
                stopId: stopId,
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


// ─── Calendar Exceptions Tab ─────────────────────────────────────────────────

class _CalendarExceptionsTab extends ConsumerWidget {
  const _CalendarExceptionsTab();

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(calendarExceptionsProvider);

    return Column(
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.end,
          children: [
            FilledButton.icon(
              onPressed: () => _showCreateExceptionDialog(context, ref),
              icon: const Icon(Icons.add, size: 18),
              label: const Text('Add Exception'),
              style: FilledButton.styleFrom(
                backgroundColor: _navy,
                foregroundColor: Colors.white,
                padding:
                    const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
                textStyle: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600,
                  fontSize: 13,
                ),
              ),
            ),
          ],
        ),
        const SizedBox(height: 16),
        Expanded(
          child: _buildContent(context, ref, state),
        ),
      ],
    );
  }

  Widget _buildContent(
      BuildContext context, WidgetRef ref, CalendarExceptionsState state) {
    switch (state.status) {
      case ScheduleStatus.idle:
      case ScheduleStatus.loading:
        return const Center(child: CircularProgressIndicator(strokeWidth: 3));
      case ScheduleStatus.error:
        return _ErrorPanel(
          message: state.errorMessage ?? 'An error occurred',
          onRetry: () => ref
              .read(calendarExceptionsProvider.notifier)
              .fetchCalendarExceptions(),
        );
      case ScheduleStatus.success:
        if (state.items.isEmpty) {
          return Center(
            child: Text(
              'No calendar exceptions found',
              style: GoogleFonts.hankenGrotesk(
                  fontSize: 14, color: Colors.grey.shade600),
            ),
          );
        }
        return _buildTable(context, ref, state.items);
    }
  }

  Widget _buildTable(BuildContext context, WidgetRef ref,
      List<CalendarExceptionModel> items) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: SingleChildScrollView(
        child: SizedBox(
          width: double.infinity,
          child: DataTable(
            headingRowColor:
                WidgetStateProperty.all(_navy.withValues(alpha: 0.05)),
            dataRowColor: WidgetStateProperty.resolveWith<Color?>(
              (states) {
                if (states.contains(WidgetState.hovered)) {
                  return Colors.grey.shade200;
                }
                return null;
              },
            ),
            headingTextStyle: GoogleFonts.hankenGrotesk(
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: _navy,
            ),
            dataTextStyle: GoogleFonts.hankenGrotesk(
              fontSize: 13,
              color: _navy,
            ),
            columnSpacing: 24,
            columns: const [
              DataColumn(label: Text('Target Date')),
              DataColumn(label: Text('Exception Type')),
              DataColumn(label: Text('Description')),
              DataColumn(label: Text('Actions')),
            ],
            rows: items.map((item) {
              return DataRow(cells: [
                DataCell(Text(_formatDate(item.targetDate))),
                DataCell(_ExceptionTypeChip(type: item.exceptionType)),
                DataCell(
                  ConstrainedBox(
                    constraints: const BoxConstraints(maxWidth: 300),
                    child: Text(
                      item.description,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                ),
                DataCell(
                  IconButton(
                    icon: Icon(Icons.delete_outlined,
                        size: 18, color: Colors.red.shade700),
                    tooltip: 'Delete',
                    onPressed: () => _confirmDelete(context, ref, item),
                  ),
                ),
              ]);
            }).toList(),
          ),
        ),
      ),
    );
  }

  String _formatDate(String dateStr) {
    try {
      final date = DateTime.parse(dateStr);
      return DateFormat('dd MMM yyyy').format(date);
    } catch (_) {
      return dateStr;
    }
  }

  void _showCreateExceptionDialog(BuildContext context, WidgetRef ref) {
    showDialog(
      context: context,
      builder: (ctx) => _CalendarExceptionFormDialog(
        onSubmit: (targetDate, exceptionType, description) {
          ref.read(calendarExceptionsProvider.notifier).addCalendarException(
                targetDate: targetDate,
                exceptionType: exceptionType,
                description: description,
              );
        },
      ),
    );
  }

  void _confirmDelete(
      BuildContext context, WidgetRef ref, CalendarExceptionModel item) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(
          'Delete Calendar Exception',
          style: GoogleFonts.hankenGrotesk(
            fontWeight: FontWeight.w600,
            color: _navy,
          ),
        ),
        content: Text(
          'Are you sure you want to delete the exception for ${_formatDate(item.targetDate)}?',
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
                  .read(calendarExceptionsProvider.notifier)
                  .deleteCalendarException(item.id);
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


// ─── Priority Schedules Tab ──────────────────────────────────────────────────

class _PrioritySchedulesTab extends ConsumerWidget {
  const _PrioritySchedulesTab();

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final state = ref.watch(prioritySchedulesProvider);

    return Column(
      children: [
        Row(
          mainAxisAlignment: MainAxisAlignment.end,
          children: [
            FilledButton.icon(
              onPressed: () => _showCreatePriorityDialog(context, ref),
              icon: const Icon(Icons.add, size: 18),
              label: const Text('Add Priority Schedule'),
              style: FilledButton.styleFrom(
                backgroundColor: _navy,
                foregroundColor: Colors.white,
                padding:
                    const EdgeInsets.symmetric(horizontal: 20, vertical: 12),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(10),
                ),
                textStyle: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600,
                  fontSize: 13,
                ),
              ),
            ),
          ],
        ),
        const SizedBox(height: 16),
        Expanded(
          child: _buildContent(context, ref, state),
        ),
      ],
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
                  fontSize: 14, color: Colors.grey.shade600),
            ),
          );
        }
        return _buildTable(context, ref, state.items);
    }
  }

  Widget _buildTable(BuildContext context, WidgetRef ref,
      List<PriorityScheduleModel> items) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: SingleChildScrollView(
        child: SizedBox(
          width: double.infinity,
          child: DataTable(
            headingRowColor:
                WidgetStateProperty.all(_navy.withValues(alpha: 0.05)),
            dataRowColor: WidgetStateProperty.resolveWith<Color?>(
              (states) {
                if (states.contains(WidgetState.hovered)) {
                  return Colors.grey.shade200;
                }
                return null;
              },
            ),
            headingTextStyle: GoogleFonts.hankenGrotesk(
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: _navy,
            ),
            dataTextStyle: GoogleFonts.hankenGrotesk(
              fontSize: 13,
              color: _navy,
            ),
            columnSpacing: 24,
            columns: const [
              DataColumn(label: Text('Student Number')),
              DataColumn(label: Text('Student Name')),
              DataColumn(label: Text('Stop')),
              DataColumn(label: Text('Day of Week')),
              DataColumn(label: Text('Status')),
              DataColumn(label: Text('Actions')),
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
          'Are you sure you want to deactivate this priority schedule for student ${item.studentId}?',
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


// ─── Schedule Time Form Dialog ───────────────────────────────────────────────

class _ScheduleTimeFormDialog extends StatefulWidget {
  const _ScheduleTimeFormDialog({
    required this.title,
    required this.onSubmit,
    this.initialDepartureTime,
    this.initialDirection,
    this.initialScheduleType,
    this.initialCampusId,
    this.initialStopId,
  });

  final String title;
  final void Function(String departureTime, String direction,
      String scheduleType, int campusId, int stopId) onSubmit;
  final String? initialDepartureTime;
  final String? initialDirection;
  final String? initialScheduleType;
  final int? initialCampusId;
  final int? initialStopId;

  @override
  State<_ScheduleTimeFormDialog> createState() =>
      _ScheduleTimeFormDialogState();
}

class _ScheduleTimeFormDialogState extends State<_ScheduleTimeFormDialog> {
  static const Color _navy = Color(0xFF132E51);

  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _departureTimeController;
  late final TextEditingController _campusIdController;
  late final TextEditingController _stopIdController;
  late String _direction;
  late String _scheduleType;

  @override
  void initState() {
    super.initState();
    _departureTimeController =
        TextEditingController(text: widget.initialDepartureTime ?? '');
    _campusIdController =
        TextEditingController(text: widget.initialCampusId?.toString() ?? '');
    _stopIdController =
        TextEditingController(text: widget.initialStopId?.toString() ?? '');
    _direction = widget.initialDirection ?? 'ToCampus';
    _scheduleType = widget.initialScheduleType ?? 'Weekday';
  }

  @override
  void dispose() {
    _departureTimeController.dispose();
    _campusIdController.dispose();
    _stopIdController.dispose();
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
              // Departure Time
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
              // Direction
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
                onChanged: (v) => setState(() => _direction = v ?? 'ToCampus'),
              ),
              const SizedBox(height: 16),
              // Schedule Type
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
                  DropdownMenuItem(value: 'Saturday', child: Text('Saturday')),
                ],
                onChanged: (v) =>
                    setState(() => _scheduleType = v ?? 'Weekday'),
              ),
              const SizedBox(height: 16),
              // Campus ID
              TextFormField(
                controller: _campusIdController,
                decoration: InputDecoration(
                  labelText: 'Campus ID',
                  labelStyle: GoogleFonts.hankenGrotesk(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                  prefixIcon: const Icon(Icons.school_outlined),
                ),
                style: GoogleFonts.hankenGrotesk(),
                keyboardType: TextInputType.number,
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Required' : null,
              ),
              const SizedBox(height: 16),
              // Stop ID
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
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Required' : null,
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
          style: FilledButton.styleFrom(
            backgroundColor: _navy,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
          ),
          child: Text('Save', style: GoogleFonts.hankenGrotesk()),
        ),
      ],
    );
  }

  void _submit() {
    if (!_formKey.currentState!.validate()) return;
    final campusId = int.tryParse(_campusIdController.text) ?? 0;
    final stopId = int.tryParse(_stopIdController.text) ?? 0;
    widget.onSubmit(
      _departureTimeController.text,
      _direction,
      _scheduleType,
      campusId,
      stopId,
    );
    Navigator.of(context).pop();
  }
}


// ─── Calendar Exception Form Dialog ─────────────────────────────────────────

class _CalendarExceptionFormDialog extends StatefulWidget {
  const _CalendarExceptionFormDialog({required this.onSubmit});

  final void Function(
      String targetDate, String exceptionType, String description) onSubmit;

  @override
  State<_CalendarExceptionFormDialog> createState() =>
      _CalendarExceptionFormDialogState();
}

class _CalendarExceptionFormDialogState
    extends State<_CalendarExceptionFormDialog> {
  static const Color _navy = Color(0xFF132E51);

  final _formKey = GlobalKey<FormState>();
  final _descriptionController = TextEditingController();
  DateTime? _selectedDate;
  String _exceptionType = 'PublicHoliday';

  @override
  void dispose() {
    _descriptionController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return AlertDialog(
      title: Text(
        'Add Calendar Exception',
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
              // Target Date
              InkWell(
                onTap: _pickDate,
                child: InputDecorator(
                  decoration: InputDecoration(
                    labelText: 'Target Date',
                    labelStyle: GoogleFonts.hankenGrotesk(),
                    border: OutlineInputBorder(
                      borderRadius: BorderRadius.circular(10),
                    ),
                    prefixIcon: const Icon(Icons.calendar_today),
                  ),
                  child: Text(
                    _selectedDate != null
                        ? DateFormat('dd MMM yyyy').format(_selectedDate!)
                        : 'Select a date',
                    style: GoogleFonts.hankenGrotesk(
                      color: _selectedDate != null ? _navy : Colors.grey,
                    ),
                  ),
                ),
              ),
              const SizedBox(height: 16),
              // Exception Type
              DropdownButtonFormField<String>(
                initialValue: _exceptionType,
                decoration: InputDecoration(
                  labelText: 'Exception Type',
                  labelStyle: GoogleFonts.hankenGrotesk(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                ),
                style: GoogleFonts.hankenGrotesk(color: _navy),
                items: const [
                  DropdownMenuItem(
                      value: 'PublicHoliday', child: Text('Public Holiday')),
                  DropdownMenuItem(
                      value: 'UniversityRecess',
                      child: Text('University Recess')),
                  DropdownMenuItem(
                      value: 'SpecialEvent', child: Text('Special Event')),
                ],
                onChanged: (v) =>
                    setState(() => _exceptionType = v ?? 'PublicHoliday'),
              ),
              const SizedBox(height: 16),
              // Description
              TextFormField(
                controller: _descriptionController,
                decoration: InputDecoration(
                  labelText: 'Description',
                  labelStyle: GoogleFonts.hankenGrotesk(),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(10),
                  ),
                  prefixIcon: const Icon(Icons.description_outlined),
                ),
                style: GoogleFonts.hankenGrotesk(),
                maxLines: 2,
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Required' : null,
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
          style: FilledButton.styleFrom(
            backgroundColor: _navy,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
          ),
          child: Text('Save', style: GoogleFonts.hankenGrotesk()),
        ),
      ],
    );
  }

  Future<void> _pickDate() async {
    final picked = await showDatePicker(
      context: context,
      initialDate: _selectedDate ?? DateTime.now(),
      firstDate: DateTime(2020),
      lastDate: DateTime(2030),
    );
    if (picked != null) {
      setState(() => _selectedDate = picked);
    }
  }

  void _submit() {
    if (!_formKey.currentState!.validate()) return;
    if (_selectedDate == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please select a target date')),
      );
      return;
    }
    final dateStr = DateFormat('yyyy-MM-dd').format(_selectedDate!);
    widget.onSubmit(dateStr, _exceptionType, _descriptionController.text);
    Navigator.of(context).pop();
  }
}


// ─── Priority Schedule Form Dialog ───────────────────────────────────────────

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
              // Student ID
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
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Required' : null,
              ),
              const SizedBox(height: 16),
              // Stop ID
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
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Required' : null,
              ),
              const SizedBox(height: 16),
              // Schedule Time ID
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
                validator: (v) =>
                    (v == null || v.isEmpty) ? 'Required' : null,
              ),
              const SizedBox(height: 16),
              // Day of Week
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
          style: FilledButton.styleFrom(
            backgroundColor: _navy,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(10),
            ),
          ),
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


// ─── Shared Chip / Badge Widgets ─────────────────────────────────────────────

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

class _ExceptionTypeChip extends StatelessWidget {
  const _ExceptionTypeChip({required this.type});
  final String type;

  @override
  Widget build(BuildContext context) {
    Color color;
    String label;
    switch (type) {
      case 'PublicHoliday':
        color = Colors.red.shade700;
        label = 'Public Holiday';
        break;
      case 'UniversityRecess':
        color = Colors.indigo.shade700;
        label = 'University Recess';
        break;
      case 'SpecialEvent':
        color = Colors.green.shade700;
        label = 'Special Event';
        break;
      default:
        color = Colors.grey.shade700;
        label = type;
    }
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
                color: Colors.grey.shade700,
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
