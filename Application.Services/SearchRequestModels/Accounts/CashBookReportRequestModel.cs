namespace Application.Services.SearchRequestModels.Accounts
{
    public class CashBookReportRequestModel
    {
        public int ReportType { get; set; }
        public Guid CashBookAccountId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
