namespace Application.Core.Entities.Platform;

/// <summary>
/// A vertical (Pharmacy, Feed, Super Shop, …): the modules a tenant of this kind
/// gets by default, plus which industry profile drives its behaviour.
/// </summary>
public class BusinessTemplate
{
    /// <summary>Stable key, e.g. "pharmacy", "feed". Primary key.</summary>
    public string Key { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>Comma-separated module keys seeded on for a new tenant of this template.</summary>
    public string DefaultModuleKeys { get; set; } = "";

    /// <summary>Industry profile key (units / report pack / sales policy). Usually == Key.</summary>
    public string IndustryProfileKey { get; set; } = null!;

    public bool IsPublic { get; set; } = true;

    public int SortOrder { get; set; }
}
