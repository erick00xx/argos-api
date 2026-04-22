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
    public required string DeviceName { get; set; }
}