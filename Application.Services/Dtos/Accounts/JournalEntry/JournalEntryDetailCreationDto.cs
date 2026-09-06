namespace Application.Services.Dtos.Accounts.JournalEntry
{
    public class JournalEntryDetailCreationDto
    {
        public Guid AccountId { get; set; }
        public string? AccountDescription { get; set; }
        public int PostType { get; set; }
        public decimal Amount { get; set; }
        public bool IsOpeningBalance { get; set; }
    }
}
