namespace Application.Services.ViewModels.Configuration
{
    public class DiscountProductWiseDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid DiscountProductWiseId { get; set; }
        public Guid ProductId { get; set; }
        public decimal DiscountAmountPerKg { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
