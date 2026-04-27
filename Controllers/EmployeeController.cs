using System.Security.Claims;
using ArgosApi.Dtos;
using ArgosApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

    [HttpGet("all")]
    [Authorize]
    [SwaggerOperation(Summary = "Para panel administrativo.")]
    public async Task<IActionResult> GetAllEmployees(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? enrolledId = null,
        [FromQuery] string? document = null,
        [FromQuery] string? firstName = null,
        [FromQuery] string? lastName = null,
        [FromQuery] string? departmentName = null,
        [FromQuery] string? branchName = null,
        [FromQuery] bool? status = null)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (!Guid.TryParse(companyIdClaim, out var companyId))
            return Unauthorized(new { error = "CompanyId not found." });
        var request = new EmployeeRequestDto
        {
            EnrolledId = enrolledId,
            Document = document,
            FirstName = firstName,
            LastName = lastName,
            DepartmentName = departmentName,
            BranchName = branchName,
            Status = status
        };

        var employees = await _employeeService.GetPagedAsync(companyId, request, pageNumber, pageSize);

        if (!employees.IsSuccess)
            return StatusCode(employees.StatusCode ?? 500, new { error = employees.Error });

        if (employees.Value == null || employees.Value.Count == 0)
            return NotFound(new { error = "No employees found with the provided criteria." });
            
        return Ok(employees);
    }
}

public class EmployeeCsvImportRequest
{
    public IFormFile File { get; set; } = null!;
}