namespace Application.Services.ViewModels.Accounts
{
    public class CustomerTransactionLedgerViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public string? CustomerMarketingOfficerName { get; set; }
        public decimal OpeningDue { get; set; }
        public decimal ThisPeriodSaleQty { get; set; }
        public decimal ThisPeriodSaleValue { get; set; }
        public decimal ThisPeriodCollection { get; set; }
        public decimal ThisPeriodAdjustment { get; set; }
        public decimal ThisPeriodDue { get; set; }
        public decimal Due { get; set; }
        public decimal CreditLimit { get; set; }
    }
}
