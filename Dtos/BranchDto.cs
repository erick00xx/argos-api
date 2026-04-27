using System.ComponentModel.DataAnnotations;

namespace ArgosApi.Dtos;

public class BranchCreateDto
{
    [MaxLength(150)]
    public required string Name { get; set; }
    [MaxLength(50)]
    public required string TimeZone { get; set; }
    [MaxLength(200)]
    public required string AddressLine1 { get; set; }
    [MaxLength(250)]
    public required string Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class BranchUpdateDto
{
    [MaxLength(150)]
    public required string Name { get; set; }
    [MaxLength(50)]
    public required string TimeZone { get; set; }
    [MaxLength(200)]
    public required string AddressLine1 { get; set; }
    [MaxLength(250)]
    public required string Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class BranchDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string TimeZone { get; set; } = null!;
    public string AddressLine1 { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsActive { get; set; }
    public int EmployeesCount { get; set; }
    public int DeviceCount { get; set; }
}