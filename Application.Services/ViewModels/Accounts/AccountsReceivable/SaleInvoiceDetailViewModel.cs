using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Accounts.AccountsReceivable
{
    public class SaleInvoiceDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid SaleInvoiceId { get; set; }
        public Guid? SaleOrderDetailId { get; set; }
        public Guid ProductId { get; set; }
        public int PrimaryQuantity { get; set; }
        public int Quantity { get; set; }
        public int PrimaryBonusQuantity { get; set; }
        public int BonusQuantity { get; set; }
        public decimal Rate { get; set; }
        public decimal NetRate { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal OfferDiscountPerUnit { get; set; }
        public decimal InvoiceDiscountPerUnit { get; set; }
        public decimal CashDiscountPerUnit { get; set; }
        public decimal SpecialDiscountPerUnit { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }
        public string? DeliveryNoteNo { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? DeliveryPlace { get; set; }
        public decimal OtherDiscountPerUnit { get; set; }
        public decimal TransportationCostPerUnit { get; set; }
        public decimal DepoChargePerUnit { get; set; }
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
