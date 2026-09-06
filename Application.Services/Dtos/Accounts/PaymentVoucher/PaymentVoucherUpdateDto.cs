namespace Application.Services.Dtos.Accounts.PaymentVoucher
{
    public class PaymentVoucherUpdateDto : PaymentVoucherCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedPaymentVoucherDetailIds { get; set; }
        public new ICollection<PaymentVoucherDetailUpdateDto>? PaymentVoucherDetails { get; set; }
    }
}
