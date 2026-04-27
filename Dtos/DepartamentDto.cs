using System.ComponentModel.DataAnnotations;

namespace ArgosApi.Dtos;

public class DepartmentCreateDto
{
    public Guid CompanyId { get; set; }
    [MaxLength(120)]
    public required string Name { get; set; }
    [MaxLength(250)]
    public required string Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DepartmentUpdateDto
{
    public Guid id { get; set; }
    [MaxLength(120)]
    public required string Name { get; set; }
    [MaxLength(250)]
    public required string Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsActive { get; set; }
    public int EmployeesCount { get; set; }
}