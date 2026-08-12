import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/core/api/dio_client.dart';
import 'package:nmu_shuttle_app/features/student/models/booking_model.dart';
import 'package:nmu_shuttle_app/features/student/models/paginated_bookings.dart';

/// Repository for booking-related API calls.
class BookingRepository {
  BookingRepository({required DioClient dioClient}) : _dioClient = dioClient;

  final DioClient _dioClient;

  /// Fetches paginated bookings for the authenticated student.
  /// GET /api/v1/bookings?filter=upcoming|past|all&page=1&pageSize=20
  Future<PaginatedBookings> getBookings({
    String filter = 'all',
    int page = 1,
    int pageSize = 20,
  }) async {
    final response = await _dioClient.dio.get(
      ApiConstants.bookings,
      queryParameters: {
        'filter': filter,
        'page': page,
        'pageSize': pageSize,
      },
    );
    return PaginatedBookings.fromJson(response.data as Map<String, dynamic>);
  }

  /// Creates a new booking for the given trip instance.
  /// POST /api/v1/bookings
  Future<BookingModel> createBooking(int tripInstanceId) async {
    final response = await _dioClient.dio.post(
      ApiConstants.bookings,
      data: {'tripInstanceId': tripInstanceId},
    );
    return BookingModel.fromJson(response.data as Map<String, dynamic>);
  }

  /// Cancels a booking by ID.
  /// DELETE /api/v1/bookings/{id}
  Future<void> cancelBooking(int bookingId) async {
    await _dioClient.dio.delete('${ApiConstants.bookings}/$bookingId');
  }

  /// Fetches a single booking by ID.
  /// GET /api/v1/bookings/{id}
  Future<BookingModel> getBookingById(int bookingId) async {
    final response =
        await _dioClient.dio.get('${ApiConstants.bookings}/$bookingId');
    return BookingModel.fromJson(response.data as Map<String, dynamic>);
  }
}
