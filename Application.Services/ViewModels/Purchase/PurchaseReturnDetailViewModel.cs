using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class PurchaseReturnDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid PurchaseReturnId { get; set; }
        public Guid ProductId { get; set; }
        public int Grnquantity { get; set; }
        public int GrnBagWeightDeductionQty { get; set; }
        public int Quantity { get; set; }
        public int BagWeightDeductionQty { get; set; }
        public int NumberOfBagQuantity { get; set; }
        public int NetQuantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
