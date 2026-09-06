namespace Application.Services.Dtos.Configuration.DiscountProductWise
{
    public class DiscountProductWiseDetailCreationDto
    {
        public Guid ProductId { get; set; }
        public decimal DiscountAmountPerKg { get; set; }
    }
}
