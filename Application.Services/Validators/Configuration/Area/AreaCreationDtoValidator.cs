using Application.Services.Dtos.Configuration.Area;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Area
{
    public class AreaCreationDtoValidator : AbstractValidator<AreaCreationDto>
    {
        public AreaCreationDtoValidator() 
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Area Name is Required");
            RuleFor(x => x.ZoneId).NotNull().NotEmpty().WithMessage("ZoneId is Required");
        }
    }
}
