import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:jwt_decoder/jwt_decoder.dart';

/// Service for managing JWT token persistence and retrieval.
/// Uses FlutterSecureStorage for secure token storage on device.
class AuthService {
  AuthService({FlutterSecureStorage? storage})
      : _storage = storage ?? const FlutterSecureStorage();

  final FlutterSecureStorage _storage;

  static const String _tokenKey = 'jwt_token';

  /// Saves the JWT token securely on device.
  Future<void> saveToken(String token) async {
    await _storage.write(key: _tokenKey, value: token);
  }

  /// Retrieves the stored JWT token, or null if not present.
  Future<String?> getToken() async {
    return await _storage.read(key: _tokenKey);
  }

  /// Deletes the stored JWT token (used for logout).
  Future<void> deleteToken() async {
    await _storage.delete(key: _tokenKey);
  }

  /// Returns true if a valid (non-expired) JWT is stored.
  Future<bool> isAuthenticated() async {
    final token = await getToken();
    if (token == null || token.isEmpty) return false;

    try {
      return !JwtDecoder.isExpired(token);
    } catch (_) {
      return false;
    }
  }

  /// Decodes the JWT and extracts the user role claim.
  /// Returns null if no token is stored or decoding fails.
  Future<String?> getUserRole() async {
    final token = await getToken();
    if (token == null || token.isEmpty) return null;

    try {
      final decodedToken = JwtDecoder.decode(token);
      // Standard claim key for role in ASP.NET Core JWTs
      return decodedToken['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] as String?
          ?? decodedToken['role'] as String?;
    } catch (_) {
      return null;
    }
  }

  /// Decodes the JWT and extracts the user ID claim.
  /// Returns null if no token is stored or decoding fails.
  Future<String?> getUserId() async {
    final token = await getToken();
    if (token == null || token.isEmpty) return null;

    try {
      final decodedToken = JwtDecoder.decode(token);
      return decodedToken['sub'] as String?
          ?? decodedToken['nameid'] as String?;
    } catch (_) {
      return null;
    }
  }
}
