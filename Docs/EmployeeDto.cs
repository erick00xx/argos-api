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