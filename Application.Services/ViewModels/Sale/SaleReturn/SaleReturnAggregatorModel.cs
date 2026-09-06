namespace Application.Services.ViewModels.Sale.SaleReturn
{
    public class SaleReturnAggregatorModel
    {
        public decimal AggregatorSubtotal { get; set; }
        public decimal AggregatorTotalPercentageDiscountAmount { get; set; }
        public decimal AggregatorOtherDiscount { get; set; }
        public decimal AggregatorTotal { get; set; }
    }
}
