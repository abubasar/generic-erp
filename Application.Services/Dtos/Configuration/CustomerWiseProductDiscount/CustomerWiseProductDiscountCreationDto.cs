namespace Application.Services.Dtos.Configuration.CustomerWiseProductDiscount
{
    public class CustomerWiseProductDiscountCreationDto
    {
        public Guid CustomerId { get; set; }
        public DateTime ApplicableDate { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<CustomerWiseProductDiscountDetailCreationDto>? CustomerWiseProductDiscountDetails { get; set; }
    }
}
