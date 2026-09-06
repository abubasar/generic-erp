using Application.Services.Dtos.Production.BillOfMaterial;
using FluentValidation;

namespace Application.Services.Validators.Production.BillOfMaterial
{
    public class BillOfMaterialDetailCreationDtoValidator : AbstractValidator<BillOfMaterialDetailCreationDto>
    {
        public BillOfMaterialDetailCreationDtoValidator()
        {
            RuleFor(x => x.RawMaterialId).NotNull().NotEmpty().WithMessage("RawMaterialId can not be Empty");
            RuleFor(x => x.Quantity).NotNull().NotEmpty().WithMessage("Quantity can not be Empty");
        }
    }
}
