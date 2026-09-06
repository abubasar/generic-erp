using Application.Core.Entities;
using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Accounts.AccountsReceivable
{
    public class ReceivePaymentViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? FundTransferTransactionTypeId { get; set; }
        public string? TransactionNumber { get; set; }
        public Guid PaymentModeId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal FeedSalesPurpose { get; set; }
        public decimal CreditRecoveryPurpose { get; set; }
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
        public virtual CustomerViewModel? Customer { get; set; }
        public virtual FundTransferTransactionTypeViewModel? FundTransferTransactionType { get; set; }
        public virtual PaymentModeViewModel? PaymentMode { get; set; }
        public virtual ICollection<ReceivePaymentDetailViewModel>? ReceivePaymentDetails { get; set; }
    }
}
