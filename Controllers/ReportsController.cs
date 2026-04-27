using System.Security.Claims;
using ArgosApi.Dtos;
using ArgosApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArgosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
	private readonly IReportService _reportService;

	public ReportsController(IReportService reportService)
	{
		_reportService = reportService;
	}

	[HttpGet("attendances/export")]
	[Authorize(Policy = "view:all")]
	[SwaggerOperation(Summary = "Exportar marcaciones a CSV", Description = "Para los filtros, todavia no usarlos, no estan refinados")]
	public async Task<IActionResult> ExportAttendancesCsv(
		[FromQuery] Guid? employeeId = null,
		[FromQuery] DateTime? startDate = null,
		[FromQuery] DateTime? endDate = null)
	{
		var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        
		if (!Guid.TryParse(companyIdClaim, out var companyId))
			return Unauthorized(new { message = "CompanyId not found in token." });

		var filter = new AttendanceReportFilterDto
		{
			EmployeeId = employeeId,
			StartDate = startDate,
			EndDate = endDate
		};

		var result = await _reportService.ExportAttendancesCsvAsync(companyId, filter);
		if (!result.IsSuccess)
			return StatusCode(result.StatusCode ?? 500, new { error = result.Error });

		var fileName = $"reporte_marcaciones_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
		return File(result.Value, "text/csv; charset=utf-8", fileName);
	}
}