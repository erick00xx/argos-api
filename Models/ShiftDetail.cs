using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ArgosApi.Models;

public class ShiftDetail : EntityBase
{
    public Guid ShiftId { get; set; }
    public Guid ScheduleId { get; set; } // Relación al "Horario"
    
    // Si la Extension del Turno es 2 (Rotativo bisemanal), aquí guardas si es la semana 1 o 2.
    public int WeekNumber { get; set; } = 1; 
    
    // Día de la semana (Lunes, Martes, etc.)
    public DayOfWeek DayOfWeek { get; set; } 

    [ForeignKey(nameof(ShiftId))]
    public Shift Shift { get; set; } = null!;

    [ForeignKey(nameof(ScheduleId))]
    public Schedule Schedule { get; set; } = null!;
}