import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:nmu_shuttle_admin/features/auth/providers/admin_auth_providers.dart';

/// Model representing a vendor company.
class Vendor {
  const Vendor({
    required this.id,
    required this.companyName,
    this.contactEmail,
    this.contactPhone,
    required this.shuttleCount,
  });

  final int id;
  final String companyName;
  final String? contactEmail;
  final String? contactPhone;
  final int shuttleCount;

  factory Vendor.fromJson(Map<String, dynamic> json) {
    return Vendor(
      id: json['vendorId'] as int? ?? json['id'] as int? ?? 0,
      companyName: json['companyName'] as String? ?? '',
      contactEmail: json['contactEmail'] as String?,
      contactPhone: json['contactPhone'] as String?,
      shuttleCount: json['shuttleCount'] as int? ?? 0,
    );
  }
}

/// State for the vendors list.
class VendorsState {
  const VendorsState({
    this.vendors = const [],
    this.isLoading = false,
    this.errorMessage,
  });

  final List<Vendor> vendors;
  final bool isLoading;
  final String? errorMessage;

  VendorsState copyWith({
    List<Vendor>? vendors,
    bool? isLoading,
    String? errorMessage,
  }) {
    return VendorsState(
      vendors: vendors ?? this.vendors,
      isLoading: isLoading ?? this.isLoading,
      errorMessage: errorMessage,
    );
  }
}

/// Provider for managing vendors.
final vendorsProvider =
    StateNotifierProvider<VendorsNotifier, VendorsState>((ref) {
  final dioClient = ref.watch(dioClientProvider);
  return VendorsNotifier(dioClient.dio);
});

class VendorsNotifier extends StateNotifier<VendorsState> {
  VendorsNotifier(this._dio) : super(const VendorsState());

  final Dio _dio;

  Future<void> fetchVendors() async {
    state = state.copyWith(isLoading: true, errorMessage: null);
    try {
      final response = await _dio.get('/admin/vendors');
      final List data = response.data as List;
      final vendors = data.map((json) => Vendor.fromJson(json)).toList();
      state = state.copyWith(vendors: vendors, isLoading: false);
    } on DioException catch (e) {
      state = state.copyWith(
        isLoading: false,
        errorMessage: e.response?.data?['message'] as String? ??
            'Failed to load vendors',
      );
    }
  }

  Future<String?> createVendor({
    required String companyName,
    String? contactEmail,
    String? contactPhone,
  }) async {
    try {
      await _dio.post('/admin/vendors', data: {
        'companyName': companyName,
        if (contactEmail != null) 'contactEmail': contactEmail,
        if (contactPhone != null) 'contactPhone': contactPhone,
      });
      await fetchVendors();
      return null;
    } on DioException catch (e) {
      return e.response?.data?['message'] as String? ??
          'Failed to create vendor';
    }
  }

  Future<String?> updateVendor({
    required int id,
    String? companyName,
    String? contactEmail,
    String? contactPhone,
  }) async {
    try {
      await _dio.patch('/admin/vendors/$id', data: {
        if (companyName != null) 'companyName': companyName,
        if (contactEmail != null) 'contactEmail': contactEmail,
        if (contactPhone != null) 'contactPhone': contactPhone,
      });
      await fetchVendors();
      return null;
    } on DioException catch (e) {
      return e.response?.data?['message'] as String? ??
          'Failed to update vendor';
    }
  }

  Future<String?> deleteVendor(int id) async {
    try {
      await _dio.delete('/admin/vendors/$id');
      await fetchVendors();
      return null;
    } on DioException catch (e) {
      return e.response?.data?['message'] as String? ??
          'Failed to delete vendor';
    }
  }
}
