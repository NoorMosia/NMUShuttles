import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_admin/features/reports/providers/reports_providers.dart';

/// Navy and gold brand colors.
const _navy = Color(0xFF132E51);
const _gold = Color(0xFFFFCC00);

/// Reports management screen with data table and expandable detail view.
class ReportsScreen extends ConsumerStatefulWidget {
  const ReportsScreen({super.key});

  @override
  ConsumerState<ReportsScreen> createState() => _ReportsScreenState();
}

class _ReportsScreenState extends ConsumerState<ReportsScreen> {
  int? _selectedReportId;

  @override
  void initState() {
    super.initState();
    // Fetch reports on first load.
    Future.microtask(() {
      ref.read(reportsProvider.notifier).fetchReports();
    });
  }

  @override
  Widget build(BuildContext context) {
    final reportsState = ref.watch(reportsProvider);

    return Padding(
      padding: const EdgeInsets.all(24),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Header
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Reports',
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 28,
                        fontWeight: FontWeight.bold,
                        color: _navy,
                      ),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Manage student reports and update statuses',
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 14,
                        color: Colors.grey.shade600,
                      ),
                    ),
                  ],
                ),
              ),
              // Refresh button
              IconButton(
                onPressed: () {
                  ref.read(reportsProvider.notifier).fetchReports();
                },
                icon: const Icon(Icons.refresh, color: _navy),
                tooltip: 'Refresh reports',
              ),
            ],
          ),
          const SizedBox(height: 24),

          // Content
          Expanded(
            child: _selectedReportId != null
                ? _ReportDetailView(
                    reportId: _selectedReportId!,
                    onBack: () {
                      setState(() => _selectedReportId = null);
                      ref.read(reportsProvider.notifier).fetchReports();
                    },
                  )
                : _buildReportsTable(reportsState),
          ),
        ],
      ),
    );
  }

  Widget _buildReportsTable(ReportsState reportsState) {
    if (reportsState.isLoading) {
      return const Center(
        child: CircularProgressIndicator(color: _navy),
      );
    }

    if (reportsState.errorMessage != null) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.error_outline, size: 48, color: Colors.red.shade300),
            const SizedBox(height: 12),
            Text(
              reportsState.errorMessage!,
              style: GoogleFonts.hankenGrotesk(
                fontSize: 16,
                color: Colors.red.shade700,
              ),
            ),
            const SizedBox(height: 16),
            ElevatedButton.icon(
              onPressed: () =>
                  ref.read(reportsProvider.notifier).fetchReports(),
              icon: const Icon(Icons.refresh),
              label: const Text('Retry'),
              style: ElevatedButton.styleFrom(
                backgroundColor: _navy,
                foregroundColor: Colors.white,
              ),
            ),
          ],
        ),
      );
    }

    if (reportsState.reports.isEmpty) {
      return Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.inbox_outlined, size: 48, color: Colors.grey.shade400),
            const SizedBox(height: 12),
            Text(
              'No reports found',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 16,
                color: Colors.grey.shade600,
              ),
            ),
          ],
        ),
      );
    }

    return Card(
      elevation: 1,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      clipBehavior: Clip.antiAlias,
      child: SingleChildScrollView(
        child: SizedBox(
          width: double.infinity,
          child: DataTable(
            headingRowColor: WidgetStateProperty.all(
              _navy.withValues(alpha: 0.05),
            ),
            dataRowColor: WidgetStateProperty.resolveWith<Color?>(
              (states) {
                if (states.contains(WidgetState.hovered)) {
                  return Colors.grey.shade200;
                }
                return null;
              },
            ),
            headingTextStyle: GoogleFonts.hankenGrotesk(
              fontWeight: FontWeight.w600,
              color: _navy,
              fontSize: 13,
            ),
            dataTextStyle: GoogleFonts.hankenGrotesk(
              fontSize: 13,
              color: Colors.grey.shade800,
            ),
            columnSpacing: 24,
            columns: const [
              DataColumn(label: Text('Report ID')),
              DataColumn(label: Text('Category')),
              DataColumn(label: Text('Student Number')),
              DataColumn(label: Text('Submission Date')),
              DataColumn(label: Text('Status')),
            ],
            rows: reportsState.reports.map((report) {
              return DataRow(
                onSelectChanged: (_) {
                  setState(() => _selectedReportId = report.id);
                  ref.read(reportDetailProvider.notifier).fetchReport(report.id);
                },
                cells: [
                  DataCell(Text('#${report.id}')),
                  DataCell(Text(report.category)),
                  DataCell(Text(report.studentNumber)),
                  DataCell(Text(_formatDate(report.submissionDate))),
                  DataCell(_StatusChip(status: report.status)),
                ],
              );
            }).toList(),
          ),
        ),
      ),
    );
  }

  String _formatDate(String dateStr) {
    try {
      final date = DateTime.parse(dateStr);
      return DateFormat('dd MMM yyyy, HH:mm').format(date);
    } catch (_) {
      return dateStr;
    }
  }
}

