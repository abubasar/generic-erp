namespace Application.Services.ViewModels.Accounts
{
    public class RootAccountBalance
    {
        public decimal NonCurrentAsset { get; set; }
        public decimal CurrentAsset { get; set; }
        public decimal NonCurrentLiability { get; set; }
        public decimal CurrentLiability { get; set; }
        public decimal OwnersEquity { get; set; }
        public decimal OthersEquity { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cogs { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal OtherIncome { get; set; }
        public decimal NonOperatingExpenses { get; set; }
        public decimal CsrFund { get; set; }
        public decimal Tax { get; set; }
        public decimal PL { get; set; }
        //others
        public decimal DirectExpenses_Standard { get; set; }
        public decimal FactoryOverhead_Standard { get; set; }

    }
}
