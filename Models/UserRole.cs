using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;

public class UserRole : EntityBase
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    [ForeignKey(nameof(RoleId))]
    public Role Role { get; set; } = null!;
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}