using Application.Services.Dtos.Configuration.DiscountProductWise;
using FluentValidation;

namespace Application.Services.Validators.Configuration.DiscountProductWise
{
    public class DiscountProductWiseDetailCreationDtoValidator : AbstractValidator<DiscountProductWiseDetailCreationDto>
    {
        public DiscountProductWiseDetailCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("ProductId can not be Empty");
        }
    }
}
