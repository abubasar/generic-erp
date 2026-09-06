namespace Application.Services.ViewModels.Accounts
{
    public class TrialBalanceViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
        public decimal OpeningDebit { get; set; }
        public decimal OpeningCredit { get; set; }
        public decimal ThisPeriodDebit { get; set; }
        public decimal ThisPeriodCredit { get; set; }
        public decimal BalanceDebit { get; set; }
        public decimal BalanceCredit { get; set; }
    }
}
