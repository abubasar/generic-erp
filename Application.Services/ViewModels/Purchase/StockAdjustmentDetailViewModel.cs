using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class StockAdjustmentDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid StockAdjustmentId { get; set; }
        public Guid ProductId { get; set; }
        public decimal AdjustmentQty { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
