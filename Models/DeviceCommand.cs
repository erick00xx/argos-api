using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;

public class DeviceCommand
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public int CommandNumber { get; set; } // 1, 2, 3, etc. to track command sequence
    public string CommandText { get; set; } = string.Empty;
    public string Status { get; set; } = "pending"; // pending, sent, success, failed, cancelled

    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public string? ReturnCode { get; set; } // 0 for success
    
    [ForeignKey(nameof(DeviceId))]
    public Device Device { get; set; } = null!;
}