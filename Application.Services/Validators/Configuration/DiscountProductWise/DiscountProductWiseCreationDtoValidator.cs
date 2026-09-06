using Application.Services.Dtos.Configuration.DiscountProductWise;
using FluentValidation;

namespace Application.Services.Validators.Configuration.DiscountProductWise
{
    public class DiscountProductWiseCreationDtoValidator : AbstractValidator<DiscountProductWiseCreationDto>
    {
        public DiscountProductWiseCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Name is Required");
            RuleFor(x => x.StartDate).NotNull().NotEmpty().WithMessage("StartDate is Required");
            RuleFor(x => x.EndDate).NotNull().NotEmpty().WithMessage("EndDate is Required");
        }
    }
}
