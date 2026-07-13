import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:nmu_shuttle_app/features/auth/providers/auth_providers.dart';

/// Login screen with two-step auth flow:
/// Step 1 — Role selector (Student/Marshal) + identifier input → Request OTP
/// Step 2 — OTP input (5 digits) → Verify OTP → Navigate to role home
class LoginScreen extends ConsumerStatefulWidget {
  const LoginScreen({super.key});

  @override
  ConsumerState<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends ConsumerState<LoginScreen> {
  final _identifierController = TextEditingController();
  final _otpController = TextEditingController();
  final _identifierFocusNode = FocusNode();
  final _otpFocusNode = FocusNode();

  @override
  void dispose() {
    _identifierController.dispose();
    _otpController.dispose();
    _identifierFocusNode.dispose();
    _otpFocusNode.dispose();
    super.dispose();
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

    if (identifier.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Please enter your number')),
      );
      return;
    }

    await notifier.requestOtp(state.role, identifier);

    // After requesting, if moved to OTP step, focus the OTP field
    final newState = ref.read(authStateProvider);
    if (newState.currentStep == AuthStep.otp && mounted) {
      _otpFocusNode.requestFocus();
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

  @override
  Widget build(BuildContext context) {
    final authState = ref.watch(authStateProvider);
    final theme = Theme.of(context);
    final colorScheme = theme.colorScheme;

    // Listen for errors → show SnackBar
    ref.listen<AuthState>(authStateProvider, (prev, next) {
      if (next.error != null && next.error != prev?.error) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(next.error!),
            backgroundColor: colorScheme.error,
          ),
        );
      }

      // Navigate on successful auth
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
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.symmetric(horizontal: 32, vertical: 24),
            child: Column(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                // ─── Logo ──────────────────────────────────────
                Image.asset(
                  'assets/logos/1.png',
                  height: 120,
                  fit: BoxFit.contain,
                ),
                const SizedBox(height: 40),

                // ─── Title ─────────────────────────────────────
                Text(
                  'Welcome',
                  style: theme.textTheme.headlineMedium?.copyWith(
                    fontWeight: FontWeight.bold,
                    color: colorScheme.secondary,
                  ),
                ),
                const SizedBox(height: 8),
                Text(
                  'Sign in to continue',
                  style: theme.textTheme.bodyLarge?.copyWith(
                    color: colorScheme.onSurface.withValues(alpha: 0.6),
                  ),
                ),
                const SizedBox(height: 32),

                // ─── Role Selector ─────────────────────────────
                SegmentedButton<AuthRole>(
                  segments: const [
                    ButtonSegment<AuthRole>(
                      value: AuthRole.student,
                      label: Text('Student'),
                      icon: Icon(Icons.school_outlined),
                    ),
                    ButtonSegment<AuthRole>(
                      value: AuthRole.marshal,
                      label: Text('Marshal'),
                      icon: Icon(Icons.security_outlined),
                    ),
                  ],
                  selected: {authState.role},
                  onSelectionChanged: authState.currentStep == AuthStep.otp
                      ? null
                      : (Set<AuthRole> selected) {
                          _onRoleChanged(selected.first);
                        },
                  style: ButtonStyle(
                    backgroundColor:
                        WidgetStateProperty.resolveWith((states) {
                      if (states.contains(WidgetState.selected)) {
                        return colorScheme.primary;
                      }
                      return null;
                    }),
                    foregroundColor:
                        WidgetStateProperty.resolveWith((states) {
                      if (states.contains(WidgetState.selected)) {
                        return colorScheme.onPrimary;
                      }
                      return colorScheme.onSurface;
                    }),
                  ),
                ),
                const SizedBox(height: 24),

                // ─── Step 1: Identifier Input ──────────────────
                TextField(
                  controller: _identifierController,
                  focusNode: _identifierFocusNode,
                  enabled: authState.currentStep == AuthStep.identifier,
                  decoration: InputDecoration(
                    labelText: authState.role == AuthRole.student
                        ? 'Student Number'
                        : 'Staff Number',
                    hintText: authState.role == AuthRole.student
                        ? 's312345678'
                        : '4827391',
                    prefixIcon: Icon(
                      authState.role == AuthRole.student
                          ? Icons.badge_outlined
                          : Icons.work_outline,
                    ),
                    border: const OutlineInputBorder(),
                  ),
                  keyboardType: authState.role == AuthRole.student
                      ? TextInputType.text
                      : TextInputType.number,
                  textInputAction: TextInputAction.done,
                  onSubmitted: (_) {
                    if (authState.currentStep == AuthStep.identifier) {
                      _onRequestOtp();
                    }
                  },
                ),
                const SizedBox(height: 16),

                // ─── Step 2: OTP Input (visible after request) ─
                if (authState.currentStep == AuthStep.otp) ...[
                  TextField(
                    controller: _otpController,
                    focusNode: _otpFocusNode,
                    decoration: const InputDecoration(
                      labelText: 'OTP Code',
                      hintText: '12345',
                      prefixIcon: Icon(Icons.lock_outline),
                      border: OutlineInputBorder(),
                    ),
                    keyboardType: TextInputType.number,
                    maxLength: 5,
                    inputFormatters: [
                      FilteringTextInputFormatter.digitsOnly,
                      LengthLimitingTextInputFormatter(5),
                    ],
                    textInputAction: TextInputAction.done,
                    onSubmitted: (_) => _onVerifyOtp(),
                  ),
                  const SizedBox(height: 8),
                ],

                const SizedBox(height: 8),

                // ─── Action Buttons ────────────────────────────
                SizedBox(
                  width: double.infinity,
                  height: 48,
                  child: authState.currentStep == AuthStep.identifier
                      ? FilledButton(
                          onPressed:
                              authState.isLoading ? null : _onRequestOtp,
                          child: authState.isLoading
                              ? const SizedBox(
                                  height: 20,
                                  width: 20,
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2,
                                    color: Colors.white,
                                  ),
                                )
                              : const Text('Request OTP'),
                        )
                      : FilledButton(
                          onPressed:
                              authState.isLoading ? null : _onVerifyOtp,
                          child: authState.isLoading
                              ? const SizedBox(
                                  height: 20,
                                  width: 20,
                                  child: CircularProgressIndicator(
                                    strokeWidth: 2,
                                    color: Colors.white,
                                  ),
                                )
                              : const Text('Verify OTP'),
                        ),
                ),

                // ─── Back / Change Number link ─────────────────
                if (authState.currentStep == AuthStep.otp) ...[
                  const SizedBox(height: 12),
                  TextButton(
                    onPressed:
                        authState.isLoading ? null : _onBackToIdentifier,
                    child: const Text('← Change number'),
                  ),
                ],
              ],
            ),
          ),
        ),
      ),
    );
  }
}
