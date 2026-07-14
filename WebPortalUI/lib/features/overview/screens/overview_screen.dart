import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:intl/intl.dart';
import 'package:nmu_shuttle_admin/features/overview/models/overview_model.dart';
import 'package:nmu_shuttle_admin/features/overview/providers/overview_providers.dart';

/// Operations Overview dashboard — default landing page after login.
class OverviewScreen extends ConsumerStatefulWidget {
  const OverviewScreen({super.key});

  @override
  ConsumerState<OverviewScreen> createState() => _OverviewScreenState();
}

class _OverviewScreenState extends ConsumerState<OverviewScreen> {
  static const Color _navy = Color(0xFF132E51);

  late DateTime _fromDate;
  late DateTime _toDate;

  @override
  void initState() {
    super.initState();
    final now = DateTime.now();
    _fromDate = DateTime(now.year, 1, 1);
    _toDate = DateTime(now.year, 12, 31);
  }

  String _formatDate(DateTime date) => DateFormat('yyyy-MM-dd').format(date);

  @override
  Widget build(BuildContext context) {
    final overviewAsync = ref.watch(overviewProvider);
    final vendorAsync = ref.watch(
      vendorReliabilityProvider(
        DateRangeParams(
          fromDate: _formatDate(_fromDate),
          toDate: _formatDate(_toDate),
        ),
      ),
    );

    return Scaffold(
      backgroundColor: const Color(0xFFF5F5F5),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Page title
            Text(
              'Operations Overview',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 28,
                fontWeight: FontWeight.bold,
                color: _navy,
              ),
            ),
            const SizedBox(height: 4),
            Text(
              'Dashboard summary and key metrics',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 14,
                color: Colors.grey.shade600,
              ),
            ),
            const SizedBox(height: 24),

            // Main content
            overviewAsync.when(
              loading: () => const _LoadingIndicator(),
              error: (error, _) => _ErrorPanel(
                message: 'Failed to load overview data',
                onRetry: () => ref.invalidate(overviewProvider),
              ),
              data: (data) => _buildDashboardGrid(data, vendorAsync),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildDashboardGrid(
    OverviewData data,
    AsyncValue<VendorReliabilityData> vendorAsync,
  ) {
    return Wrap(
      spacing: 20,
      runSpacing: 20,
      children: [
        // Top row: Demand panels
        SizedBox(
          width: 400,
          child: _MostInDemandCard(stops: data.mostInDemandStops),
        ),
        SizedBox(
          width: 400,
          child: _LeastInDemandCard(stops: data.leastInDemandStops),
        ),
        SizedBox(
          width: 400,
          child: _ReportsSummaryCard(summary: data.reportStatusSummary),
        ),
        // Breakdown & Cancellation panel — full width
        SizedBox(
          width: 840,
          child: _BreakdownCancellationCard(
            tally: data.breakdownCancellationTally,
            vendorAsync: vendorAsync,
            fromDate: _fromDate,
            toDate: _toDate,
            onDateRangeChanged: (from, to) {
              setState(() {
                _fromDate = from;
                _toDate = to;
              });
            },
            onRetry: () => ref.invalidate(
              vendorReliabilityProvider(
                DateRangeParams(
                  fromDate: _formatDate(_fromDate),
                  toDate: _formatDate(_toDate),
                ),
              ),
            ),
          ),
        ),
        // View Detailed Analytics button
        SizedBox(
          width: 840,
          child: Align(
            alignment: Alignment.centerRight,
            child: FilledButton.icon(
              onPressed: () => context.go('/analytics/detailed'),
              icon: const Icon(Icons.analytics_outlined),
              label: const Text('View Detailed Analytics'),
              style: FilledButton.styleFrom(
                backgroundColor: _navy,
                foregroundColor: Colors.white,
                padding:
                    const EdgeInsets.symmetric(horizontal: 24, vertical: 14),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                ),
                textStyle: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600,
                  fontSize: 14,
                ),
              ),
            ),
          ),
        ),
      ],
    );
  }
}

