/// Environment configuration for the NMU Shuttle admin portal.
///
/// Set [useNgrok] to `true` when testing against an ngrok tunnel.
/// Leave `false` to hit localhost directly.
class EnvConfig {
  EnvConfig._();

  /// Toggle this to `true` to route traffic through ngrok.
  static const bool useNgrok = false;

  static const String _localBaseUrl = 'http://localhost:5050/api/v1';
  static const String _ngrokBaseUrl =
      'https://juicy-dubiously-pushpin.ngrok-free.dev/api/v1';

  /// Resolved base URL based on [useNgrok].
  static const String baseUrl = useNgrok ? _ngrokBaseUrl : _localBaseUrl;
}
