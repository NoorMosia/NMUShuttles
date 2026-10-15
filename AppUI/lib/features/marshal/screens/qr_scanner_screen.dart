import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile_scanner/mobile_scanner.dart';
import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/marshal/models/passenger_model.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/passenger_list_provider.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/convoy_provider.dart';

/// QR scanner screen for marshal check-in.
/// Decodes the QR code token and submits it to POST /api/v1/checkin.
///
/// Continuous scanning: the camera stays active after each scan. A successful
/// check-in shows a persistent result panel (student + assigned shuttle) that
/// remains until the next code is scanned. The same code is never processed
/// twice in a row — a different code (or a short cooldown) is required before
/// the scanner fires again.
class QrScannerScreen extends ConsumerStatefulWidget {
  const QrScannerScreen({super.key, required this.tripId});

  final int tripId;

  @override
  ConsumerState<QrScannerScreen> createState() => _QrScannerScreenState();
}

class _QrScannerScreenState extends ConsumerState<QrScannerScreen> {
  final MobileScannerController _scannerController = MobileScannerController(
    detectionSpeed: DetectionSpeed.normal,
    facing: CameraFacing.back,
  );

  /// Cooldown after a scan before the SAME value may fire again. A different
  /// value is accepted immediately. This stops the camera from re-triggering
  /// on the same QR that is still sitting in front of the lens.
  static const Duration _sameCodeCooldown = Duration(seconds: 3);

  bool _isProcessing = false;
  String? _errorMessage;

  /// The raw value of the last barcode we acted on. A detection is ignored
  /// while it matches this value within the cooldown window.
  String? _lastScannedValue;
  DateTime? _lastScanAt;

  /// The most recent successful check-in, shown in a persistent panel until
  /// the next scan. Also returned to the caller on back-navigation so the
  /// passenger list refreshes.
  CheckInResultModel? _lastResult;

  @override
  void dispose() {
    _scannerController.dispose();
    super.dispose();
  }

  /// Processes a detected barcode/QR code.
  Future<void> _onDetect(BarcodeCapture capture) async {
    if (_isProcessing) return;

    final barcodes = capture.barcodes;
    if (barcodes.isEmpty) return;

    final rawValue = barcodes.first.rawValue;
    if (rawValue == null || rawValue.isEmpty) return;

    // Dedupe: ignore the same code until it changes or the cooldown elapses.
    if (rawValue == _lastScannedValue) {
      final elapsed = _lastScanAt == null
          ? _sameCodeCooldown
          : DateTime.now().difference(_lastScanAt!);
      if (elapsed < _sameCodeCooldown) return;
    }

    setState(() {
      _isProcessing = true;
      _errorMessage = null;
      _lastScannedValue = rawValue;
      _lastScanAt = DateTime.now();
    });

    await _submitCheckIn(rawValue);
  }

  /// Submits the check-in with the scanned QR token.
  Future<void> _submitCheckIn(String qrCodeToken) async {
    try {
      final dioClient = ref.read(dioClientProvider);
      final response = await dioClient.dio.post(
        ApiConstants.checkIn,
        data: {'qrCodeToken': qrCodeToken},
      );

      final result =
          CheckInResultModel.fromJson(response.data as Map<String, dynamic>);

      if (!mounted) return;

      // Refresh the manifest + convoy counts immediately so the passenger list
      // is current even while the marshal keeps scanning.
      ref.invalidate(passengerManifestProvider(widget.tripId));
      ref.invalidate(convoyStatusProvider(widget.tripId));

      // Keep scanning: show the result in a persistent panel, reset the
      // processing flag so the next (different) code can be scanned. Record
      // the scan time so the cooldown applies to this same code.
      setState(() {
        _isProcessing = false;
        _errorMessage = null;
        _lastResult = result;
        _lastScanAt = DateTime.now();
      });
    } on DioException catch (e) {
      final message = _mapCheckInError(e);
      if (!mounted) return;
      setState(() {
        _isProcessing = false;
        _errorMessage = message;
        _lastResult = null;
        _lastScanAt = DateTime.now();
      });
    } catch (e) {
      if (!mounted) return;
      setState(() {
        _isProcessing = false;
        _errorMessage = 'An unexpected error occurred.';
        _lastResult = null;
        _lastScanAt = DateTime.now();
      });
    }
  }

  /// Maps API error responses to user-friendly messages.
  String _mapCheckInError(DioException e) {
    final statusCode = e.response?.statusCode;
    final data = e.response?.data;
    String? serverMessage;

    if (data is Map<String, dynamic>) {
      serverMessage = data['message'] as String? ??
          data['error'] as String? ??
          data['title'] as String?;
    }

    switch (statusCode) {
      case 404:
        return serverMessage ?? 'QR code not found — invalid token.';
      case 409:
        return serverMessage ?? 'Student already boarded.';
      case 422:
        return serverMessage ??
            'Check-in failed — booking not confirmed, scan window not open, or convoy full.';
      case 403:
        return serverMessage ??
            'You are not authorised to scan at this location.';
      default:
        return serverMessage ?? 'Check-in failed. Please try again.';
    }
  }

