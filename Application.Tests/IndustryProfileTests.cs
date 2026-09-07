using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Industry;
using Application.Core.Interfaces;

namespace Application.Tests;

/// <summary>
/// The industry behaviour profile: Feed converts bag quantities to weight
/// (bags × BagWeight = Kg); Pharma passes them through unchanged. Resolution is
/// keyed off the tenant's business template, so a mis-wired container would put a
/// Feed tenant on identity units (or vice-versa) and silently mis-price orders.
/// </summary>
public sealed class IndustryProfileTests
{
    private static Product ProductWithBagWeight(int bagWeight) => new() { Id = Guid.NewGuid(), Name = "p", BagWeight = bagWeight };

    [Fact]
    public void Feed_Uom_multiplies_bags_by_bag_weight()
    {
        IIndustryProfile feed = new FeedProfile();
        Assert.Equal("feed", feed.Key);
        Assert.Equal(250m, feed.Uom.ToSellable(10m, ProductWithBagWeight(25)));
    }

    [Fact]
    public void Pharmacy_Uom_is_identity()
    {
        IIndustryProfile pharmacy = new PharmacyProfile();
        Assert.Equal("pharmacy", pharmacy.Key);
        Assert.Equal(10m, pharmacy.Uom.ToSellable(10m, ProductWithBagWeight(25)));
    }

    [Theory]
    [InlineData("feed", "feed")]
    [InlineData("Feed", "feed")]
    [InlineData(" FEED ", "feed")]
    [InlineData("pharmacy", "pharmacy")]
    [InlineData("unknown", "pharmacy")]
    [InlineData(null, "pharmacy")]
    public void For_maps_template_key_to_profile(string? key, string expectedProfileKey)
        => Assert.Equal(expectedProfileKey, IndustryProfiles.For(key).Key);

    [Theory]
    [InlineData("feed", "feed")]
    [InlineData("pharmacy", "pharmacy")]
    [InlineData(null, "pharmacy")]
    public void Profile_resolves_from_the_request_tenant_template(string? templateKey, string expectedProfileKey)
    {
        // Mirrors the Autofac registration in InfrastructureModule: the per-request
        // IIndustryProfile is IndustryProfiles.For(ITenantContext.BusinessTemplateKey).
        var tenantContext = new HttpTenantContext();
        tenantContext.Load(modules: [], businessTemplateKey: templateKey,
            status: Core.Enums.SubscriptionStatus.Active, quotas: new Dictionary<string, int>());

        ITenantContext ctx = tenantContext;
        Assert.Equal(expectedProfileKey, IndustryProfiles.For(ctx.BusinessTemplateKey).Key);
    }

    [Fact]
    public void Feed_sales_policy_matches_the_pre_refactor_BusinessType_2_branches()
    {
        var sales = new FeedProfile().Sales;
        Assert.True(sales.RequireApprovedCustomerDiscountOnOrder);   // was: if (BusinessType == 2) on SaleOrder create
        Assert.False(sales.OneInvoicePerSaleOrder);                  // was: if (BusinessType == Primary) on SaleInvoice create
        Assert.False(sales.BlockInvoiceUnpostWhenReceiptExists);     // was: if (BusinessType == Primary) on SaleInvoice unpost
        Assert.True(sales.CollectionReportGroupsByZone);             // was: if (businessType == Secondary) -> zone else region
    }

    [Fact]
    public void Pharmacy_sales_policy_matches_the_pre_refactor_BusinessType_1_branches()
    {
        var sales = new PharmacyProfile().Sales;
        Assert.False(sales.RequireApprovedCustomerDiscountOnOrder);
        Assert.True(sales.OneInvoicePerSaleOrder);
        Assert.True(sales.BlockInvoiceUnpostWhenReceiptExists);
        Assert.False(sales.CollectionReportGroupsByZone);
    }

    [Fact]
    public void Dashboard_kpis_are_quantity_for_feed_and_value_for_pharmacy()
    {
        Assert.True(new FeedProfile().Dashboard.ShowQuantityKpis);
        Assert.False(new FeedProfile().Dashboard.ShowValueKpis);
        Assert.False(new PharmacyProfile().Dashboard.ShowQuantityKpis);
        Assert.True(new PharmacyProfile().Dashboard.ShowValueKpis);
    }

    [Fact]
    public void Report_layout_is_compact_for_feed_and_full_for_pharmacy()
    {
        Assert.True(new FeedProfile().Reports.CompactLayout);
        Assert.False(new PharmacyProfile().Reports.CompactLayout);
    }

    [Theory]
    [InlineData("feed", "Broiler Starter (BS-01)")]
    [InlineData("pharmacy", "Napa (500mg) (NP-500)")]
    public void Product_label_includes_pack_size_only_for_pharmacy(string profileKey, string expected)
    {
        var name = profileKey == "feed" ? "Broiler Starter" : "Napa";
        var code = profileKey == "feed" ? "BS-01" : "NP-500";
        var pack = profileKey == "feed" ? "50kg" : "500mg";
        Assert.Equal(expected, IndustryProfiles.For(profileKey).Reports.ProductLabel(name, code, pack));
    }
}
