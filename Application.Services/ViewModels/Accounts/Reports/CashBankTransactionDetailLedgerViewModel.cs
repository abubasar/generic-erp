namespace Application.Services.ViewModels.Accounts
{
    public class CashBankTransactionDetailLedgerViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public Guid? ParentId { get; set; }
        public string? Name { get; set; }
        public decimal ThisPeriodReceipt { get; set; }
        public decimal ThisPeriodPayment { get; set; }
        public decimal OpeningBalance { get; set; }
        public List<ReceiptDetail> ReceiptDetails { get; set; } = new();
        public List<PaymentDetail> PaymentDetails { get; set; } = new();
        public decimal Balance { get; set; }
    }
}

public class ReceiptDetail
{
    public Guid Id { get; set; }
    public string? Vnumber { get; set; }
    public DateTime Date { get; set; }
    public string? Particular { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentDetail
{
    public Guid Id { get; set; }
    public string? Vnumber { get; set; }
    public DateTime Date { get; set; }
    public string? Particular { get; set; }
    public decimal Amount { get; set; }
}
