namespace Application.Services.ViewModels.Production.Production
{
    public class ProductionAggregatorModel
    {
        public int AggregatorExtraDamageQuantity { get; set; }
        public decimal AggregatorTotalRmused { get; set; }
        public decimal AggregatorRmCost { get; set; }
        public decimal AggregatorProductionQuantity { get; set; }
        public decimal AggregatorActualProductionQuantity { get; set; }
        public decimal AggregatorTotalCost { get; set; }
        public decimal AggregatorTotalAdjustmentQuantity { get; set; }
        public decimal AggregatorTotalAdjustmentCost { get; set; }
    }
}
