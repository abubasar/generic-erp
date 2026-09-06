namespace Application.Services.ViewModels.Sale.SaleOrder
{
    public class SaleOrderAggregatorModel
    {
        public decimal AggregatorSubtotal { get; set; }
        public decimal AggregatorTotalPercentageDiscountAmount { get; set; }
        public decimal AggregatorDiscount { get; set; }
        public decimal AggregatorOfferDiscount { get; set; }
        public decimal AggregatorOtherDiscount { get; set; }
        public decimal AggregatorTotal { get; set; }
        public decimal AggregatorTransportationCost { get; set; }
        public decimal AggregatorDepoCharge { get; set; }
        public decimal AggregatorNetTotal { get; set; }
    }
}
