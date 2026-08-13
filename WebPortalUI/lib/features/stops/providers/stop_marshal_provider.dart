import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

/// Model for a marshal assigned to a stop.
class StopMarshal {
  const StopMarshal({
    required this.marshalId,
    required this.firstName,
    required this.lastName,
    required this.staffNumber,
  });

  final int marshalId;
  final String firstName;
  final String lastName;
  final String staffNumber;

  factory StopMarshal.fromJson(Map<String, dynamic> json) {
    return StopMarshal(
      marshalId: json['marshalId'] as int,
      firstName: json['firstName'] as String? ?? '',
      lastName: json['lastName'] as String? ?? '',
      staffNumber: json['staffNumber'] as String? ?? '',
    );
  }
}

/// Model for marshals available to be assigned to a stop.
class StopMarshalOption {
  const StopMarshalOption({
    required this.marshalId,
    required this.firstName,
    required this.lastName,
    required this.staffNumber,
    this.currentStopName,
  });

  final int marshalId;
  final String firstName;
  final String lastName;
  final String staffNumber;
  final String? currentStopName;

  factory StopMarshalOption.fromJson(Map<String, dynamic> json) {
    return StopMarshalOption(
      marshalId: json['marshalId'] as int,
      firstName: json['firstName'] as String? ?? '',
      lastName: json['lastName'] as String? ?? '',
      staffNumber: json['staffNumber'] as String? ?? '',
      currentStopName: json['currentStopName'] as String?,
    );
  }
}

/// Fetches the marshal assigned to a given stop.
/// Returns null if no marshal is assigned.
final stopMarshalProvider =
    FutureProvider.autoDispose.family<StopMarshal?, int>((ref, stopId) async {
  final dioClient = ref.watch(dioClientProvider);
  try {
    final response = await dioClient.dio.get('/admin/stops/$stopId/marshal');
    if (response.data == null) return null;
    return StopMarshal.fromJson(response.data as Map<String, dynamic>);
  } catch (_) {
    return null;
  }
});
