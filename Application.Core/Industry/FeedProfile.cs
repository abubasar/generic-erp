using Application.Core.Entities;

namespace Application.Core.Industry
{
    /// <summary>Animal / poultry feed industry — sells by weight (bags × <see cref="Product.BagWeight"/> = Kg); quantity-driven; A5 documents; distributor flows.</summary>
    public sealed class FeedProfile : IIndustryProfile
    {
        public const string ProfileKey = "feed";

        public string Key => ProfileKey;

        public IUomPolicy Uom { get; } = new BagWeightUomPolicy();
        public ISalesPolicy Sales { get; } = new FeedSalesPolicy();
        public IReportProfile Reports { get; } = new FeedReportProfile();
        public IDashboardProfile Dashboard { get; } = new FeedDashboardProfile();

        private sealed class BagWeightUomPolicy : IUomPolicy
        {
            public decimal ToSellable(decimal primaryQuantity, Product product) => primaryQuantity * product.BagWeight;
        }

        private sealed class FeedSalesPolicy : ISalesPolicy
        {
            public bool RequireApprovedCustomerDiscountOnOrder => true;
            public bool OneInvoicePerSaleOrder => false;
            public bool BlockInvoiceUnpostWhenReceiptExists => false;
            public bool CollectionReportGroupsByZone => true;
        }

        private sealed class FeedReportProfile : IReportProfile
        {
            public bool CompactLayout => true;

            public string ProductLabel(string? name, string? code, string? packSize) =>
                $"{name} ({code})";
        }

        private sealed class FeedDashboardProfile : IDashboardProfile
        {
            public bool ShowQuantityKpis => true;
            public bool ShowValueKpis => false;
        }
    }
}