/// Detail view for a single report with status update form.
class _ReportDetailView extends ConsumerStatefulWidget {
  const _ReportDetailView({
    required this.reportId,
    required this.onBack,
  });

  final int reportId;
  final VoidCallback onBack;

  @override
  ConsumerState<_ReportDetailView> createState() => _ReportDetailViewState();
}

class _ReportDetailViewState extends ConsumerState<_ReportDetailView> {
  final _adminNotesController = TextEditingController();
  String _selectedStatus = 'UnderReview';
  bool _isSubmitting = false;

  @override
  void dispose() {
    _adminNotesController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final detailState = ref.watch(reportDetailProvider);

    // Pre-fill form when report loads
    ref.listen<ReportDetailState>(reportDetailProvider, (prev, next) {
      if (next.report != null && prev?.report?.id != next.report?.id) {
        setState(() {
          _selectedStatus = next.report!.status == 'Submitted'
              ? 'UnderReview'
              : next.report!.status;
          _adminNotesController.text = next.report!.adminNotes ?? '';
        });
      }
    });

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Back button
        TextButton.icon(
          onPressed: widget.onBack,
          icon: const Icon(Icons.arrow_back, size: 18),
          label: Text(
            'Back to Reports',
            style: GoogleFonts.hankenGrotesk(fontWeight: FontWeight.w500),
          ),
          style: TextButton.styleFrom(foregroundColor: _navy),
        ),
        const SizedBox(height: 16),

        if (detailState.isLoading)
          const Expanded(
            child: Center(
              child: CircularProgressIndicator(color: _navy),
            ),
          )
        else if (detailState.errorMessage != null)
          Expanded(
            child: Center(
              child: Text(
                detailState.errorMessage!,
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 16,
                  color: Colors.red.shade700,
                ),
              ),
            ),
          )
        else if (detailState.report != null)
          Expanded(
            child: SingleChildScrollView(
              child: _buildDetailContent(detailState.report!),
            ),
          ),
      ],
    );
  }

  Widget _buildDetailContent(Report report) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Report info card
        Expanded(
          flex: 3,
          child: Card(
            elevation: 1,
            shape:
                RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  // Title row
                  Row(
                    children: [
                      Text(
                        'Report #${report.id}',
                        style: GoogleFonts.hankenGrotesk(
                          fontSize: 20,
                          fontWeight: FontWeight.bold,
                          color: _navy,
                        ),
                      ),
                      const SizedBox(width: 12),
                      _StatusChip(status: report.status),
                    ],
                  ),
                  const SizedBox(height: 24),
                  const Divider(),
                  const SizedBox(height: 16),

                  // Info grid
                  _DetailRow(label: 'Category', value: report.category),
                  const SizedBox(height: 12),
                  _DetailRow(
                      label: 'Student Number', value: report.studentNumber),
                  const SizedBox(height: 12),
                  _DetailRow(
                    label: 'Submission Date',
                    value: _formatDate(report.submissionDate),
                  ),
                  if (report.tripInstanceId != null) ...[
                    const SizedBox(height: 12),
                    _DetailRow(
                      label: 'Linked Trip Instance',
                      value: '#${report.tripInstanceId}',
                    ),
                  ],
                  const SizedBox(height: 24),
                  const Divider(),
                  const SizedBox(height: 16),

                  // Description
                  Text(
                    'Description',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 14,
                      fontWeight: FontWeight.w600,
                      color: _navy,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Container(
                    width: double.infinity,
                    padding: const EdgeInsets.all(16),
                    decoration: BoxDecoration(
                      color: Colors.grey.shade50,
                      borderRadius: BorderRadius.circular(8),
                      border: Border.all(color: Colors.grey.shade200),
                    ),
                    child: Text(
                      report.description?.isNotEmpty == true
                          ? report.description!
                          : 'No description provided',
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 14,
                        color: Colors.grey.shade800,
                        height: 1.5,
                      ),
                    ),
                  ),

                  if (report.adminNotes?.isNotEmpty == true) ...[
                    const SizedBox(height: 24),
                    Text(
                      'Admin Notes',
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 14,
                        fontWeight: FontWeight.w600,
                        color: _navy,
                      ),
                    ),
                    const SizedBox(height: 8),
                    Container(
                      width: double.infinity,
                      padding: const EdgeInsets.all(16),
                      decoration: BoxDecoration(
                        color: _gold.withValues(alpha: 0.08),
                        borderRadius: BorderRadius.circular(8),
                        border: Border.all(color: _gold.withValues(alpha: 0.3)),
                      ),
                      child: Text(
                        report.adminNotes!,
                        style: GoogleFonts.hankenGrotesk(
                          fontSize: 14,
                          color: Colors.grey.shade800,
                          height: 1.5,
                        ),
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ),
        ),
        const SizedBox(width: 24),

        // Status update form
        Expanded(
          flex: 2,
          child: Card(
            elevation: 1,
            shape:
                RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
            child: Padding(
              padding: const EdgeInsets.all(24),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Update Status',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                      color: _navy,
                    ),
                  ),
                  const SizedBox(height: 20),

                  // Status dropdown
                  Text(
                    'Status',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                      color: Colors.grey.shade700,
                    ),
                  ),
                  const SizedBox(height: 8),
                  DropdownButtonFormField<String>(
                    value: _selectedStatus,
                    decoration: InputDecoration(
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                      contentPadding: const EdgeInsets.symmetric(
                        horizontal: 12,
                        vertical: 12,
                      ),
                    ),
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 14,
                      color: Colors.grey.shade800,
                    ),
                    items: const [
                      DropdownMenuItem(
                        value: 'UnderReview',
                        child: Text('Under Review'),
                      ),
                      DropdownMenuItem(
                        value: 'Resolved',
                        child: Text('Resolved'),
                      ),
                    ],
                    onChanged: (value) {
                      if (value != null) {
                        setState(() => _selectedStatus = value);
                      }
                    },
                  ),
                  const SizedBox(height: 20),

                  // Admin notes text field
                  Text(
                    'Admin Notes',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                      color: Colors.grey.shade700,
                    ),
                  ),
                  const SizedBox(height: 8),
                  TextField(
                    controller: _adminNotesController,
                    maxLines: 5,
                    style: GoogleFonts.hankenGrotesk(fontSize: 14),
                    decoration: InputDecoration(
                      hintText: 'Add notes about this report...',
                      hintStyle: GoogleFonts.hankenGrotesk(
                        fontSize: 14,
                        color: Colors.grey.shade400,
                      ),
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(8),
                      ),
                      contentPadding: const EdgeInsets.all(12),
                    ),
                  ),
                  const SizedBox(height: 24),

                  // Submit button
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: _isSubmitting ? null : _handleStatusUpdate,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: _navy,
                        foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(8),
                        ),
                      ),
                      child: _isSubmitting
                          ? const SizedBox(
                              height: 20,
                              width: 20,
                              child: CircularProgressIndicator(
                                strokeWidth: 2,
                                color: Colors.white,
                              ),
                            )
                          : Text(
                              'Update Report',
                              style: GoogleFonts.hankenGrotesk(
                                fontSize: 14,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ),
      ],
    );
  }

  Future<void> _handleStatusUpdate() async {
    setState(() => _isSubmitting = true);

    final error = await ref.read(reportDetailProvider.notifier).updateReportStatus(
          id: widget.reportId,
          status: _selectedStatus,
          adminNotes: _adminNotesController.text.trim(),
        );

    if (!mounted) return;
    setState(() => _isSubmitting = false);

    if (error != null) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(error),
          backgroundColor: Colors.red.shade600,
        ),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            'Report status updated successfully',
            style: GoogleFonts.hankenGrotesk(),
          ),
          backgroundColor: Colors.green.shade600,
        ),
      );
    }
  }

  String _formatDate(String dateStr) {
    try {
      final date = DateTime.parse(dateStr);
      return DateFormat('dd MMM yyyy, HH:mm').format(date);
    } catch (_) {
      return dateStr;
    }
  }
}

