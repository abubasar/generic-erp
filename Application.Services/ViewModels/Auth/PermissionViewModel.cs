namespace Application.Services.ViewModels.Auth
{
    public class PermissionViewModel
    {
        public Guid Id { get; set; }

        public Guid RoleId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Group { get; set; } = string.Empty;

        public bool Selected { get; set; }
    }
}
