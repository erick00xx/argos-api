using ArgosApi.Dtos;

namespace ArgosApi.Services;

public interface IAttendanceService
{
    Task<bool> SaveMultipleAttendancesAsync(List<AttendanceLogDto> attendances);
    Task<bool> SaveSingleAttendanceAsync(AttendanceLogDto attendance);
}