import 'package:flutter/material.dart';

/// A minimal date separator with a small label and a thin horizontal line.
/// Used to group bookings by date in the list.
class DateSeparator extends StatelessWidget {
  const DateSeparator({super.key, required this.label});

  /// The date label to display (e.g. "Today", "Tomorrow", "15/08").
  final String label;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(top: 16, bottom: 8),
      child: Row(
        children: [
          Text(
            label,
            style: TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w500,
              color: Colors.grey.shade500,
              letterSpacing: 0.2,
            ),
          ),
          const SizedBox(width: 8),
          Expanded(
            child: Divider(
              height: 1,
              thickness: 0.5,
              color: Colors.grey.shade300,
            ),
          ),
        ],
      ),
    );
  }
}
