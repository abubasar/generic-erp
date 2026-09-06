namespace Application.Services.Dtos.Accounts.FundTransfer
{
    public class FundTransferCreationDto
    {
        public DateTime FundTransferDate { get; set; }
        public Guid FundTransferTransactionTypeId { get; set; }
        public Guid CostCenterId { get; set; }
        public Guid TransferFromAccountId { get; set; }
        public Guid TransferToAccountId { get; set; }
        public decimal Amount { get; set; }
        public decimal? Charges { get; set; }
        public string? Remark { get; set; }
    }
}
