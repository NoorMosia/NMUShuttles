import 'package:flutter/material.dart';
import 'package:nmu_shuttle_app/core/api/api_exception.dart';
import 'package:nmu_shuttle_app/core/theme/app_theme.dart';

/// Shows a themed error [SnackBar] with the given [message].
///
/// For network errors, shows an amber warning color.
/// For conflict/duplicate errors, shows red.
/// For other errors, shows red.
void showErrorSnackBar(BuildContext context, String message) {
  ScaffoldMessenger.of(context).hideCurrentSnackBar();
  ScaffoldMessenger.of(context).showSnackBar(
    SnackBar(
      content: Row(
        children: [
          const Icon(Icons.error_outline, color: Colors.white, size: 20),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              message,
              style: const TextStyle(color: Colors.white),
            ),
          ),
        ],
      ),
      backgroundColor: Colors.red.shade700,
      behavior: SnackBarBehavior.floating,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
      margin: const EdgeInsets.all(16),
      duration: const Duration(seconds: 4),
    ),
  );
}

/// Shows a themed warning [SnackBar] (amber) for non-critical issues
/// like network timeouts.
void showWarningSnackBar(BuildContext context, String message) {
  ScaffoldMessenger.of(context).hideCurrentSnackBar();
  ScaffoldMessenger.of(context).showSnackBar(
    SnackBar(
      content: Row(
        children: [
          const Icon(Icons.wifi_off_outlined, color: Colors.white, size: 20),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              message,
              style: const TextStyle(color: Colors.white),
            ),
          ),
        ],
      ),
      backgroundColor: Colors.orange.shade700,
      behavior: SnackBarBehavior.floating,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(8)),
      margin: const EdgeInsets.all(16),
      duration: const Duration(seconds: 5),
    ),
  );
}

/// Shows an informational dialog for critical errors like duplicate bookings
/// or conflicts, giving the user more context and a clear action.
///
/// Use this for 409 Conflict errors where the user needs to understand
/// what happened and what to do next.
Future<void> showErrorDialog(
  BuildContext context, {
  required String title,
  required String message,
  String? actionLabel,
  VoidCallback? onAction,
}) async {
  return showDialog<void>(
    context: context,
    builder: (context) => AlertDialog(
      icon: Icon(
        Icons.info_outline,
        color: Theme.of(context).colorScheme.error,
        size: 32,
      ),
      title: Text(title),
      content: Text(message),
      actions: [
        if (onAction != null)
          TextButton(
            style: TextButton.styleFrom(foregroundColor: AppTheme.navy),
            onPressed: () {
              Navigator.of(context).pop();
              onAction();
            },
            child: Text(actionLabel ?? 'View Bookings'),
          ),
        TextButton(
          style: TextButton.styleFrom(foregroundColor: AppTheme.navy),
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('OK'),
        ),
      ],
    ),
  );
}

/// Convenience method that picks the right display style based on the
/// [ApiException] type:
/// - Network errors → warning snackbar
/// - Conflict errors (409) → error dialog
/// - Other errors → error snackbar
void handleApiError(
  BuildContext context,
  ApiException error, {
  String? conflictTitle,
  String? conflictActionLabel,
  VoidCallback? onConflictAction,
}) {
  if (error.isNetworkError) {
    showWarningSnackBar(context, error.message);
  } else if (error.isConflict) {
    showErrorDialog(
      context,
      title: conflictTitle ?? 'Already Booked',
      message: error.message,
      actionLabel: conflictActionLabel,
      onAction: onConflictAction,
    );
  } else {
    showErrorSnackBar(context, error.message);
  }
}

/// Derives a relevant, user-friendly message from an error thrown by a
/// data fetch. Prefers the [ApiException.message] (which already maps status
/// codes and network issues to friendly text); otherwise falls back to a
/// generic message.
String messageForError(Object error) {
  if (error is ApiException) return error.message;
  return 'Something went wrong. Please try again.';
}

/// A reusable error state for screens whose API call failed.
///
/// Shows an icon, a relevant message derived from [error], and a navy
/// "Retry" link that calls [onRetry].
///
/// Use inside `AsyncValue.when(error: (e, _) => ErrorRetry(error: e, onRetry: ...))`.
class ErrorRetry extends StatelessWidget {
  const ErrorRetry({
    super.key,
    required this.error,
    required this.onRetry,
    this.title,
  });

  /// The error thrown by the failed API call.
  final Object error;

  /// Called when the user taps "Retry".
  final VoidCallback onRetry;

  /// Optional headline shown above the message (e.g. "Failed to load bookings").
  final String? title;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.error_outline, size: 48, color: theme.colorScheme.error),
            const SizedBox(height: 12),
            if (title != null) ...[
              Text(
                title!,
                style: theme.textTheme.titleMedium
                    ?.copyWith(fontWeight: FontWeight.w600, color: AppTheme.navy),
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 6),
            ],
            Text(
              messageForError(error),
              style: theme.textTheme.bodyMedium
                  ?.copyWith(color: Colors.grey.shade700),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 8),
            TextButton(
              onPressed: onRetry,
              style: TextButton.styleFrom(foregroundColor: AppTheme.navy),
              child: const Text('Retry'),
            ),
          ],
        ),
      ),
    );
  }
}
