using Application.Services.Dtos.Configuration.CostCenter;
using FluentValidation;

namespace Application.Services.Validators.Configuration.CostCenter
{
    public class CostCenterUpdateDtoValidator : AbstractValidator<CostCenterUpdateDto>
    {
        public CostCenterUpdateDtoValidator()
        {
            Include(new CostCenterCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
