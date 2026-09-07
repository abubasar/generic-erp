namespace Application.Core.Entities.Platform;

/// <summary>What a tenant has switched on. One row per (tenant, module).</summary>
public class TenantModule
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string ModuleKey { get; set; } = null!;

    /// <summary>Active | Suspended.</summary>
    public string Status { get; set; } = "Active";

    public DateTime ActivatedOn { get; set; }

    public DateTime? ExpiresOn { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public PlatformModule Module { get; set; } = null!;
}
