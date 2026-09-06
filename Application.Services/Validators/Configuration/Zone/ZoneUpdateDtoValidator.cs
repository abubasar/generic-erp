using Application.Services.Dtos.Configuration.Zone;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Zone
{
    public class ZoneUpdateDtoValidator : AbstractValidator<ZoneUpdateDto>
    {
        public ZoneUpdateDtoValidator()
        {
            Include(new ZoneCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
