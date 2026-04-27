
using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;

public class RolePermission : EntityBase
{
    public Guid RoleId { get; set; }

    public Guid PermissionId { get; set; }
    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }
    [ForeignKey(nameof(PermissionId))]
    public Permission? Permission { get; set; }
}
