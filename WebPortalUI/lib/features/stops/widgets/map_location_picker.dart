import 'package:flutter/material.dart';
import 'package:flutter_map/flutter_map.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:latlong2/latlong.dart';

/// An interactive map that lets an admin pick a shuttle stop location by
/// tapping. The selected point is written back to the shared [latController]
/// and [lngController] (as plain decimal strings), keeping the existing
/// "lat,lng" GpsCoordinates pipeline unchanged.
///
/// The map also reacts to manual edits of those two text fields: typing a
/// valid coordinate moves the marker and recentres the map.
class MapLocationPicker extends StatefulWidget {
  const MapLocationPicker({
    super.key,
    required this.latController,
    required this.lngController,
    this.height = 260,
  });

  final TextEditingController latController;
  final TextEditingController lngController;
  final double height;

  @override
  State<MapLocationPicker> createState() => _MapLocationPickerState();
}

class _MapLocationPickerState extends State<MapLocationPicker> {
  static const Color _navy = Color(0xFF132E51);

  /// Default centre: NMU South Campus, Gqeberha.
  static const LatLng _defaultCenter = LatLng(-33.9803, 25.6659);
  static const double _defaultZoom = 13;

  final MapController _mapController = MapController();
  LatLng? _selected;

  @override
  void initState() {
    super.initState();
    _selected = _readControllers();
    // Re-sync the marker when the user types into the lat/lng fields.
    widget.latController.addListener(_onControllersChanged);
    widget.lngController.addListener(_onControllersChanged);
  }

  @override
  void dispose() {
    widget.latController.removeListener(_onControllersChanged);
    widget.lngController.removeListener(_onControllersChanged);
    _mapController.dispose();
    super.dispose();
  }

  /// Parses the current controller text into a valid LatLng, or null.
  LatLng? _readControllers() {
    final lat = double.tryParse(widget.latController.text.trim());
    final lng = double.tryParse(widget.lngController.text.trim());
    if (lat == null || lng == null) return null;
    if (lat < -90 || lat > 90 || lng < -180 || lng > 180) return null;
    return LatLng(lat, lng);
  }

  void _onControllersChanged() {
    final parsed = _readControllers();
    if (parsed == null) return;
    // Avoid feedback loops: only update when the value actually changed.
    if (_selected != null &&
        (parsed.latitude - _selected!.latitude).abs() < 1e-9 &&
        (parsed.longitude - _selected!.longitude).abs() < 1e-9) {
      return;
    }
    setState(() => _selected = parsed);
    _safeMove(parsed, _mapController.camera.zoom);
  }

  void _safeMove(LatLng target, double zoom) {
    // The controller throws if the map isn't mounted yet; guard with a frame.
    WidgetsBinding.instance.addPostFrameCallback((_) {
      if (!mounted) return;
      try {
        _mapController.move(target, zoom);
      } catch (_) {
        // Map not ready yet; ignore.
      }
    });
  }

  void _onMapTap(TapPosition _, LatLng point) {
    setState(() => _selected = point);
    // Write back as trimmed decimals (5 dp ≈ 1.1 m precision).
    widget.latController.text = point.latitude.toStringAsFixed(5);
    widget.lngController.text = point.longitude.toStringAsFixed(5);
  }

  @override
  Widget build(BuildContext context) {
    final center = _selected ?? _defaultCenter;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Row(
          children: [
            const Icon(Icons.map_outlined, size: 18, color: _navy),
            const SizedBox(width: 6),
            Text(
              'Pick location on map',
              style: GoogleFonts.hankenGrotesk(
                fontWeight: FontWeight.w600,
                color: _navy,
                fontSize: 14,
              ),
            ),
            const Spacer(),
            Text(
              _selected == null ? 'Tap the map to set' : 'Tap to adjust',
              style: GoogleFonts.hankenGrotesk(
                fontSize: 12,
                color: Colors.grey.shade500,
              ),
            ),
          ],
        ),
        const SizedBox(height: 8),
        ClipRRect(
          borderRadius: BorderRadius.circular(8),
          child: SizedBox(
            height: widget.height,
            child: Stack(
              children: [
                FlutterMap(
                  mapController: _mapController,
                  options: MapOptions(
                    initialCenter: center,
                    initialZoom: _selected == null ? _defaultZoom : 16,
                    onTap: _onMapTap,
                    interactionOptions: const InteractionOptions(
                      flags: InteractiveFlag.all & ~InteractiveFlag.rotate,
                    ),
                  ),
                  children: [
                    TileLayer(
                      urlTemplate:
                          'https://tile.openstreetmap.org/{z}/{x}/{y}.png',
                      userAgentPackageName: 'za.ac.nmu.shuttle.admin',
                      maxZoom: 19,
                    ),
                    if (_selected != null)
                      MarkerLayer(
                        markers: [
                          Marker(
                            point: _selected!,
                            width: 40,
                            height: 40,
                            alignment: Alignment.topCenter,
                            child: const Icon(
                              Icons.location_on,
                              color: Color(0xFFD32F2F),
                              size: 40,
                            ),
                          ),
                        ],
                      ),
                  ],
                ),
                // Attribution required by OpenStreetMap tile usage policy.
                Positioned(
                  right: 0,
                  bottom: 0,
                  child: Container(
                    padding:
                        const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
                    color: Colors.white70,
                    child: Text(
                      '© OpenStreetMap contributors',
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 9,
                        color: Colors.grey.shade700,
                      ),
                    ),
                  ),
                ),
              ],
            ),
          ),
        ),
      ],
    );
  }
}
