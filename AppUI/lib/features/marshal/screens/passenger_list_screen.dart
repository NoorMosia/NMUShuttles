import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/marshal/models/passenger_model.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/passenger_list_provider.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/convoy_provider.dart';
import 'package:nmu_shuttle_app/features/marshal/screens/qr_scanner_screen.dart';

/// Displays the passenger manifest for a given trip, with real-time counts
/// and the ability to scan QR codes and flag breakdowns.
class PassengerListScreen extends ConsumerStatefulWidget {
  const PassengerListScreen({super.key, required this.tripId});

  final String tripId;

  @override
  ConsumerState<PassengerListScreen> createState() =>
      _PassengerListScreenState();
}

class _PassengerListScreenState extends ConsumerState<PassengerListScreen> {
  int get _tripIdInt => int.parse(widget.tripId);

  /// Navigate to QR scanner and handle the result.
  Future<void> _openQrScanner() async {
    final result = await Navigator.of(context).push<CheckInResultModel>(
      MaterialPageRoute(
        builder: (_) => QrScannerScreen(tripId: _tripIdInt),
      ),
    );

    if (result != null && mounted) {
      // Invalidate manifest to get instant update
      ref.invalidate(passengerManifestProvider(_tripIdInt));
      // Also refresh convoy counts immediately
      ref.invalidate(convoyStatusProvider(_tripIdInt));
    }
  }

  /// Show the breakdown flag dialog.
  Future<void> _showBreakdownDialog() async {
    final shuttles = await showDialog<ShuttleAllocationModel?>(
      context: context,
      builder: (ctx) => _BreakdownDialog(tripId: _tripIdInt),
    );

    if (shuttles != null && mounted) {
      // Confirm the flag
      final confirmed = await showDialog<bool>(
        context: context,
        builder: (ctx) => AlertDialog(
          title: const Text('Confirm Breakdown'),
          content: Text(
            'Flag shuttle ${shuttles.licensePlate} (Position ${shuttles.orderPosition}) as broken down?',
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx, false),
              child: const Text('Cancel'),
            ),
            FilledButton(
              onPressed: () => Navigator.pop(ctx, true),
              style: FilledButton.styleFrom(
                backgroundColor: Colors.red.shade700,
              ),
              child: const Text('Flag Breakdown'),
            ),
          ],
        ),
      );

