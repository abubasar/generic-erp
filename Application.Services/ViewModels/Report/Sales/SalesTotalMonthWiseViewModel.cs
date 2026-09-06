namespace Application.Services.ViewModels.Report.Sales
{
    public class SalesTotalMonthWiseViewModel
    {
        public int? Year { get; set; }
        public int? Month { get; set; }
        public int? SaleQuantity { get; set; }
        public int? SaleReturnQuantity { get; set; }
        public decimal? SaleAmount { get; set; }
        public decimal? SaleCommission { get; set; }
        public decimal? TransportationCost { get; set; }
        public decimal? DepoCharge { get; set; }
        public decimal? SaleReturnValue { get; set; }
    }
}
