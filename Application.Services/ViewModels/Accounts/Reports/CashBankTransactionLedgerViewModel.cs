namespace Application.Services.ViewModels.Accounts
{
    public class CashBankTransactionLedgerViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public Guid? ParentId { get; set; }
        public string? Name { get; set; }
        public decimal OpeningBalance{ get; set; }
        public decimal ThisPeriodReceipt { get; set; }
        public decimal ThisPeriodFundTransferReceipt { get; set; }
        public decimal ThisPeriodPayment { get; set; }
        public decimal ThisPeriodFundTransferPayment { get; set; }
        public decimal ThisPeriodFundTransferCharge { get; set; }
        public decimal ThisPeriodBalance { get; set; }
        public decimal Balance { get; set; }
    }
}
