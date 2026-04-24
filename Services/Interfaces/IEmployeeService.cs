using ArgosApi.Dtos;
using ArgosApi.Shared;

namespace ArgosApi.Services;

public interface IEmployeeService
{
	Task<Result<EmployeeCsvImportResultDto>> ImportFromCsvAsync(IFormFile file, Guid companyId, Guid? userId);
}