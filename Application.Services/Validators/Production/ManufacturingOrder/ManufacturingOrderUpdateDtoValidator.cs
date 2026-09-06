using Application.Services.Dtos.Production.ManufacturingOrder;
using FluentValidation;

namespace Application.Services.Validators.Production.ManufacturingOrder
{
    public class ManufacturingOrderUpdateDtoValidator : AbstractValidator<ManufacturingOrderUpdateDto>
    {
        public ManufacturingOrderUpdateDtoValidator()
        {
            Include(new ManufacturingOrderCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
