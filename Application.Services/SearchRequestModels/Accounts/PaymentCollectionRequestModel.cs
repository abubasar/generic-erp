namespace Application.Services.SearchRequestModels.Accounts
{
    public class PaymentCollectionRequestModel
    {
        public int ReportType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CostCenterId { get; set; }
        public Guid? PaymentModeId { get; set; }
        public Guid? CustomerId { get; set; }
        public int? AccountTransactionType { get; set; }
        public Guid? CustomerZoneId { get; set; }
        public Guid? CustomerMarketingOfficerId { get; set; }
    }
}
