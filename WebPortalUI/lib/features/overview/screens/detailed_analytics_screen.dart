import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/features/overview/models/detailed_analytics_model.dart';
import 'package:nmu_shuttle_admin/features/overview/providers/detailed_analytics_provider.dart';

/// Detailed Analytics page — shows per-stop booking demand and overall stats.
class DetailedAnalyticsScreen extends ConsumerWidget {
  const DetailedAnalyticsScreen({super.key});

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final analyticsAsync = ref.watch(detailedAnalyticsProvider);

    return Scaffold(
      backgroundColor: const Color(0xFFF5F5F5),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Header with back button
            Row(
              children: [
                IconButton(
                  onPressed: () => context.go('/overview'),
                  icon: const Icon(Icons.arrow_back),
                  tooltip: 'Back to Overview',
                  color: _navy,
                ),
                const SizedBox(width: 8),
                Text(
                  'Detailed Analytics',
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 28,
                    fontWeight: FontWeight.bold,
                    color: _navy,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 4),
            Padding(
              padding: const EdgeInsets.only(left: 48),
              child: Text(
                'Per-stop booking demand and overall system statistics',
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 14,
                  color: Colors.grey.shade600,
                ),
              ),
            ),
            const SizedBox(height: 24),

            // Content
            analyticsAsync.when(
              loading: () => const Center(
                child: Padding(
                  padding: EdgeInsets.only(top: 100),
                  child: CircularProgressIndicator(),
                ),
              ),
              error: (error, _) => _ErrorPanel(
                message: 'Failed to load detailed analytics',
                onRetry: () => ref.invalidate(detailedAnalyticsProvider),
              ),
              data: (data) => _buildContent(data),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildContent(DetailedAnalyticsData data) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        // Overall stats cards
        _OverallStatsRow(stats: data.overallStats),
        const SizedBox(height: 32),

        // Per-stop demand table
        Text(
          'Booking Demand by Stop',
          style: GoogleFonts.hankenGrotesk(
            fontSize: 20,
            fontWeight: FontWeight.w600,
            color: _navy,
          ),
        ),
        const SizedBox(height: 16),
        _StopDemandTable(stops: data.perStopDemand),
        const SizedBox(height: 32),

        // Bar chart visualization
        Text(
          'Load Distribution',
          style: GoogleFonts.hankenGrotesk(
            fontSize: 20,
            fontWeight: FontWeight.w600,
            color: _navy,
          ),
        ),
        const SizedBox(height: 16),
        _StopDemandBarChart(stops: data.perStopDemand),
      ],
    );
  }
}

// ─── Overall Stats Row ───────────────────────────────────────────────────────

class _OverallStatsRow extends StatelessWidget {
  const _OverallStatsRow({required this.stats});
  final OverallBookingStats stats;

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    return Wrap(
      spacing: 16,
      runSpacing: 16,
      children: [
        _StatCard(
          label: 'Total Bookings',
          value: stats.total.toString(),
          icon: Icons.book_outlined,
          color: _navy,
        ),
        _StatCard(
          label: 'Confirmed',
          value: stats.confirmed.toString(),
          icon: Icons.check_circle_outline,
          color: Colors.green.shade600,
        ),
        _StatCard(
          label: 'Waitlisted',
          value: stats.waitlisted.toString(),
          icon: Icons.hourglass_empty,
          color: Colors.orange.shade600,
        ),
        _StatCard(
          label: 'Cancelled',
          value: stats.cancelled.toString(),
          icon: Icons.cancel_outlined,
          color: Colors.red.shade600,
        ),
      ],
    );
  }
}

class _StatCard extends StatelessWidget {
  const _StatCard({
    required this.label,
    required this.value,
    required this.icon,
    required this.color,
  });

  final String label;
  final String value;
  final IconData icon;
  final Color color;

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 220,
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(icon, size: 20, color: color),
              const SizedBox(width: 8),
              Expanded(
                child: Text(
                  label,
                  style: GoogleFonts.hankenGrotesk(
                    fontSize: 13,
                    fontWeight: FontWeight.w500,
                    color: Colors.grey.shade600,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 12),
          Text(
            value,
            style: GoogleFonts.hankenGrotesk(
              fontSize: 28,
              fontWeight: FontWeight.bold,
              color: _navy,
            ),
          ),
        ],
      ),
    );
  }
}

