using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ArgosApi.Enums;

namespace ArgosApi.Models;

public class User : EntityBase
{
    public Guid? CompanyId { get; set; }
    public DocumentType DocumentType { get; set; }
    [MaxLength(80)]
    public required string Document { get; set; }
    public required string PasswordHash { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company? Company { get; set; }
}
