namespace Application.Services.SearchRequestModels.Report
{
    public class StockLedgerRequestModel
    {
        /* ReportType is being used to indentify Pdf or excel in report design*/
        public int ReportType { get; set; }
        public bool IsProductWiseReport { get; set; }
        public bool IsDepotWiseReport { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? ProductTypeId { get; set; }
        public Guid? InventoryTypeId { get; set; }

    }
}
