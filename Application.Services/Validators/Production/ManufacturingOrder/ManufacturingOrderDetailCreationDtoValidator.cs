using Application.Services.Dtos.Production.ManufacturingOrder;
using FluentValidation;

namespace Application.Services.Validators.Production.ManufacturingOrder
{
    public class ManufacturingOrderDetailCreationDtoValidator : AbstractValidator<ManufacturingOrderDetailCreationDto>
    {
        public ManufacturingOrderDetailCreationDtoValidator()
        {
            RuleFor(x => x.RawMaterialId).NotNull().NotEmpty().WithMessage("RawMaterialId can not be Empty");
            RuleFor(x => x.Quantity).NotNull().NotEmpty().WithMessage("Quantity can not be Empty");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount can not be Empty");
        }
    }
}