// ─── Most In-Demand Stops Card ───────────────────────────────────────────────

class _MostInDemandCard extends StatelessWidget {
  const _MostInDemandCard({required this.stops});
  final List<StopDemand> stops;

  @override
  Widget build(BuildContext context) {
    return _DashboardCard(
      title: 'Top 3 Most In-Demand Stops',
      icon: Icons.trending_up_rounded,
      iconColor: Colors.green.shade600,
      child: _StopDemandList(stops: stops, barColor: Colors.green.shade400),
    );
  }
}

// ─── Least In-Demand Stops Card ──────────────────────────────────────────────

class _LeastInDemandCard extends StatelessWidget {
  const _LeastInDemandCard({required this.stops});
  final List<StopDemand> stops;

  @override
  Widget build(BuildContext context) {
    return _DashboardCard(
      title: 'Top 3 Least In-Demand Stops',
      icon: Icons.trending_down_rounded,
      iconColor: Colors.orange.shade600,
      child: _StopDemandList(stops: stops, barColor: Colors.orange.shade400),
    );
  }
}

// ─── Stop Demand List with Progress Bars ─────────────────────────────────────

class _StopDemandList extends StatelessWidget {
  const _StopDemandList({required this.stops, required this.barColor});
  final List<StopDemand> stops;
  final Color barColor;

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    if (stops.isEmpty) {
      return Padding(
        padding: const EdgeInsets.symmetric(vertical: 16),
        child: Text(
          'No data available',
          style: GoogleFonts.hankenGrotesk(color: Colors.grey),
        ),
      );
    }

    final maxCount =
        stops.map((s) => s.confirmedBookingCount).reduce((a, b) => a > b ? a : b);

    return Column(
      children: stops.map((stop) {
        final ratio = maxCount > 0 ? stop.confirmedBookingCount / maxCount : 0.0;
        return Padding(
          padding: const EdgeInsets.symmetric(vertical: 6),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                children: [
                  Expanded(
                    child: Text(
                      stop.stopName,
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 13,
                        fontWeight: FontWeight.w500,
                        color: _navy,
                      ),
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  Text(
                    '${stop.confirmedBookingCount}',
                    style: GoogleFonts.hankenGrotesk(
                      fontSize: 13,
                      fontWeight: FontWeight.w600,
                      color: _navy,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 4),
              ClipRRect(
                borderRadius: BorderRadius.circular(4),
                child: LinearProgressIndicator(
                  value: ratio,
                  minHeight: 8,
                  backgroundColor: Colors.grey.shade200,
                  valueColor: AlwaysStoppedAnimation<Color>(barColor),
                ),
              ),
            ],
          ),
        );
      }).toList(),
    );
  }
}

// ─── Reports Summary Card ────────────────────────────────────────────────────

class _ReportsSummaryCard extends StatelessWidget {
  const _ReportsSummaryCard({required this.summary});
  final ReportStatusSummary summary;

  static const Color _navy = Color(0xFF132E51);
  static const Color _gold = Color(0xFFFFCC00);

  @override
  Widget build(BuildContext context) {
    return _DashboardCard(
      title: 'Reports Summary',
      icon: Icons.assignment_outlined,
      iconColor: _navy,
      child: Row(
        children: [
          Expanded(
            child: _StatBox(
              label: 'Submitted',
              count: summary.submitted,
              color: Colors.blue.shade600,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: _StatBox(
              label: 'Under Review',
              count: summary.underReview,
              color: _gold,
            ),
          ),
          const SizedBox(width: 12),
          Expanded(
            child: _StatBox(
              label: 'Resolved',
              count: summary.resolved,
              color: Colors.green.shade600,
            ),
          ),
        ],
      ),
    );
  }
}

class _StatBox extends StatelessWidget {
  const _StatBox({
    required this.label,
    required this.count,
    required this.color,
  });

