namespace Application.Services.Dtos.Sale.SaleOrder
{
    public class SaleOrderDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public int BagWeight { get; set; }
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
        public int CurrentStockQuantity { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal PercentageDiscountAmount { get; set; }
        public decimal VatPercentage { get; set; }
    }
}
