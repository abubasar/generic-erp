namespace Application.Services.ViewModels.Sale
{
    public class CustomerDiscountViewModel
    {
        public Guid CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
        public Guid ProductId { get; set; }
        public int TotalQuantity { get; set; }
        public decimal MonthlyDiscount { get; set; }
        public decimal YearlyDiscount { get; set; }
        public decimal TargetDiscount { get; set; }
    }
}
