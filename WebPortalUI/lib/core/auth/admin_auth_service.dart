import 'package:jwt_decoder/jwt_decoder.dart';
import 'package:shared_preferences/shared_preferences.dart';

/// Admin authentication service using SharedPreferences for web-safe token storage.
/// Provides methods to save, retrieve, delete JWT tokens and check auth state.
class AdminAuthService {
  static const String _tokenKey = 'admin_jwt_token';

  /// Save the JWT token to SharedPreferences (web localStorage).
  Future<void> saveToken(String token) async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.setString(_tokenKey, token);
  }

  /// Retrieve the stored JWT token. Returns null if not found.
  Future<String?> getToken() async {
    final prefs = await SharedPreferences.getInstance();
    return prefs.getString(_tokenKey);
  }

  /// Delete the stored JWT token (logout / session expiry).
  Future<void> deleteToken() async {
    final prefs = await SharedPreferences.getInstance();
    await prefs.remove(_tokenKey);
  }

  /// Check if the user is authenticated with a valid (non-expired) token.
  Future<bool> isAuthenticated() async {
    final token = await getToken();
    if (token == null || token.isEmpty) return false;

    try {
      return !JwtDecoder.isExpired(token);
    } catch (_) {
      // If token is malformed, treat as unauthenticated
      return false;
    }
  }
}
