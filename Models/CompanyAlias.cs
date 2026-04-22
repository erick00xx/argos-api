using ArgosApi.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;
public class CompanyAlias : EntityBase
{
    public Guid CompanyId { get; set; }
    [MaxLength(200)]
    public required string Name { get; set; }
    public required TaxIdType TaxType { get; set; }
    [MaxLength(30)]
    public required string TaxId { get; set; }
    public required DocumentType LegalRepresentativeDocumentType { get; set; }
    [MaxLength(30)]
    public required string LegalRepresentativeDocument { get; set; }
    [MaxLength(200)]
    public required string LegalRepresentativeName { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();

}