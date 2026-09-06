namespace Application.Services.SearchRequestModels.Accounts
{
    public class SubsidiaryLedgerRequestModel
    {
        /* ReportType is being used to indentify Pdf or excel in report design*/
        public int? ReportType { get; set; }
        public Guid AccountId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? CostCenterId { get; set; }
    }
}
