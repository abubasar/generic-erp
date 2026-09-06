namespace Application.Services.Dtos.Purchase.StockAdjustment
{
    public class StockAdjustmentDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public decimal AdjustmentQty { get; set; }
    }
}
