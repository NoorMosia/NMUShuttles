import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

/// Full-screen standalone admin login screen with two-step OTP flow.
/// Step 1: Enter email, tap "Login" to request OTP.
/// Step 2: Enter OTP, tap "Confirm OTP" to verify and authenticate.
/// Shows session-expiry message when redirected from an expired token.
class AdminLoginScreen extends ConsumerStatefulWidget {
  const AdminLoginScreen({super.key});

  @override
  ConsumerState<AdminLoginScreen> createState() => _AdminLoginScreenState();
}

class _AdminLoginScreenState extends ConsumerState<AdminLoginScreen> {
  static const Color _navy = Color(0xFF132E51);
  static const Color _gold = Color(0xFFFFCC00);

  final _emailController = TextEditingController();
  final _otpController = TextEditingController();
  final _emailFormKey = GlobalKey<FormState>();
  final _otpFormKey = GlobalKey<FormState>();

  @override
  void dispose() {
    _emailController.dispose();
    _otpController.dispose();
    super.dispose();
  }

  Future<void> _handleRequestOtp() async {
    if (!_emailFormKey.currentState!.validate()) return;

    final email = _emailController.text.trim();
    await ref.read(loginProvider.notifier).requestOtp(email);
  }

  Future<void> _handleVerifyOtp() async {
    if (!_otpFormKey.currentState!.validate()) return;

    final otp = _otpController.text.trim();
    final success = await ref.read(loginProvider.notifier).verifyOtp(otp);

    if (success && mounted) {
      context.go('/overview');
    }
  }

  void _handleBack() {
    _otpController.clear();
    ref.read(loginProvider.notifier).backToEmail();
  }

  @override
  Widget build(BuildContext context) {
    final loginState = ref.watch(loginProvider);
    final isOtpStep = loginState.status == LoginStatus.otpSent ||
        loginState.status == LoginStatus.verifying;
    final isLoading = loginState.status == LoginStatus.loading ||
        loginState.status == LoginStatus.verifying;

    // Check for session expiry query parameter
    final uri = GoRouterState.of(context).uri;
    final isSessionExpired = uri.queryParameters['expired'] == 'true';

    return Scaffold(
      backgroundColor: const Color(0xFFF5F5F5),
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(32),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 420),
            child: Card(
              elevation: 4,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(16),
              ),
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  horizontal: 40,
                  vertical: 48,
                ),
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  crossAxisAlignment: CrossAxisAlignment.stretch,
                  children: [
                    // Logo
                    Center(
                      child: Image.asset(
                        'assets/logos/1.png',
                        height: 72,
                        fit: BoxFit.contain,
                        errorBuilder: (context, error, stackTrace) {
                          return Text(
                            'NMU Shuttles',
                            textAlign: TextAlign.center,
                            style: GoogleFonts.hankenGrotesk(
                              fontSize: 24,
                              fontWeight: FontWeight.bold,
                              color: _navy,
                            ),
                          );
                        },
                      ),
                    ),
                    const SizedBox(height: 16),
                    Text(
                      'Admin Portal',
                      textAlign: TextAlign.center,
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 20,
                        fontWeight: FontWeight.w600,
                        color: _navy,
                      ),
                    ),
                    const SizedBox(height: 32),

