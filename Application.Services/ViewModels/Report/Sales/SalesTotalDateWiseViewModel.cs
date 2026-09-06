namespace Application.Services.ViewModels.Report.Sales
{
    public class SalesTotalDateWiseViewModel
    {
        public DateTime Date { get; set; }
        public int? SaleQuantity { get; set; }
        public int? SaleReturnQuantity { get; set; }
        public decimal? SaleAmount { get; set; }
        public decimal? SaleCommission { get; set; }
        public decimal? TransportationCost { get; set; }
        public decimal? DepoCharge { get; set; }
        public decimal? SaleReturnValue { get; set; }
    }
}
