using Application.Core.Entities;

namespace Application.Core.Industry
{
    /// <summary>Animal / poultry feed industry — sells by weight: bags × <see cref="Product.BagWeight"/> = Kg.</summary>
    public sealed class FeedProfile : IIndustryProfile
    {
        public const string ProfileKey = "feed";

        public string Key => ProfileKey;

        public IUomPolicy Uom { get; } = new BagWeightUomPolicy();

        private sealed class BagWeightUomPolicy : IUomPolicy
        {
            public decimal ToSellable(decimal primaryQuantity, Product product) => primaryQuantity * product.BagWeight;
        }
    }
}
