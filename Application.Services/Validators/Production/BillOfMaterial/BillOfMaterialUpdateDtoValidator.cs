using Application.Services.Dtos.Production.BillOfMaterial;
using FluentValidation;

namespace Application.Services.Validators.Production.BillOfMaterial
{
    public class BillOfMaterialUpdateDtoValidator : AbstractValidator<BillOfMaterialUpdateDto>
    {
        public BillOfMaterialUpdateDtoValidator()
        {
            Include(new BillOfMaterialCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
