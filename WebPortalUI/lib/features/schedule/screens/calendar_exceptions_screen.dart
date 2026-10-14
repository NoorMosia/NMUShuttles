import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_admin/features/schedule/providers/schedule_providers.dart';

/// Standalone screen for managing Calendar Exceptions.
class CalendarExceptionsScreen extends ConsumerStatefulWidget {
  const CalendarExceptionsScreen({super.key});

  @override
  ConsumerState<CalendarExceptionsScreen> createState() =>
      _CalendarExceptionsScreenState();
}

class _CalendarExceptionsScreenState
    extends ConsumerState<CalendarExceptionsScreen> {
  static const Color _navy = Color(0xFF132E51);

  @override
  void initState() {
    super.initState();
    Future.microtask(() {
      ref
          .read(calendarExceptionsProvider.notifier)
          .fetchCalendarExceptions();
    });
  }

  @override
  Widget build(BuildContext context) {
    final state = ref.watch(calendarExceptionsProvider);

    return Scaffold(
      backgroundColor: const Color(0xFFF8F9FB),
      body: Padding(
        padding: const EdgeInsets.all(32),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              'Calendar Exceptions',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 22,
                fontWeight: FontWeight.w700,
                color: _navy,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              'Manage public holidays, recesses, and special events that affect shuttle operations',
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
                  onPressed: () => _showCreateExceptionDialog(context, ref),
                  icon: const Icon(Icons.add, size: 18),
                  label: const Text('Add Exception'),
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
                  fontSize: 14, color: const Color(0xFF5A6B7B)),
            ),
          );
        }
        return _buildTable(context, ref, state.items);
    }
  }

  Widget _buildTable(
      BuildContext context, WidgetRef ref, List<CalendarExceptionModel> items) {
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
              DataColumn(label: Text('TARGET DATE')),
              DataColumn(label: Text('EXCEPTION TYPE')),
              DataColumn(label: Text('DESCRIPTION')),
              DataColumn(label: Text('ACTIONS')),
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
        onSubmit: (targetDate, exceptionType, description) async {
          final messenger = ScaffoldMessenger.of(context);
          final ok = await ref
              .read(calendarExceptionsProvider.notifier)
              .addCalendarException(
                targetDate: targetDate,
                exceptionType: exceptionType,
                description: description,
              );
          if (!context.mounted) return;
          messenger.showSnackBar(
            SnackBar(
              content: Text(ok
                  ? 'Calendar exception added.'
                  : ref.read(calendarExceptionsProvider).errorMessage ??
                      'Could not add calendar exception. It may already exist for that date.'),
              backgroundColor: ok ? null : Colors.red.shade700,
            ),
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
            onPressed: () async {
              final messenger = ScaffoldMessenger.of(context);
              Navigator.of(ctx).pop();
              final ok = await ref
                  .read(calendarExceptionsProvider.notifier)
                  .deleteCalendarException(item.id);
              messenger.showSnackBar(
                SnackBar(
                  content: Text(ok
                      ? 'Calendar exception deleted.'
                      : 'Could not delete calendar exception.'),
                  backgroundColor: ok ? null : Colors.red.shade700,
                ),
              );
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
                ],
                onChanged: (v) =>
                    setState(() => _exceptionType = v ?? 'PublicHoliday'),
              ),
              const SizedBox(height: 16),
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

// ─── Chip Widgets ────────────────────────────────────────────────────────────

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
