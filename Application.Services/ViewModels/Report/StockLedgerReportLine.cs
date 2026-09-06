namespace Application.Services.ViewModels.Report
{
    public class StockLedgerReportLine
    {
        public string? ItemName { get; set; }
        public string? StoreName { get; set; }
        public decimal OpeningQty { get; set; }
        public decimal OpeningValue { get; set; }
        public decimal InQty { get; set; }
        public decimal InValue { get; set; }
        public decimal OutQty { get; set; }
        public decimal OutValue { get; set; }
        public decimal ClosingQty { get; set; }
        public decimal ClosingValue { get; set; }
    }
}
