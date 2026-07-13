/// Model representing a shuttle stop with GPS coordinates for the Maps screen.
class MapStopModel {
  const MapStopModel({
    required this.stopId,
    required this.stopName,
    required this.fullAddress,
    required this.latitude,
    required this.longitude,
    required this.isActive,
  });

  final int stopId;
  final String stopName;
  final String fullAddress;
  final double latitude;
  final double longitude;
  final bool isActive;

  /// Parses GPS coordinates from "lat,lon" string format.
  /// Returns null if the format is invalid.
  static ({double lat, double lon})? parseGpsCoordinates(String? gpsString) {
    if (gpsString == null || gpsString.isEmpty) return null;
    final parts = gpsString.split(',');
    if (parts.length != 2) return null;
    final lat = double.tryParse(parts[0].trim());
    final lon = double.tryParse(parts[1].trim());
    if (lat == null || lon == null) return null;
    return (lat: lat, lon: lon);
  }

  factory MapStopModel.fromJson(Map<String, dynamic> json) {
    final gpsString = json['gpsCoordinates'] as String?;
    final coords = parseGpsCoordinates(gpsString);

    return MapStopModel(
      stopId: json['stopId'] as int,
      stopName: json['stopName'] as String? ?? '',
      fullAddress: json['fullAddress'] as String? ?? '',
      latitude: coords?.lat ?? 0.0,
      longitude: coords?.lon ?? 0.0,
      isActive: json['isActive'] as bool? ?? true,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'stopId': stopId,
      'stopName': stopName,
      'fullAddress': fullAddress,
      'gpsCoordinates': '$latitude,$longitude',
      'isActive': isActive,
    };
  }

  /// Whether this stop has valid GPS coordinates (non-zero).
  bool get hasValidCoordinates => latitude != 0.0 || longitude != 0.0;
}
