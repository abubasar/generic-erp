namespace Application.Core.Entities.Platform;

/// <summary>
/// Tenant-scoped typed key/value settings. Replaces the non-tenant-scoped
/// <see cref="Setting"/> table for anything per-tenant.
/// </summary>
public class TenantSetting
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Key { get; set; } = null!;

    public string? Value { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
