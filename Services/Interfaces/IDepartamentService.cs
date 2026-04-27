using ArgosApi.Dtos;
using ArgosApi.Shared;

namespace ArgosApi.Services;

public interface IDepartmentService
{
    Task<Result<DepartmentDto>> CreateAsync(DepartmentCreateDto dto, Guid? userId);
    Task<Result<DepartmentDto>> UpdateAsync(Guid id, Guid? userId, DepartmentUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<PagedResult<List<DepartmentDto>>> GetPagedAsync(Guid? userId, bool? status, int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
}