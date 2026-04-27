namespace ArgosApi.Dtos
{
    public class AuthEmployeeDto
    {
        public string Token { get; set; } = null!;
        public AuthEmployeeDataDto? Employee { get; set; }
    }

    public class AuthUserDto
    {
        public string Token { get; set; } = null!;
        public AuthUserDataDto? User { get; set; }
    }

    public class AuthEmployeeDataDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Document { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }

    public class AuthUserDataDto
    {
        public Guid Id { get; set; }
        public Guid? CompanyId { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }
}