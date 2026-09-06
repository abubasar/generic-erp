namespace Application.Services.ViewModels.Report.Sales
{
    public class SalesChartModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int Day { get; set; }
        public Guid? ZoneId { get; set; }
        public decimal TotalSales { get; set; }
    }
}
