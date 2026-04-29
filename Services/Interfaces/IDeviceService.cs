using ArgosApi.Dtos;
using ArgosApi.Shared;

namespace ArgosApi.Services;

public interface IDeviceService
{
    Task<PagedResult<List<DeviceDto>>> GetPagedDevicesByCompanyIdAsync(Guid companyId, DeviceRequestDto requestDto, int pageNumber = 1, int pageSize = 10);
}