namespace Application.Core.Entities.Platform;

/// <summary>The tenant's current commercial state. One active row per tenant.</summary>
public class Subscription
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>Named plan, or null for "build your own".</summary>
    public string? PlanKey { get; set; }

    /// <summary>Trial | Active | PastDue | Suspended | Cancelled.</summary>
    public string Status { get; set; } = "Trial";

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public DateTime? TrialEndsOn { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
