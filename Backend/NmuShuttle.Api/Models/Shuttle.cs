using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class Shuttle
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ShuttleId { get; set; }

    [Required]
    [MaxLength(20)]
    public string LicensePlate { get; set; } = string.Empty;

    [Required]
    public int Capacity { get; set; }

    [Required]
    [MaxLength(100)]
    public string VendorCompanyName { get; set; } = string.Empty;

    [Required]
    public int VendorId { get; set; }

    [Required]
    public bool IsAccessibleVehicle { get; set; }

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public Vendor Vendor { get; set; } = null!;
    public ICollection<TripShuttleAllocation> TripShuttleAllocations { get; set; } = new List<TripShuttleAllocation>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<BreakdownLog> BreakdownLogs { get; set; } = new List<BreakdownLog>();
    public ICollection<BreakdownLog> ReplacementBreakdownLogs { get; set; } = new List<BreakdownLog>();
    public ICollection<StopShuttleDefault> StopShuttleDefaults { get; set; } = new List<StopShuttleDefault>();
}
