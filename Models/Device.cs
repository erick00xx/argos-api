using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;

public class Device : EntityBase // También puedes llamarlo 'ClockDevice'
{
    public Guid BranchId { get; set; } // "Associated branch"
    // --- ESTADO Y UBICACIÓN ---
    public bool IsActive { get; set; } = true; // "State" (Activo/Inactivo)
    public int ClockNumber { get; set; } // "Clock No." (ID interno del reloj para la empresa)
    [MaxLength(120)]
    public required string Name { get; set; } // "Name" (Ej: Tacna)
    

    [MaxLength(50)]
    public string? TimeZone { get; set; } // "Time zone used" (Ej: UTC-05:00)

    // --- IDENTIFICADORES DEL HARDWARE (CRUCIAL PARA ADMS) ---
    [MaxLength(80)]
    public required string Manufacturer { get; set; } // "Manufacturer" (Ej: ZKTeco, RelojControl, etc.)
    [MaxLength(80)]
    public required string Model { get; set; } // "Model" (Ej: T5)
    [MaxLength(120)]
    public required string SerialNumber { get; set; } // "Series No." (CRUCIAL: Con esto se identifica el reloj por HTTP)
    [MaxLength(20)]
    public string? FingerprintAlgorithmVersion { get; set; } // "FP version" (Ej: 10 o 9. ZKTeco usa algoritmos distintos)

    // --- CONFIGURACIONES DE ALERTA ---
    public bool GenerateConnectionAlert { get; set; } = false; // "¿Generar alerta de desconexión?"
    public bool LimitOneFingerprintPerEmployee { get; set; } = false; // "Limit 1 fingerprint..."

    // --- INFORMACIÓN DE RED (Network) ---
    [MaxLength(45)]
    public string? IpAddress { get; set; } // "IP address (local)"
    [MaxLength(45)]
    public string? Gateway { get; set; } 
    [MaxLength(100)]
    public string? Dns { get; set; }

    // --- ESTADO DE CONEXIÓN Y SINCRONIZACIÓN DINÁMICA ---
    public DateTime? LastConnectionDate { get; set; } // "Last connection" (Se actualiza con cada 'latido' ADMS)
    
    // Los siguientes datos se actualizan solos cuando el reloj manda el comando "INFO" por ADMS
    public int AttendanceLogCount { get; set; } = 0; // "Registers" (Marcaciones almacenadas)
    public int UserCount { get; set; } = 0; // "Users" (Usuarios en memoria)
    public int FingerprintCount { get; set; } = 0; // "Footprints" (Huellas en memoria)
    public int FaceCount { get; set; } = 0; // "Faces" (Rostros en memoria)

    [ForeignKey(nameof(BranchId))]
    public Branch Branch { get; set; } = null!;
    // Relación: Un dispositivo puede tener muchas marcaciones registradas en él
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
}