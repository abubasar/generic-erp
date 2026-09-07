using Application.Core.Interfaces;

namespace Application.Core.Common
{
    /// <summary>Request-scoped <see cref="IPlatformAdminContext"/>. Filled once per request by <c>PlatformAuthMiddleware</c>.</summary>
    public sealed class PlatformAdminContext : IPlatformAdminContext
    {
        public bool IsAuthenticated { get; private set; }
        public Guid AdminId { get; private set; }
        public string? Email { get; private set; }
        public string? Name { get; private set; }
        public string? Role { get; private set; }

        public void Set(Guid adminId, string email, string? name, string? role)
        {
            IsAuthenticated = true;
            AdminId = adminId;
            Email = email;
            Name = name;
            Role = role;
        }
    }
}
