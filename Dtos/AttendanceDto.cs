using ArgosApi.Enums;

namespace ArgosApi.Dtos;

public class AttendanceDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public AttendancePunchType Type { get; set; }
    public string TypeDescription => Type.ToString();
    public AttendanceMethod Method { get; set; }
    public string MethodDescription => Method.ToString();
    public AttendanceSource Source { get; set; }
    public string SourceDescription => Source.ToString();
    public required string DeviceName { get; set; }
}

public class AttendanceDetailDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string FullName { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public AttendancePunchType Type { get; set; }
    public string TypeDescription => Type.ToString();
    public AttendanceMethod Method { get; set; }
    public string MethodDescription => Method.ToString();
    public AttendanceSource Source { get; set; }
    public string SourceDescription => Source.ToString();
    public required string DeviceName { get; set; }
    public bool IsAttValid { get; set; }
    public string EnrolledId { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string DocumentType { get; set; } = null!;
    public string EmployeeDocument { get; set; } = null!;
    public bool IsEmployeeTracked { get; set; }
    public string Checksum { get; set; } = null!;
    public bool isChecksumValid { get; set; }
    public AttendanceCompanyDto CompanyInfo { get; set; } = null!;
}

public class AttendanceCompanyDto
{
    public string CompanyName { get; set; } = null!;
    public string TaxType { get; set; } = null!;
    public string CompanyTaxId { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
}
