using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArgosApi.Models;
[Index(nameof(EmployeeId), nameof(StartDate), nameof(EndDate))]
public class EmployeeShift : EntityBase
{
    public Guid EmployeeId { get; set; }
    public Guid ShiftId { get; set; }
    
    // Rango de validez de este turno para este trabajador
    public DateTime StartDate { get; set; } 
    public DateTime? EndDate { get; set; } // Null significa que tiene este turno indefinidamente
    public bool IsActive { get; set; } // Para marcar turnos que ya no se usan pero mantener el histórico

    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;

    [ForeignKey(nameof(ShiftId))]
    public Shift Shift { get; set; } = null!;
}