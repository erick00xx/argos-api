using System.ComponentModel.DataAnnotations;

namespace ArgosApi.Dtos;
// borrar todos los comentarios
public class DeviceDto
{
    public Guid Id { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    
    public bool IsActive { get; set; } = true; 
    public int ClockNumber { get; set; } 
    [MaxLength(120)]
    public required string Name { get; set; } 


    [MaxLength(50)]
    public string? TimeZone { get; set; }

    [MaxLength(80)]
    public required string Manufacturer { get; set; } 
    [MaxLength(80)]
    public required string Model { get; set; }
    [MaxLength(120)]
    public required string SerialNumber { get; set; }
    [MaxLength(20)]
    public string? FingerprintAlgorithmVersion { get; set; }

    public bool GenerateConnectionAlert { get; set; } = false;
    public bool LimitOneFingerprintPerEmployee { get; set; } = false;
    [MaxLength(45)]
    public string? IpAddress { get; set; } 
    [MaxLength(45)]
    public string? Gateway { get; set; }
    [MaxLength(100)]
    public string? Dns { get; set; }

    public DateTime? LastConnectionDate { get; set; } 

    
    public int AttendanceLogCount { get; set; } = 0; 
    public int UserCount { get; set; } = 0; 
    public int FingerprintCount { get; set; } = 0; 
    public int FaceCount { get; set; } = 0; 
}

public class DeviceRequestDto
{
    public int? ClockNumber { get; set; }
    public string? Name { get; set; }
    public string? BranchName { get; set; }
    public string? Model { get; set; }
    public bool? IsActive { get; set; }
}

