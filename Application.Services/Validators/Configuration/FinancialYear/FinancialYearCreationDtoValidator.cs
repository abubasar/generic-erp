using Application.Services.Dtos.Configuration.FinancialYear;
using FluentValidation;

namespace Application.Services.Validators.Configuration.FinancialYear
{
    public class FinancialYearCreationDtoValidator : AbstractValidator<FinancialYearCreationDto>
    {
        public FinancialYearCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Name is Required");
            RuleFor(x => x.StartDate).NotNull().NotEmpty().WithMessage("StartDate is Required");
            RuleFor(x => x.EndDate).NotNull().NotEmpty().WithMessage("EndDate Name is Required");
        }
    }
}
