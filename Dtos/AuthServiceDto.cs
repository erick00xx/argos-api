using ArgosApi.Models;

namespace ArgosApi.Dtos
{
    public class AuthEmployeeDto
    {
        public string Token { get; set; } = null!;
        public Employee? Employee { get; set; }
    }
}