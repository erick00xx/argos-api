using ArgosApi.Dtos;
using ArgosApi.Shared;

namespace ArgosApi.Services;

public interface IAttendanceService
{
    Task<bool> SaveMultipleAttendancesAsync(List<AttendanceLogDto> attendances);
    Task<bool> SaveSingleAttendanceAsync(AttendanceLogDto attendance);
    Task<PagedResult<List<AttendanceDto>>> GetPagedAsync(int pageNumber = 1, int pageSize = 10, Guid? employeeId = null);
}