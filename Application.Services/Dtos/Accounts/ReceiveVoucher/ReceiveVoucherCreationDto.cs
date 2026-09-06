namespace Application.Services.Dtos.Accounts.ReceiveVoucher
{
    public class ReceiveVoucherCreationDto
    {
        public DateTime VoucherDate { get; set; }
        public Guid? FundTransferTransactionTypeId { get; set; }
        public string? TransactionNumber { get; set; }
        public Guid CostCenterId { get; set; }
        public Guid PaymentModeId { get; set; }
        public string? ReferenceNo { get; set; }
        public Guid CashBankAccountId { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Remark { get; set; }

        public virtual ICollection<ReceiveVoucherDetailCreationDto>? ReceiveVoucherDetails { get; set; }
    }
}
