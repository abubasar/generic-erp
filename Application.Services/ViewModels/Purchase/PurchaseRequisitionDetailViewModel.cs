using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class PurchaseRequisitionDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid PurchaseRequisitionId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal LastPurchaseRate { get; set; }
        public int OrderedQuantity { get; set; }
        public int AlertQuantity { get; set; }
        public decimal CurrentStockQuantity { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual MeasurementUnitViewModel? MeasurementUnit { get; set; }
        public virtual ProductViewModel? Product { get; set; }
    }
}
