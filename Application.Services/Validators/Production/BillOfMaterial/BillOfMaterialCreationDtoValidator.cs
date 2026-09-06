using Application.Services.Dtos.Production.BillOfMaterial;
using FluentValidation;

namespace Application.Services.Validators.Production.BillOfMaterial
{
    public class BillOfMaterialCreationDtoValidator : AbstractValidator<BillOfMaterialCreationDto>
    {
        public BillOfMaterialCreationDtoValidator()
        {
            RuleFor(x => x.FinishedProductId).NotNull().NotEmpty().WithMessage("FinishedProductId can not be Empty");
            RuleFor(x => x.FormulationNo).NotNull().NotEmpty().WithMessage("FormulationNo can not be Empty");
            RuleFor(x => x.DosageQuantity).NotNull().NotEmpty().WithMessage("Quantity can not be Empty");
        }
    }
}
