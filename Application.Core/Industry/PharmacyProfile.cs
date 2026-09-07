using Application.Core.Entities;

namespace Application.Core.Industry
{
    /// <summary>Pharmaceutical industry — sells by the pack/bag, so units pass through unchanged.</summary>
    public sealed class PharmacyProfile : IIndustryProfile
    {
        public const string ProfileKey = "pharmacy";

        public string Key => ProfileKey;

        public IUomPolicy Uom { get; } = new IdentityUomPolicy();

        private sealed class IdentityUomPolicy : IUomPolicy
        {
            public decimal ToSellable(decimal primaryQuantity, Product product) => primaryQuantity;
        }
    }
}
