using ArgosApi.Dtos;
using ArgosApi.Shared;

namespace ArgosApi.Services;

public interface IBranchService
{
    Task<Result<BranchDto>> CreateAsync(Guid userId, BranchCreateDto dto);
    Task<Result<BranchDto>> UpdateAsync(Guid id, Guid? userId, BranchUpdateDto dto);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<PagedResult<List<BranchDto>>> GetPagedAsync(Guid userId, bool? status, int pageNumber = 1, int pageSize = 10, string? searchTerm = null);
}