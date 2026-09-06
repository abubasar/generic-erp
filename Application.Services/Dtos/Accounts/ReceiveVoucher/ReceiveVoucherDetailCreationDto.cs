namespace Application.Services.Dtos.Accounts.ReceiveVoucher
{
    public class ReceiveVoucherDetailCreationDto
    {
        public Guid AccountId { get; set; }
        public string? AccountDescription { get; set; }
        public decimal Amount { get; set; }
    }
}
