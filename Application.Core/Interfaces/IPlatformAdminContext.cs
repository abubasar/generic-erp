namespace Application.Core.Interfaces
{
    /// <summary>
    /// The platform admin behind the current request, populated by
    /// <c>PlatformAuthMiddleware</c> from the platform token. Empty on every
    /// tenant-facing request. Never carries a tenant id.
    /// </summary>
    public interface IPlatformAdminContext
    {
        bool IsAuthenticated { get; }
        Guid AdminId { get; }
        string? Email { get; }
        string? Name { get; }
        string? Role { get; }
    }
}
