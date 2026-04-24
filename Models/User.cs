using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArgosApi.Enums;

namespace ArgosApi.Models;

public class User : EntityBase
{
    public Guid? CompanyId { get; set; }

    [MaxLength(50)]
    public required string Username { get; set; }
    [MaxLength(100)]
    public required string Email { get; set; }
    [MaxLength(100)]
    public required string PhoneNumber { get; set; }
    [MaxLength(100)]
    public required string FirstName { get; set; }
    [MaxLength(100)]
    public required string LastName { get; set; }
    [MaxLength(100)]
    public required string PasswordHash { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
