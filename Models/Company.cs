using ArgosApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace ArgosApi.Models;

public class Company : EntityBase
{
    // General Info
    [MaxLength(200)]
    public required string CompanyName { get; set; }
    public required TaxIdType TaxType { get; set; }
    [MaxLength(30)]
    public required string TaxId { get; set; }
    [MaxLength(120)]
    public required string BusinessLine { get; set; }
    [MaxLength(30)]
    public required string PhoneNumber { get; set; }

    // System Data
    [MaxLength(200)]
    public required string Nickname { get; set; }
    [MaxLength(200)]
    [EmailAddress]
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    // Location
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    // Legal Representative
    public required DocumentType LegalRepresentativeDocumentType { get; set; }
    [MaxLength(30)]
    public required string LegalRepresentativeDocument { get; set; }
    [MaxLength(200)]
    public required string LegalRepresentativeFullName { get; set; }
    [MaxLength(150)]
    [EmailAddress]
    public required string LegalRepresentativeEmail { get; set; }
    [MaxLength(30)]
    public required string LegalRepresentativePhone { get; set; }

    // HR / Corporate Data
    public string? HRManagerName { get; set; }
    public string? HRManagerEmail { get; set; }
    public string? HRDirectorName { get; set; }
    public string? HRDirectorEmail { get; set; }

    // Status
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ICollection<CompanyAlias> Aliases { get; set; } = new List<CompanyAlias>();
    public ICollection<Branch> Branches { get; set; } = new List<Branch>();
    public ICollection<Department> Departments { get; set; } = new List<Department>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}