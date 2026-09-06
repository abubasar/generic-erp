using Application.Services.Dtos.Configuration.CustomerWiseProductDiscount;
using FluentValidation;

namespace Application.Services.Validators.Configuration.CustomerWiseProductDiscount
{
    public class CustomerWiseProductDiscountCreationDtoValidator : AbstractValidator<CustomerWiseProductDiscountCreationDto>
    {
        public CustomerWiseProductDiscountCreationDtoValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage("CustomerId is Required");
        }
    }
}
