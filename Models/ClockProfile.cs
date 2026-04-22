using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArgosApi.Models;

[Index(nameof(EmployeeId), IsUnique = true)]
public class ClockProfile : EntityBase
{
    public Guid EmployeeId { get; set; }
    // Equivale a "Controlar asistencia" en la pestaña de control
    public bool IsAttendanceTracked { get; set; } = true;
    [MaxLength(50)]
    // ID con el que el reloj identifica al empleado (enrolamiento)
    public required string EnrolledId { get; set; }
    [MaxLength(100)]
    public required string ClockName { get; set; }
    public bool ClockPrivilege { get; set; } = false;
    public bool IsPasswordAllowed { get; set; } = false;
    [MaxLength(200)]
    public string? ClockPasswordHash { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;
}