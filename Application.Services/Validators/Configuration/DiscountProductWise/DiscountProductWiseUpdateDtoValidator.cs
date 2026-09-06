using Application.Services.Dtos.Configuration.DiscountProductWise;
using FluentValidation;

namespace Application.Services.Validators.Configuration.DiscountProductWise
{
    public class DiscountProductWiseUpdateDtoValidator : AbstractValidator<DiscountProductWiseUpdateDto>
    {
        public DiscountProductWiseUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new DiscountProductWiseCreationDtoValidator());
        }
    }
}
