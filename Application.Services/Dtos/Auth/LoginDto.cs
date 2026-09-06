namespace Application.Services.Dtos.Auth
{
    public class LoginDto
    {
        // [Required]
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