  final String label;
  final int count;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(vertical: 16, horizontal: 12),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(12),
        border: Border.all(color: color.withValues(alpha: 0.3)),
      ),
      child: Column(
        children: [
          Text(
            '$count',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 24,
              fontWeight: FontWeight.bold,
              color: color,
            ),
          ),
          const SizedBox(height: 4),
          Text(
            label,
            style: GoogleFonts.hankenGrotesk(
              fontSize: 11,
              fontWeight: FontWeight.w500,
              color: const Color(0xFF132E51),
            ),
            textAlign: TextAlign.center,
          ),
        ],
      ),
    );
  }
}

// ─── Breakdown & Cancellation Card ───────────────────────────────────────────

class _BreakdownCancellationCard extends StatelessWidget {
  const _BreakdownCancellationCard({
    required this.tally,
    required this.vendorAsync,
    required this.fromDate,
    required this.toDate,
    required this.onDateRangeChanged,
    required this.onRetry,
  });

  final BreakdownCancellationTally tally;
  final AsyncValue<VendorReliabilityData> vendorAsync;
  final DateTime fromDate;
  final DateTime toDate;
  final void Function(DateTime from, DateTime to) onDateRangeChanged;
  final VoidCallback onRetry;

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    return _DashboardCard(
      title: 'Breakdown & Cancellation Analytics',
      icon: Icons.build_circle_outlined,
      iconColor: Colors.red.shade600,
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Totals row
          Row(
            children: [
              _TotalChip(
                label: 'Total Breakdowns',
                count: tally.totalBreakdowns,
                color: Colors.red.shade600,
              ),
              const SizedBox(width: 16),
              _TotalChip(
                label: 'Total Cancellations',
                count: tally.totalCancellations,
                color: Colors.orange.shade700,
              ),
            ],
          ),
          const SizedBox(height: 16),
          const Divider(),
          const SizedBox(height: 12),

          // Date range picker row
          Row(
            children: [
              Text(
                'Date Range:',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 13,
                  fontWeight: FontWeight.w500,
                  color: _navy,
                ),
              ),
              const SizedBox(width: 12),
              _DatePickerButton(
                date: fromDate,
                label: 'From',
                onPicked: (picked) {
                  onDateRangeChanged(picked, toDate);
                },
              ),
              const SizedBox(width: 8),
              const Text('—'),
              const SizedBox(width: 8),
              _DatePickerButton(
                date: toDate,
                label: 'To',
                onPicked: (picked) {
                  onDateRangeChanged(fromDate, picked);
                },
              ),
            ],
          ),
          const SizedBox(height: 16),

          // Vendor reliability data
          vendorAsync.when(
            loading: () => const Padding(
              padding: EdgeInsets.symmetric(vertical: 16),
              child: Center(child: CircularProgressIndicator(strokeWidth: 2)),
            ),
            error: (_, __) => _ErrorPanel(
              message: 'Failed to load vendor reliability data',
              onRetry: onRetry,
            ),
            data: (vendorData) =>
                _VendorReliabilityContent(data: vendorData),
          ),
        ],
      ),
    );
  }
}

class _TotalChip extends StatelessWidget {
  const _TotalChip({
    required this.label,
    required this.count,
    required this.color,
  });

  final String label;
  final int count;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 8),
      decoration: BoxDecoration(
        color: color.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Icons.circle, size: 10, color: color),
          const SizedBox(width: 8),
          Text(
            '$label: $count',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 13,
              fontWeight: FontWeight.w600,
              color: color,
            ),
          ),
        ],
      ),
    );
  }
}

class _DatePickerButton extends StatelessWidget {
  const _DatePickerButton({
    required this.date,
    required this.label,
    required this.onPicked,
  });

  final DateTime date;
  final String label;
  final ValueChanged<DateTime> onPicked;

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    return OutlinedButton(
      onPressed: () async {
        final picked = await showDatePicker(
          context: context,
          initialDate: date,
          firstDate: DateTime(2020),
          lastDate: DateTime(2030),
        );
        if (picked != null) {
          onPicked(picked);
        }
      },
      style: OutlinedButton.styleFrom(
        padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
        side: BorderSide(color: _navy.withValues(alpha: 0.3)),
      ),
      child: Text(
        DateFormat('dd MMM yyyy').format(date),
        style: GoogleFonts.hankenGrotesk(
          fontSize: 12,
          fontWeight: FontWeight.w500,
          color: _navy,
        ),
      ),
    );
  }
}

