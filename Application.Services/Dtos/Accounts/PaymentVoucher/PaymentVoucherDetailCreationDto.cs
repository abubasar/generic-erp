namespace Application.Services.Dtos.Accounts.PaymentVoucher
{
    public class PaymentVoucherDetailCreationDto
    {
        public Guid AccountId { get; set; }
        public string? AccountDescription { get; set; }
        public decimal Amount { get; set; }
    }
}
