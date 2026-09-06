namespace Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment
{
    public class ReceivePaymentDetailCreationDto
    {
        public Guid AccountId { get; set; }
        public string? AccountDescription { get; set; }
        public decimal Amount { get; set; }
    }
}
