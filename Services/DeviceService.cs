using ArgosApi.Data;
using ArgosApi.Dtos;
using ArgosApi.Shared;
using Microsoft.EntityFrameworkCore;

namespace ArgosApi.Services;

public class DeviceService : IDeviceService
{
    ApplicationDbContext _context;

    public DeviceService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<PagedResult<List<DeviceDto>>> GetPagedDevicesByCompanyIdAsync(Guid companyId, DeviceRequestDto requestDto, int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            var baseQuery = _context.Devices
                .Where(d => d.Branch.CompanyId == companyId)
                .Where(d =>
                (!requestDto.ClockNumber.HasValue || d.ClockNumber == requestDto.ClockNumber.Value) &&
                (string.IsNullOrEmpty(requestDto.Name) || d.Name.ToLower().Contains(requestDto.Name.ToLower())) &&
                (string.IsNullOrEmpty(requestDto.BranchName) || d.Branch.Name.ToLower().Contains(requestDto.BranchName.ToLower())) &&
                (string.IsNullOrEmpty(requestDto.Model) || d.Model.ToLower().Contains(requestDto.Model.ToLower())) &&
                (!requestDto.IsActive.HasValue || d.IsActive == requestDto.IsActive.Value)
                ).AsQueryable();

            var totalRecords = await baseQuery.CountAsync();

            var skip = (pageNumber - 1) * pageSize;


            var devices = await baseQuery
                .OrderBy(d => d.Name)
                .Skip(skip)
                .Take(pageSize)
                .Select(d => new DeviceDto
                {
                    Id = d.Id,
                    BranchId = d.BranchId,
                    BranchName = d.Branch.Name,
                    IsActive = d.IsActive,
                    ClockNumber = d.ClockNumber,
                    Name = d.Name,
                    TimeZone = d.Branch.TimeZone,
                    Manufacturer = d.Manufacturer,
                    Model = d.Model,
                    SerialNumber = d.SerialNumber,
                    IpAddress = d.IpAddress,
                    Gateway = d.Gateway,
                    Dns = d.Dns,
                    LastConnectionDate = d.LastConnectionDate,
                    AttendanceLogCount = 0,
                    UserCount = 0,
                    FingerprintCount = 0,
                    FaceCount = 0
                })
                .ToListAsync();

            return PagedResult<List<DeviceDto>>.Ok(devices, pageNumber, pageSize, totalRecords);
        }
        catch (Exception ex)
        {
            return PagedResult<List<DeviceDto>>.Fail("An error occurred while retrieving devices: " + ex.Message, 500);
        }
    }
}