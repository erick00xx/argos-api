using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;

public class Department : EntityBase
{
    public Guid CompanyId { get; set; }
    [MaxLength(120)]
    public required string Name { get; set; }
    [MaxLength(250)]
    public required string Description { get; set; }
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}