namespace Application.Services.Dtos.Accounts.JournalEntry
{
    public class JournalEntryUpdateDto : JournalEntryCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedJournalEntryDetailIds { get; set; }
        public new ICollection<JournalEntryDetailUpdateDto>? JournalEntryDetails { get; set; }
    }
}
