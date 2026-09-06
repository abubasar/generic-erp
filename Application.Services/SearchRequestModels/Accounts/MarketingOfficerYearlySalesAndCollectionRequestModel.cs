namespace Application.Services.SearchRequestModels.Accounts
{
    public class MarketingOfficerYearlySalesAndCollectionRequestModel
    {
        public int? ReportType { get; set; }
        public int Year { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
    }
}
