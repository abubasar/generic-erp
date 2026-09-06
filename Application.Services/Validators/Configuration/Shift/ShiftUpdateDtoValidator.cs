using Application.Services.Dtos.Configuration.Shift;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Shift
{
    public class ShiftUpdateDtoValidator : AbstractValidator<ShiftUpdateDto>
    {
        public ShiftUpdateDtoValidator()
        {
            Include(new ShiftCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
