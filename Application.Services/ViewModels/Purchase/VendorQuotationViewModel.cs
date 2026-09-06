using Application.Services.ViewModels.Accounts;

namespace Application.Services.ViewModels.Purchase
{
    public class VendorQuotationViewModel
    {
        public Guid Id { get; set; }
        public string? QuotationNo { get; set; }
        public string? RequisitionNo { get; set; }
        public string? ReferenceNo { get; set; }
        public Guid StoreId { get; set; }
        public Guid SupplierId { get; set; }
        public int Transport { get; set; }
        public int PaymentMode { get; set; }
        public int PaymentTermInDays { get; set; }
        public int DeliveryTermInDays { get; set; }
        public DateTime DeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public string? ApprovedBy { get; set; }
        public string? TermAndCondition { get; set; }
        public Guid? CurrencyId { get; set; }
        public int ImportPurchaseIncoTerm { get; set; }
        public int ImportPurchasePaymentTerm { get; set; }
        public string? Remark { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public Guid FinancialYearId { get; set; }

        public virtual SupplierViewModel? Supplier { get; set; }
        public virtual ICollection<VendorQuotationDetailViewModel>? VendorQuotationDetails { get; set; }
    }
}
