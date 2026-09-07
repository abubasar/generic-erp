namespace Application.Core.Entities.Platform;

/// <summary>
/// A named commercial package a tenant can subscribe to (Starter, Growth, …).
/// The module set it grants is a comma-separated key list, same shape as
/// <see cref="BusinessTemplate.DefaultModuleKeys"/>. Price comes from the
/// published <see cref="PriceBook"/>, not from here.
/// </summary>
public class Plan
{
    public string Key { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>Comma-separated <see cref="PlatformModule.Key"/> list this plan includes.</summary>
    public string ModuleKeys { get; set; } = "";

    /// <summary>Entitlement limits granted by the plan, as "key=limit" pairs, comma-separated (e.g. "max_users=10,max_branches=2").</summary>
    public string Quotas { get; set; } = "";

    public bool IsPublic { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}
