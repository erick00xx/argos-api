using ArgosApi.Dtos;
using ArgosApi.Shared;

namespace ArgosApi.Services;

public interface IReportService
{
	Task<Result<byte[]>> ExportAttendancesCsvAsync(Guid companyId, AttendanceReportFilterDto? filter = null);
	Task<Result<byte[]>> ExportEmployeesCsvAsync(Guid companyId);
}