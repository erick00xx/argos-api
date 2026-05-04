using ArgosApi.Enums;

namespace ArgosApi.Dtos;

public class AttendanceReportFilterDto
{
    public Guid? EmployeeId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class EmployeesReportDto
{
    public string Document { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string FileCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;

    public string BirthDate { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    // public string OriginCountry { get; set; } = string.Empty;
    // public string City { get; set; } = string.Empty;
    public string CorporateEmail { get; set; } = string.Empty;
    public string ContractStartDate { get; set; } = string.Empty;
    public string AddressLine1 { get; set; } = string.Empty;
    // public string HomePhone { get; set; } = string.Empty;
    // public string MobilePhone { get; set; } = string.Empty;
}

public class AttendanceReportRowDto
{
    public string Document { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string FileCode { get; set; } = string.Empty;
    public string Rut { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string ShiftAssigned { get; set; } = string.Empty;
    public string ScheduleOrEventName { get; set; } = string.Empty;
    public string BreakMinutes { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public string EntryTime { get; set; } = string.Empty;
    public string ExitTime { get; set; } = string.Empty;
    public AttendancePunchType PunchType { get; set; }
}