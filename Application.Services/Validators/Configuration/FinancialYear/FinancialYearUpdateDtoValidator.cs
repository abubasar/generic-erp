using Application.Services.Dtos.Configuration.FinancialYear;
using FluentValidation;

namespace Application.Services.Validators.Configuration.FinancialYear
{
    public class FinancialYearUpdateDtoValidator : AbstractValidator<FinancialYearUpdateDto>
    {
        public FinancialYearUpdateDtoValidator()
        {
            Include(new FinancialYearCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
