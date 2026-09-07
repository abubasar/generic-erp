namespace Application.Core.Entities.Platform;

/// <summary>
/// One idempotency record for the tenant provisioning job. Every step keys on
/// <c>(TenantId, StepKey)</c> — a re-run skips steps already <c>Done</c> and
/// retries <c>Failed</c> / <c>Pending</c> ones, so a half-failed provision is
/// safe to run again. See <c>docs/saas-platform-plan.md</c> §06 "Provisioning".
/// </summary>
public class ProvisioningStep
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>e.g. "subdomain", "financial-year", "owner-role", "owner-user".</summary>
    public string StepKey { get; set; } = null!;

    /// <summary>Pending | Done | Failed.</summary>
    public string Status { get; set; } = "Pending";

    public string? Error { get; set; }

    public int Attempts { get; set; }

    public DateTime? StartedOn { get; set; }

    public DateTime? CompletedOn { get; set; }
}
