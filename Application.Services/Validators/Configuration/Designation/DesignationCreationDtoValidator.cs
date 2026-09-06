using Application.Services.Dtos.Configuration.Designation;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Designation
{
    public class DesignationCreationDtoValidator : AbstractValidator<DesignationCreationDto>
    {
        public DesignationCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Designation Name is Required");
            RuleFor(x => x.DepartmentId).NotNull().NotEmpty().WithMessage("Department Id can not be Empty");
        }
    }
}
