using Application.Services.Dtos.Configuration.Region;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Region
{
    public class RegionCreationDtoValidator : AbstractValidator<RegionCreationDto>
    {
        public RegionCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Region Name is Required");
        }
    }
}
