namespace Application.Services.Dtos.Purchase.PurchaseInvoice
{
    public class PurchaseInvoiceDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public int Grnquantity { get; set; }
        public int BagWeightDeductionQuantity { get; set; }
        public int NumberOfBagQuantity { get; set; }
        public int NetQuantity { get; set; }
        public decimal CurrencyRate { get; set; }
        public decimal Rate { get; set; }
        public decimal RateAfterBagWeightDeduction { get; set; }
        public decimal CurrencyAmount { get; set; }
        public decimal Amount { get; set; }
        public string? Grnno { get; set; }
        public DateTime? Grndate { get; set; }
        public decimal VatPercentage { get; set; }
    }
}
