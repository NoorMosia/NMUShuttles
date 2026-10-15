import 'dart:io';
import 'dart:ui' as ui;

import 'package:flutter/material.dart';
import 'package:flutter/rendering.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
import 'package:media_store_plus/media_store_plus.dart';
import 'package:path_provider/path_provider.dart';
import 'package:qr_flutter/qr_flutter.dart';
import 'package:nmu_shuttle_app/core/api/api_exception.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/core/ui/error_display.dart';
import 'package:nmu_shuttle_app/features/student/models/booking_model.dart';
import 'package:nmu_shuttle_app/features/student/providers/booking_providers.dart';
import 'package:nmu_shuttle_app/features/student/providers/profile_providers.dart';
import 'package:nmu_shuttle_app/features/student/screens/report_form_screen.dart';
import 'package:nmu_shuttle_app/shared/widgets/app_drawer.dart';
import 'package:nmu_shuttle_app/shared/widgets/back_button.dart';

/// Booking detail screen with QR code display.
class BookingDetailScreen extends ConsumerStatefulWidget {
  const BookingDetailScreen({super.key, required this.bookingId});

  final String bookingId;

  @override
  ConsumerState<BookingDetailScreen> createState() =>
      _BookingDetailScreenState();
}

class _BookingDetailScreenState extends ConsumerState<BookingDetailScreen> {
  final GlobalKey _qrKey = GlobalKey();
  bool _isCancelling = false;

  bool _isDownloading = false;

  /// Timestamp burned into the captured QR image so a saved screenshot is
  /// identifiable. Only shown while capturing, then cleared.
  DateTime? _captureStamp;

