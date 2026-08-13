import 'package:dio/dio.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:mobile_scanner/mobile_scanner.dart';
import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';
import 'package:nmu_shuttle_app/features/marshal/providers/time_slot_passengers_provider.dart';

/// Combined check-in screen for a time slot.
/// Tab 1: QR Scanner (primary path)
/// Tab 2: Passenger list with search (dead-phone fallback)
///
/// After any check-in (QR or manual), shows the shuttle assignment so the
/// marshal can direct the student.
class MarshalCheckinScreen extends ConsumerStatefulWidget {
  const MarshalCheckinScreen({
    super.key,
    required this.departureTime,
    required this.direction,
    required this.tripDate,
  });

  final String departureTime;
  final String direction;
  final String tripDate;

  @override
  ConsumerState<MarshalCheckinScreen> createState() =>
      _MarshalCheckinScreenState();
}

class _MarshalCheckinScreenState extends ConsumerState<MarshalCheckinScreen>
    with SingleTickerProviderStateMixin {
  late TabController _tabController;
  final TextEditingController _searchController = TextEditingController();
  String _searchQuery = '';

  @override
  void initState() {
    super.initState();
    _tabController = TabController(length: 2, vsync: this);
  }

  @override
  void dispose() {
    _tabController.dispose();
    _searchController.dispose();
    super.dispose();
  }

  /// Display-friendly time for the app bar.
  String get _displayTime {
    if (widget.departureTime.length >= 5) {
      return widget.departureTime.substring(0, 5);
    }
    return widget.departureTime;
  }

  String get _directionLabel =>
      widget.direction == 'FromCampus' ? 'From Campus' : 'To Campus';

  TimeSlotQuery get _query => TimeSlotQuery(
        departureTime: widget.departureTime,
        direction: widget.direction,
        date: widget.tripDate,
        search: _searchQuery.isNotEmpty ? _searchQuery : null,
      );

  void _refreshPassengers() {
    ref.invalidate(timeSlotPassengersProvider(_query));
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text('$_displayTime  $_directionLabel'),
        backgroundColor: AppTheme.navy,
        foregroundColor: Colors.white,
        bottom: TabBar(
          controller: _tabController,
          indicatorColor: AppTheme.primaryGold,
          labelColor: Colors.white,
          unselectedLabelColor: Colors.white60,
          tabs: const [
            Tab(icon: Icon(Icons.qr_code_scanner), text: 'Scan QR'),
            Tab(icon: Icon(Icons.people), text: 'Passengers'),
          ],
        ),
      ),
      body: TabBarView(
        controller: _tabController,
        children: [
          _QrScannerTab(
            onCheckInSuccess: _refreshPassengers,
          ),
          _PassengerListTab(
            query: _query,
            searchController: _searchController,
            onSearchChanged: (value) {
              setState(() => _searchQuery = value);
            },
            onManualCheckIn: (bookingId) => _performManualCheckIn(bookingId),
          ),
        ],
      ),
    );
  }

  Future<void> _performManualCheckIn(int bookingId) async {
    try {
      final dioClient = ref.read(dioClientProvider);
      final response = await dioClient.dio.post(
        ApiConstants.manualCheckIn,
        data: {'bookingId': bookingId},
      );

      final data = response.data as Map<String, dynamic>;
      if (mounted) {
        _showCheckInSuccess(data);
        _refreshPassengers();
      }
    } on DioException catch (e) {
      if (mounted) {
        final message = _extractError(e);
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(message),
            backgroundColor: Colors.red.shade700,
          ),
        );
      }
    }
  }

  void _showCheckInSuccess(Map<String, dynamic> data) {
    final studentName = data['studentName'] as String? ?? 'Student';
    final convoyPosition = data['convoyPosition'] as int?;
    final licensePlate = data['licensePlate'] as String?;

    final shuttleInfo = convoyPosition != null
        ? 'Bus $convoyPosition${licensePlate != null ? ' ($licensePlate)' : ''}'
        : 'Assigned';

    showDialog(
      context: context,
      barrierDismissible: true,
      builder: (ctx) => AlertDialog(
        icon: const Icon(Icons.check_circle, color: Colors.green, size: 48),
        title: Text(studentName),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text('Successfully boarded'),
            const SizedBox(height: 12),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
              decoration: BoxDecoration(
                color: AppTheme.primaryGold.withValues(alpha: 0.2),
                borderRadius: BorderRadius.circular(8),
                border: Border.all(color: AppTheme.primaryGold),
              ),
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  const Icon(Icons.directions_bus,
                      color: AppTheme.navy, size: 20),
                  const SizedBox(width: 8),
                  Text(
                    shuttleInfo,
                    style: const TextStyle(
                      color: AppTheme.navy,
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
        actions: [
          FilledButton(
            onPressed: () => Navigator.pop(ctx),
            style: FilledButton.styleFrom(backgroundColor: AppTheme.navy),
            child: const Text('OK'),
          ),
        ],
      ),
    );
  }

  String _extractError(DioException e) {
    final data = e.response?.data;
    if (data is Map<String, dynamic>) {
      return data['message'] as String? ??
          data['detail'] as String? ??
          data['title'] as String? ??
          'Check-in failed';
    }
    return 'Check-in failed. Please try again.';
  }
}

// ─── QR Scanner Tab ────────────────────────────────────────────

class _QrScannerTab extends ConsumerStatefulWidget {
  const _QrScannerTab({required this.onCheckInSuccess});

  final VoidCallback onCheckInSuccess;

  @override
  ConsumerState<_QrScannerTab> createState() => _QrScannerTabState();
}

class _QrScannerTabState extends ConsumerState<_QrScannerTab>
    with AutomaticKeepAliveClientMixin {
  final MobileScannerController _scannerController = MobileScannerController(
    detectionSpeed: DetectionSpeed.normal,
    facing: CameraFacing.back,
  );

  bool _isProcessing = false;
  String? _errorMessage;
  bool _hasScanned = false;

  @override
  bool get wantKeepAlive => true;

  @override
  void dispose() {
    _scannerController.dispose();
    super.dispose();
  }

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

  Future<void> _submitCheckIn(String qrCodeToken) async {
    try {
      final dioClient = ref.read(dioClientProvider);
      final response = await dioClient.dio.post(
        ApiConstants.checkIn,
        data: {'qrCodeToken': qrCodeToken},
      );

      final data = response.data as Map<String, dynamic>;

      if (mounted) {
        widget.onCheckInSuccess();
        _showSuccessAndReset(data);
      }
    } on DioException catch (e) {
      final message = _mapError(e);
      setState(() {
        _isProcessing = false;
        _errorMessage = message;
        _hasScanned = false;
      });
    } catch (_) {
      setState(() {
        _isProcessing = false;
        _errorMessage = 'An unexpected error occurred.';
        _hasScanned = false;
      });
    }
  }

  void _showSuccessAndReset(Map<String, dynamic> data) {
    final studentName = data['studentName'] as String? ?? 'Student';
    final convoyPosition = data['convoyPosition'] as int?;
    final licensePlate = data['licensePlate'] as String?;

    final shuttleText = convoyPosition != null
        ? 'Bus $convoyPosition${licensePlate != null ? ' ($licensePlate)' : ''}'
        : '';

    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Row(
          children: [
            const Icon(Icons.check_circle, color: Colors.white),
            const SizedBox(width: 12),
            Expanded(
              child: Text(
                '$studentName — $shuttleText',
                style: const TextStyle(color: Colors.white),
              ),
            ),
          ],
        ),
        backgroundColor: Colors.green.shade700,
        duration: const Duration(seconds: 3),
      ),
    );

    // Reset after short delay to allow next scan
    Future.delayed(const Duration(seconds: 2), () {
      if (mounted) {
        setState(() {
          _isProcessing = false;
          _hasScanned = false;
        });
      }
    });
  }

  String _mapError(DioException e) {
    final statusCode = e.response?.statusCode;
    final data = e.response?.data;
    String? serverMessage;

    if (data is Map<String, dynamic>) {
      serverMessage = data['message'] as String? ??
          data['detail'] as String? ??
          data['title'] as String?;
    }

    switch (statusCode) {
      case 404:
        return serverMessage ?? 'QR code not found — invalid token.';
      case 409:
        return serverMessage ?? 'Student already boarded.';
      case 422:
        return serverMessage ?? 'Check-in failed — not confirmed or scan window closed.';
      case 403:
        return serverMessage ?? 'Not authorised to scan at this location.';
      default:
        return serverMessage ?? 'Check-in failed. Please try again.';
    }
  }

  void _dismissError() {
    setState(() {
      _errorMessage = null;
      _hasScanned = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    super.build(context);

    return Stack(
      children: [
        MobileScanner(
          controller: _scannerController,
          onDetect: _onDetect,
        ),

        // Scan frame overlay
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

        // Error overlay
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
                            style: TextStyle(
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

        // Instruction text
        if (_errorMessage == null && !_isProcessing)
          Positioned(
            left: 16,
            right: 16,
            bottom: 40,
            child: Container(
              padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
              decoration: BoxDecoration(
                color: Colors.black87,
                borderRadius: BorderRadius.circular(24),
              ),
              child: const Text(
                'Point camera at student\'s QR code',
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.white, fontSize: 14),
              ),
            ),
          ),
      ],
    );
  }
}

// ─── Passenger List Tab ────────────────────────────────────────

class _PassengerListTab extends ConsumerWidget {
  const _PassengerListTab({
    required this.query,
    required this.searchController,
    required this.onSearchChanged,
    required this.onManualCheckIn,
  });

  final TimeSlotQuery query;
  final TextEditingController searchController;
  final ValueChanged<String> onSearchChanged;
  final Future<void> Function(int bookingId) onManualCheckIn;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final passengersAsync = ref.watch(timeSlotPassengersProvider(query));

    return Column(
      children: [
        // Search bar
        Container(
          padding: const EdgeInsets.all(12),
          color: Colors.grey.shade50,
          child: TextField(
            controller: searchController,
            decoration: InputDecoration(
              hintText: 'Search by name or student number',
              prefixIcon: const Icon(Icons.search),
              suffixIcon: searchController.text.isNotEmpty
                  ? IconButton(
                      icon: const Icon(Icons.clear),
                      onPressed: () {
                        searchController.clear();
                        onSearchChanged('');
                      },
                    )
                  : null,
              border: OutlineInputBorder(
                borderRadius: BorderRadius.circular(10),
                borderSide: BorderSide(color: Colors.grey.shade300),
              ),
              filled: true,
              fillColor: Colors.white,
              contentPadding: const EdgeInsets.symmetric(horizontal: 16),
            ),
            onChanged: onSearchChanged,
          ),
        ),

        // Counts header
        passengersAsync.when(
          data: (list) => _buildCountsHeader(context, list),
          loading: () => const SizedBox.shrink(),
          error: (_, __) => const SizedBox.shrink(),
        ),

        const Divider(height: 1),

        // Passenger list
        Expanded(
          child: passengersAsync.when(
            data: (list) => _buildPassengerList(context, list),
            loading: () => const Center(
              child: CircularProgressIndicator(color: AppTheme.primaryGold),
            ),
            error: (error, _) => Center(
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.error_outline, size: 48, color: Colors.red),
                    const SizedBox(height: 12),
                    const Text('Failed to load passengers'),
                    const SizedBox(height: 12),
                    FilledButton.icon(
                      onPressed: () =>
                          ref.invalidate(timeSlotPassengersProvider(query)),
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
    );
  }

  Widget _buildCountsHeader(BuildContext context, TimeSlotPassengerList list) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
      color: AppTheme.navy.withValues(alpha: 0.05),
      child: Row(
        children: [
          _CountBadge(
              label: 'Total', count: list.totalConfirmed, color: AppTheme.navy),
          const SizedBox(width: 12),
          _CountBadge(
              label: 'Boarded',
              count: list.boardedCount,
              color: Colors.green.shade700),
          const SizedBox(width: 12),
          _CountBadge(
              label: 'Pending',
              count: list.pendingCount,
              color: Colors.orange.shade700),
        ],
      ),
    );
  }

  Widget _buildPassengerList(BuildContext context, TimeSlotPassengerList list) {
    if (list.passengers.isEmpty) {
      return const Center(
        child: Text(
          'No passengers found',
          style: TextStyle(color: Colors.grey, fontSize: 16),
        ),
      );
    }

    return ListView.separated(
      padding: const EdgeInsets.symmetric(vertical: 8),
      itemCount: list.passengers.length,
      separatorBuilder: (_, __) => const Divider(height: 1, indent: 72),
      itemBuilder: (context, index) {
        final passenger = list.passengers[index];
        return _PassengerListItem(
          passenger: passenger,
          onManualCheckIn: () => onManualCheckIn(passenger.bookingId),
        );
      },
    );
  }
}

// ─── Passenger List Item ───────────────────────────────────────

class _PassengerListItem extends StatelessWidget {
  const _PassengerListItem({
    required this.passenger,
    required this.onManualCheckIn,
  });

  final TimeSlotPassenger passenger;
  final VoidCallback onManualCheckIn;

  @override
  Widget build(BuildContext context) {
    final isBoarded = passenger.isBoarded;

    return ListTile(
      leading: CircleAvatar(
        backgroundColor:
            isBoarded ? Colors.green.shade100 : Colors.grey.shade200,
        child: Icon(
          isBoarded ? Icons.check_circle : Icons.circle_outlined,
          color: isBoarded ? Colors.green.shade700 : Colors.grey,
        ),
      ),
      title: Text(
        passenger.studentName,
        style: const TextStyle(fontWeight: FontWeight.w600),
      ),
      subtitle: Text(
        '${passenger.studentNumber}  •  ${passenger.stopName}',
        style: TextStyle(color: Colors.grey.shade600, fontSize: 12),
      ),
      trailing: isBoarded
          ? Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
              decoration: BoxDecoration(
                color: Colors.green.shade50,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: Colors.green.shade300),
              ),
              child: Text(
                'Boarded',
                style: TextStyle(
                  color: Colors.green.shade700,
                  fontSize: 12,
                  fontWeight: FontWeight.w600,
                ),
              ),
            )
          : FilledButton.tonal(
              onPressed: onManualCheckIn,
              style: FilledButton.styleFrom(
                backgroundColor: AppTheme.primaryGold.withValues(alpha: 0.3),
                foregroundColor: AppTheme.navy,
                padding:
                    const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                minimumSize: Size.zero,
                tapTargetSize: MaterialTapTargetSize.shrinkWrap,
              ),
              child: const Text(
                'Board',
                style: TextStyle(fontSize: 12, fontWeight: FontWeight.w700),
              ),
            ),
    );
  }
}

// ─── Count Badge Widget ────────────────────────────────────────

class _CountBadge extends StatelessWidget {
  const _CountBadge({
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
        padding: const EdgeInsets.symmetric(vertical: 8),
        decoration: BoxDecoration(
          color: color.withValues(alpha: 0.1),
          borderRadius: BorderRadius.circular(8),
          border: Border.all(color: color.withValues(alpha: 0.3)),
        ),
        child: Column(
          children: [
            Text(
              '$count',
              style: TextStyle(
                color: color,
                fontSize: 18,
                fontWeight: FontWeight.bold,
              ),
            ),
            Text(
              label,
              style: TextStyle(color: color, fontSize: 11),
            ),
          ],
        ),
      ),
    );
  }
}
