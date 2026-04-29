using ArgosApi.Dtos;
using ArgosApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArgosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    
    public DeviceController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpGet]
    [Authorize(Policy = "view:all")]
    [SwaggerOperation(Summary = "Obtiene una lista paginada de dispositivos. Permite filtrar por número de reloj, nombre, sucursal, modelo y estado.")]
    public async Task<IActionResult> GetDevices(
        [FromQuery] int? clockNumber,
        [FromQuery] string? name,
        [FromQuery] string? branchName,
        [FromQuery] string? model,
        [FromQuery] bool? isActive,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
        if (companyIdClaim == null || !Guid.TryParse(companyIdClaim.Value, out var companyId))
        {
            return Unauthorized(new { error = "Company ID claim is missing or invalid." });
        }

        var requestDto = new DeviceRequestDto
        {
            ClockNumber = clockNumber,
            Name = name,
            BranchName = branchName,
            Model = model,
            IsActive = isActive
        };

        var result = await _deviceService.GetPagedDevicesByCompanyIdAsync(companyId, requestDto, pageNumber, pageSize);
        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.Error });
        }

        var response = new
        {
            Data = result.Value,
            Pagination = new
            {
                result.PageNumber,
                result.PageSize,
                result.TotalPages,
                result.TotalRecords
            }
        };
        return Ok(response);
    }
}