using ArgosApi.Enums;

namespace ArgosApi.Dtos;
public class AttendanceDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime Timestamp { get; set; }
    public AttendancePunchType Type { get; set; }
    public required string DeviceName { get; set; }
}