// ─── Stop Demand Table ───────────────────────────────────────────────────────

class _StopDemandTable extends StatelessWidget {
  const _StopDemandTable({required this.stops});
  final List<StopBookingDemand> stops;

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(12),
        child: DataTable(
          headingRowColor: WidgetStateProperty.all(
            _navy.withOpacity(0.05),
          ),
          columnSpacing: 32,
          columns: [
            DataColumn(
              label: Text(
                'Stop',
                style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
            ),
            DataColumn(
              numeric: true,
              label: Text(
                'Total',
                style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
            ),
            DataColumn(
              numeric: true,
              label: Text(
                'Confirmed',
                style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
            ),
            DataColumn(
              numeric: true,
              label: Text(
                'Waitlisted',
                style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
            ),
            DataColumn(
              numeric: true,
              label: Text(
                'Load %',
                style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600,
                  color: _navy,
                ),
              ),
            ),
          ],
          rows: stops.map((stop) {
            final loadPercent = stops.isNotEmpty && stops.first.totalBookings > 0
                ? (stop.totalBookings / stops.first.totalBookings * 100)
                    .toStringAsFixed(0)
                : '0';
            return DataRow(cells: [
              DataCell(Text(
                stop.stopName,
                style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w500,
                  color: _navy,
                ),
              )),
              DataCell(Text(
                stop.totalBookings.toString(),
                style: GoogleFonts.hankenGrotesk(),
              )),
              DataCell(Text(
                stop.confirmedBookings.toString(),
                style: GoogleFonts.hankenGrotesk(
                  color: Colors.green.shade700,
                ),
              )),
              DataCell(Text(
                stop.waitlistedBookings.toString(),
                style: GoogleFonts.hankenGrotesk(
                  color: Colors.orange.shade700,
                ),
              )),
              DataCell(Text(
                '$loadPercent%',
                style: GoogleFonts.hankenGrotesk(
                  fontWeight: FontWeight.w600,
                ),
              )),
            ]);
          }).toList(),
        ),
      ),
    );
  }
}

// ─── Bar Chart Visualization ─────────────────────────────────────────────────

class _StopDemandBarChart extends StatelessWidget {
  const _StopDemandBarChart({required this.stops});
  final List<StopBookingDemand> stops;

  static const Color _navy = Color(0xFF132E51);

  @override
  Widget build(BuildContext context) {
    if (stops.isEmpty) {
      return const SizedBox(
        height: 200,
        child: Center(child: Text('No data available')),
      );
    }

    final maxBookings =
        stops.map((s) => s.totalBookings).reduce((a, b) => a > b ? a : b);

    return Container(
      padding: const EdgeInsets.all(24),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: Colors.black.withOpacity(0.05),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: Column(
        children: stops.map((stop) {
          final ratio =
              maxBookings > 0 ? stop.totalBookings / maxBookings : 0.0;
          return Padding(
            padding: const EdgeInsets.symmetric(vertical: 8),
            child: Row(
              children: [
                SizedBox(
                  width: 160,
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
                const SizedBox(width: 12),
                Expanded(
                  child: Stack(
                    children: [
                      Container(
                        height: 28,
                        decoration: BoxDecoration(
                          color: Colors.grey.shade100,
                          borderRadius: BorderRadius.circular(6),
                        ),
                      ),
                      FractionallySizedBox(
                        widthFactor: ratio,
                        child: Container(
                          height: 28,
                          decoration: BoxDecoration(
                            gradient: LinearGradient(
                              colors: [_navy, _navy.withOpacity(0.7)],
                            ),
                            borderRadius: BorderRadius.circular(6),
                          ),
                          alignment: Alignment.centerRight,
                          padding: const EdgeInsets.only(right: 8),
                          child: Text(
                            '${stop.totalBookings}',
                            style: GoogleFonts.hankenGrotesk(
                              fontSize: 12,
                              fontWeight: FontWeight.w600,
                              color: Colors.white,
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          );
        }).toList(),
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
        padding: const EdgeInsets.only(top: 100),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.error_outline, size: 48, color: Colors.red.shade300),
            const SizedBox(height: 16),
            Text(
              message,
              style: GoogleFonts.hankenGrotesk(fontSize: 16),
            ),
            const SizedBox(height: 12),
            ElevatedButton.icon(
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
