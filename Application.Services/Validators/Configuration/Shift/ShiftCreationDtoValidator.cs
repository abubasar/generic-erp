using Application.Services.Dtos.Configuration.Shift;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Shift
{
    public class ShiftCreationDtoValidator : AbstractValidator<ShiftCreationDto>
    {
        public ShiftCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Shift Name is Required");
            RuleFor(x => x.FromTime).NotNull().NotEmpty().WithMessage("FromTime is Required");
            RuleFor(x => x.ToTime).NotNull().NotEmpty().WithMessage("ToTime is Required");
        }
    }
}
