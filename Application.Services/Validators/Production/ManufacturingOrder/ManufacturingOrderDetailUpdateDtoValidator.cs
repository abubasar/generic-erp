using Application.Services.Dtos.Production.ManufacturingOrder;
using FluentValidation;

namespace Application.Services.Validators.Production.ManufacturingOrder
{
    public class ManufacturingOrderDetailUpdateDtoValidator : AbstractValidator<ManufacturingOrderDetailUpdateDto>
    {
        public ManufacturingOrderDetailUpdateDtoValidator()
        {
            Include(new ManufacturingOrderDetailCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
