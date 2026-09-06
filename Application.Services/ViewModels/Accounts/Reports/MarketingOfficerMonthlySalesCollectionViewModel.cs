namespace Application.Services.ViewModels.Accounts.Reports
{
    public class MarketingOfficerMonthlySalesCollectionViewModel
    {
        public decimal ThisPeriodSaleValue { get; set; }
        public decimal ThisPeriodSaleReturnValue { get; set; }
        public decimal ThisPeriodCollection { get; set; }
        public decimal ThisPeriodAdjustment { get; set; }
        public decimal ThisPeriodBalance { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string? MonthName { get; set; }
    }
}
