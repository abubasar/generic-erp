using Application.Core.Entities;

namespace Application.Core.Industry
{
    /// <summary>
    /// The behaviour profile for a tenant's industry, resolved once per request
    /// from <c>ITenantContext.BusinessTemplateKey</c>. Replaces the scattered
    /// <c>if (BusinessType == Primary/Secondary)</c> branches: industry is a
    /// behaviour profile, never a paid module — Pharma and Feed run 100% of the
    /// same features, they just differ in policy (units, sales rules, report
    /// layout, dashboard KPIs). See <c>docs/saas-platform-plan.md</c> §08 / §11.
    /// </summary>
    public interface IIndustryProfile
    {
        /// <summary>Industry key: "pharmacy" | "feed" | ...</summary>
        string Key { get; }

        /// <summary>How a primary (bag) quantity converts to the sellable / stock unit.</summary>
        IUomPolicy Uom { get; }

        /// <summary>Industry-specific sale-order / invoice rules.</summary>
        ISalesPolicy Sales { get; }

        /// <summary>Industry-specific report / PDF presentation.</summary>
        IReportProfile Reports { get; }

        /// <summary>Which KPIs the dashboard computes for this industry.</summary>
        IDashboardProfile Dashboard { get; }
    }

    /// <summary>
    /// Unit-of-measure policy. On a sale-detail row the <c>Primary*</c> quantities
    /// are counted in bags; the plain quantity is the sellable unit. Feed sells by
    /// weight (bags × <see cref="Product.BagWeight"/> = Kg); Pharma sells by the
    /// bag/pack itself, so the conversion is the identity.
    /// </summary>
    public interface IUomPolicy
    {
        /// <summary>Converts a primary (bag) quantity to the sellable / stock unit for <paramref name="product"/>.</summary>
        decimal ToSellable(decimal primaryQuantity, Product product);
    }

    /// <summary>
    /// Sale-order / invoice rules that differ by industry. Each is a plain flag so
    /// call sites read as <c>if (_industry.Sales.OneInvoicePerSaleOrder)</c>.
    /// </summary>
    public interface ISalesPolicy
    {
        /// <summary>Feed: a sale order may only be created for a customer that has an active, approved customer-wise product discount.</summary>
        bool RequireApprovedCustomerDiscountOnOrder { get; }

        /// <summary>Pharma: at most one sale invoice per sale order.</summary>
        bool OneInvoicePerSaleOrder { get; }

        /// <summary>Pharma: an approved invoice with a money receipt against it cannot be unposted until the receipt is unposted.</summary>
        bool BlockInvoiceUnpostWhenReceiptExists { get; }

        /// <summary>Feed (distributor): the collection report groups customers by zone; otherwise by region.</summary>
        bool CollectionReportGroupsByZone { get; }
    }

    /// <summary>Report / PDF presentation choices that differ by industry.</summary>
    public interface IReportProfile
    {
        /// <summary>
        /// Feed uses the compact document layouts — A5 documents with the compact
        /// footer, portrait / denser list reports; Pharma uses the full-page
        /// layouts (A4 landscape lists, the standard footer).
        /// </summary>
        bool CompactLayout { get; }

        /// <summary>How a finished product is labelled on production / BOM documents. Pharma includes the pack size.</summary>
        string ProductLabel(string? name, string? code, string? packSize);
    }

    /// <summary>
    /// Dashboard KPI selection. Feed is a quantity-driven business (bags, Kg);
    /// Pharma is value-driven (purchase value, production value, FG value).
    /// </summary>
    public interface IDashboardProfile
    {
        /// <summary>Feed: show quantity KPIs (purchase total, production qty, sales qty, FG qty).</summary>
        bool ShowQuantityKpis { get; }

        /// <summary>Pharma: show value KPIs (RM purchase value, production value, FG purchase/value).</summary>
        bool ShowValueKpis { get; }
    }
}
