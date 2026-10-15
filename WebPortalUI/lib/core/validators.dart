import 'package:flutter/services.dart';

/// Shared form-field validators for the admin portal.
///
/// Each method returns `null` when the value is valid, or an error message
/// string when it is not — matching the contract expected by
/// `TextFormField.validator`.
class Validators {
  Validators._();

  // ── Shared constraints ────────────────────────────────────────────────────

  /// Maximum seating capacity allowed for any single shuttle.
  static const int maxShuttleCapacity = 80;

  /// Maximum length of a licence plate. South African plates are at most
  /// 9 characters; we allow a little slack for spacing/formatting.
  static const int maxLicensePlateLength = 10;

  /// Minimum sensible licence plate length.
  static const int minLicensePlateLength = 2;

  /// Reasonable upper bound for a single email address.
  static const int maxEmailLength = 254;

  // RFC-5322-inspired practical email pattern. Not a full RFC parser, but
  // rejects the common invalid shapes (missing local part, missing domain,
  // missing TLD, spaces, double dots).
  static final RegExp _emailRegex = RegExp(
    "^[a-zA-Z0-9.!#\$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9]"
    r'(?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?'
    r'(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)+$',
  );

  // ── Generic validators ─────────────────────────────────────────────────────

  /// Requires a non-empty (after trim) value.
  static String? required(String? value, {String fieldName = 'This field'}) {
    if (value == null || value.trim().isEmpty) {
      return '$fieldName is required';
    }
    return null;
  }

  /// Validates an email address.
  ///
  /// When [optional] is true an empty value is accepted (returns null),
  /// otherwise an empty value is reported as required.
  static String? email(
    String? value, {
    bool optional = false,
    String fieldName = 'Email',
  }) {
    final v = value?.trim() ?? '';
    if (v.isEmpty) {
      return optional ? null : '$fieldName is required';
    }
    if (v.length > maxEmailLength) {
      return '$fieldName is too long';
    }
    if (!_emailRegex.hasMatch(v)) {
      return 'Enter a valid email address';
    }
    return null;
  }

  /// Validates a shuttle licence plate: required, within a sensible length,
  /// and limited to letters, digits, spaces and hyphens.
  static String? licensePlate(String? value) {
    final v = value?.trim() ?? '';
    if (v.isEmpty) {
      return 'License plate is required';
    }
    if (v.length < minLicensePlateLength) {
      return 'License plate is too short';
    }
    if (v.length > maxLicensePlateLength) {
      return 'License plate must be at most $maxLicensePlateLength characters';
    }
    if (!RegExp(r'^[A-Za-z0-9 -]+$').hasMatch(v)) {
      return 'License plate contains invalid characters';
    }
    return null;
  }

  /// Validates shuttle capacity: required, a positive integer, and at most
  /// [maxShuttleCapacity].
  static String? shuttleCapacity(String? value) {
    final v = value?.trim() ?? '';
    if (v.isEmpty) {
      return 'Required';
    }
    final n = int.tryParse(v);
    if (n == null) {
      return 'Invalid';
    }
    if (n <= 0) {
      return '> 0';
    }
    if (n > maxShuttleCapacity) {
      return '≤ $maxShuttleCapacity';
    }
    return null;
  }

  /// Validates a required positive integer (e.g. an entity ID reference).
  static String? positiveInt(String? value, {String fieldName = 'Value'}) {
    final v = value?.trim() ?? '';
    if (v.isEmpty) {
      return '$fieldName is required';
    }
    final n = int.tryParse(v);
    if (n == null || n <= 0) {
      return '$fieldName must be a positive number';
    }
    return null;
  }

  /// Validates a South African 4-digit postal code.
  static String? postalCode(String? value) {
    final v = value?.trim() ?? '';
    if (v.isEmpty) {
      return 'Postal code is required';
    }
    if (!RegExp(r'^\d{4}$').hasMatch(v)) {
      return 'Postal code must be 4 digits';
    }
    return null;
  }
}

/// Input formatter that caps the number of characters entered.
class MaxLengthFormatter extends TextInputFormatter {
  const MaxLengthFormatter(this.maxLength);

  final int maxLength;

  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    if (newValue.text.length <= maxLength) {
      return newValue;
    }
    return oldValue;
  }
}

/// Input formatter that keeps an integer value within [min]..[max].
class IntRangeFormatter extends TextInputFormatter {
  const IntRangeFormatter({required this.min, required this.max});

  final int min;
  final int max;

  @override
  TextEditingValue formatEditUpdate(
    TextEditingValue oldValue,
    TextEditingValue newValue,
  ) {
    if (newValue.text.isEmpty) return newValue;
    final n = int.tryParse(newValue.text);
    if (n == null) return oldValue;
    if (n < min || n > max) return oldValue;
    return newValue;
  }
}
