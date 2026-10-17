/// Environment configuration for the NMU Shuttle app.
///
/// Switch [environment] to choose which backend the app talks to:
///   • [Environment.production] — the live Fly.io backend (default)
///   • [Environment.ngrok]      — an ngrok tunnel to your local machine
///   • [Environment.local]      — localhost (Android emulator: 10.0.2.2)
class EnvConfig {
  EnvConfig._();

  /// The active environment. Change this to switch backends.
  static const Environment environment = Environment.production;

  static const String _localBaseUrl = 'http://10.0.2.2:5050/api/v1';
  static const String _ngrokBaseUrl =
      'https://juicy-dubiously-pushpin.ngrok-free.dev/api/v1';
  static const String _productionBaseUrl =
      'https://nmu-shuttle-api.fly.dev/api/v1';

  /// Resolved base URL based on [environment].
  /// Uses const ternaries (not a switch expression) so it stays a compile-time
  /// constant, which `const String` requires.
  static const String baseUrl = environment == Environment.production
      ? _productionBaseUrl
      : (environment == Environment.ngrok ? _ngrokBaseUrl : _localBaseUrl);

  /// Whether the active environment is the ngrok tunnel (adds a skip-warning
  /// header so the browser interstitial doesn't block API calls).
  static const bool useNgrok = environment == Environment.ngrok;
}

/// The set of backends the app can target.
enum Environment { local, ngrok, production }
