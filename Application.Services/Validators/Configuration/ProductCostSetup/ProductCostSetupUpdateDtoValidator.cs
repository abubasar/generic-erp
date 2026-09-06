using Application.Services.Dtos.Configuration.ProductCostSetup;
using FluentValidation;

namespace Application.Services.Validators.Configuration.ProductCostSetup
{
    public class ProductCostSetupUpdateDtoValidator : AbstractValidator<ProductCostSetupUpdateDto>
    {
        public ProductCostSetupUpdateDtoValidator()
        {
            Include(new ProductCostSetupCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
