using Application.Services.Dtos.Production.ManufacturingOrder;
using FluentValidation;

namespace Application.Services.Validators.Production.ManufacturingOrder
{
    public class ManufacturingOrderCreationDtoValidator : AbstractValidator<ManufacturingOrderCreationDto>
    {
        public ManufacturingOrderCreationDtoValidator()
        {
            RuleFor(x => x.BomNo).NotNull().NotEmpty().WithMessage("BomNo can not be Empty");
            RuleFor(x => x.ScheduledDate).NotNull().NotEmpty().WithMessage("ScheduledDate can not be Empty");
            RuleFor(x => x.FormulationNo).NotNull().NotEmpty().WithMessage("FormulationNo can not be Empty");
            RuleFor(x => x.FinishedProductId).NotNull().NotEmpty().WithMessage("FinishedProductId can not be Empty");
            RuleFor(x => x.ProductionQuantity).NotNull().NotEmpty().WithMessage("ProductionQuantity can not be Empty");
            RuleFor(x => x.RawMaterialStoreId).NotNull().NotEmpty().WithMessage("RawMaterialStoreId can not be Empty");
            RuleFor(x => x.TotalRmused).NotNull().NotEmpty().WithMessage("TotalRmused can not be Empty");
            //RuleFor(x => x.TotalCost).NotNull().NotEmpty().WithMessage("TotalCost can not be Empty");
        }
    }
}
