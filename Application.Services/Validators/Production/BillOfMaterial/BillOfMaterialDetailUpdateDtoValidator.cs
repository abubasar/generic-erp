using Application.Services.Dtos.Production.BillOfMaterial;
using FluentValidation;

namespace Application.Services.Validators.Production.BillOfMaterial
{
    public class BillOfMaterialDetailUpdateDtoValidator : AbstractValidator<BillOfMaterialDetailUpdateDto>
    {
        public BillOfMaterialDetailUpdateDtoValidator()
        {
            Include(new BillOfMaterialDetailCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
