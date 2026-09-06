namespace Application.Services.ViewModels.Configuration
{
    public class CustomerWiseProductDiscountDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid CustomerWiseProductDiscountId { get; set; }
        public Guid ProductId { get; set; }
        public decimal SalePrice { get; set; }
        public decimal InvoiceDiscount { get; set; }
        public decimal CashDiscount { get; set; }
        public decimal SpecialDiscount { get; set; }
        public decimal MonthlyDiscount { get; set; }
        public decimal YearlyDiscount { get; set; }
        public decimal TargetDiscount { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
