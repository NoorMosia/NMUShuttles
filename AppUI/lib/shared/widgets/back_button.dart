import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';

/// Floating back button matching the hamburger menu style —
/// white square background with small border radius, positioned top-left.
class FloatingBackButton extends StatelessWidget {
  const FloatingBackButton({super.key});

  @override
  Widget build(BuildContext context) {
    return Positioned(
      top: MediaQuery.of(context).padding.top + 12,
      left: 16,
      child: Material(
        color: Colors.white,
        borderRadius: BorderRadius.circular(8),
        elevation: 0,
        child: InkWell(
          borderRadius: BorderRadius.circular(8),
          onTap: () {
            if (Navigator.of(context).canPop()) {
              Navigator.of(context).pop();
            } else {
              context.go('/student/home');
            }
          },
          child: const SizedBox(
            width: 44,
            height: 44,
            child: Icon(
              Icons.arrow_back,
              color: AppTheme.navy,
              size: 22,
            ),
          ),
        ),
      ),
    );
  }
}