/// Color-coded status chip widget.
/// Blue = Submitted, Orange = UnderReview, Green = Resolved.
class _StatusChip extends StatelessWidget {
  const _StatusChip({required this.status});

  final String status;

  @override
  Widget build(BuildContext context) {
    final (Color bg, Color fg, String label) = switch (status) {
      'Submitted' => (
          Colors.blue.shade50,
          Colors.blue.shade700,
          'Submitted',
        ),
      'UnderReview' => (
          Colors.orange.shade50,
          Colors.orange.shade700,
          'Under Review',
        ),
      'Resolved' => (
          Colors.green.shade50,
          Colors.green.shade700,
          'Resolved',
        ),
      _ => (
          Colors.grey.shade100,
          Colors.grey.shade700,
          status,
        ),
    };

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
      decoration: BoxDecoration(
        color: bg,
        borderRadius: BorderRadius.circular(12),
      ),
      child: Text(
        label,
        style: GoogleFonts.hankenGrotesk(
          fontSize: 12,
          fontWeight: FontWeight.w600,
          color: fg,
        ),
      ),
    );
  }
}

/// Helper widget for label-value rows in the detail view.
class _DetailRow extends StatelessWidget {
  const _DetailRow({required this.label, required this.value});

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        SizedBox(
          width: 160,
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
              fontSize: 14,
              color: Colors.grey.shade800,
            ),
          ),
        ),
      ],
    );
  }
}
