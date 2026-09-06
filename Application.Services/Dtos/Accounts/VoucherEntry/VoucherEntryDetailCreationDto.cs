namespace Application.Services.Dtos.Accounts.VoucherEntry
{
    public class VoucherEntryDetailCreationDto
    {
        public Guid AccountId { get; set; }
        public string? AccountDescription { get; set; }
        public decimal Amount { get; set; }
    }
}
