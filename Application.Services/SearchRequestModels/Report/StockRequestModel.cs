namespace Application.Services.SearchRequestModels.Report
{
    public class StockRequestModel
    {
        /* ReportType is being used to indentify Pdf or excel in report design*/
        public int ReportType { get; set; }
        public Guid? InventoryTypeId { get; set; }
        public Guid? ProductTypeId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? StoreId { get; set; }
        public int Page { get; set; } = 0;
        public int RowsPerPage { get; set; } = 5;

    }
}
