using Microsoft.AspNetCore.Http;

namespace Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment
{
    public class ReceivePaymentCreationDto
    {
        public DateTime PaymentDate { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? FundTransferTransactionTypeId { get; set; }
        public string? TransactionNumber { get; set; }
        public Guid PaymentModeId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal FeedSalesPurpose { get; set; }
        public decimal CreditRecoveryPurpose { get; set; }
        public Guid CostCenterId { get; set; }
        public string? Remark { get; set; }
        public IFormFile? FileDetails { get; set; }
        public virtual ICollection<ReceivePaymentDetailCreationDto>? ReceivePaymentDetails { get; set; }
    }
}
