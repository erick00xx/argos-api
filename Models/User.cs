using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;

public class User : EntityBase
{
    public Guid? EmployeeId { get; set; }
    [MaxLength(80)]
    public required string UserName { get; set; }
    [MaxLength(200)]
    public required string PasswordHash { get; set; }

    [ForeignKey(nameof(EmployeeId))]
    public Employee? Employee { get; set; }
}
