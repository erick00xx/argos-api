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

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request.Document, request.Password);

        if (response == null)
        {
            return Unauthorized(new { message = "Invalid credentials." }); // In English
        }

        return Ok(response);
    }
}

public class LoginRequest
{
    public required string Document { get; set; }
    public required string Password { get; set; } // Representa el PasswordHash en texto plano por ahora
}
