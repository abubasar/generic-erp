using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Accounts
{
    public class FundTransferViewModel
    {
        public Guid Id { get; set; }
        public string? FundTransferNo { get; set; }
        public DateTime FundTransferDate { get; set; }
        public Guid FundTransferTransactionTypeId { get; set; }
        public Guid CostCenterId { get; set; }
        public Guid TransferFromAccountId { get; set; }
        public Guid TransferToAccountId { get; set; }
        public decimal Amount { get; set; }
        public decimal? Charges { get; set; }
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
        public virtual AccountViewModel? TransferFromAccount { get; set; }
        public virtual AccountViewModel? TransferToAccount { get; set; }
    }
}
