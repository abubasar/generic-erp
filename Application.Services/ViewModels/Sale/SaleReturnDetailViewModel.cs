using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Sale
{
    public class SaleReturnDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid SaleReturnId { get; set; }
        public Guid ProductId { get; set; }
        public int BagWeight { get; set; }
        public int PrimaryQuantity { get; set; }
        public int Quantity { get; set; }
        public int PrimaryBonusQuantity { get; set; }
        public int BonusQuantity { get; set; }
        public decimal Rate { get; set; }
        public int ReturnPrimaryQuantity { get; set; }
        public int ReturnQuantity { get; set; }
        public int ReturnPrimaryBonusQuantity { get; set; }
        public int ReturnBonusQuantity { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal PercentageDiscountAmount { get; set; }
        public decimal OtherDiscountPerUnit { get; set; }
        public decimal VatPercentage { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
