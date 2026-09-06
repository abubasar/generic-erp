namespace Application.Services.Dtos.Purchase.StockAdjustment
{
    public class StockAdjustmentCreationDto
    {
        public DateTime AdjustmentDate { get; set; }
        public Guid StoreId { get; set; }
        public decimal TotalAdjustmentQty { get; set; }
        public string? Remark { get; set; }

        public virtual ICollection<StockAdjustmentDetailCreationDto>? StockAdjustmentDetails { get; set; }
    }
}
