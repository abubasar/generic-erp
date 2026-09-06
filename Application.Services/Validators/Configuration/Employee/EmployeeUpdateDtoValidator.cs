using Application.Services.Dtos.Configuration.Employee;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Employee
{
    public class EmployeeUpdateDtoValidator : AbstractValidator<EmployeeUpdateDto>
    {
        public EmployeeUpdateDtoValidator()
        {
            Include(new EmployeeCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
