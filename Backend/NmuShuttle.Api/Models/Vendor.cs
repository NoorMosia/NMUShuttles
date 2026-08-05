using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class Vendor
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int VendorId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ContactEmail { get; set; }

    [MaxLength(20)]
    public string? ContactPhone { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Shuttle> Shuttles { get; set; } = new List<Shuttle>();
}
