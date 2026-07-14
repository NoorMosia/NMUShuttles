import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile_scanner/mobile_scanner.dart';
import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/marshal/models/passenger_model.dart';

/// QR scanner screen for marshal check-in.
/// Decodes the QR code token and submits it to POST /api/v1/checkin.
/// On success, shows a green banner and pops back with the result.
/// On failure, shows an inline error with the reason.
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

  bool _isProcessing = false;
  String? _errorMessage;
  bool _hasScanned = false;

  @override
  void dispose() {
    _scannerController.dispose();
    super.dispose();
  }

  /// Processes a detected barcode/QR code.
  Future<void> _onDetect(BarcodeCapture capture) async {
    if (_isProcessing || _hasScanned) return;

    final barcodes = capture.barcodes;
    if (barcodes.isEmpty) return;

    final rawValue = barcodes.first.rawValue;
    if (rawValue == null || rawValue.isEmpty) return;

    setState(() {
      _isProcessing = true;
      _errorMessage = null;
      _hasScanned = true;
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

      if (mounted) {
        // Show success banner
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Row(
              children: [
                const Icon(Icons.check_circle, color: Colors.white),
                const SizedBox(width: 12),
                Expanded(
                  child: Text(
                    '${result.studentName} — Boarded (Shuttle ${result.orderPosition})',
                    style: const TextStyle(color: Colors.white),
                  ),
                ),
              ],
            ),
            backgroundColor: Colors.green.shade700,
            duration: const Duration(seconds: 2),
          ),
        );

        // Pop back with the result so the passenger list updates
        Navigator.of(context).pop(result);
      }
    } on DioException catch (e) {
      final message = _mapCheckInError(e);
      setState(() {
        _isProcessing = false;
        _errorMessage = message;
        _hasScanned = false; // Allow re-scanning after error
      });
    } catch (e) {
      setState(() {
        _isProcessing = false;
        _errorMessage = 'An unexpected error occurred.';
        _hasScanned = false;
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

  /// Dismisses the error and allows re-scanning.
  void _dismissError() {
    setState(() {
      _errorMessage = null;
      _hasScanned = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('Scan QR Code'),
        backgroundColor: AppTheme.navy,
        foregroundColor: Colors.white,
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

          // Error message overlay
          if (_errorMessage != null)
            Positioned(
              left: 16,
              right: 16,
              bottom: 40,
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

          // Instruction text at bottom (when no error)
          if (_errorMessage == null && !_isProcessing)
            Positioned(
              left: 16,
              right: 16,
              bottom: 40,
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
    );
  }
}
