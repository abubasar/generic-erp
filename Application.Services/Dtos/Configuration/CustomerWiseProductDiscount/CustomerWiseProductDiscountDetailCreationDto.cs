namespace Application.Services.Dtos.Configuration.CustomerWiseProductDiscount
{
    public class CustomerWiseProductDiscountDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public decimal SalePrice { get; set; }
        public decimal InvoiceDiscount { get; set; }
        public decimal CashDiscount { get; set; }
        public decimal SpecialDiscount { get; set; }
        public decimal MonthlyDiscount { get; set; }
        public decimal YearlyDiscount { get; set; }
        public decimal TargetDiscount { get; set; }
    }
}