// ─── Vendor Reliability Content ──────────────────────────────────────────────

class _VendorReliabilityContent extends StatelessWidget {
  const _VendorReliabilityContent({required this.data});
  final VendorReliabilityData data;

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Per Shuttle table
        if (data.perShuttle.isNotEmpty) ...[
          Text(
            'Per Shuttle',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 13,
              fontWeight: FontWeight.w600,
              color: _navy,
            ),
          ),
          const SizedBox(height: 8),
          _buildShuttleTable(),
          const SizedBox(height: 16),
        ],

        // Per Vendor table
        if (data.perVendor.isNotEmpty) ...[
          Text(
            'Per Vendor',
            style: GoogleFonts.hankenGrotesk(
              fontSize: 13,
              fontWeight: FontWeight.w600,
              color: _navy,
            ),
          ),
          const SizedBox(height: 8),
          _buildVendorTable(),
        ],

        if (data.perShuttle.isEmpty && data.perVendor.isEmpty)
          Padding(
            padding: const EdgeInsets.symmetric(vertical: 12),
            child: Text(
              'No data for selected date range',
              style: GoogleFonts.hankenGrotesk(color: Colors.grey),
            ),
          ),
      ],
    );
  }

  Widget _buildShuttleTable() {
    return Table(
      columnWidths: const {
        0: FlexColumnWidth(2),
        1: FlexColumnWidth(1.5),
        2: FlexColumnWidth(1.5),
      },
      border: TableBorder.all(color: Colors.grey.shade300, width: 0.5),
      children: [
        TableRow(
          decoration: BoxDecoration(color: _navy.withValues(alpha: 0.05)),
          children: [
            _tableHeader('License Plate'),
            _tableHeader('Breakdowns'),
            _tableHeader('Cancellations'),
          ],
        ),
        ...data.perShuttle.map((s) => TableRow(
              children: [
                _tableCell(s.licensePlate),
                _tableCell('${s.breakdownCount}'),
                _tableCell('${s.cancellationCount}'),
              ],
            )),
      ],
    );
  }

  Widget _buildVendorTable() {
    return Table(
      columnWidths: const {
        0: FlexColumnWidth(2),
        1: FlexColumnWidth(1.5),
        2: FlexColumnWidth(1.5),
      },
      border: TableBorder.all(color: Colors.grey.shade300, width: 0.5),
      children: [
        TableRow(
          decoration: BoxDecoration(color: _navy.withValues(alpha: 0.05)),
          children: [
            _tableHeader('Vendor'),
            _tableHeader('Breakdowns'),
            _tableHeader('Cancellations'),
          ],
        ),
        ...data.perVendor.map((v) => TableRow(
              children: [
                _tableCell(v.vendorCompanyName),
                _tableCell('${v.totalBreakdowns}'),
                _tableCell('${v.totalCancellations}'),
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
}

// ─── Shared Widgets ──────────────────────────────────────────────────────────

class _DashboardCard extends StatelessWidget {
  const _DashboardCard({
    required this.title,
    required this.icon,
    required this.iconColor,
    required this.child,
  });

  final String title;
  final IconData icon;
  final Color iconColor;
  final Widget child;

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(16),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withValues(alpha: 0.05),
            blurRadius: 10,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        mainAxisSize: MainAxisSize.min,
        children: [
          Row(
            children: [
              Icon(icon, size: 20, color: iconColor),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  title,
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 15,
                    fontWeight: FontWeight.w600,
                    color: _navy,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          child,
        ],
      ),
    );
  }
}

class _LoadingIndicator extends StatelessWidget {
  const _LoadingIndicator();

  @override
  Widget build(BuildContext context) {
    return const Center(
      child: Padding(
        padding: EdgeInsets.symmetric(vertical: 80),
        child: CircularProgressIndicator(strokeWidth: 3),
      ),
    );
  }
}

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