      if (confirmed == true && mounted) {
        _submitBreakdown(shuttles.shuttleId);
      }
    }
  }

  Future<void> _submitBreakdown(int shuttleId) async {
    try {
      final request = BreakdownRequest(
        tripId: _tripIdInt,
        shuttleId: shuttleId,
      );
      await ref.read(flagBreakdownProvider(request).future);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Breakdown flagged successfully'),
            backgroundColor: Colors.orange,
          ),
        );
        ref.invalidate(passengerManifestProvider(_tripIdInt));
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Failed to flag breakdown: ${_extractError(e)}'),
            backgroundColor: Colors.red.shade700,
          ),
        );
      }
    }
  }

  String _extractError(Object error) {
    if (error is Exception) {
      return error.toString().replaceFirst('Exception: ', '');
    }
    return 'Unknown error';
  }

  @override
  Widget build(BuildContext context) {
    final manifestAsync = ref.watch(passengerManifestProvider(_tripIdInt));
    final convoyAsync = ref.watch(convoyStatusProvider(_tripIdInt));
    final theme = Theme.of(context);

    // Determine if scanning is required (default to true if convoy not loaded yet)
    final requiresScanning = convoyAsync.valueOrNull?.requiresScanning ?? true;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Passenger List'),
        backgroundColor: AppTheme.navy,
        foregroundColor: Colors.white,
        actions: [
          IconButton(
            icon: const Icon(Icons.warning_amber_rounded),
            tooltip: 'Flag Breakdown',
            onPressed: _showBreakdownDialog,
          ),
        ],
      ),
      body: Column(
        children: [
          // Scan gating banner
          if (!requiresScanning)
            Container(
              width: double.infinity,
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
              color: Colors.amber.shade100,
              child: Row(
                children: [
                  Icon(Icons.info_outline, size: 18, color: Colors.amber.shade900),
                  const SizedBox(width: 8),
                  Expanded(
                    child: Text(
                      'This trip is open boarding — no QR scan required.',
                      style: theme.textTheme.bodySmall?.copyWith(
                        color: Colors.amber.shade900,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          // Convoy panel
          _buildConvoyPanel(convoyAsync, theme),
          const Divider(height: 1),
          // Main manifest content
          Expanded(
            child: manifestAsync.when(
              data: (manifest) => _buildManifestContent(manifest, theme),
              loading: () => const Center(child: CircularProgressIndicator()),
              error: (error, _) => Center(
                child: Padding(
                  padding: const EdgeInsets.all(24),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Icon(Icons.error_outline, size: 48, color: theme.colorScheme.error),
                      const SizedBox(height: 16),
                      Text('Failed to load passengers', style: theme.textTheme.titleMedium),
                      const SizedBox(height: 8),
                      Text(_extractError(error), textAlign: TextAlign.center, style: theme.textTheme.bodyMedium),
                      const SizedBox(height: 16),
                      FilledButton.icon(
                        onPressed: () => ref.invalidate(passengerManifestProvider(_tripIdInt)),
                        icon: const Icon(Icons.refresh),
                        label: const Text('Retry'),
                      ),
                    ],
                  ),
                ),
              ),
            ),
          ),
        ],
      ),
      floatingActionButton: requiresScanning
          ? FloatingActionButton.extended(
              onPressed: _openQrScanner,
              icon: const Icon(Icons.qr_code_scanner),
              label: const Text('Scan QR'),
            )
          : null,
    );
  }

  Widget _buildConvoyPanel(AsyncValue<ConvoyStatus> convoyAsync, ThemeData theme) {
    return convoyAsync.when(
      loading: () => const SizedBox.shrink(),
      error: (_, __) => const SizedBox.shrink(),
      data: (convoy) {
        if (convoy.shuttles.isEmpty) return const SizedBox.shrink();

        return Container(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
          color: AppTheme.navy.withValues(alpha: 0.03),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Text(
                'Convoy',
                style: theme.textTheme.labelLarge?.copyWith(
                  color: AppTheme.navy,
                  fontWeight: FontWeight.w700,
                ),
              ),
              const SizedBox(height: 6),
              SizedBox(
                height: 56,
                child: ListView.separated(
                  scrollDirection: Axis.horizontal,
                  itemCount: convoy.shuttles.length,
                  separatorBuilder: (_, __) => const SizedBox(width: 8),
                  itemBuilder: (context, index) {
                    final shuttle = convoy.shuttles[index];
                    return _ConvoyShuttleChip(shuttle: shuttle);
                  },
                ),
              ),
            ],
          ),
        );
      },
    );
  }

  Widget _buildManifestContent(ManifestModel manifest, ThemeData theme) {
    return Column(
      children: [
        // Counts header
        _buildCountsHeader(manifest, theme),
        const Divider(height: 1),
        // Passenger list
        Expanded(
          child: manifest.passengers.isEmpty
              ? Center(
                  child: Text(
                    'No confirmed passengers',
                    style: theme.textTheme.bodyLarge?.copyWith(
                      color: Colors.grey,
                    ),
                  ),
                )
              : ListView.separated(
                  padding: const EdgeInsets.symmetric(vertical: 8),
                  itemCount: manifest.passengers.length,
                  separatorBuilder: (_, __) =>
                      const Divider(height: 1, indent: 72),
                  itemBuilder: (context, index) {
                    final passenger = manifest.passengers[index];
                    return _PassengerTile(passenger: passenger);
                  },
                ),
        ),
      ],
    );
  }

  Widget _buildCountsHeader(ManifestModel manifest, ThemeData theme) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 12),
      color: AppTheme.navy.withValues(alpha: 0.05),
      child: Row(
        children: [
          _CountChip(
            label: 'Total',
            count: manifest.totalConfirmed,
            color: AppTheme.navy,
          ),
          const SizedBox(width: 12),
          _CountChip(
            label: 'Boarded',
            count: manifest.boardedCount,
            color: Colors.green.shade700,
          ),
          const SizedBox(width: 12),
          _CountChip(
            label: 'Pending',
            count: manifest.pendingCount,
            color: Colors.orange.shade700,
          ),
        ],
      ),
    );
  }
}

// ─── Count Chip Widget ─────────────────────────────────────────

class _CountChip extends StatelessWidget {
  const _CountChip({
    required this.label,
    required this.count,
    required this.color,
  });

  final String label;
  final int count;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Expanded(
      child: Container(
        padding: const EdgeInsets.symmetric(vertical: 8, horizontal: 8),
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: color.withValues(alpha: 0.3)),
        ),
        child: Column(
          children: [
            Text(
              '$count',
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    color: color,
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 2),
            Text(
              label,
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: color,
                  ),
            ),
          ],
        ),
      ),
    );
  }
}

// ─── Passenger Tile Widget ─────────────────────────────────────

class _PassengerTile extends StatelessWidget {
  const _PassengerTile({required this.passenger});

  final PassengerModel passenger;

