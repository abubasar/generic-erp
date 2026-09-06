using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Sale
{
    public class SaleQuotationDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid SaleQuotationId { get; set; }
        public Guid ProductId { get; set; }
        public int BagWeight { get; set; }
        public int PrimaryQuantity { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
