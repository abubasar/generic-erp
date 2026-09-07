using Application.Core.Entities;

namespace Application.Core.Industry
{
    /// <summary>Pharmaceutical industry — sells by the pack/bag; value-driven; full-page documents.</summary>
    public sealed class PharmacyProfile : IIndustryProfile
    {
        public const string ProfileKey = "pharmacy";

        public string Key => ProfileKey;

        public IUomPolicy Uom { get; } = new IdentityUomPolicy();
        public ISalesPolicy Sales { get; } = new PharmacySalesPolicy();
        public IReportProfile Reports { get; } = new PharmacyReportProfile();
        public IDashboardProfile Dashboard { get; } = new PharmacyDashboardProfile();

        private sealed class IdentityUomPolicy : IUomPolicy
        {
            public decimal ToSellable(decimal primaryQuantity, Product product) => primaryQuantity;
        }

        private sealed class PharmacySalesPolicy : ISalesPolicy
        {
            public bool RequireApprovedCustomerDiscountOnOrder => false;
            public bool OneInvoicePerSaleOrder => true;
            public bool BlockInvoiceUnpostWhenReceiptExists => true;
            public bool CollectionReportGroupsByZone => false;
        }

        private sealed class PharmacyReportProfile : IReportProfile
        {
            public bool CompactLayout => false;

            public string ProductLabel(string? name, string? code, string? packSize) =>
                $"{name} ({packSize}) ({code})";
        }

        private sealed class PharmacyDashboardProfile : IDashboardProfile
        {
            public bool ShowQuantityKpis => false;
            public bool ShowValueKpis => true;
        }
    }
}
