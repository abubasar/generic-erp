using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Accounts.AccountsReceivable
{
    public class ReceivePaymentAgainstSaleViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public DateTime PaymentDate { get; set; }
        public string? InvoiceNo { get; set; }
        public Guid CustomerId { get; set; }
        public decimal Amount { get; set; }
        public Guid CostCenterId { get; set; }
        public Guid ToAccountId { get; set; }
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
        public virtual AccountViewModel? ToAccount { get; set; }
        public virtual CustomerViewModel? Customer { get; set; }
    }
}
