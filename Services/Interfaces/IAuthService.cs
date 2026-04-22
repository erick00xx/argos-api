namespace ArgosApi.Services.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(string document, string password);
}
