using System.ComponentModel.DataAnnotations;

namespace NmuShuttle.Api.DTOs;

public class CreateBookingDto
{
    [Required]
    public int TripInstanceId { get; set; }
}
