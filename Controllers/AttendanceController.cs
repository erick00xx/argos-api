using ArgosApi.Services;
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
    public async Task<IActionResult> GetAttendances([FromQuery] Guid? employeeId = null, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
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