using Application.Services.Dtos.Production.Production;
using FluentValidation;

namespace Application.Services.Validators.Production.Productions
{
    public class ProductionDetailUpdateDtoValidator : AbstractValidator<ProductionDetailUpdateDto>
    {
        public ProductionDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new ProductionDetailCreationDtoValidator());
        }
    }
}
