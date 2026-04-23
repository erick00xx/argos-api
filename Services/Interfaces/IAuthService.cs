using ArgosApi.Dtos;

namespace ArgosApi.Services;

public interface IAuthService
{
    Task<AuthEmployeeDto?> LoginAsync(string document, string password);
}
