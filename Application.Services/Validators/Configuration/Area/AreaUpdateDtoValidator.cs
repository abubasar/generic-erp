using Application.Services.Dtos.Configuration.Area;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Area
{
    public class AreaUpdateDtoValidator : AbstractValidator<AreaUpdateDto>
    {
        public AreaUpdateDtoValidator() 
        {
            Include(new AreaCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
