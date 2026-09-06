using Application.Services.Dtos.Configuration.DiscountProductWise;
using FluentValidation;

namespace Application.Services.Validators.Configuration.DiscountProductWise
{
    public class DiscountProductWiseDetailUpdateDtoValidator : AbstractValidator<DiscountProductWiseDetailUpdateDto>
    {
        public DiscountProductWiseDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new DiscountProductWiseDetailCreationDtoValidator());
        }
    }
}
