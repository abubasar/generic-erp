using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Accounts
{
    public class JournalEntryViewModel
    {
        public Guid Id { get; set; }
        public string? VoucherNo { get; set; }
        public DateTime VoucherDate { get; set; }
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
        public virtual ICollection<JournalEntryDetailViewModel>? JournalEntryDetails { get; set; }
    }
}
