using ArgosApi.Data;
using ArgosApi.Dtos;
using ArgosApi.Models;
using ArgosApi.Shared;
using Microsoft.EntityFrameworkCore;

namespace ArgosApi.Services;

public class BranchService : IBranchService
{
    private readonly ApplicationDbContext _context;

    public BranchService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BranchDto>> CreateAsync(Guid userId, BranchCreateDto dto)
    {
        if (dto == null)
            return Result<BranchDto>.Fail("Invalid branch data", 400);

        var companyId = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => u.CompanyId)
            .FirstOrDefaultAsync();

        if (companyId == Guid.Empty || companyId == null)
            return Result<BranchDto>.Fail("User's company not found", 404);

        var branch = new Branch
        {
            Name = dto.Name,
            TimeZone = dto.TimeZone,
            AddressLine1 = dto.AddressLine1,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CompanyId = companyId.Value
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        return Result<BranchDto>.Ok(new BranchDto
        {
            Id = branch.Id,
            Name = branch.Name,
            TimeZone = branch.TimeZone,
            AddressLine1 = branch.AddressLine1,
            Description = branch.Description,
            IsActive = branch.IsActive,
        });
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var branch = _context.Branches
                .FirstOrDefault(b => b.Id == id && b.IsActive);

            if (branch == null)
                return Result<bool>.Fail("Branch not found", 404);

            var hasEmployees = await _context.Employees
                .AnyAsync(e => e.BranchId == id);

            if (hasEmployees)
                return Result<bool>.Fail("Cannot delete branch with associated employees", 400);

            var hasDevices = await _context.Devices
                .AnyAsync(d => d.BranchId == id);
            if (hasDevices)
                return Result<bool>.Fail("Cannot delete branch with associated devices", 400);

            _context.Branches.Remove(branch);

            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return Result<bool>.Fail($"Error deleting branch: {message}", 500);
        }
    }

    public async Task<Result<BranchDto>> GetByIdAsync(Guid id)
    {
        var branch = _context.Branches.FirstOrDefault(b => b.Id == id);
        if (branch == null)
            return Result<BranchDto>.Fail("Branch not found", 404);

        return Result<BranchDto>.Ok(new BranchDto
        {
            Id = branch.Id,
            Name = branch.Name,
            TimeZone = branch.TimeZone,
            AddressLine1 = branch.AddressLine1,
            Description = branch.Description,
            IsActive = branch.IsActive
        });
    }

    public async Task<PagedResult<List<BranchDto>>> GetPagedAsync(Guid userId, bool? status, int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
    {
        try
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var companyId = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.CompanyId)
                .FirstOrDefaultAsync();

            if (!companyId.HasValue || companyId == Guid.Empty)
                return PagedResult<List<BranchDto>>.Fail("User's company not found", 404);

            var baseQuery = _context.Branches
                .AsNoTracking()
                .Where(b => b.CompanyId == companyId.Value)
                .Where(b => !status.HasValue || b.IsActive == status.Value)
                .Where(b => string.IsNullOrEmpty(searchTerm) || b.Name.ToLower().Contains(searchTerm.ToLower()));

            var totalRecords = await baseQuery.CountAsync();

            var skip = (pageNumber - 1) * pageSize;

            var branches = await baseQuery
                .OrderBy(b => b.Name)
                .Skip(skip)
                .Take(pageSize)
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    TimeZone = b.TimeZone,
                    AddressLine1 = b.AddressLine1,
                    Description = b.Description,
                    IsActive = b.IsActive,
                    EmployeesCount = 0,
                    DeviceCount = 0
                })
                .ToListAsync();

            var branchIds = branches.Select(b => b.Id).ToList();

            var employeesByBranch = await _context.Employees
                .AsNoTracking()
                .Where(e => branchIds.Contains(e.BranchId))
                .GroupBy(e => e.BranchId)
                .Select(g => new { BranchId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BranchId, x => x.Count);

            var devicesByBranch = await _context.Devices
                .AsNoTracking()
                .Where(d => branchIds.Contains(d.BranchId))
                .GroupBy(d => d.BranchId)
                .Select(g => new { BranchId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BranchId, x => x.Count);

            foreach (var branch in branches)
            {
                branch.EmployeesCount = employeesByBranch.TryGetValue(branch.Id, out var employeeCount)
                    ? employeeCount
                    : 0;

                branch.DeviceCount = devicesByBranch.TryGetValue(branch.Id, out var deviceCount)
                    ? deviceCount
                    : 0;
            }

            return PagedResult<List<BranchDto>>.Ok(branches, pageNumber, pageSize, totalRecords);
        }
        catch (Exception ex)
        {
            return PagedResult<List<BranchDto>>.Fail($"Error retrieving branches: {ex.Message}", 500);
        }
    }

    public async Task<Result<BranchDto>> UpdateAsync(Guid id, Guid? userId, BranchUpdateDto dto)
    {
        var branch = _context.Branches.FirstOrDefault(b => b.Id == id);
        if (branch == null)
            return Result<BranchDto>.Fail("Branch not found", 404);

        // Update the branch properties
        branch.Name = dto.Name;
        branch.TimeZone = dto.TimeZone;
        branch.AddressLine1 = dto.AddressLine1;
        branch.Description = dto.Description;
        branch.IsActive = dto.IsActive;

        _context.Branches.Update(branch);
        await _context.SaveChangesAsync();

        var employeesCount = await _context.Employees
            .AsNoTracking()
            .CountAsync(e => e.BranchId == branch.Id);

        var deviceCount = await _context.Devices
            .AsNoTracking()
            .CountAsync(d => d.BranchId == branch.Id);

        return Result<BranchDto>.Ok(new BranchDto
        {
            Id = branch.Id,
            Name = branch.Name,
            TimeZone = branch.TimeZone,
            AddressLine1 = branch.AddressLine1,
            Description = branch.Description,
            IsActive = branch.IsActive,
            EmployeesCount = employeesCount,
            DeviceCount = deviceCount
        });
    }
}