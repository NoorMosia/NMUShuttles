import 'dart:math';

import 'package:flutter/foundation.dart' show kIsWeb;
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:geolocator/geolocator.dart';
import 'package:go_router/go_router.dart';
import 'package:google_maps_flutter/google_maps_flutter.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/core/ui/error_display.dart';
import 'package:nmu_shuttle_app/features/student/models/stop_model.dart';
import 'package:nmu_shuttle_app/features/student/providers/maps_provider.dart';
import 'package:nmu_shuttle_app/features/student/providers/stops_provider.dart';
import 'package:nmu_shuttle_app/shared/widgets/hamburger_menu_button.dart';
import 'package:url_launcher/url_launcher.dart';

/// Maps screen displaying all active shuttle stops on an interactive Google Map.
/// Features: search filtering, marker popups, get directions, and nearby stops list.
/// Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6
class MapsScreen extends ConsumerStatefulWidget {
  const MapsScreen({super.key});

  @override
  ConsumerState<MapsScreen> createState() => _MapsScreenState();
}

class _MapsScreenState extends ConsumerState<MapsScreen>
    with SingleTickerProviderStateMixin {
  GoogleMapController? _mapController;
  final TextEditingController _searchController = TextEditingController();
  final FocusNode _searchFocusNode = FocusNode();
  String _searchQuery = '';

  // Device location state
  double? _deviceLat;
  double? _deviceLon;
  bool _locationPermissionDenied = false;
  bool _loadingLocation = false;

  // NMU campus area default center (Summerstrand campus)
  static const LatLng _defaultCenter = LatLng(-33.9880, 25.6680);
  static const double _defaultZoom = 12.0;

  // Bottom sheet state
  bool _showNearbyStops = false;

  // Single controller drives BOTH the sheet and the FAB row so they stay
  // perfectly in sync. 0 = hidden, 1 = fully shown.
  late final AnimationController _sheetController;
  late final Animation<double> _sheetAnim;

  // Sheet height (must match the container in _buildNearbyStopsSheet).
  static const double _sheetHeight = 300;

  @override
  void initState() {
    super.initState();
    _sheetController = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 450),
    );
    _sheetAnim = CurvedAnimation(
      parent: _sheetController,
      curve: Curves.easeOutCubic,
      reverseCurve: Curves.easeInCubic,
    );
    _requestDeviceLocation();
  }

  /// Opens or closes the nearby stops sheet, driving the shared animation.
  void _setNearbyStops(bool show) {
    setState(() => _showNearbyStops = show);
    if (show) {
      _sheetController.forward();
    } else {
      _sheetController.reverse();
    }
  }

  @override
  void dispose() {
    _sheetController.dispose();
    _searchController.dispose();
    _searchFocusNode.dispose();
    _mapController?.dispose();
    super.dispose();
  }

  /// Requests device location permission and retrieves GPS position using geolocator.
  Future<void> _requestDeviceLocation() async {
    setState(() => _loadingLocation = true);

    try {
      // Check if location services are enabled
      final serviceEnabled = await Geolocator.isLocationServiceEnabled();
      if (!serviceEnabled) {
        setState(() {
          _loadingLocation = false;
          _locationPermissionDenied = true;
        });
        return;
      }

      // Check and request permission
      var permission = await Geolocator.checkPermission();
      if (permission == LocationPermission.denied) {
        permission = await Geolocator.requestPermission();
        if (permission == LocationPermission.denied) {
          setState(() {
            _loadingLocation = false;
            _locationPermissionDenied = true;
          });
          return;
        }
      }

      if (permission == LocationPermission.deniedForever) {
        setState(() {
          _loadingLocation = false;
          _locationPermissionDenied = true;
        });
        return;
      }

      // Get the current position
      final position = await Geolocator.getCurrentPosition(
        desiredAccuracy: LocationAccuracy.high,
      );

      setState(() {
        _deviceLat = position.latitude;
        _deviceLon = position.longitude;
        _loadingLocation = false;
        _locationPermissionDenied = false;
      });

      // Invalidate nearby stops provider so it recalculates
      ref.invalidate(nearbyStopsProvider);
    } catch (e) {
      setState(() {
        _loadingLocation = false;
        _locationPermissionDenied = true;
      });
    }
  }

  /// Calculates distance in km between two GPS points using Haversine formula.
  double _calculateDistance(
      double lat1, double lon1, double lat2, double lon2) {
    const earthRadius = 6371.0; // km
    final dLat = _toRadians(lat2 - lat1);
    final dLon = _toRadians(lon2 - lon1);
    final a = sin(dLat / 2) * sin(dLat / 2) +
        cos(_toRadians(lat1)) *
            cos(_toRadians(lat2)) *
            sin(dLon / 2) *
            sin(dLon / 2);
    final c = 2 * atan2(sqrt(a), sqrt(1 - a));
    return earthRadius * c;
  }

  double _toRadians(double degrees) => degrees * pi / 180;

  /// Launches device navigation to the given coordinates via Google Maps intent.
  Future<void> _launchDirections(double lat, double lon) async {
    final uri = Uri.parse(
      'https://www.google.com/maps/dir/?api=1&destination=$lat,$lon&travelmode=driving',
    );
    if (await canLaunchUrl(uri)) {
      await launchUrl(uri, mode: LaunchMode.externalApplication);
    } else {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Could not open maps application')),
        );
      }
    }
  }

  /// Shows a bottom sheet with stop details and a "Get Directions" button.
  void _showStopDetails(MapStopModel stop) {
    showModalBottomSheet(
      context: context,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(16)),
      ),
      builder: (context) => Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            // Drag handle
            Center(
              child: Container(
                width: 40,
                height: 4,
                margin: const EdgeInsets.only(bottom: 16),
                decoration: BoxDecoration(
                  color: Colors.grey.shade300,
                  borderRadius: BorderRadius.circular(2),
                ),
              ),
            ),
            // Stop name
            Text(
              stop.stopName,
              style: Theme.of(context).textTheme.titleLarge?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: AppTheme.navy,
                  ),
            ),
            const SizedBox(height: 8),
            // Full address
            Row(
              children: [
                Icon(Icons.location_on_outlined,
                    size: 18, color: Colors.grey.shade600),
                const SizedBox(width: 6),
                Expanded(
                  child: Text(
                    stop.fullAddress,
                    style: Theme.of(context).textTheme.bodyMedium?.copyWith(
                          color: Colors.grey.shade700,
                        ),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 8),
            // Coordinates
            Row(
              children: [
                Icon(Icons.gps_fixed, size: 18, color: Colors.grey.shade600),
                const SizedBox(width: 6),
                Text(
                  '${stop.latitude.toStringAsFixed(4)}, ${stop.longitude.toStringAsFixed(4)}',
                  style: Theme.of(context).textTheme.bodySmall?.copyWith(
                        color: Colors.grey.shade500,
                      ),
                ),
              ],
            ),
            // Distance from device (if available)
            if (_deviceLat != null && _deviceLon != null) ...[
              const SizedBox(height: 8),
              Row(
                children: [
                  Icon(Icons.directions_walk,
                      size: 18, color: Colors.grey.shade600),
                  const SizedBox(width: 6),
                  Text(
                    '${_calculateDistance(_deviceLat!, _deviceLon!, stop.latitude, stop.longitude).toStringAsFixed(1)} km away',
                    style: Theme.of(context).textTheme.bodySmall?.copyWith(
                          color: Colors.grey.shade600,
                        ),
                  ),
                ],
              ),
            ],
            const SizedBox(height: 20),
            // Book button — takes user to the booking page with this stop
            // pre-selected as the origin.
            SizedBox(
              width: double.infinity,
              child: ElevatedButton.icon(
                onPressed: () {
                  Navigator.pop(context);
                  context.go(
                    '/student/booking/new?stopId=${stop.stopId}',
                  );
                },
                icon: const Icon(Icons.directions_bus),
                label: const Text('Book a Shuttle'),
                style: ElevatedButton.styleFrom(
                  elevation: 0,
                  backgroundColor: AppTheme.primaryGold,
                  foregroundColor: AppTheme.navy,
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
            ),
            const SizedBox(height: 10),
            // Get Directions button (secondary)
            SizedBox(
              width: double.infinity,
              child: OutlinedButton.icon(
                onPressed: () {
                  Navigator.pop(context);
                  _launchDirections(stop.latitude, stop.longitude);
                },
                icon: const Icon(Icons.directions),
                label: const Text('Get Directions'),
                style: OutlinedButton.styleFrom(
                  foregroundColor: AppTheme.navy,
                  side: const BorderSide(color: AppTheme.navy),
                  padding: const EdgeInsets.symmetric(vertical: 14),
                  shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(12),
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }

  /// Builds map markers from the filtered stops list.
  Set<Marker> _buildMarkers(List<MapStopModel> stops) {
    return stops.map((stop) {
      return Marker(
        markerId: MarkerId('stop_${stop.stopId}'),
        position: LatLng(stop.latitude, stop.longitude),
        infoWindow: InfoWindow(
          title: stop.stopName,
          snippet: stop.fullAddress,
        ),
        icon: BitmapDescriptor.defaultMarkerWithHue(BitmapDescriptor.hueOrange),
        onTap: () => _showStopDetails(stop),
      );
    }).toSet();
  }

  /// Filters stops by search query matching stopName or fullAddress.
  List<MapStopModel> _filterStops(List<MapStopModel> stops) {
    if (_searchQuery.isEmpty) return stops;
    final query = _searchQuery.toLowerCase();
    return stops.where((stop) {
      return stop.stopName.toLowerCase().contains(query) ||
          stop.fullAddress.toLowerCase().contains(query);
    }).toList();
  }

  /// Sorts stops by proximity to device location.
  List<MapStopModel> _sortByProximity(List<MapStopModel> stops) {
    if (_deviceLat == null || _deviceLon == null) return stops;
    final sorted = List<MapStopModel>.from(stops);
    sorted.sort((a, b) {
      final distA =
          _calculateDistance(_deviceLat!, _deviceLon!, a.latitude, a.longitude);
      final distB =
          _calculateDistance(_deviceLat!, _deviceLon!, b.latitude, b.longitude);
      return distA.compareTo(distB);
    });
    return sorted;
  }

  @override
  Widget build(BuildContext context) {
    final stopsAsync = ref.watch(mapStopsProvider);

    return Scaffold(
      body: stopsAsync.when(
        loading: () => const Center(
          child: CircularProgressIndicator(color: AppTheme.primaryGold),
        ),
        error: (error, stack) => ErrorRetry(
          error: error,
          title: 'Failed to load stops',
          onRetry: () => ref.invalidate(mapStopsProvider),
        ),
        data: (allStops) {
          final filteredStops = _filterStops(allStops);
          final markers = _buildMarkers(filteredStops);

          return Stack(
            children: [
              // Google Map base layer
              GoogleMap(
                initialCameraPosition: const CameraPosition(
                  target: _defaultCenter,
                  zoom: _defaultZoom,
                ),
                markers: markers,
                myLocationEnabled: !_locationPermissionDenied,
                myLocationButtonEnabled: false,
                zoomControlsEnabled: false,
                mapToolbarEnabled: false,
                onMapCreated: (controller) {
                  _mapController = controller;
                },
                // Tapping the map (outside any control) dismisses the keyboard,
                // closes the nearby stops sheet, and clears the search dropdown.
                onTap: (_) {
                  _searchFocusNode.unfocus();
                  if (_showNearbyStops) _setNearbyStops(false);
                  if (_searchQuery.isNotEmpty) {
                    setState(() {
                      _searchQuery = '';
                      _searchController.clear();
                    });
                  }
                },
              ),

              // Hamburger menu button (top-left)
              const HamburgerMenuButton(),

              // Search bar overlay (top)
              Positioned(
                top: MediaQuery.of(context).padding.top + 12,
                left: 64,
                right: 16,
                child: Material(
                  elevation: 0,
                  borderRadius: BorderRadius.circular(12),
                  child: TextField(
                    controller: _searchController,
                    focusNode: _searchFocusNode,
                    maxLength: 60,
                    onChanged: (value) {
                      setState(() {
                        _searchQuery = value;
                      });
                      if (value.isNotEmpty && _showNearbyStops) {
                        _setNearbyStops(false);
                      }
                    },
                    onTap: () {
                      if (_showNearbyStops) _setNearbyStops(false);
                    },
                    decoration: InputDecoration(
                      counterText: '',
                      hintText: 'Search stops...',
                      prefixIcon:
                          const Icon(Icons.search, color: AppTheme.navy),
                      suffixIcon: _searchQuery.isNotEmpty
                          ? IconButton(
                              icon: const Icon(Icons.clear),
                              onPressed: () {
                                _searchController.clear();
                                setState(() => _searchQuery = '');
                              },
                            )
                          : null,
                      filled: true,
                      fillColor: Colors.white,
                      border: OutlineInputBorder(
                        borderRadius: BorderRadius.circular(12),
                        borderSide: BorderSide.none,
                      ),
                      contentPadding: const EdgeInsets.symmetric(
                        horizontal: 16,
                        vertical: 12,
                      ),
                    ),
                  ),
                ),
              ),

              // Location permission denied banner
              if (_locationPermissionDenied)
                Positioned(
                  top: MediaQuery.of(context).padding.top + 72,
                  left: 16,
                  right: 16,
                  child: Material(
                    elevation: 0,
                    borderRadius: BorderRadius.circular(8),
                    color: Colors.orange.shade50,
                    child: Padding(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 12, vertical: 10),
                      child: Row(
                        children: [
                          Icon(Icons.location_off,
                              size: 20, color: Colors.orange.shade700),
                          const SizedBox(width: 8),
                          Expanded(
                            child: Text(
                              'Location access denied. Enable it for nearby stops.',
                              style: Theme.of(context)
                                  .textTheme
                                  .bodySmall
                                  ?.copyWith(color: Colors.orange.shade800),
                            ),
                          ),
                          TextButton(
                            onPressed: () async {
                              // `openAppSettings()` is not implemented on web
                              // (geolocator_web throws). On web, re-requesting
                              // triggers the browser's permission prompt instead.
                              if (!kIsWeb) {
                                await Geolocator.openAppSettings();
                              }
                              _requestDeviceLocation();
                            },
                            child: const Text('Enable'),
                          ),
                        ],
                      ),
                    ),
                  ),
                ),

              // Search results dropdown (when searching)
              if (_searchQuery.isNotEmpty && filteredStops.isNotEmpty)
                Positioned(
                  top: MediaQuery.of(context).padding.top + 68,
                  left: 64,
                  right: 16,
                  child: Material(
                    elevation: 0,
                    borderRadius: BorderRadius.circular(12),
                    child: ConstrainedBox(
                      constraints: const BoxConstraints(maxHeight: 200),
                      child: ListView.separated(
                        shrinkWrap: true,
                        padding: const EdgeInsets.symmetric(vertical: 4),
                        itemCount: filteredStops.length,
                        separatorBuilder: (_, __) => Divider(
                            height: 1, thickness: 0.5, color: Colors.grey.shade200, indent: 16, endIndent: 16),
                        itemBuilder: (context, index) {
                          final stop = filteredStops[index];
                          return ListTile(
                            dense: true,
                            leading: const Icon(Icons.location_on,
                                color: AppTheme.primaryGold, size: 20),
                            title: Text(
                              stop.stopName,
                              style: TextStyle(
                                  fontSize: 14, color: Colors.grey.shade800),
                            ),
                            subtitle: Text(
                              stop.fullAddress,
                              style: TextStyle(
                                  fontSize: 12, color: Colors.grey.shade500),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                            onTap: () {
                              // Move camera to the stop
                              _mapController?.animateCamera(
                                CameraUpdate.newLatLngZoom(
                                  LatLng(stop.latitude, stop.longitude),
                                  15.0,
                                ),
                              );
                              _showStopDetails(stop);
                              // Clear search
                              _searchController.clear();
                              setState(() => _searchQuery = '');
                            },
                          );
                        },
                      ),
                    ),
                  ),
                ),

              // Nearby Stops FAB + My Location button (side by side).
              // Driven by the shared sheet animation so it rises/falls exactly
              // in step with the sheet.
              AnimatedBuilder(
                animation: _sheetAnim,
                builder: (context, child) {
                  // Travels from 24 (hidden) to 320 (shown) in lockstep.
                  final bottom = 24 + (_sheetAnim.value * (320 - 24));
                  return Positioned(
                    bottom: bottom,
                    right: 16,
                    child: child!,
                  );
                },
                child: Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    // Nearby stops button
                    FloatingActionButton.extended(
                      heroTag: 'nearby_stops',
                      elevation: 0,
                      highlightElevation: 0,
                      onPressed: () {
                        final willShow = !_showNearbyStops;
                        if (willShow) {
                          _searchFocusNode.unfocus();
                          _searchController.clear();
                          setState(() => _searchQuery = '');
                        }
                        _setNearbyStops(willShow);
                      },
                      backgroundColor: AppTheme.primaryGold,
                      foregroundColor: AppTheme.navy,
                      icon: Icon(
                          _showNearbyStops ? Icons.close : Icons.near_me),
                      label: Text(
                          _showNearbyStops ? 'Close' : 'Nearby Stops'),
                    ),
                    const SizedBox(width: 12),
                    // My Location button (same height as extended FAB)
                    SizedBox(
                      height: 48,
                      width: 48,
                      child: FloatingActionButton(
                        heroTag: 'my_location',
                        elevation: 0,
                        highlightElevation: 0,
                        onPressed: () {
                          if (_deviceLat != null && _deviceLon != null) {
                            _mapController?.animateCamera(
                              CameraUpdate.newLatLngZoom(
                                LatLng(_deviceLat!, _deviceLon!),
                                14.0,
                              ),
                            );
                          } else {
                            _requestDeviceLocation();
                          }
                        },
                        backgroundColor: Colors.white,
                        shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(16),
                        ),
                        child: Icon(
                          _locationPermissionDenied
                              ? Icons.location_disabled
                              : Icons.my_location,
                          color: AppTheme.navy,
                        ),
                      ),
                    ),
                  ],
                ),
              ),

              // Nearby Stops bottom sheet — slides up/down smoothly, driven by
              // the shared animation so it moves in lockstep with the FAB row.
              AnimatedBuilder(
                animation: _sheetAnim,
                builder: (context, child) {
                  // Travels from -_sheetHeight (hidden) to 0 (shown).
                  final bottom = (_sheetAnim.value - 1) * _sheetHeight;
                  return Positioned(
                    bottom: bottom,
                    left: 0,
                    right: 0,
                    child: Opacity(
                      opacity: _sheetAnim.value.clamp(0.0, 1.0),
                      child: IgnorePointer(
                        ignoring: !_showNearbyStops,
                        child: child,
                      ),
                    ),
                  );
                },
                child: _buildNearbyStopsSheet(allStops),
              ),
            ],
          );
        },
      ),
    );
  }

  /// Builds the nearby stops bottom sheet panel.
  Widget _buildNearbyStopsSheet(List<MapStopModel> allStops) {
    final nearbyStops = _sortByProximity(allStops);

    return Container(
      height: 300,
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: const BorderRadius.vertical(top: Radius.circular(16)),
      ),
      child: Column(
        children: [
          // Drag handle
          Padding(
            padding: const EdgeInsets.only(top: 12, bottom: 8),
            child: Container(
              width: 40,
              height: 4,
              decoration: BoxDecoration(
                color: Colors.grey.shade300,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
          ),
          // Title
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 4),
            child: Row(
              children: [
                const Icon(Icons.near_me, color: AppTheme.navy, size: 20),
                const SizedBox(width: 8),
                Text(
                  'Nearby Stops',
                  style: Theme.of(context).textTheme.titleMedium?.copyWith(
                        fontWeight: FontWeight.bold,
                        color: AppTheme.navy,
                      ),
                ),
                const Spacer(),
                if (_loadingLocation)
                  const SizedBox(
                    width: 16,
                    height: 16,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  ),
              ],
            ),
          ),
          Divider(height: 1, thickness: 0.5, color: Colors.grey.shade200),
          // Stops list
          Expanded(
            child: _locationPermissionDenied
                ? _buildLocationPermissionPrompt()
                : nearbyStops.isEmpty
                    ? const Center(child: Text('No stops available'))
                    : ListView.separated(
                        padding: const EdgeInsets.symmetric(vertical: 4),
                        itemCount: nearbyStops.length,
                        separatorBuilder: (_, __) => Divider(
                            height: 1, thickness: 0.5, color: Colors.grey.shade200, indent: 56, endIndent: 16),
                        itemBuilder: (context, index) {
                          final stop = nearbyStops[index];
                          final distance = _deviceLat != null
                              ? _calculateDistance(_deviceLat!, _deviceLon!,
                                  stop.latitude, stop.longitude)
                              : null;

                          return ListTile(
                            leading: const CircleAvatar(
                              backgroundColor: AppTheme.primaryGold,
                              child: Icon(Icons.location_on,
                                  color: AppTheme.navy, size: 20),
                            ),
                            title: Text(
                              stop.stopName,
                              style: Theme.of(context)
                                  .textTheme
                                  .titleSmall
                                  ?.copyWith(
                                    fontWeight: FontWeight.w600,
                                    color: AppTheme.navy,
                                  ),
                            ),
                            subtitle: Text(
                              stop.fullAddress,
                              style: TextStyle(
                                  fontSize: 12, color: Colors.grey.shade500),
                              maxLines: 1,
                              overflow: TextOverflow.ellipsis,
                            ),
                            trailing: distance != null
                                ? Text(
                                    '${distance.toStringAsFixed(1)} km',
                                    style: TextStyle(
                                      fontSize: 12,
                                      color: Colors.grey.shade500,
                                      fontWeight: FontWeight.w500,
                                    ),
                                  )
                                : null,
                            onTap: () {
                              _mapController?.animateCamera(
                                CameraUpdate.newLatLngZoom(
                                  LatLng(stop.latitude, stop.longitude),
                                  15.0,
                                ),
                              );
                              _setNearbyStops(false);
                              _showStopDetails(stop);
                            },
                          );
                        },
                      ),
          ),
        ],
      ),
    );
  }

  /// Builds the permission prompt shown when location access is denied.
  Widget _buildLocationPermissionPrompt() {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(Icons.location_off, size: 40, color: Colors.orange.shade400),
            const SizedBox(height: 12),
            Text(
              'Location Permission Required',
              style: Theme.of(context).textTheme.titleSmall?.copyWith(
                    fontWeight: FontWeight.bold,
                  ),
            ),
            const SizedBox(height: 8),
            Text(
              'Allow location access to see stops sorted by distance from your current position.',
              textAlign: TextAlign.center,
              style: Theme.of(context).textTheme.bodySmall?.copyWith(
                    color: Colors.grey.shade600,
                  ),
            ),
            const SizedBox(height: 16),
            ElevatedButton.icon(
              onPressed: _requestDeviceLocation,
              icon: const Icon(Icons.location_on),
              label: const Text('Grant Permission'),
              style: ElevatedButton.styleFrom(
                elevation: 0,
                backgroundColor: AppTheme.primaryGold,
                foregroundColor: AppTheme.navy,
              ),
            ),
          ],
        ),
      ),
    );
  }
}
