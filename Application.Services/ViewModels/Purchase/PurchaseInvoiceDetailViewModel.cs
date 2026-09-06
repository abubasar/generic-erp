using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class PurchaseInvoiceDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid PurchaseInvoiceId { get; set; }
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
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
