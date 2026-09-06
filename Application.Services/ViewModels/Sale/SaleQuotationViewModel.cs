using Application.Services.ViewModels.Accounts;

namespace Application.Services.ViewModels.Sale
{
    public class SaleQuotationViewModel
    {
        public Guid Id { get; set; }
        public string? QuotationNo { get; set; }
        public DateTime QuotationDate { get; set; }
        public Guid CustomerId { get; set; }
        public string? ReferenceNo { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? TermAndCondition { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public bool IsMailSent { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public string? CheckedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public Guid FinancialYearId { get; set; }

        public virtual CustomerViewModel? Customer { get; set; }
        public virtual ICollection<SaleQuotationDetailViewModel>? SaleQuotationDetails { get; set; }
    }
}