  @override
  Widget build(BuildContext context) {
    final isBoarded = passenger.isBoarded;
    final theme = Theme.of(context);

    return ListTile(
      leading: CircleAvatar(
        backgroundColor: isBoarded
            ? Colors.green.shade100
            : Colors.grey.shade200,
        child: Icon(
          isBoarded ? Icons.check_circle : Icons.circle_outlined,
          color: isBoarded ? Colors.green.shade700 : Colors.grey,
        ),
      ),
      title: Text(
        passenger.studentName,
        style: theme.textTheme.bodyLarge?.copyWith(
          fontWeight: FontWeight.w600,
        ),
      ),
      subtitle: Text(
        passenger.studentNumber,
        style: theme.textTheme.bodySmall?.copyWith(
          color: Colors.grey.shade600,
        ),
      ),
      trailing: Container(
        padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
        decoration: BoxDecoration(
          color: isBoarded
              ? Colors.green.shade50
              : Colors.orange.shade50,
          borderRadius: BorderRadius.circular(12),
          border: Border.all(
            color: isBoarded
                ? Colors.green.shade300
                : Colors.orange.shade300,
          ),
        ),
        child: Text(
          passenger.boardedStatus,
          style: theme.textTheme.labelSmall?.copyWith(
            color: isBoarded
                ? Colors.green.shade700
                : Colors.orange.shade700,
            fontWeight: FontWeight.w600,
          ),
        ),
      ),
    );
  }
}

// ─── Convoy Shuttle Chip ───────────────────────────────────────

class _ConvoyShuttleChip extends StatelessWidget {
  const _ConvoyShuttleChip({required this.shuttle});

  final ConvoyShuttle shuttle;

  @override
  Widget build(BuildContext context) {
    final isOperational = shuttle.isOperational;

    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
      decoration: BoxDecoration(
        color: isOperational ? Colors.white : Colors.grey.shade200,
        borderRadius: BorderRadius.circular(10),
        border: Border.all(
          color: isOperational
              ? AppTheme.navy.withValues(alpha: 0.2)
              : Colors.grey.shade400,
        ),
      ),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.center,
        children: [
          Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(
                '#${shuttle.orderPosition}',
                style: TextStyle(
                  fontSize: 11,
                  fontWeight: FontWeight.w700,
                  color: isOperational ? AppTheme.navy : Colors.grey,
                  decoration: isOperational ? null : TextDecoration.lineThrough,
                ),
              ),
              const SizedBox(width: 4),
              if (shuttle.isAccessibleVehicle)
                Icon(Icons.accessible, size: 12, color: Colors.blue.shade700),
              Text(
                shuttle.licensePlate,
                style: TextStyle(
                  fontSize: 11,
                  fontWeight: FontWeight.w600,
                  color: isOperational ? AppTheme.navy : Colors.grey,
                  decoration: isOperational ? null : TextDecoration.lineThrough,
                ),
              ),
            ],
          ),
          const SizedBox(height: 2),
          Text(
            '${shuttle.boardedCount}/${shuttle.capacity}',
            style: TextStyle(
              fontSize: 13,
              fontWeight: FontWeight.w700,
              color: isOperational ? Colors.green.shade700 : Colors.grey,
            ),
          ),
        ],
      ),
    );
  }
}

// ─── Breakdown Dialog ──────────────────────────────────────────

class _BreakdownDialog extends ConsumerWidget {
  const _BreakdownDialog({required this.tripId});

  final int tripId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final shuttlesAsync = ref.watch(tripShuttlesProvider(tripId));

    return AlertDialog(
      title: const Text('Select Shuttle to Flag'),
      content: SizedBox(
        width: double.maxFinite,
        child: shuttlesAsync.when(
          data: (shuttles) {
            final operational =
                shuttles.where((s) => s.isOperational).toList();
            if (operational.isEmpty) {
              return const Text('No operational shuttles on this trip.');
            }
            return ListView.builder(
              shrinkWrap: true,
              itemCount: operational.length,
              itemBuilder: (context, index) {
                final shuttle = operational[index];
                return ListTile(
                  leading: CircleAvatar(
                    backgroundColor: AppTheme.navy.withValues(alpha: 0.1),
                    child: Text(
                      '${shuttle.orderPosition}',
                      style: const TextStyle(
                        color: AppTheme.navy,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                  ),
                  title: Text(shuttle.licensePlate),
                  subtitle: Text('Position ${shuttle.orderPosition}'),
                  onTap: () => Navigator.pop(context, shuttle),
                );
              },
            );
          },
          loading: () => const Center(
            child: Padding(
              padding: EdgeInsets.all(24),
              child: CircularProgressIndicator(),
            ),
          ),
          error: (error, _) => Text('Failed to load shuttles: $error'),
        ),
      ),
      actions: [
        TextButton(
          onPressed: () => Navigator.pop(context),
          child: const Text('Cancel'),
        ),
      ],
    );
  }
}
