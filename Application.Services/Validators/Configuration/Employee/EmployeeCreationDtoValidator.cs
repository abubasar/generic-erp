using Application.Services.Dtos.Configuration.Employee;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Employee
{
    public class EmployeeCreationDtoValidator : AbstractValidator<EmployeeCreationDto>
    {
        public EmployeeCreationDtoValidator()
        {
            RuleFor(x => x.FirstName).NotNull().NotEmpty().WithMessage("First Name is Required");
            RuleFor(x => x.LastName).NotNull().NotEmpty().WithMessage("Last Name is Required");
            RuleFor(x => x.EmployeeIdNo).NotNull().NotEmpty().WithMessage("Employee ID Number is Required");
            RuleFor(x => x.ContactNo).NotNull().NotEmpty().WithMessage("Contact Number is Required");
            RuleFor(x => x.DateOfBirth).NotNull().NotEmpty().WithMessage("Date of Birth is Required");
            RuleFor(x => x.JoiningDate).NotNull().NotEmpty().WithMessage("Joining Date is Required");
            RuleFor(x => x.Gender).NotNull().NotEmpty().WithMessage("Gender is Required");
            RuleFor(x => x.DepartmentId).NotNull().NotEmpty().WithMessage("Department Id can not be Empty");
            RuleFor(x => x.DesignationId).NotNull().NotEmpty().WithMessage("Designation Id can not be Empty");
            RuleFor(x => x.JobLocationId).NotNull().NotEmpty().WithMessage("JobLocation Id can not be Empty");
        }
    }
}
