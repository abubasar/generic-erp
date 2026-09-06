using Application.Services.Dtos.Production.Production;
using FluentValidation;

namespace Application.Services.Validators.Production.Productions
{
    public class ProductionUpdateDtoValidator : AbstractValidator<ProductionUpdateDto>
    {
        public ProductionUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new ProductionCreationDtoValidator());
        }
    }
}
