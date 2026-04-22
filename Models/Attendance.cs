using ArgosApi.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;

public class Attendance : EntityBase
{
    public Guid EmployeeId { get; set; }
    // Puede venir nulo cuando la marcación se consolida desde otra fuente
    public Guid? DeviceId { get; set; }

    // Fecha/hora exacta del punch recibido o procesado
    public DateTime PunchDateTime { get; set; }
    public AttendancePunchType PunchType { get; set; }
    public AttendanceSource Source { get; set; }
    public AttendanceMethod Method { get; set; }

    public bool IsValid { get; set; } = true;
    // Identificador crudo enviado por reloj para conciliación
    [MaxLength(50)]
    public string? RawClockUserId { get; set; }
    [MaxLength(100)]
    public string? Checksum { get; set; }
    [MaxLength(250)]
    public string? Location { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;
    [ForeignKey(nameof(DeviceId))]
    public Device? Device { get; set; }
}
