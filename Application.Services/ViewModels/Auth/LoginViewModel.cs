namespace Application.Services.ViewModels.Auth
{
    public class LoginViewModel
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public IList<string> Permissions { get; set; } = new List<string>();

    }
}
