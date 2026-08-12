import 'package:nmu_shuttle_app/features/student/models/booking_model.dart';

/// Paginated response from GET /api/v1/bookings.
class PaginatedBookings {
  const PaginatedBookings({
    required this.items,
    required this.page,
    required this.pageSize,
    required this.totalCount,
    required this.totalPages,
  });

  final List<BookingModel> items;
  final int page;
  final int pageSize;
  final int totalCount;
  final int totalPages;

  factory PaginatedBookings.fromJson(Map<String, dynamic> json) {
    final itemsList = (json['items'] as List<dynamic>)
        .map((e) => BookingModel.fromJson(e as Map<String, dynamic>))
        .toList();
    return PaginatedBookings(
      items: itemsList,
      page: json['page'] as int,
      pageSize: json['pageSize'] as int,
      totalCount: json['totalCount'] as int,
      totalPages: json['totalPages'] as int,
    );
  }
}
