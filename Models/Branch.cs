using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;

public class Branch : EntityBase
{
    public Guid CompanyId { get; set; }
    [MaxLength(150)]
    public required string Name { get; set; }
    [MaxLength(50)]
    public required string TimeZone { get; set; }
    [MaxLength(200)]
    public required string AddressLine1 { get; set; }
    [MaxLength(250)]
    public required string Description { get; set; }
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;
    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}