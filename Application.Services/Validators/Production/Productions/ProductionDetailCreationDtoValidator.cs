using Application.Services.Dtos.Production.Production;
using FluentValidation;

namespace Application.Services.Validators.Production.Productions
{
    public class ProductionDetailCreationDtoValidator : AbstractValidator<ProductionDetailCreationDto>
    {
        public ProductionDetailCreationDtoValidator()
        {
            RuleFor(x => x.RawMaterialId).NotNull().NotEmpty().WithMessage("RawMaterialId can not be Empty");
            RuleFor(x => x.Quantity).NotNull().NotEmpty().WithMessage("Quantity can not be Empty");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount can not be Empty");
        }
    }
}
