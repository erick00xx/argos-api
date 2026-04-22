using ArgosApi.Enums;

namespace ArgosApi.Dtos;
public class AttendanceLogDto
{
    public string Sn { get; set; } = null!;
    public string Table { get; set; } = null!;
    public string Pin { get; set; } = null!;
    public string PunchType { get; set; } = null!;
    public string Method { get; set; } = null!;
    public DateTime PunchDateTime { get; set; }
}