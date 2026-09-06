namespace Application.Services.ViewModels.Configuration
{
    public class ProductCostSetupViewModel
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public decimal DirectExpense { get; set; }
        public decimal FactoryOverhead { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ProductViewModel? Product { get; set; }
    }
}
