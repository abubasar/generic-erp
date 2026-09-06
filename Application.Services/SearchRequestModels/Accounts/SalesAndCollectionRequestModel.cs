namespace Application.Services.SearchRequestModels.Accounts
{
    public class SalesAndCollectionRequestModel
    {
        public int? ReportType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CustomerZoneId { get; set; }
        public Guid? CustomerRegionId { get; set; }
        public Guid? CustomerAreaId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
        public Guid? CustomerTerritoryId { get; set; }
        public Guid? CustomerId { get; set; }
    }
}
