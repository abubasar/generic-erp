namespace Application.Services.SearchRequestModels.Accounts
{
    public class AccountReportRequestModel
    {
        public int ReportType { get; set; }
        public Guid? RootAccountHeadId { get; set; }
        public int Level { get; set; } = 0;
        public int LevelsToLoad { get; set; } = 10;
        public Guid FinancialYearId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
