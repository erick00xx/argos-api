using ArgosApi.Dtos;

namespace ArgosApi.Services;

public interface IAuthService
{
    Task<AuthEmployeeDto?> LoginEmployeeAsync(string document, string password);
    Task<AuthUserDto?> LoginAdminAsync(string username, string password);
}
