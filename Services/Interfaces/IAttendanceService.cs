using ArgosApi.Dtos;
using ArgosApi.Enums;
using ArgosApi.Shared;

namespace ArgosApi.Services;

public interface IAttendanceService
{
    Task<bool> SaveMultipleAttendancesAsync(List<AttendanceLogDto> attendances);
    Task<bool> SaveSingleAttendanceAsync(AttendanceLogDto attendance);
    Task<PagedResult<List<AttendanceDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? employeeId = null, DateTime? startDate = null, DateTime? endDate = null, AttendancePunchType? punchType = null, AttendanceMethod? method = null, AttendanceSource? source = null, string? deviceName = null);
    // un metodo para que el empleado pueda marcar asistencia desde la web
    Task<Result<bool>> MarkAttendanceWebAsync(Guid employeeId, int punchType);
    Task<Result<AttendanceDetailDto>> GetAttendanceDetailAsync(Guid attendanceId, Guid employeeId);
}