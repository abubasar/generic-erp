using Application.Services.Dtos.Configuration.CostCenter;
using FluentValidation;

namespace Application.Services.Validators.Configuration.CostCenter
{
    public class CostCenterCreationDtoValidator : AbstractValidator<CostCenterCreationDto>
    {
        public CostCenterCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("Cost Center Name is Required");
        }
    }
}
