namespace Application.Services.SearchRequestModels.Sale
{
    public class CustomerDiscountRequestModel
    {
        /* ReportType is being used to indentify Pdf or excel in report design*/
        public int? ReportType { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Year { get; set; }
        public int? Month { get; set; }
        public Guid? CustomerId { get; set; }
    }
}
