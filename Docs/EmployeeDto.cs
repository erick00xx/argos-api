using System.ComponentModel.DataAnnotations;
using ArgosApi.Enums;

namespace ArgosApi.Dtos;

public class EmployeeDto
{
    public Guid Id { get; set; }
    public string EnrolledId { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public string DocumentNumber { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class EmployeeRequestDto
{
    // Guid? companyId, string? enrolledId,string? document, string? firstName, string? lastName, string? branchName, string? departmentName, bool? status,
    public string? EnrolledId { get; set; }
    public string? Document { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? BranchName { get; set; }
    public string? DepartmentName { get; set; }
    public bool? Status { get; set; }
}

public class EmployeeCreateDto
{
    // Organización base del empleado dentro de la compañía
    public Guid DepartmentId { get; set; }
    public Guid BranchId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? AliasId { get; set; }

    public DocumentType DocumentType { get; set; }
    // Documento real del empleado (DNI, CE, etc.)
    [MaxLength(30)]
    public required string Document { get; set; }
    // Código interno visible en UI ("Código ficha")
    [MaxLength(10)]
    public required string EnrolledId { get; set; } // Pin en reloj
    [MaxLength(30)]
    public string? FileCode { get; set; }
    [MaxLength(100)]
    public required string FirstName { get; set; }
    [MaxLength(100)]
    public required string LastName { get; set; }
    [MaxLength(120)]
    public string? Profession { get; set; }
    public DateTime? BirthDate { get; set; }
    [MaxLength(20)]
    public string? Gender { get; set; }
    [MaxLength(100)]
    public string? OriginCountry { get; set; }
    [MaxLength(150)]
    [EmailAddress]
    public string? PersonalEmail { get; set; }
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
    public bool notifyAttByEmail { get; set; } = false;
    public bool attWebAllowed { get; set; } = false; // Permitir marcar asistencia desde la web (sin reloj)
    public bool IsAttendanceTracked { get; set; } = true;
    public bool ClockPrivilege { get; set; } = false; // Tipo de usuario (Usuario = false, Admin = true)
    
    [MaxLength(50)]
    public required string ClockName { get; set; }

    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }
}

public class EmployeeUpdateDto
{
    public Guid Id { get; set; }
    // Organización base del empleado dentro de la compañía
    public Guid DepartmentId { get; set; }
    public Guid BranchId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? AliasId { get; set; }

    [MaxLength(30)]
    public required string Document { get; set; }
    // Código interno visible en UI ("Código ficha")
    [MaxLength(30)]
    public string? FileCode { get; set; }
    [MaxLength(120)]
    public string? Profession { get; set; }
    public DateTime? BirthDate { get; set; }
    [MaxLength(20)]
    public string? Gender { get; set; }
    [MaxLength(100)]
    public string? OriginCountry { get; set; }
    [MaxLength(150)]
    [EmailAddress]
    public string? PersonalEmail { get; set; }
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
    public bool notifyAttByEmail { get; set; } = false;
    public bool attWebAllowed { get; set; } = false; // Permitir marcar asistencia desde la web (sin reloj)
    public bool IsAttendanceTracked { get; set; } = true;
    public bool ClockPrivilege { get; set; } = false; // Tipo de usuario (Usuario = false, Admin = true)
    
    public bool EmployeeIsActive { get; set; } = true;

    public DateTime? ContractStartDate { get; set; }
    public DateTime? ContractEndDate { get; set; }

}