using Application.Services.Dtos.Production.Production;
using FluentValidation;

namespace Application.Services.Validators.Production.Productions
{
    public class ProductionCreationDtoValidator : AbstractValidator<ProductionCreationDto>
    {
        public ProductionCreationDtoValidator()
        {
            RuleFor(x => x.ManufacturingOrderNo).NotNull().NotEmpty().WithMessage("ManufacturingOrderNo can not be Empty");
            RuleFor(x => x.BomNo).NotNull().NotEmpty().WithMessage("BomNo can not be Empty");
            //RuleFor(x => x.ExtraDamageQuantity).NotNull().NotEmpty().WithMessage("ExtraDamageQuantity can not be Empty");
            RuleFor(x => x.FgstoreId).NotNull().NotEmpty().WithMessage("FgstoreId can not be Empty");
            RuleFor(x => x.TotalRmused).NotNull().NotEmpty().WithMessage("TotalRmused can not be Empty");
            RuleFor(x => x.ProductionDate).NotNull().NotEmpty().WithMessage("ProductionDate can not be Empty");
            RuleFor(x => x.FormulationNo).NotNull().NotEmpty().WithMessage("FormulationNo can not be Empty");
            RuleFor(x => x.BatchNo).NotNull().NotEmpty().WithMessage("BatchNo can not be Empty");
            RuleFor(x => x.FinishedProductId).NotNull().NotEmpty().WithMessage("FinishedProductId can not be Empty");
            RuleFor(x => x.ProductionQuantity).NotNull().NotEmpty().WithMessage("ProductionQuantity can not be Empty");
            RuleFor(x => x.ActualProductionQuantity).NotNull().NotEmpty().WithMessage("ActualProductionQuantity can not be Empty");
            //RuleFor(x => x.TotalCost).NotNull().NotEmpty().WithMessage("TotalCost can not be Empty");
        }
    }
}
