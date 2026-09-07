// Originally EF Power Tools scaffold; now hand-maintained (code-first migrations).
#nullable disable
using System;
using System.Collections.Generic;

namespace Application.Core.Entities;

public partial class Tenant
{
    public Guid Id { get; set; }

    public string Code { get; set; }

    public string Name { get; set; }

    public string TimeZoneId { get; set; }

    public string Address { get; set; }

    public string ContactNo { get; set; }

    public string Binno { get; set; }

    public string Email { get; set; }

    /// <summary>1 = Pharmaceutical, 2 = Feed. Compatibility shim until BusinessTemplateKey fully replaces it.</summary>
    public int BusinessType { get; set; }

    public string Logo { get; set; }

    // ---- Platform layer (Phase 1) ----

    /// <summary>Sub-domain label, e.g. "abdullahfeed" in abdullahfeed.butsbd.com. Unique when set.</summary>
    public string Subdomain { get; set; }

    public string CustomDomain { get; set; }

    /// <summary><see cref="Platform.BusinessTemplate"/> key this tenant was created from.</summary>
    public string BusinessTemplateKey { get; set; }

    /// <summary>Trial | Active | PastDue | Suspended | Cancelled.</summary>
    public string Status { get; set; }

    /// <summary>ISO 4217 code, e.g. "BDT".</summary>
    public string Currency { get; set; }

    /// <summary>Null = shared pool. Set = a dedicated database connection key.</summary>
    public string DbConnectionKey { get; set; }

    public DateTime CreatedOn { get; set; }

    public DateTime UpdatedOn { get; set; }

    public string CreatedBy { get; set; }

    public string UpdatedBy { get; set; }

    public bool Deleted { get; set; }
}