namespace Application.Services.Dtos.Accounts.AccountsReceivable.SaleInvoice
{
    public class SaleInvoiceDetailCreationDto
    {
        public Guid? SaleOrderDetailId { get; set; }
        public Guid ProductId { get; set; }
        public int PrimaryQuantity { get; set; }
        public int PrimaryBonusQuantity { get; set; }
        public int Quantity { get; set; }
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
    }
}
