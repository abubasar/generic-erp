using Application.Services.Dtos.Configuration.CustomerWiseProductDiscount;
using FluentValidation;

namespace Application.Services.Validators.Configuration.CustomerWiseProductDiscount
{
    public class CustomerWiseProductDiscountUpdateDtoValidator : AbstractValidator<CustomerWiseProductDiscountUpdateDto>
    {
        public CustomerWiseProductDiscountUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new CustomerWiseProductDiscountCreationDtoValidator());
        }
    }
}
