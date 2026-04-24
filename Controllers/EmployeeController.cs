using System.Security.Claims;
using ArgosApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArgosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpPost("import-csv")]
    [Authorize]
    public async Task<IActionResult> ImportCsv([FromForm] EmployeeCsvImportRequest request)
    {
        if (request.File == null)
            return BadRequest(new { error = "Debe enviar un archivo CSV." });

        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (!Guid.TryParse(companyIdClaim, out var companyId))
            return Unauthorized(new { error = "No se encontró CompanyId válido en el token." });

        Guid? userId = null;
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var parsedUserId))
            userId = parsedUserId;

        var result = await _employeeService.ImportFromCsvAsync(request.File, companyId, userId);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode ?? 500, new { error = result.Error });

        return Ok(result.Value);
    }
}

public class EmployeeCsvImportRequest
{
    public IFormFile File { get; set; } = null!;
}