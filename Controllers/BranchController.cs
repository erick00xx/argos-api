using System.Security.Claims;
using ArgosApi.Dtos;
using ArgosApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArgosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BranchController : ControllerBase
{
    private readonly IBranchService _branchService;

    public BranchController(IBranchService branchService)
    {
        _branchService = branchService;
    }

    [HttpGet]
    [Authorize(Policy = "view:all")]
    [SwaggerOperation(Summary = "Obtiene una lista paginada de sucursales. Permite filtrar por estado y término de búsqueda.")]
    public async Task<IActionResult> GetBranches(
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
        var result = await _branchService.GetPagedAsync(userId,status, pageNumber, pageSize, searchTerm);

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
    [SwaggerOperation(Summary = "Crea una nueva sucursal.")]
    public async Task<IActionResult> CreateBranch([FromBody] BranchCreateDto branchDto)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { error = "User ID claim is missing or invalid." });
        }

        var result = await _branchService.CreateAsync(userId, branchDto);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode ?? 500, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "view:all")]
    [SwaggerOperation(Summary = "Actualiza una sucursal existente.")]
    public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] BranchUpdateDto branchDto)
    {
        if(!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { error = "User ID claim is missing or invalid." });
        }

        var result = await _branchService.UpdateAsync(id, userId, branchDto);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode ?? 500, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "delete:all")]
    [SwaggerOperation(Summary = "Elimina una sucursal existente. (no eliminar ni una que ya estaba antes xd)")]
    public async Task<IActionResult> DeleteBranch(Guid id)
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { error = "User ID claim is missing or invalid." });
        }

        var result = await _branchService.DeleteAsync(id);

        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode ?? 500, new { error = result.Error });
        }

        return Ok(result.Value);
    }
}