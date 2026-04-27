using ArgosApi.Dtos;
using ArgosApi.Shared;

namespace ArgosApi.Services;

public interface IEmployeeService
{
	Task<Result<EmployeeCsvImportResultDto>> ImportFromCsvAsync(IFormFile file, Guid companyId, Guid? userId);
	Task<PagedResult<List<EmployeeDto>>> GetPagedAsync(Guid companyId, EmployeeRequestDto request, int pageNumber = 1, int pageSize = 10);
}