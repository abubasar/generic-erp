namespace Application.Services.Dtos.Accounts.JournalEntry
{
    public class JournalEntryCreationDto
    {
        public DateTime VoucherDate { get; set; }
        public Guid CostCenterId { get; set; }
        public string? Remark { get; set; }
        public virtual ICollection<JournalEntryDetailCreationDto>? JournalEntryDetails { get; set; }
    }
}
