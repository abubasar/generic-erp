namespace Application.Core.Entities.Platform;

/// <summary>A per-tenant numeric limit, checked at the point of creation.</summary>
public class Entitlement
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>max_users | max_branches | max_pos_terminals | storage_mb.</summary>
    public string Key { get; set; } = null!;

    public int Limit { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
