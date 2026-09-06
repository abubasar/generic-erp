using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Sale
{
    public class SaleOrderDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid SaleOrderId { get; set; }
        public Guid ProductId { get; set; }
        public int BagWeight { get; set; }
        public int PrimaryQuantity { get; set; }
        public int PrimaryBonusQuantity { get; set; }
        public int Quantity { get; set; }
        public int BonusQuantity { get; set; }
        public int DeliveredPrimaryQuantity { get; set; }
        public int DeliveredPrimaryBonusQuantity { get; set; }
        public decimal Rate { get; set; }
        public decimal NetRate { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal OfferDiscountPerUnit { get; set; }
        public decimal InvoiceDiscountPerUnit { get; set; }
        public decimal CashDiscountPerUnit { get; set; }
        public decimal SpecialDiscountPerUnit { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }
        public decimal OtherDiscountPerUnit { get; set; }
        public decimal TransportationCostPerUnit { get; set; }
        public decimal DepoChargePerUnit { get; set; }
        public int CurrentStockQuantity { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal PercentageDiscountAmount { get; set; }
        public decimal VatPercentage { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
