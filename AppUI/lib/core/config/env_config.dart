/// Environment configuration for the NMU Shuttle app.
///
/// Set [useNgrok] to `true` when testing against an ngrok tunnel.
/// Leave `false` to hit localhost (Android emulator: 10.0.2.2).
class EnvConfig {
  EnvConfig._();

  /// Toggle this to `true` to route traffic through ngrok.
  static const bool useNgrok = true;

  static const String _localBaseUrl = 'http://10.0.2.2:5050/api/v1';
  static const String _ngrokBaseUrl =
      'https://juicy-dubiously-pushpin.ngrok-free.dev/api/v1';

  /// Resolved base URL based on [useNgrok].
  static const String baseUrl = useNgrok ? _ngrokBaseUrl : _localBaseUrl;
}
