using System.Security.Claims;
using ArgosApi.Enums;
using ArgosApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArgosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendancesController : ControllerBase
{
    private readonly IAttendanceService _attendanceService;

    public AttendancesController(IAttendanceService attendanceService)
    {
        _attendanceService = attendanceService;
    }

    [HttpGet]
    [Authorize]
    [SwaggerOperation(Summary = "Obtener marcaciones", Description = "Obtiene las marcaciones del empleado autenticado.")]
    public async Task<IActionResult> GetAttendances(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] AttendancePunchType? punchType = null,
        [FromQuery] AttendanceMethod? method = null,
        [FromQuery] AttendanceSource? source = null,
        [FromQuery] string? deviceName = null
        )
    {
        var employeeIdString = User.FindFirst(ClaimTypes.NameIdentifier);
        if (employeeIdString == null)
        {
            return Unauthorized(new { message = "Employee not identified." });
        }
        if (!Guid.TryParse(employeeIdString.Value, out Guid employeeId))
        {
            return BadRequest(new { message = "The employee ID is not a valid GUID." });
        }

        var result = await _attendanceService.GetPagedAsync(pageNumber, pageSize, employeeId, startDate, endDate, punchType, method, source, deviceName);

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
    [Authorize(Policy = "view:attendances")]
    [SwaggerOperation(Summary = "Obtener marcaciones de todos los empleados", Description = "Obtiene todas las marcaciones y por id de empleado (por ahora, luego sera por nombre, fechas, y mas filtros)")]
    public async Task<IActionResult> GetAllAttendances(
        [FromQuery] Guid? employeeId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] AttendancePunchType? punchType = null,
        [FromQuery] AttendanceMethod? method = null,
        [FromQuery] AttendanceSource? source = null,
        [FromQuery] string? deviceName = null)
    {
        var result = await _attendanceService.GetPagedAsync(pageNumber, pageSize, employeeId, startDate, endDate, punchType, method, source, deviceName);

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

    [HttpPost("mark")]
    [Authorize]
    [SwaggerOperation(Summary = "Marcar asistencia desde la web", Description = "Permite a un empleado marcar su asistencia desde la web solo si tiene activado la propiedad attWebAllowed.")]
    public async Task<IActionResult> MarkAttendanceWeb([FromBody] TypeCheckDto dto)
    {
        var employeeIdString = User.FindFirst(ClaimTypes.NameIdentifier);
        if (employeeIdString == null)
        {
            return Unauthorized(new { message = "Employee not identified." });
        }
        if (!Guid.TryParse(employeeIdString.Value, out Guid employeeId))
        {
            return BadRequest(new { message = "The employee ID is not a valid GUID." });
        }

        var result = await _attendanceService.MarkAttendanceWebAsync(employeeId, dto.PunchType);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode ?? 500, new { error = result.Error });
        }
        return Ok(result.Value);
    }

    [HttpGet("{attendanceId}")]
    [Authorize]
    [SwaggerOperation(Summary = "Obtener detalle de una marcación", Description = "Obtiene el detalle de una marcación específica, incluyendo información del empleado y la compañía.")]
    public async Task<IActionResult> GetAttendanceDetail(Guid attendanceId)
    {
        var employeeIdString = User.FindFirst(ClaimTypes.NameIdentifier);
        if (employeeIdString == null)
        {
            return Unauthorized(new { message = "Employee not identified." });
        }
        if (!Guid.TryParse(employeeIdString.Value, out Guid employeeId))
        {
            return BadRequest(new { message = "The employee ID is not a valid GUID." });
        }

        var result = await _attendanceService.GetAttendanceDetailAsync(attendanceId, employeeId);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode ?? 500, new { error = result.Error });
        }
        return Ok(result.Value);
    }

    public class TypeCheckDto
    {
        public int PunchType { get; set; }
    }
}

