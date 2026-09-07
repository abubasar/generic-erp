using Application.Core.Entities;

namespace Application.Core.Industry
{
    /// <summary>
    /// The behaviour profile for a tenant's industry, resolved once per request
    /// from <c>ITenantContext.BusinessTemplateKey</c>. Replaces the scattered
    /// <c>if (BusinessType == Primary/Secondary)</c> branches: industry is a
    /// behaviour profile, never a paid module — Pharma and Feed run 100% of the
    /// same features, they just differ in policy (units, report layout, KPIs).
    /// See <c>docs/saas-platform-plan.md</c> §08 Phase 1 / §11.
    /// </summary>
    public interface IIndustryProfile
    {
        /// <summary>Industry key: "pharmacy" | "feed" | ...</summary>
        string Key { get; }

        /// <summary>How a primary (bag) quantity converts to the sellable / stock unit.</summary>
        IUomPolicy Uom { get; }
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
}
