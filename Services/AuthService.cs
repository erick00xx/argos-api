using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArgosApi.Data;
using ArgosApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ArgosApi.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string?> LoginAsync(string document, string password)
    {
        // Se busca el empleado por el Document y PasswordHash en texto plano (como fue solicitado)
        var employee = await _context.Set<Models.Employee>()
            .FirstOrDefaultAsync(e => e.Document == document && e.PasswordHash == password);

        if (employee == null)
        {
            return null; // Credenciales inválidas
        }

        // Configuración para el JWT
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "SuperSecretKeyForArgosApiThatNeedsToBeAtLeast32CharactersLong!");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{employee.FirstName} {employee.LastName}"),
                new Claim("Document", employee.Document),
                new Claim("CompanyId", employee.CompanyId.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(8), // Token con validez de 8 horas
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