                    // Session expired message
                    if (isSessionExpired) ...[
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Colors.orange.shade50,
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: Colors.orange.shade200),
                        ),
                        child: Row(
                          children: [
                            Icon(Icons.info_outline,
                                color: Colors.orange.shade700, size: 20),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                'Your session has expired. Please log in again.',
                                style: GoogleFonts.hankenGrotesk(
                                  fontSize: 13,
                                  color: Colors.orange.shade800,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 20),
                    ],

                    // Error message
                    if (loginState.errorMessage != null) ...[
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Colors.red.shade50,
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: Colors.red.shade200),
                        ),
                        child: Row(
                          children: [
                            Icon(Icons.error_outline,
                                color: Colors.red.shade700, size: 20),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                loginState.errorMessage!,
                                style: GoogleFonts.hankenGrotesk(
                                  fontSize: 13,
                                  color: Colors.red.shade800,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 20),
                    ],

                    // Step 1: Email form
                    if (!isOtpStep) ...[
                      Form(
                        key: _emailFormKey,
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: [
                            TextFormField(
                              controller: _emailController,
                              keyboardType: TextInputType.emailAddress,
                              autofillHints: const [AutofillHints.email],
                              decoration: InputDecoration(
                                labelText: 'Email',
                                prefixIcon: const Icon(Icons.email_outlined),
                                border: OutlineInputBorder(
                                  borderRadius: BorderRadius.circular(8),
                                ),
                              ),
                              validator: (value) {
                                if (value == null || value.trim().isEmpty) {
                                  return 'Please enter your email';
                                }
                                if (!RegExp(r'^[^@]+@[^@]+\.[^@]+$')
                                    .hasMatch(value.trim())) {
                                  return 'Please enter a valid email address';
                                }
                                return null;
                              },
                              onFieldSubmitted: (_) => _handleRequestOtp(),
                            ),
                            const SizedBox(height: 28),

                            // Login button
                            SizedBox(
                              height: 48,
                              child: FilledButton(
                                onPressed: isLoading ? null : _handleRequestOtp,
                                style: FilledButton.styleFrom(
                                  backgroundColor: _gold,
                                  foregroundColor: _navy,
                                  disabledBackgroundColor:
                                      _gold.withValues(alpha: 0.5),
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(8),
                                  ),
                                ),
                                child: isLoading
                                    ? const SizedBox(
                                        height: 20,
                                        width: 20,
                                        child: CircularProgressIndicator(
                                          strokeWidth: 2,
                                        ),
                                      )
                                    : Text(
                                        'Login',
                                        style: GoogleFonts.hankenGrotesk(
                                          fontWeight: FontWeight.w600,
                                          fontSize: 16,
                                        ),
                                      ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],

                    // Step 2: OTP form
                    if (isOtpStep) ...[
                      // Info message showing which email the OTP was sent to
                      Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          color: Colors.green.shade50,
                          borderRadius: BorderRadius.circular(8),
                          border: Border.all(color: Colors.green.shade200),
                        ),
                        child: Row(
                          children: [
                            Icon(Icons.check_circle_outline,
                                color: Colors.green.shade700, size: 20),
                            const SizedBox(width: 8),
                            Expanded(
                              child: Text(
                                'OTP sent to ${loginState.email}',
                                style: GoogleFonts.hankenGrotesk(
                                  fontSize: 13,
                                  color: Colors.green.shade800,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 20),

                      Form(
                        key: _otpFormKey,
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.stretch,
                          children: [
                            TextFormField(
                              controller: _otpController,
                              keyboardType: TextInputType.number,
                              inputFormatters: [
                                FilteringTextInputFormatter.digitsOnly,
                                LengthLimitingTextInputFormatter(5),
                              ],
                              decoration: InputDecoration(
                                labelText: 'Enter OTP',
                                prefixIcon: const Icon(Icons.lock_outlined),
                                border: OutlineInputBorder(
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                hintText: '5-digit code',
                              ),
                              validator: (value) {
                                if (value == null || value.trim().isEmpty) {
                                  return 'Please enter the OTP';
                                }
                                if (value.trim().length != 5) {
                                  return 'OTP must be 5 digits';
                                }
                                return null;
                              },
                              onFieldSubmitted: (_) => _handleVerifyOtp(),
                            ),
                            const SizedBox(height: 28),

                            // Confirm OTP button
                            SizedBox(
                              height: 48,
                              child: FilledButton(
                                onPressed: isLoading ? null : _handleVerifyOtp,
                                style: FilledButton.styleFrom(
                                  backgroundColor: _gold,
                                  foregroundColor: _navy,
                                  disabledBackgroundColor:
                                      _gold.withValues(alpha: 0.5),
                                  shape: RoundedRectangleBorder(
                                    borderRadius: BorderRadius.circular(8),
                                  ),
                                ),
                                child: isLoading
                                    ? const SizedBox(
                                        height: 20,
                                        width: 20,
                                        child: CircularProgressIndicator(
                                          strokeWidth: 2,
                                        ),
                                      )
                                    : Text(
                                        'Confirm OTP',
                                        style: GoogleFonts.hankenGrotesk(
                                          fontWeight: FontWeight.w600,
                                          fontSize: 16,
                                        ),
                                      ),
                              ),
                            ),
                            const SizedBox(height: 12),

                            // Back button
                            TextButton(
                              onPressed: isLoading ? null : _handleBack,
                              child: Text(
                                'Back to email',
                                style: GoogleFonts.hankenGrotesk(
                                  color: _navy,
                                  fontSize: 14,
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ],
                ),
              ),
            ),
          ),
        ),
      ),
    );
  }
}
