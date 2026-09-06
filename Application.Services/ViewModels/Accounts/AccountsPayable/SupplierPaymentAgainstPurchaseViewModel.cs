using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Purchase;

namespace Application.Services.ViewModels.Accounts.AccountsPayable
{
    public class SupplierPaymentAgainstPurchaseViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid PurchaseInvoiceId { get; set; }
        public Guid SupplierId { get; set; }
        public decimal Amount { get; set; }
        public Guid CostCenterId { get; set; }
        public Guid FromAccountId { get; set; }
        public string? Remark { get; set; }
        public int Status { get; set; }
        public string? StatusName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public string? CheckedBy { get; set; }
        public string? ApprovedBy { get; set; }
        public Guid FinancialYearId { get; set; }

        public virtual CostCenterViewModel? CostCenter { get; set; }
        public virtual AccountViewModel? FromAccount { get; set; }
        public virtual PurchaseInvoiceViewModel? PurchaseInvoice { get; set; }
    }
}
