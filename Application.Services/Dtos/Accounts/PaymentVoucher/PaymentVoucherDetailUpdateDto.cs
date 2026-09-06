namespace Application.Services.Dtos.Accounts.PaymentVoucher
{
    public class PaymentVoucherDetailUpdateDto : PaymentVoucherDetailCreationDto
    {
        public Guid? Id { get; set; }
    }
}
