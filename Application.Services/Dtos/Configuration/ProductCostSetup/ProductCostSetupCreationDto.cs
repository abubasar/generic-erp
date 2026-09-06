namespace Application.Services.Dtos.Configuration.ProductCostSetup
{
    public class ProductCostSetupCreationDto
    {
        public Guid ProductId { get; set; }
        public decimal DirectExpense { get; set; }
        public decimal FactoryOverhead { get; set; }
    }
}
