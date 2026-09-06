using Application.Services.Dtos.Configuration.CustomerWiseProductDiscount;
using FluentValidation;

namespace Application.Services.Validators.Configuration.CustomerWiseProductDiscount
{
    public class CustomerWiseProductDiscountDetailUpdateDtoValidator : AbstractValidator<CustomerWiseProductDiscountDetailUpdateDto>
    {
        public CustomerWiseProductDiscountDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new CustomerWiseProductDiscountDetailCreationDtoValidator());
        }
    }
}
