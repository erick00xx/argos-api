using ArgosApi.Data;
using ArgosApi.Dtos;
using ArgosApi.Models;
using ArgosApi.Shared;
using Microsoft.EntityFrameworkCore;

namespace ArgosApi.Services;

public class DepartmentService : IDepartmentService
{
    ApplicationDbContext _context;
    IHttpContextAccessor _httpContextAccessor;

    public DepartmentService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<DepartmentDto>> CreateAsync(DepartmentCreateDto dto, Guid? userId)
    {
        if (dto == null)
            return Result<DepartmentDto>.Fail("Invalid department data", 400);

        if (dto.CompanyId == Guid.Empty || !_context.Companies.Any(c => c.Id == dto.CompanyId))
            return Result<DepartmentDto>.Fail("CompanyId is required", 400);

        var department = new Department
        {
            CompanyId = dto.CompanyId,
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        DepartmentDto response = new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            IsActive = department.IsActive
        };

        return Result<DepartmentDto>.Ok(response);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        if (department == null)
            return Result<bool>.Fail("Department not found", 404);

        _context.Departments.Remove(department);

        await _context.SaveChangesAsync();

        return Result<bool>.Ok(true);
    }

    public async Task<PagedResult<List<DepartmentDto>>> GetPagedAsync(Guid? userId, bool? status, int pageNumber = 1, int pageSize = 10, string? searchTerm = null)
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
                return PagedResult<List<DepartmentDto>>.Fail("User's company not found", 404);

            var baseQuery = _context.Departments
                .AsNoTracking()
                .Where(d => d.CompanyId == companyId.Value)
                .Where(d => !status.HasValue || d.IsActive == status.Value)
                .Where(d => string.IsNullOrEmpty(searchTerm) || d.Name.ToLower().Contains(searchTerm.ToLower()))
                ;

            var totalRecords = await baseQuery.CountAsync();


            var skip = (pageNumber - 1) * pageSize;

            var departments = await baseQuery
                .OrderBy(d => d.Name)
                .Skip(skip)
                .Take(pageSize)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    IsActive = d.IsActive,
                    EmployeesCount = 0
                })
                .ToListAsync();

            var departmentIds = departments.Select(d => d.Id).ToList();
            var employeesCountByDepartment = await _context.Employees
                .AsNoTracking()
                .Where(e => departmentIds.Contains(e.DepartmentId))
                .GroupBy(e => e.DepartmentId)
                .Select(g => new { DepartmentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.DepartmentId, x => x.Count);

            foreach (var department in departments)
            {
                department.EmployeesCount = employeesCountByDepartment.TryGetValue(department.Id, out var count)
                    ? count
                    : 0;
            }

            return PagedResult<List<DepartmentDto>>.Ok(departments, pageNumber, pageSize, totalRecords);
        }
        catch (Exception ex)
        {
            return PagedResult<List<DepartmentDto>>.Fail($"Error retrieving departments: {ex.Message}", 500);
        }
    }

    public async Task<Result<DepartmentDto>> UpdateAsync(Guid id, Guid? userId, DepartmentUpdateDto dto)
    {
        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == id);
        if (department == null)
            return Result<DepartmentDto>.Fail("Department not found", 404);

        department.Name = dto.Name;
        department.Description = dto.Description;
        department.IsActive = dto.IsActive;
        department.UpdatedAt = DateTime.UtcNow;
        department.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        var employeesCount = await _context.Employees
            .AsNoTracking()
            .CountAsync(e => e.DepartmentId == department.Id);

        DepartmentDto response = new DepartmentDto
        {
            Id = department.Id,
            Name = department.Name,
            Description = department.Description,
            IsActive = department.IsActive,
            EmployeesCount = employeesCount
        };
        return Result<DepartmentDto>.Ok(response);
    }
}