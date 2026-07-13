import 'package:nmu_shuttle_app/core/api/api_constants.dart';
import 'package:nmu_shuttle_app/core/api/dio_client.dart';
import 'package:nmu_shuttle_app/features/student/models/booking_model.dart';

/// Repository for booking-related API calls.
class BookingRepository {
  BookingRepository({required DioClient dioClient}) : _dioClient = dioClient;

  final DioClient _dioClient;

  /// Fetches all bookings for the authenticated student.
  /// GET /api/v1/bookings
  Future<List<BookingModel>> getBookings() async {
    final response = await _dioClient.dio.get(ApiConstants.bookings);
    final List<dynamic> data = response.data as List<dynamic>;
    return data
        .map((json) => BookingModel.fromJson(json as Map<String, dynamic>))
        .toList();
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
