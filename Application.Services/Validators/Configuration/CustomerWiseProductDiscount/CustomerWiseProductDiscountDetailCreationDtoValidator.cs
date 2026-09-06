using Application.Services.Dtos.Configuration.CustomerWiseProductDiscount;
using FluentValidation;

namespace Application.Services.Validators.Configuration.CustomerWiseProductDiscount
{
    public class CustomerWiseProductDiscountDetailCreationDtoValidator : AbstractValidator<CustomerWiseProductDiscountDetailCreationDto>
    {
        public CustomerWiseProductDiscountDetailCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("ProductId can not be Empty");
        }
    }
}
