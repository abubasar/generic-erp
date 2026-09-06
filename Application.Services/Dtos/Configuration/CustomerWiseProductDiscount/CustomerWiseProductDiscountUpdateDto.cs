namespace Application.Services.Dtos.Configuration.CustomerWiseProductDiscount
{
    public class CustomerWiseProductDiscountUpdateDto : CustomerWiseProductDiscountCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedCustomerWiseProductDiscountDetailIds { get; set; }
        public new ICollection<CustomerWiseProductDiscountDetailUpdateDto>? CustomerWiseProductDiscountDetails { get; set; }
    }
}
