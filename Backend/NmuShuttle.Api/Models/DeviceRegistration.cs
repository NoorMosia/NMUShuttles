using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class DeviceRegistration
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int DeviceRegistrationId { get; set; }

    /// <summary>
    /// The StudentId or MarshalId depending on OwnerType.
    /// </summary>
    [Required]
    public int OwnerId { get; set; }

    [Required]
    [MaxLength(10)]
    public string OwnerType { get; set; } = string.Empty;

    [Required]
    [MaxLength(512)]
    public string FcmToken { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string Platform { get; set; } = string.Empty;

    [Required]
    public DateTime UpdatedAt { get; set; }
}
