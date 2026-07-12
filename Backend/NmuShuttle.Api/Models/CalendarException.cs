using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NmuShuttle.Api.Models;

public class CalendarException
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CalendarExceptionId { get; set; }

    [Required]
    public DateOnly TargetDate { get; set; }

    [Required]
    [MaxLength(30)]
    public string ExceptionType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }
}
