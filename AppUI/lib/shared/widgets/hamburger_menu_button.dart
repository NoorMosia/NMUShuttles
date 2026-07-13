import 'package:flutter/material.dart';

/// Floating hamburger menu button (top-left) with white square background
/// and small border radius. Tapping opens the Scaffold's Drawer.
class HamburgerMenuButton extends StatelessWidget {
  const HamburgerMenuButton({super.key});

  @override
  Widget build(BuildContext context) {
    return Positioned(
      top: MediaQuery.of(context).padding.top + 12,
      left: 16,
      child: Material(
        color: Colors.white,
        elevation: 2,
        borderRadius: BorderRadius.circular(8),
        child: InkWell(
          borderRadius: BorderRadius.circular(8),
          onTap: () => Scaffold.of(context).openDrawer(),
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
