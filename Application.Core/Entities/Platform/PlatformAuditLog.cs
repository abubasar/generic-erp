namespace Application.Core.Entities.Platform;

/// <summary>
/// An append-only record of what a platform admin did — every tenant create /
/// suspend / module toggle / price override / impersonation. Never tenant-scoped.
/// </summary>
public class PlatformAuditLog
{
    public Guid Id { get; set; }

    public Guid PlatformAdminId { get; set; }

    public string PlatformAdminEmail { get; set; } = null!;

    /// <summary>e.g. "tenant.create", "tenant.suspend", "module.toggle", "subscription.override", "tenant.impersonate".</summary>
    public string Action { get; set; } = null!;

    /// <summary>The tenant this action targeted, when applicable.</summary>
    public Guid? TenantId { get; set; }

    /// <summary>Free-form JSON / text describing the change (before → after, keys touched, …).</summary>
    public string? Detail { get; set; }

    public DateTime CreatedOn { get; set; }
}
