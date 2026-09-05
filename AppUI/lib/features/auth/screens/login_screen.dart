import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_svg/flutter_svg.dart';
import 'package:go_router/go_router.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:pin_code_fields/pin_code_fields.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';

/// Minimalist login screen following the UI mockups:
/// Step 1 — Tab role selector (Student/Marshal) + underline input → Request OTP
/// Step 2 — Underline OTP boxes → Verify OTP → Navigate to role home
class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  static const Color _navy = Color(0xFF132E51);
  static const Color _gold = Color(0xFFFFCC00);
  static const int _cooldownSeconds = 60;

  final _identifierController = TextEditingController();
  final _otpController = TextEditingController();

  Timer? _cooldownTimer;
  int _remainingSeconds = 0;

  bool get _isCooldownActive => _remainingSeconds > 0;

  @override
  void initState() {
    super.initState();
    // Always start fresh on the identifier step (e.g. after logout)
    WidgetsBinding.instance.addPostFrameCallback((_) {
      ref.read(authStateProvider.notifier).resetToIdentifier();
    });
  }

  @override
  void dispose() {
    _identifierController.dispose();
    _otpController.dispose();
    _cooldownTimer?.cancel();
    super.dispose();
  }

  void _startCooldown() {
    _cooldownTimer?.cancel();
    setState(() => _remainingSeconds = _cooldownSeconds);
    _cooldownTimer = Timer.periodic(const Duration(seconds: 1), (timer) {
      setState(() {
        _remainingSeconds--;
        if (_remainingSeconds <= 0) {
          timer.cancel();
        }
      });
    });
  }

  void _onRoleChanged(AuthRole role) {
    ref.read(authStateProvider.notifier).setRole(role);
    _identifierController.clear();
    _otpController.clear();
  }

  Future<void> _onRequestOtp() async {
    final notifier = ref.read(authStateProvider.notifier);
    final state = ref.read(authStateProvider);
    final identifier = _identifierController.text.trim();
    final isStudent = state.role == AuthRole.student;

    if (identifier.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(isStudent
              ? 'Please enter your student number'
              : 'Please enter your staff number'),
        ),
      );
      return;
    }

    // Validate the exact expected format before hitting the network.
    final validFormat = isStudent
        ? RegExp(r'^s\d{9}$').hasMatch(identifier)
        : RegExp(r'^\d{7}$').hasMatch(identifier);
    if (!validFormat) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(isStudent
              ? 'Enter a valid student number (e.g. s312847563)'
              : 'Enter a valid 7-digit staff number'),
        ),
      );
      return;
    }

    if (_isCooldownActive) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text('Please wait $_remainingSeconds seconds before requesting again'),
        ),
      );
      return;
    }

    await notifier.requestOtp(state.role, identifier);

    // Start cooldown only if OTP was sent successfully
    final newState = ref.read(authStateProvider);
    if (newState.currentStep == AuthStep.otp) {
      _startCooldown();
    }
  }

  Future<void> _onVerifyOtp() async {
    final notifier = ref.read(authStateProvider.notifier);
    final state = ref.read(authStateProvider);
    final identifier = _identifierController.text.trim();
    final otp = _otpController.text.trim();

    if (otp.length != 5) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please enter the 5-digit OTP')),
      );
      return;
    }

    await notifier.verifyOtp(state.role, identifier, otp);
  }

  void _onBackToIdentifier() {
    _otpController.clear();
    ref.read(authStateProvider.notifier).resetToIdentifier();
  }

  Future<void> _onResendOtp() async {
    if (_isCooldownActive) return;

    final notifier = ref.read(authStateProvider.notifier);
    final state = ref.read(authStateProvider);
    final identifier = _identifierController.text.trim();

    if (identifier.isEmpty) return;
    await notifier.requestOtp(state.role, identifier);

    _startCooldown();

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('OTP resent')),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authStateProvider);

    // Listen for errors and navigation
    ref.listen<AuthState>(authStateProvider, (prev, next) {
      if (next.error != null && next.error != prev?.error) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(next.error!),
            backgroundColor: Colors.red.shade700,
          ),
        );
      }

      if (next.authenticatedRole != null &&
          next.authenticatedRole != prev?.authenticatedRole) {
        if (next.authenticatedRole == 'Marshal') {
          context.go('/marshal/home');
        } else {
          context.go('/student/home');
        }
      }
    });

    return Scaffold(
      backgroundColor: Colors.white,
      resizeToAvoidBottomInset: false,
      body: GestureDetector(
        // Dismiss the keyboard when tapping outside any input field.
        onTap: () => FocusScope.of(context).unfocus(),
        behavior: HitTestBehavior.opaque,
        child: SafeArea(
          child: Column(
            children: [
            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                child: Align(
                  alignment: Alignment.topCenter,
                  child: ConstrainedBox(
                    constraints: const BoxConstraints(maxWidth: 400),
                    child: Column(
                      children: [
                        const SizedBox(height: 48),

                  // ─── Logo ────────────────────────────────────
                  Padding(
                    padding: const EdgeInsets.only(top: 10),
                    child: SizedBox(
                      width: MediaQuery.of(context).size.width * 0.32,
                      child: AspectRatio(
                        aspectRatio: 1,
                        child: SvgPicture.asset(
                          'assets/logo.svg',
                          fit: BoxFit.contain,
                          colorFilter: const ColorFilter.mode(
                            AppTheme.navy,
                            BlendMode.srcIn,
                          ),
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(height: 32),

                  // ─── Step 1: Identifier ──────────────────────
                  if (authState.currentStep == AuthStep.identifier) ...[
                    // Tab role selector
                    const SizedBox(height: 48),
                    _buildRoleTabs(authState.role),
                    const SizedBox(height: 40),

                    // Input field
                    _buildIdentifierField(authState),
                    const SizedBox(height: 40),

                    // Login button
                    _buildActionButton(
                      label: 'LOGIN',
                      isLoading: authState.isLoading,
                      onPressed: _onRequestOtp,
                    ),
                  ],

                  // ─── Step 2: OTP ─────────────────────────────
                  if (authState.currentStep == AuthStep.otp) ...[
                    const SizedBox(height: 48),
                    Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 16),
                      child: Text.rich(
                        TextSpan(
                          text: 'A verification code has been sent to ',
                          style: GoogleFonts.hankenGrotesk(
                            fontSize: 16,
                            color: _navy.withValues(alpha: 0.7),
                            height: 24 / 16,
                          ),
                          children: [
                            TextSpan(
                              text: 'your university email',
                              style: GoogleFonts.hankenGrotesk(
                                fontSize: 16,
                                fontWeight: FontWeight.w700,
                                color: _navy,
                              ),
                            ),
                          ],
                        ),
                        textAlign: TextAlign.center,
                      ),
                    ),
                    const SizedBox(height: 40),

                    // OTP underline inputs
                    _buildOtpField(),
                    const SizedBox(height: 28),

                    // CONFIRM button
                    _buildActionButton(
                      label: 'CONFIRM',
                      isLoading: authState.isLoading,
                      onPressed: _onVerifyOtp,
                    ),
                    const SizedBox(height: 32),

                    // Back to login
                    GestureDetector(
                      onTap: authState.isLoading ? null : _onBackToIdentifier,
                      child: Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(
                            Icons.arrow_back,
                            size: 16,
                            color: _navy.withValues(alpha: 0.7),
                          ),
                          const SizedBox(width: 4),
                          Text(
                            'Back to login',
                            style: GoogleFonts.hankenGrotesk(
                              fontSize: 14,
                              fontWeight: FontWeight.w600,
                              color: _navy.withValues(alpha: 0.7),
                              letterSpacing: 0.01 * 14,
                            ),
                          ),
                        ],
                      ),
                    ),
                    const SizedBox(height: 16),

                    // Resend section
                    Text(
                      "Didn't receive the code?",
                      style: GoogleFonts.hankenGrotesk(
                        fontSize: 12,
                        fontWeight: FontWeight.w500,
                        color: _navy.withValues(alpha: 0.7),
                        letterSpacing: 0.02 * 12,
                      ),
                    ),
                    const SizedBox(height: 8),
                    GestureDetector(
                      onTap: (authState.isLoading || _isCooldownActive)
                          ? null
                          : _onResendOtp,
                      child: Text(
                        _isCooldownActive
                            ? 'Resend OTP (${_remainingSeconds}s)'
                            : 'Resend OTP',
                        style: GoogleFonts.hankenGrotesk(
                          fontSize: 14,
                          fontWeight: FontWeight.w700,
                          color: _isCooldownActive
                              ? _navy.withValues(alpha: 0.4)
                              : _navy,
                          letterSpacing: 0.01 * 14,
                        ),
                      ),
                    ),
                  ],

                        const SizedBox(height: 48),
                      ],
                    ),
                  ),
                ),
              ),
            ),
          ],
        ),
        ),
      ),
    );
  }

  // ─── Tab-style role selector ──────────────────────────────────

  Widget _buildRoleTabs(AuthRole selectedRole) {
    return LayoutBuilder(
      builder: (context, constraints) {
        final tabWidth = constraints.maxWidth / 2;
        final isStudent = selectedRole == AuthRole.student;

        return Column(
          children: [
            Row(
              children: [
                Expanded(
                  child: GestureDetector(
                    onTap: () => _onRoleChanged(AuthRole.student),
                    behavior: HitTestBehavior.opaque,
                    child: Padding(
                      padding: const EdgeInsets.symmetric(vertical: 12),
                      child: Center(
                        child: AnimatedDefaultTextStyle(
                          duration: const Duration(milliseconds: 250),
                          style: GoogleFonts.hankenGrotesk(
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                            letterSpacing: 0.01 * 14,
                            color: isStudent
                                ? _navy
                                : _navy.withValues(alpha: 0.5),
                          ),
                          child: const Text('Student'),
                        ),
                      ),
                    ),
                  ),
                ),
                Expanded(
                  child: GestureDetector(
                    onTap: () => _onRoleChanged(AuthRole.marshal),
                    behavior: HitTestBehavior.opaque,
                    child: Padding(
                      padding: const EdgeInsets.symmetric(vertical: 12),
                      child: Center(
                        child: AnimatedDefaultTextStyle(
                          duration: const Duration(milliseconds: 250),
                          style: GoogleFonts.hankenGrotesk(
                            fontSize: 14,
                            fontWeight: FontWeight.w600,
                            letterSpacing: 0.01 * 14,
                            color: !isStudent
                                ? _navy
                                : _navy.withValues(alpha: 0.5),
                          ),
                          child: const Text('Marshal'),
                        ),
                      ),
                    ),
                  ),
                ),
              ],
            ),
            // Animated underline
            Container(
              height: 4,
              color: _navy.withValues(alpha: 0.1),
              child: Align(
                alignment: Alignment.centerLeft,
                child: AnimatedContainer(
                  duration: const Duration(milliseconds: 250),
                  curve: Curves.easeInOut,
                  width: tabWidth,
                  margin: EdgeInsets.only(left: isStudent ? 0 : tabWidth),
                  height: 4,
                  color: _gold,
                ),
              ),
            ),
          ],
        );
      },
    );
  }

  // ─── Underline input field ─────────────────────────────────────

  Widget _buildIdentifierField(AuthState authState) {
    final isStudent = authState.role == AuthRole.student;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        TextField(
          controller: _identifierController,
          decoration: InputDecoration(
            hintText: isStudent
                ? 'Enter your student number'
                : 'Enter your staff number',
            hintStyle: GoogleFonts.hankenGrotesk(
              fontSize: 16,
              color: _navy.withValues(alpha: 0.4),
            ),
            border: UnderlineInputBorder(
              borderSide: BorderSide(color: _navy.withValues(alpha: 0.2)),
            ),
            enabledBorder: UnderlineInputBorder(
              borderSide: BorderSide(color: _navy.withValues(alpha: 0.2)),
            ),
            focusedBorder: const UnderlineInputBorder(
              borderSide: BorderSide(color: _gold, width: 2),
            ),
            contentPadding:
                const EdgeInsets.symmetric(horizontal: 0, vertical: 12),
          ),
          style: GoogleFonts.hankenGrotesk(
            fontSize: 16,
            color: _navy,
          ),
          keyboardType:
              isStudent ? TextInputType.text : TextInputType.number,
          textInputAction: TextInputAction.done,
          // Constrain input to the only possible formats:
          //  • Student number: "s" + 9 digits (10 chars total)
          //  • Marshal staff number: 7 digits
          inputFormatters: isStudent
              ? [
                  _StudentNumberFormatter(),
                  LengthLimitingTextInputFormatter(10),
                ]
              : [
                  FilteringTextInputFormatter.digitsOnly,
                  LengthLimitingTextInputFormatter(7),
                ],
          onSubmitted: (_) => _onRequestOtp(),
        ),
        const SizedBox(height: 8),
        Text(
          isStudent ? 'Student Number' : 'Staff Number',
          style: GoogleFonts.hankenGrotesk(
            fontSize: 14,
            fontWeight: FontWeight.w600,
            color: _navy,
          ),
        ),
      ],
    );
  }

  // ─── OTP underline pin fields ─────────────────────────────────

  Widget _buildOtpField() {
    return PinCodeTextField(
      appContext: context,
      length: 5,
      controller: _otpController,
      keyboardType: TextInputType.number,
      inputFormatters: [FilteringTextInputFormatter.digitsOnly],
      animationType: AnimationType.none,
      enableActiveFill: false,
      autoFocus: true,
      textStyle: GoogleFonts.hankenGrotesk(
        fontSize: 24,
        fontWeight: FontWeight.w600,
        color: _navy,
      ),
      pinTheme: PinTheme(
        shape: PinCodeFieldShape.underline,
        fieldHeight: 56,
        fieldWidth: 48,
        activeColor: _gold,
        inactiveColor: _navy.withValues(alpha: 0.15),
        selectedColor: _gold,
        borderWidth: 2,
      ),
      onCompleted: (_) => _onVerifyOtp(),
      onChanged: (_) {},
    );
  }

  // ─── Full-width gold action button ────────────────────────────

  Widget _buildActionButton({
    required String label,
    required bool isLoading,
    required VoidCallback onPressed,
  }) {
    return SizedBox(
      width: double.infinity,
      height: 56,
      child: ElevatedButton(
        onPressed: isLoading ? null : onPressed,
        style: ElevatedButton.styleFrom(
          backgroundColor: _gold,
          foregroundColor: _navy,
          disabledBackgroundColor: _gold.withValues(alpha: 0.5),
          elevation: 0,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(5),
          ),
        ),
        child: isLoading
            ? const SizedBox(
                height: 20,
                width: 20,
                child: CircularProgressIndicator(
                  strokeWidth: 2.5,
                  color: _navy,
                ),
              )
            : Text(
                label,
                style: GoogleFonts.hankenGrotesk(
                  fontSize: 14,
                  fontWeight: FontWeight.w700,
                  letterSpacing: 0.05 * 14,
                ),
              ),
      ),
    );
  }
}

/// Formats a student-number entry: forces a leading lowercase "s" followed by
/// digits only. Anything else the user types is rejected as they type, so the
/// field can only ever hold values shaped like "s" + digits.
class _StudentNumberFormatter extends TextInputFormatter {
  static final RegExp _valid = RegExp(r'^s?\d*$');

  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    // Normalise a leading "S" to lowercase so "S3128..." becomes "s3128...".
    var text = newValue.text;
    if (text.isNotEmpty && text[0] == 'S') {
      text = 's${text.substring(1)}';
    }
    if (!_valid.hasMatch(text)) {
      // Reject the change; keep the previous valid value.
      return oldValue;
    }
    if (text == newValue.text) return newValue;
    return TextEditingValue(
      text: text,
      selection: TextSelection.collapsed(offset: text.length),
    );
  }
}
