namespace Application.Services.Dtos.Accounts.ReceiveVoucher
{
    public class ReceiveVoucherUpdateDto : ReceiveVoucherCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedReceiveVoucherDetailIds { get; set; }
        public new ICollection<ReceiveVoucherDetailUpdateDto>? ReceiveVoucherDetails { get; set; }
    }
}
