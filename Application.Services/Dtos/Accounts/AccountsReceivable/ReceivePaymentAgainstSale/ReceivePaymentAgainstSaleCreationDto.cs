namespace Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePaymentAgainstSale
{
    public class ReceivePaymentAgainstSaleCreationDto
    {
        public DateTime PaymentDate { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public decimal Amount { get; set; }
        public Guid CostCenterId { get; set; }
        public Guid ToAccountId { get; set; }
        public string? Remark { get; set; }
    }
}
