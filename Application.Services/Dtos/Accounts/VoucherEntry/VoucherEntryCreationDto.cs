namespace Application.Services.Dtos.Accounts.VoucherEntry
{
    public class VoucherEntryCreationDto
    {
        public DateTime VoucherDate { get; set; }
        public Guid CostCenterId { get; set; }
        public Guid PaymentModeId { get; set; }
        public string? ReferenceNo { get; set; }
        public int VoucherType { get; set; }
        public Guid CashBankAccountId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remark { get; set; }

        public virtual ICollection<VoucherEntryDetailCreationDto>? VoucherEntryDetails { get; set; }
    }
}