  /// Dismisses the error and allows the same code to be re-scanned immediately.
  void _dismissError() {
    setState(() {
      _errorMessage = null;
      _lastScannedValue = null;
      _lastScanAt = null;
    });
  }

  /// Returns the most recent successful result to the caller (if any) so the
  /// passenger list refreshes, then leaves the scanner.
  void _finishScanning() {
    Navigator.of(context).pop(_lastResult);
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return PopScope(
      canPop: false,
      onPopInvokedWithResult: (didPop, result) {
        if (didPop) return;
        Navigator.of(context).pop(_lastResult);
      },
      child: Scaffold(
      appBar: AppBar(
        title: const Text('Scan QR Code'),
        backgroundColor: AppTheme.navy,
        foregroundColor: Colors.white,
        leading: IconButton(
          icon: const Icon(Icons.arrow_back),
          onPressed: _finishScanning,
        ),
        actions: [
          TextButton(
            onPressed: _finishScanning,
            child: const Text('Done', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
      body: Stack(
        children: [
          // Camera scanner
          MobileScanner(
            controller: _scannerController,
            onDetect: _onDetect,
          ),

          // Scan overlay frame
          Center(
            child: Container(
              width: 280,
              height: 280,
              decoration: BoxDecoration(
                border: Border.all(
                  color: _errorMessage != null
                      ? Colors.red
                      : _lastResult != null
                          ? Colors.green
                          : AppTheme.primaryGold,
                  width: 3,
                ),
                borderRadius: BorderRadius.circular(16),
              ),
            ),
          ),

          // Processing indicator
          if (_isProcessing)
            Container(
              color: Colors.black54,
              child: const Center(
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    CircularProgressIndicator(color: AppTheme.primaryGold),
                    SizedBox(height: 16),
                    Text(
                      'Processing check-in...',
                      style: TextStyle(color: Colors.white, fontSize: 16),
                    ),
                  ],
                ),
              ),
            ),

          // Error message overlay. Clear the bottom safe-area inset so the
          // "Scan Again" button under a two-line message is never clipped.
          if (_errorMessage != null)
            Positioned(
              left: 16,
              right: 16,
              bottom: 24 + MediaQuery.of(context).padding.bottom,
              child: Card(
                color: Colors.red.shade50,
                elevation: 0,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                  side: BorderSide(color: Colors.red.shade300),
                ),
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      Row(
                        children: [
                          Icon(Icons.error, color: Colors.red.shade700),
                          const SizedBox(width: 12),
                          Expanded(
                            child: Text(
                              _errorMessage!,
                              style: theme.textTheme.bodyMedium?.copyWith(
                                color: Colors.red.shade900,
                                fontWeight: FontWeight.w500,
                              ),
                            ),
                          ),
                        ],
                      ),
                      const SizedBox(height: 12),
                      SizedBox(
                        width: double.infinity,
                        child: FilledButton(
                          onPressed: _dismissError,
                          style: FilledButton.styleFrom(
                            backgroundColor: AppTheme.navy,
                          ),
                          child: const Text('Scan Again'),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),

          // Persistent success panel — stays until the next scan.
          if (_errorMessage == null && !_isProcessing && _lastResult != null)
            Positioned(
              left: 16,
              right: 16,
              bottom: 24 + MediaQuery.of(context).padding.bottom,
              child: Card(
                color: Colors.green.shade50,
                elevation: 0,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(12),
                  side: BorderSide(color: Colors.green.shade300),
                ),
                child: Padding(
                  padding: const EdgeInsets.all(16),
                  child: Row(
                    children: [
                      Icon(Icons.check_circle, color: Colors.green.shade700),
                      const SizedBox(width: 12),
                      Expanded(
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            Text(
                              _lastResult!.studentName,
                              style: theme.textTheme.titleMedium?.copyWith(
                                color: Colors.green.shade900,
                                fontWeight: FontWeight.w700,
                              ),
                            ),
                            const SizedBox(height: 2),
                            Text(
                              'Boarded — Shuttle ${_lastResult!.orderPosition} '
                              '(${_lastResult!.assignedShuttleLicensePlate})',
                              style: theme.textTheme.bodyMedium?.copyWith(
                                color: Colors.green.shade800,
                              ),
                            ),
                            const SizedBox(height: 4),
                            Text(
                              'Scan the next student',
                              style: theme.textTheme.bodySmall?.copyWith(
                                color: Colors.green.shade700,
                                fontStyle: FontStyle.italic,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
            ),

          // Instruction text at bottom (before the first scan).
          if (_errorMessage == null && !_isProcessing && _lastResult == null)
            Positioned(
              left: 16,
              right: 16,
              bottom: 24 + MediaQuery.of(context).padding.bottom,
              child: Container(
                padding: const EdgeInsets.symmetric(
                  horizontal: 24,
                  vertical: 12,
                ),
                decoration: BoxDecoration(
                  color: Colors.black87,
                  borderRadius: BorderRadius.circular(24),
                ),
                child: const Text(
                  'Point camera at student\'s QR code',
                  textAlign: TextAlign.center,
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 14,
                  ),
                ),
              ),
            ),
        ],
      ),
      ),
    );
  }
}
