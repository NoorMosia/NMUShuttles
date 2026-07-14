import 'package:flutter/material.dart';
import 'package:nmu_shuttle_app/shared/widgets/student_shell.dart';

/// Floating hamburger menu button (top-left) with white square background
/// and small border radius. Tapping opens the shell's Drawer (full height, over bottom nav).
class HamburgerMenuButton extends StatelessWidget {
  const HamburgerMenuButton({super.key});

  @override
  Widget build(BuildContext context) {
    return Positioned(
      top: MediaQuery.of(context).padding.top + 12,
      left: 16,
      child: Material(
        color: Colors.white,
        elevation: 0,
        borderRadius: BorderRadius.circular(8),
        child: InkWell(
          borderRadius: BorderRadius.circular(8),
          onTap: () {
            // Try the shell scaffold first (covers full height including bottom nav)
            if (studentShellScaffoldKey.currentState?.hasDrawer ?? false) {
              studentShellScaffoldKey.currentState!.openDrawer();
            } else {
              // Fallback to nearest scaffold (for screens outside the shell)
              Scaffold.of(context).openDrawer();
            }
          },
          child: const Padding(
            padding: EdgeInsets.all(10),
            child: Icon(
              Icons.menu,
              color: Color(0xFF132E51),
              size: 24,
            ),
          ),
        ),
      ),
    );
  }
}
