namespace Application.Services.Dtos.Accounts.VoucherEntry
{
    public class VoucherEntryUpdateDto : VoucherEntryCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedVoucherEntryDetailIds { get; set; }
        public new ICollection<VoucherEntryDetailUpdateDto>? VoucherEntryDetails { get; set; }
    }
}
