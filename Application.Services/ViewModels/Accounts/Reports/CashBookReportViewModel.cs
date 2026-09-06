namespace Application.Services.ViewModels.Accounts
{
    public class CashBookReportViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public Guid? ParentId { get; set; }
        public string? Name { get; set; }
        public decimal ThisPeriodReceipt { get; set; }
        public decimal ThisPeriodPayment { get; set; }
        public decimal OpeningBalance { get; set; }
        public decimal ThisPeriodOB_TRDM { get; set; }
        public decimal Balance { get; set; }
        public List<CombinedReceiptAndPayment> CombinedReceiptsAndPayments { get; set; } = new();
        //.......
    }
}

public class CombinedReceiptAndPayment
{
    public string? Vnumber1 { get; set; }
    public string? Date1 { get; set; }
    public string? CostCenterName1 { get; set; }
    public string? HeadOfAccountName1 { get; set; }
    public string? Particular1 { get; set; }
    public string? Amount1 { get; set; }
    public string? Vnumber2 { get; set; }
    public string? Date2 { get; set; }
    public string? CostCenterName2 { get; set; }
    public string? HeadOfAccountName2 { get; set; }
    public string? Particular2 { get; set; }
    public string? Amount2 { get; set; }
}

public class Receipt
{
    public Guid Id1 { get; set; }
    public string? Vnumber1 { get; set; }
    public string? Date1 { get; set; }
    public string? CostCenterName1 { get; set; }
    public string? HeadOfAccountName1 { get; set; }
    public string? Particular1 { get; set; }
    public decimal Amount1 { get; set; }
}

public class Payment
{
    public Guid Id2 { get; set; }
    public string? Vnumber2 { get; set; }
    public string? Date2 { get; set; }
    public string? CostCenterName2 { get; set; }
    public string? HeadOfAccountName2 { get; set; }
    public string? Particular2 { get; set; }
    public decimal Amount2 { get; set; }
}