  Future<void> _downloadQrCode() async {
    if (_isDownloading) return;
    final now = DateTime.now();
    setState(() {
      _isDownloading = true;
      // Burn the timestamp into the captured image.
      _captureStamp = now;
    });
    try {
      // Let the caption render into the RepaintBoundary before we capture it.
      await WidgetsBinding.instance.endOfFrame;

      final boundary =
          _qrKey.currentContext?.findRenderObject() as RenderRepaintBoundary?;
      if (boundary == null) {
        throw StateError('QR code is not ready yet.');
      }

      final image = await boundary.toImage(pixelRatio: 3.0);
      final byteData = await image.toByteData(format: ui.ImageByteFormat.png);
      if (byteData == null) {
        throw StateError('Could not render the QR code image.');
      }
      final pngBytes = byteData.buffer.asUint8List();

      // Write the PNG to a temporary file first, then hand the path to
      // MediaStore which copies it into the public Pictures folder (visible in
      // the gallery). No storage permission is needed on Android 10+ for this.
      final tempDir = await getTemporaryDirectory();
      // Human-readable timestamp (local SAST time) so each saved QR screenshot
      // is identifiable, e.g. nmu_shuttle_qr_2026-10-15_15-34-26.png
      final stamp = DateFormat('yyyy-MM-dd_HH-mm-ss').format(now);
      final fileName = 'nmu_shuttle_qr_$stamp.png';
      final tempFile = File('${tempDir.path}/$fileName');
      await tempFile.writeAsBytes(pngBytes);

      MediaStore.appFolder = 'NMU Shuttle';
      final mediaStore = MediaStore();
      final saveInfo = await mediaStore.saveFile(
        tempFilePath: tempFile.path,
        dirType: DirType.photo,
        dirName: DirName.pictures,
      );

      // Clean up the temp file regardless of outcome.
      if (await tempFile.exists()) {
        await tempFile.delete();
      }

      if (!mounted) return;
      if (saveInfo != null && saveInfo.isSuccessful) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('QR code saved to Pictures/NMU Shuttle'),
            backgroundColor: Colors.green,
          ),
        );
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Could not save the QR code. Please try again.'),
            backgroundColor: Colors.red,
          ),
        );
      }
    } catch (e) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text('Failed to save QR code: $e'),
            backgroundColor: Colors.red,
          ),
        );
      }
    } finally {
      if (mounted) {
        setState(() {
          _isDownloading = false;
          _captureStamp = null;
        });
      }
    }
  }

  Future<void> _cancelBooking(int bookingId) async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Cancel Booking'),
        content: const Text(
            'Are you sure you want to cancel this booking? This action cannot be undone.'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Keep Booking'),
          ),
          TextButton(
            onPressed: () => Navigator.pop(context, true),
            style: TextButton.styleFrom(foregroundColor: Colors.red),
            child: const Text('Cancel Booking'),
          ),
        ],
      ),
    );

    if (confirmed != true) return;

    setState(() => _isCancelling = true);
    try {
      final repository = ref.read(bookingRepositoryProvider);
      await repository.cancelBooking(bookingId);
      // A cancelled booking leaves the upcoming list and appears in past.
      ref.invalidate(upcomingBookingsProvider);
      ref.invalidate(pastBookingsProvider);
      ref.invalidate(bookingDetailProvider(bookingId));
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(
            content: Text('Booking cancelled successfully'),
            backgroundColor: Colors.green,
          ),
        );
        context.go('/student/home');
      }
    } on ApiException catch (e) {
      if (mounted) {
        handleApiError(
          context,
          e,
          conflictTitle: 'Cannot Cancel',
        );
      }
    } catch (e) {
      if (mounted) {
        showErrorSnackBar(context, 'Something went wrong. Please try again.');
      }
    } finally {
      if (mounted) setState(() => _isCancelling = false);
    }
  }

  void _navigateToReport(BuildContext context, BookingModel booking) {
    Navigator.of(context).push(
      MaterialPageRoute(
        builder: (_) => ReportFormScreen(bookingId: booking.bookingId),
      ),
    );
  }

  Widget _buildReportButton(BuildContext context, BookingModel booking) {
    final reportsAsync = ref.watch(reportsProvider);

    return reportsAsync.when(
      loading: () => const SizedBox.shrink(),
      error: (_, __) => SizedBox(
        width: double.infinity,
        child: OutlinedButton.icon(
          onPressed: () => _navigateToReport(context, booking),
          icon: const Icon(Icons.report_outlined),
          label: const Text('Report an Issue'),
          style: OutlinedButton.styleFrom(
            foregroundColor: AppTheme.navy,
            side: BorderSide(color: Colors.grey.shade400),
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(8),
            ),
            padding: const EdgeInsets.symmetric(vertical: 14),
          ),
        ),
      ),
      data: (reports) {
        final existingReport = reports
            .where((r) => r.bookingId == booking.bookingId)
            .toList();

        if (existingReport.isNotEmpty) {
          final report = existingReport.first;
          return SizedBox(
            width: double.infinity,
            child: OutlinedButton.icon(
              onPressed: null,
              icon: const Icon(Icons.report_outlined),
              label: Text('Report ${report.status}'),
              style: OutlinedButton.styleFrom(
                foregroundColor: Colors.grey,
                disabledForegroundColor: Colors.grey.shade500,
                side: BorderSide(color: Colors.grey.shade300),
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(8),
                ),
                padding: const EdgeInsets.symmetric(vertical: 14),
              ),
            ),
          );
        }

        return SizedBox(
          width: double.infinity,
          child: OutlinedButton.icon(
            onPressed: () => _navigateToReport(context, booking),
            icon: const Icon(Icons.report_outlined),
            label: const Text('Report an Issue'),
            style: OutlinedButton.styleFrom(
              foregroundColor: AppTheme.navy,
              side: BorderSide(color: Colors.grey.shade400),
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
              padding: const EdgeInsets.symmetric(vertical: 14),
            ),
          ),
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    final bookingId = int.tryParse(widget.bookingId) ?? 0;
    final bookingAsync = ref.watch(bookingDetailProvider(bookingId));

    return Scaffold(
      drawer: AppDrawer(
        onLogout: () => context.go('/login'),
      ),
      body: Stack(
        children: [
          bookingAsync.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (error, _) => ErrorRetry(
              error: error,
              title: 'Failed to load booking details',
              onRetry: () => ref.invalidate(bookingDetailProvider(bookingId)),
            ),
            data: (booking) => _buildContent(context, booking),
          ),
          const FloatingBackButton(),
        ],
      ),
    );
  }

  Widget _buildContent(BuildContext context, BookingModel booking) {
    final dateFormat = DateFormat('EEEE, d MMMM yyyy');
    final directionLabel =
        booking.direction == 'ToCampus' ? 'To Campus' : 'From Campus';
    final directionIcon = booking.direction == 'ToCampus'
        ? Icons.arrow_forward
        : Icons.arrow_back;

    // Only show cancel if trip is still in Scheduling status
    final canCancel = booking.bookingStatus != 'Cancelled';

    return SafeArea(
      child: SingleChildScrollView(
        padding: const EdgeInsets.fromLTRB(16, 72, 16, 24),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.center,
          children: [
            // Title
            Text(
              'Booking Details',
              style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: AppTheme.navy,
                  ),
            ),
            const SizedBox(height: 24),

            // QR Code
            if (booking.qrCodeToken != null &&
                booking.qrCodeToken!.isNotEmpty) ...[
              RepaintBoundary(
                key: _qrKey,
                child: Container(
                  padding: const EdgeInsets.all(16),
                  decoration: BoxDecoration(
                    color: Colors.white,
                    borderRadius: BorderRadius.circular(16),
                  ),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      QrImageView(
                        data: booking.qrCodeToken!,
                        version: QrVersions.auto,
                        size: 256,
                        embeddedImage:
                            const AssetImage('assets/logos/qrImg.png'),
                        embeddedImageStyle: const QrEmbeddedImageStyle(
                          size: Size(48, 48),
                        ),
                      ),
                      // Trip identity + save timestamp, burned into the saved
                      // image so a screenshot is identifiable on its own.
                      const SizedBox(height: 12),
                      Text(
                        '$directionLabel \u2022 ${booking.stopName}',
                        textAlign: TextAlign.center,
                        style: const TextStyle(
                          fontSize: 12,
                          fontWeight: FontWeight.w600,
                          color: AppTheme.navy,
                        ),
                      ),
                      const SizedBox(height: 2),
                      Text(
                        dateFormat.format(booking.tripDate),
                        textAlign: TextAlign.center,
                        style: const TextStyle(
                          fontSize: 11,
                          color: Colors.black87,
                        ),
                      ),
                      if (_captureStamp != null) ...[
                        const SizedBox(height: 2),
                        Text(
                          'Saved ${DateFormat('yyyy-MM-dd HH:mm').format(_captureStamp!)}',
                          textAlign: TextAlign.center,
                          style: const TextStyle(
                            fontSize: 10,
                            color: Colors.black54,
                          ),
                        ),
                      ],
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
              // Download QR button
              OutlinedButton.icon(
                onPressed: _isDownloading ? null : _downloadQrCode,
                icon: _isDownloading
                    ? const SizedBox(
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Icon(Icons.download),
                label: Text(
                    _isDownloading ? 'Preparing...' : 'Download QR Code'),
                style: OutlinedButton.styleFrom(
                  foregroundColor: AppTheme.navy,
                  side: const BorderSide(color: AppTheme.navy),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(8),
                  ),
                ),
              ),
              const SizedBox(height: 24),
            ],

            // Booking info card
            Card(
              elevation: 0,
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12)),
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Column(
                  children: [
                    _InfoRow(
                      icon: directionIcon,
                      label: 'Direction',
                      value: directionLabel,
                    ),
                    const Divider(height: 24),
                    _InfoRow(
                      icon: Icons.location_on_outlined,
                      label: 'Stop',
                      value: booking.stopName,
                    ),
                    const Divider(height: 24),
                    _InfoRow(
                      icon: Icons.school_outlined,
                      label: 'Campus',
                      value: booking.campusName,
                    ),
                    const Divider(height: 24),
                    _InfoRow(
                      icon: Icons.calendar_today_outlined,
                      label: 'Date',
                      value: dateFormat.format(booking.tripDate),
                    ),
                    const Divider(height: 24),
                    _InfoRow(
                      icon: Icons.access_time,
                      label: 'Departure',
                      value: booking.departureTime,
                    ),
                    const Divider(height: 24),
                    _InfoRow(
                      icon: Icons.check_circle_outline,
                      label: 'Status',
                      value: booking.bookingStatus,
                      valueColor: _statusColor(booking.bookingStatus),
                    ),
                    const Divider(height: 24),
                    _InfoRow(
                      icon: Icons.directions_bus_outlined,
                      label: 'Boarded',
                      value: _formatBoardedStatus(booking.boardedStatus),
                      valueColor: booking.boardedStatus == 'Boarded'
                          ? Colors.green
                          : Colors.grey.shade600,
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 24),

            // Cancel booking button
            if (canCancel)
              SizedBox(
                width: double.infinity,
                child: OutlinedButton.icon(
                  onPressed: _isCancelling
                      ? null
                      : () => _cancelBooking(booking.bookingId),
                  icon: _isCancelling
                      ? const SizedBox(
                          width: 18,
                          height: 18,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        )
                      : const Icon(Icons.cancel_outlined),
                  label:
                      Text(_isCancelling ? 'Cancelling...' : 'Cancel Booking'),
                  style: OutlinedButton.styleFrom(
                    foregroundColor: Colors.red,
                    side: const BorderSide(color: Colors.red),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(8),
                    ),
                    padding: const EdgeInsets.symmetric(vertical: 14),
                  ),
                ),
              ),

            // Report issue button
            const SizedBox(height: 12),
            _buildReportButton(context, booking),
          ],
        ),
      ),
    );
  }

  Color _statusColor(String status) {
    switch (status) {
      case 'Confirmed':
        return Colors.green;
      case 'Waitlisted':
        return Colors.orange;
      case 'Cancelled':
        return Colors.red;
      default:
        return Colors.grey.shade700;
    }
  }

  String _formatBoardedStatus(String status) {
    switch (status) {
      case 'Boarded':
        return 'Boarded ✓';
      case 'NotBoarded':
        return 'Not Boarded';
      case 'NoShow':
        return 'No Show';
      default:
        return status;
    }
  }
}

/// Row displaying a single info field with icon, label, and value.
class _InfoRow extends StatelessWidget {
  const _InfoRow({
    required this.icon,
    required this.label,
    required this.value,
    this.valueColor,
  });

  final IconData icon;
  final String label;
  final String value;
  final Color? valueColor;

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Icon(icon, size: 20, color: AppTheme.navy),
        const SizedBox(width: 12),
        Text(
          label,
          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                color: Colors.grey.shade600,
              ),
        ),
        const Spacer(),
        Text(
          value,
          style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                fontWeight: FontWeight.w600,
                color: valueColor ?? AppTheme.navy,
              ),
        ),
      ],
    );
  }
}
