using Application.Services.Dtos.Configuration.Zone;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Zone
{
    public class ZoneCreationDtoValidator : AbstractValidator<ZoneCreationDto>
    {
        public ZoneCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Zone Name is Required");
            RuleFor(x => x.RegionId).NotNull().NotEmpty().WithMessage("RegionId is Required");
        }
    }
}
