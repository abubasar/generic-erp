namespace Application.Services.Dtos.Auth
{
    public class RegisterDto
    {
     
        public string Username { get; set; }=string.Empty;
        public Guid RoleId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? Password { get; set; }
    }
}
