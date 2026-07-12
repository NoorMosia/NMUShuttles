using NmuShuttle.Api.Models;

namespace NmuShuttle.Api.Services;

public interface ICheckInService
{
    Task<Booking> CheckInAsync(string qrCodeToken, int marshalId);
}
