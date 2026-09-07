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
}
