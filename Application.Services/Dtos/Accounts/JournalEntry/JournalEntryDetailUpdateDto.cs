namespace Application.Services.Dtos.Accounts.JournalEntry
{
    public class JournalEntryDetailUpdateDto : JournalEntryDetailCreationDto
    {
        public Guid? Id { get; set; }
    }
}
