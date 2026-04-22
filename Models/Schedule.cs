using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;

public class Schedule : EntityBase // En la UI es "Horarios"
{
    public Guid CompanyId { get; set; }
    public required string Name { get; set; }
    public string? CalendarColor { get; set; } // "Color in calendar"
    
    // Crucial para turnos que cruzan la medianoche (ej. 22:00 a 06:00)
    public bool IsTwoDaySchedule { get; set; } = false; 

    // --- BLOQUE DE ENTRADA (Entry) ---
    public TimeOnly MinEntryTime { get; set; } // "Home entry" (Desde qué hora puede marcar)
    public TimeOnly TargetEntryTime { get; set; } // "Entry time" (La hora oficial)
    public TimeOnly MaxEntryTime { get; set; } // "Entry term" (Hasta qué hora se considera su entrada)

    // --- BLOQUE DE SALIDA (Exit) ---
    public TimeOnly MinExitTime { get; set; } // "Start exit" (Desde qué hora le vale la salida)
    public TimeOnly TargetExitTime { get; set; } // "Departure time" (La hora oficial de salida)
    public TimeOnly MaxExitTime { get; set; } // "Exit term" (Hasta qué hora el reloj acepta su salida)

    // --- BLOQUE DE DESCANSO (Rest) ---
    public bool UsesBreak { get; set; } = false;
    public TimeOnly? BreakStartTime { get; set; } // "Break begins"
    public TimeOnly? BreakEndTime { get; set; } // "Break ends"
    public int BreakMinutes { get; set; } = 0; // "Minutes break"
    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;

    public ICollection<ShiftDetail> ShiftDetails { get; set; } = new List<ShiftDetail>();
}