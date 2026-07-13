import 'dart:math';

import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:geolocator/geolocator.dart';
import 'package:nmu_shuttle_app/features/student/models/stop_model.dart';
import 'package:nmu_shuttle_app/features/student/providers/stops_provider.dart';

/// Provides the current device position after handling permissions.
/// Returns null if permission is denied or location services are disabled.
final devicePositionProvider = FutureProvider<Position?>((ref) async {
  final serviceEnabled = await Geolocator.isLocationServiceEnabled();
  if (!serviceEnabled) return null;

  var permission = await Geolocator.checkPermission();
  if (permission == LocationPermission.denied) {
    permission = await Geolocator.requestPermission();
    if (permission == LocationPermission.denied) return null;
  }

  if (permission == LocationPermission.deniedForever) return null;

  return Geolocator.getCurrentPosition(
    desiredAccuracy: LocationAccuracy.high,
  );
});

/// Provides all active stops sorted by proximity to the device location.
/// Falls back to unsorted list if device location is unavailable.
final nearbyStopsProvider = FutureProvider<List<MapStopModel>>((ref) async {
  final stops = await ref.watch(mapStopsProvider.future);
  final position = await ref.watch(devicePositionProvider.future);

  if (position == null) return stops;

  final sorted = List<MapStopModel>.from(stops);
  sorted.sort((a, b) {
    final distA = _haversineDistance(
      position.latitude,
      position.longitude,
      a.latitude,
      a.longitude,
    );
    final distB = _haversineDistance(
      position.latitude,
      position.longitude,
      b.latitude,
      b.longitude,
    );
    return distA.compareTo(distB);
  });
  return sorted;
});

/// Calculates distance in km between two GPS points using the Haversine formula.
double _haversineDistance(
  double lat1,
  double lon1,
  double lat2,
  double lon2,
) {
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
