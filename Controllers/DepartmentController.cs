using System.Security.Claims;
using ArgosApi.Dtos;
using ArgosApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArgosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet]
    [Authorize(Policy = "view:all")]
    [SwaggerOperation(Summary = "Obtiene una lista paginada de departamentos. Permite filtrar por estado y término de búsqueda.")]
    public async Task<IActionResult> GetDepartments(
        [FromQuery] bool? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { error = "User ID claim is missing or invalid." });
        }
        var result = await _departmentService.GetPagedAsync(userId, status, pageNumber, pageSize, searchTerm);

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

    [HttpPost]
    [Authorize(Policy = "view:all")]
    [SwaggerOperation(Summary = "Crea un nuevo departamento.")]
    public async Task<IActionResult> CreateDepartment([FromBody] DepartmentCreateDto dto)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { error = "User ID claim is missing or invalid." });
        }

        var result = await _departmentService.CreateAsync(dto, userId);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode ?? 500, new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetDepartments), new { id = result.Value.Id }, result.Value);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "view:all")]
    [SwaggerOperation(Summary = "Actualiza un departamento existente.")]
    public async Task<IActionResult> UpdateDepartment(Guid id, [FromBody] DepartmentUpdateDto dto)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { error = "User ID claim is missing or invalid." });
        }

        var result = await _departmentService.UpdateAsync(id, userId, dto);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode ?? 500, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "delete:all")]
    [SwaggerOperation(Summary = "Elimina un departamento existente. (no eliminar ni uno que ya estaba antes xd)")]
    public async Task<IActionResult> DeleteDepartment(Guid id)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { error = "User ID claim is missing or invalid." });
        }

        var result = await _departmentService.DeleteAsync(id);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode ?? 500, new { error = result.Error });
        }

        return Ok(result);
    }

}