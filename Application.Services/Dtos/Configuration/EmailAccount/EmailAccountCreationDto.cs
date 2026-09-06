namespace Application.Services.Dtos.Configuration.EmailAccount
{
    public class EmailAccountCreationDto
    {
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? Host { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public int Port { get; set; }
        public bool EnableSsl { get; set; }
        public bool IsDefaultEmailAccount { get; set; }
    }
}
