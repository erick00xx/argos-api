using ArgosApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArgosApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login/employee")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginEmployee([FromBody] LoginEmployeeRequest request)
    {
        var response = await _authService.LoginEmployeeAsync(request.Document, request.Password);

        if (response == null)
        {
            return Unauthorized(new { message = "Invalid credentials." });
        }

        return Ok(response);
    }

    [HttpPost("login/admin")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAdmin([FromBody] LoginAdminRequest request)
    {
        var response = await _authService.LoginAdminAsync(request.Username, request.Password);

        if (response == null)
        {
            return Unauthorized(new { message = "Invalid credentials." }); // In English
        }

        return Ok(response);
    }
}

public class LoginEmployeeRequest
{
    public required string Document { get; set; }
    public required string Password { get; set; } // Representa el PasswordHash en texto plano por ahora
}

public class LoginAdminRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
