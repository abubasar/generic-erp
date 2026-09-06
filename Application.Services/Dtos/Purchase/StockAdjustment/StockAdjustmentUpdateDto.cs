namespace Application.Services.Dtos.Purchase.StockAdjustment
{
    public class StockAdjustmentUpdateDto : StockAdjustmentCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedStockAdjustmentDetailIds { get; set; }
        public new ICollection<StockAdjustmentDetailUpdateDto>? StockAdjustmentDetails { get; set; }
    }
}
