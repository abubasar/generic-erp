using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class VendorQuotationDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid VendorQuotationId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public virtual ProductViewModel? Product { get; set; }
    }
}
