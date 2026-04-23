using System.Security.Claims;
using ArgosApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArgosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendanceController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAttendances([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var employeeIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (employeeIdString == null)
        {
            return Unauthorized(new { message = "Employee not identified." });
        }
        if (!Guid.TryParse(employeeIdString, out Guid employeeId))
        {
            return BadRequest(new { message = "The employee ID is not a valid GUID." });
        }

        var result = await _attendanceService.GetPagedAsync(pageNumber, pageSize, employeeId);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode ?? 500, new { error = result.Error });
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

    [HttpGet("all")]
    [Authorize]
    public async Task<IActionResult> GetAllAttendances([FromQuery] Guid? employeeId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _attendanceService.GetPagedAsync(pageNumber, pageSize, employeeId);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode ?? 500, new { error = result.Error });
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