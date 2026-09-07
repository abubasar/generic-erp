namespace Application.Core.Entities.Platform;

/// <summary>
/// A versioned set of prices. Exactly one price book is "published" at a time;
/// editing prices means cloning to a new draft version and publishing it, so past
/// invoices always resolve against the price book that was live when they were cut.
/// </summary>
public class PriceBook
{
    public Guid Id { get; set; }

    /// <summary>Monotonic version number; the highest published one is live.</summary>
    public int Version { get; set; }

    public string Currency { get; set; } = "BDT";

    public string Status { get; set; } = "Draft"; // Draft | Published | Archived

    public DateTime? PublishedOn { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public ICollection<PriceBookEntry> Entries { get; set; } = new List<PriceBookEntry>();
}

/// <summary>One priced line in a <see cref="PriceBook"/> — either a plan or a single add-on module.</summary>
public class PriceBookEntry
{
    public Guid Id { get; set; }

    public Guid PriceBookId { get; set; }

    /// <summary>"plan" or "module".</summary>
    public string ItemType { get; set; } = "plan";

    /// <summary><see cref="Plan.Key"/> or <see cref="PlatformModule.Key"/>.</summary>
    public string ItemKey { get; set; } = null!;

    /// <summary>Recurring price per billing period (month).</summary>
    public decimal MonthlyPrice { get; set; }

    /// <summary>Optional per-unit metered price (e.g. per extra user).</summary>
    public decimal? UnitPrice { get; set; }

    public PriceBook PriceBook { get; set; } = null!;
}
