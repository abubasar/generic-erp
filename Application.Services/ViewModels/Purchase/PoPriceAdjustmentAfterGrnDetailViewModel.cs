using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class PoPriceAdjustmentAfterGrnDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid PoPriceAdjustmentAfterGrnId { get; set; }
        public Guid ProductId { get; set; }
        public int GrnQuantity { get; set; }
        public decimal GrnRate { get; set; }
        public int AdjustmentQuantity { get; set; }
        public decimal AdjustmentRate { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
