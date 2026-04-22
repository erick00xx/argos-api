using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArgosApi.Enums;

namespace ArgosApi.Models;

public class BiometricTemplate : EntityBase
{
    public Guid EmployeeId { get; set; }
    public BiometricType Type { get; set; }
    // Índice según el dispositivo (puede variar entre fabricantes)
    public int? FingerIndex { get; set; }
    // Datos binarios reales
    public byte[] TemplateData { get; set; } = null!;
    // Formato del template
    [MaxLength(50)]
    public string? Format { get; set; }
    public int? Quality { get; set; }
    [ForeignKey(nameof(EmployeeId))]
    public Employee Employee { get; set; } = null!;
}