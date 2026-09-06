namespace Application.Services.ViewModels.Accounts
{
    public class VoucherEntryDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid VoucherEntryId { get; set; }
        public Guid AccountId { get; set; }
        public string? AccountDescription { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual AccountViewModel? Account { get; set; }
    }
}
