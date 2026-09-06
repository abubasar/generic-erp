using Application.Services.Dtos.Configuration.Designation;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Designation
{
    public class DesignationUpdateDtoValidator : AbstractValidator<DesignationUpdateDto>
    {
        public DesignationUpdateDtoValidator()
        {
            Include(new DesignationCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
