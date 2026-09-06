namespace Application.Services.ViewModels.Accounts
{
    public class SupplierTransactionLedgerViewModel
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public decimal OpeningDue { get; set; }
        public decimal ThisPeriodPurchaseQty { get; set; }
        public decimal ThisPeriodPurchaseValue { get; set; }
        public decimal ThisPeriodPayment { get; set; }
        public decimal ThisPeriodAdjustment { get; set; }
        public decimal Due { get; set; }
        public decimal Advance { get; set; }
    }
}
