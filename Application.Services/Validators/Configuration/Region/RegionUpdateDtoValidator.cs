using Application.Services.Dtos.Configuration.Region;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Region
{
    public class RegionUpdateDtoValidator : AbstractValidator<RegionUpdateDto>
    {
        public RegionUpdateDtoValidator()
        {
            Include(new RegionCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
