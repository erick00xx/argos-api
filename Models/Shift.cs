using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;

public class Shift : EntityBase // En la UI es "Turnos"
{
    public Guid CompanyId { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    public required string Period { get; set; } = "Weekly"; // Ej: "Weekly" (Semanal), "Monthly"
    public int Extension { get; set; } = 1; // De cuántas semanas es el ciclo (1 por defecto)
    public bool IsFlexible { get; set; } = false; // "Especial (Flexible)"
    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;

    // Lista de los días que componen esta plantilla
    public ICollection<ShiftDetail> ShiftDetails { get; set; } = new List<ShiftDetail>();
    
    public ICollection<EmployeeShift> EmployeeShifts { get; set; } = new List<EmployeeShift>();
}