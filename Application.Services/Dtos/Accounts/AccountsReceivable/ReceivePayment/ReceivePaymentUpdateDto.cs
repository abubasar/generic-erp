namespace Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment
{
    public class ReceivePaymentUpdateDto : ReceivePaymentCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedReceivePaymentDetailIds { get; set; }
        public new ICollection<ReceivePaymentDetailUpdateDto>? ReceivePaymentDetails { get; set; }
    }
}
