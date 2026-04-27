using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ArgosApi.Data;
using ArgosApi.Dtos;
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

    public async Task<AuthUserDto?> LoginAdminAsync(string username, string password)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == password);

        if (user == null)
        {
            return null; // Credenciales inválidas  
        }

        var roleNames = user.UserRoles
            .Where(ur => ur.Role != null && ur.Role.IsActive)
            .Select(ur => ur.Role!.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var permissionNames = user.UserRoles
            .Where(ur => ur.Role != null && ur.Role.IsActive)
            .SelectMany(ur => ur.Role!.RolePermissions)
            .Where(rp => rp.Permission != null)
            .Select(rp => rp.Permission!.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Configuración para el JWT
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? "SuperSecretKeyForArgosApiThatNeedsToBeAtLeast32CharactersLong!");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim("username", user.Username)
        };

        if (user.CompanyId.HasValue)
            claims.Add(new Claim("CompanyId", user.CompanyId.Value.ToString()));

        foreach (var roleName in roleNames)
            claims.Add(new Claim(ClaimTypes.Role, roleName));

        foreach (var permissionName in permissionNames)
            claims.Add(new Claim("permission", permissionName));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
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
        var response = new AuthUserDto
        {
            Token = tokenHandler.WriteToken(token),
            User = new AuthUserDataDto
            {
                Id = user.Id,
                CompanyId = user.CompanyId,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Roles = roleNames,
                Permissions = permissionNames
            }
        };

        return response;
    }

    public async Task<AuthEmployeeDto?> LoginEmployeeAsync(string document, string password)
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
                // new Claim(ClaimTypes.Role, employee.attWebAllowed ? "AttWebAllowed" : "NotAttWebAllowed"), 
                new Claim("Document", employee.Document),
                new Claim("CompanyId", employee.CompanyId.ToString()),
                new Claim("AttWebAllowed", employee.attWebAllowed ? "true" : "false") // Agrega un claim específico para attWebAllowed
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
        var response = new AuthEmployeeDto
        {
            Token = tokenHandler.WriteToken(token),
            Employee = new AuthEmployeeDataDto
            {
                Id = employee.Id,
                CompanyId = employee.CompanyId,
                Document = employee.Document,
                FirstName = employee.FirstName,
                LastName = employee.LastName
            }
        };

        return response;
    }
}
