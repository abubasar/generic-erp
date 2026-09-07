namespace Application.Core.Entities.Platform;

/// <summary>
/// A feature module that can be switched on per tenant. Code-defined, seeded from
/// <c>Permissions.AccessModules.*</c>. Not tenant-scoped — this is the shared catalog.
/// </summary>
public class PlatformModule
{
    /// <summary>Stable key, e.g. "sales", "accounts". Primary key.</summary>
    public string Key { get; set; } = null!;

    public string Name { get; set; } = null!;

    /// <summary>Core | Business | Industry.</summary>
    public string Category { get; set; } = "Business";

    public string? Description { get; set; }

    /// <summary>Comma-separated module keys this one needs (e.g. "inventory,sales" for POS).</summary>
    public string? DependsOn { get; set; }

    /// <summary>The <c>Permissions.AccessModules.*</c> value this module gates, if any.</summary>
    public string? PermissionGroup { get; set; }

    /// <summary>True when usage of this module is billed by quantity (users, terminals, …).</summary>
    public bool IsMetered { get; set; }

    public int SortOrder { get; set; }
}
