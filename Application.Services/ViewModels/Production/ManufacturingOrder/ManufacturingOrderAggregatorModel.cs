namespace Application.Services.ViewModels.Production.ManufacturingOrder
{
    public class ManufacturingOrderAggregatorModel
    {
        public int AggregatorProductionQuantity { get; set; }
        public decimal AggregatorTotalRmused { get; set; }
        public decimal AggregatorRmCost { get; set; }
        public decimal AggregatorStandardDirectExpenseAmount { get; set; }
        public decimal AggregatorStandardFactoryOverheadAmount { get; set; }
        public decimal AggregatorTotalCost { get; set; }
    }
}
