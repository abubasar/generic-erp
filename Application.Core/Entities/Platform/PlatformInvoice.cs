namespace Application.Core.Entities.Platform;

/// <summary>
/// A manually-issued bill for a tenant's subscription period (docs/saas-platform-plan.md
/// §06/§08 Phase 2 — "Invoicing stays manual here: your team issues the bill and marks
/// it paid"). Deliberately not called "Invoice" — SaleInvoice and PurchaseInvoice are
/// existing, unrelated tenant-side ERP documents — and deliberately without line items:
/// the itemised, PricingEngine-driven TenantInvoice/TenantInvoiceLine schema is Phase 4
/// (automated billing). This is just enough to record "we billed them X for period
/// Y, and whether they've paid" until that lands.
/// </summary>
public class PlatformInvoice
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>Human-facing number, e.g. "INV-2026-0001". Unique, assigned on create.</summary>
    public string Number { get; set; } = null!;

    public DateTime PeriodStart { get; set; }

    public DateTime PeriodEnd { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "BDT";

    /// <summary>Unpaid | Paid | Void.</summary>
    public string Status { get; set; } = "Unpaid";

    public DateTime IssuedOn { get; set; }

    public DateTime? DueOn { get; set; }

    public DateTime? PaidOn { get; set; }

    public string? Note { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
