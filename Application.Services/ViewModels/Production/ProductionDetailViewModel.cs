using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Production
{
    public class ProductionDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid ProductionId { get; set; }
        public Guid RawMaterialId { get; set; }
        public decimal Quantity { get; set; }
        public decimal AdjustmentQuantity { get; set; }
        public decimal ActualUsedQuantity { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? RawMaterial { get; set; }
    }
}
