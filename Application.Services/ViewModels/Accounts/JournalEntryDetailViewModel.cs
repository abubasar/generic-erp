namespace Application.Services.ViewModels.Accounts
{
    public class JournalEntryDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid JournalEntryId { get; set; }
        public Guid AccountId { get; set; }
        public string? AccountDescription { get; set; }
        public int PostType { get; set; }
        public decimal Amount { get; set; }
        public bool IsOpeningBalance { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual AccountViewModel? Account { get; set; }
    }
}
