using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Purchase
{
    public class SupplierPaymentViewModel
    {
        public Guid Id { get; set; }
        public int SupplierPaymentType { get; set; }
        public string? SupplierPaymentTypeName { get; set; }
        public string? Code { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid? FundTransferTransactionTypeId { get; set; }
        public string? TransactionNumber { get; set; }
        public string? Ponumber { get; set; }
        public Guid SupplierId { get; set; }
        public Guid PaymentModeId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal UsedAmountInPurchaseInvoice { get; set; }
        public Guid CostCenterId { get; set; }
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

        public virtual CostCenterViewModel? CostCenter { get; set; }
        public virtual FundTransferTransactionTypeViewModel? FundTransferTransactionType { get; set; }
        public virtual PaymentModeViewModel? PaymentMode { get; set; }
        public virtual SupplierViewModel? Supplier { get; set; }
        public virtual ICollection<SupplierPaymentDetailViewModel>? SupplierPaymentDetails { get; set; }
    }
}
