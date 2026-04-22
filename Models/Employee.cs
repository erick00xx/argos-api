using ArgosApi.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArgosApi.Models;

public class Employee : EntityBase
{
    // Organización base del empleado dentro de la compañía
    public Guid DepartmentId { get; set; }
    public Guid BranchId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? AliasId { get; set; }

    public DateTime RegistrationDate { get; set; }
    public DocumentType DocumentType { get; set; }
    // Documento real del empleado (DNI, CE, etc.)
    [MaxLength(30)]
    public required string Document { get; set; }
    // Código interno visible en UI ("Código ficha")
    [MaxLength(10)]
    public required string EnrolledId { get; set; } // Pin en reloj
    public required string PasswordHash { get; set; }
    [MaxLength(30)]
    public string? FileCode { get; set; }
    [MaxLength(100)]
    public required string FirstName { get; set; }
    [MaxLength(100)]
    public required string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    [MaxLength(20)]
    public string? Gender { get; set; }
    [MaxLength(100)]
    public string? Nationality { get; set; }
    [MaxLength(100)]
    public string? OriginCountry { get; set; }
    [MaxLength(150)]
    [EmailAddress]
    public string? PersonalEmail { get; set; }
    public bool IsPersonalEmailVerified { get; set; } = false;
    [MaxLength(150)]
    [EmailAddress]
    public string? CorporateEmail { get; set; }
    [MaxLength(200)]
    public string? AddressLine1 { get; set; }
    [MaxLength(200)]
    public string? AddressLine2 { get; set; }
    [MaxLength(100)]
    public string? City { get; set; }
    [MaxLength(30)]
    public string? HomePhone { get; set; }
    [MaxLength(30)]
    public string? MobilePhone { get; set; }
    public bool IsPhoneVerified { get; set; } = false;
    public bool notifyAttByEmail { get; set; } = false;
    public bool attWebAllowed { get; set; } = false; // Permitir marcar asistencia desde la web (sin reloj)
    public bool IsAttendanceTracked { get; set; } = true;
    
    [MaxLength(100)]
    public required string ClockName { get; set; }
    public bool ClockPrivilege { get; set; } = false;
    public bool IsPasswordAllowed { get; set; } = false; // Permite marcar por contraseña en reloj
    [MaxLength(20)]
    public string? ClockPasswordHash { get; set; }

    public DateTime ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }

    [ForeignKey(nameof(DepartmentId))]
    public Department Department { get; set; } = null!;
    [ForeignKey(nameof(BranchId))]
    public Branch Branch { get; set; } = null!;
    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;
    [ForeignKey(nameof(AliasId))]
    public CompanyAlias? Alias { get; set; }

    // Perfil de enrolamiento/control en reloj (1 a 1)
    public User? User { get; set; }
    public ICollection<EmployeeShift> EmployeeShifts { get; set; } = new List<EmployeeShift>();
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<BiometricTemplate> BiometricTemplates { get; set; } = new List<BiometricTemplate>();
}