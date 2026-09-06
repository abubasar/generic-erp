using Application.Services.Dtos.Configuration.ProductCostSetup;
using FluentValidation;

namespace Application.Services.Validators.Configuration.ProductCostSetup
{
    public class ProductCostSetupCreationDtoValidator : AbstractValidator<ProductCostSetupCreationDto>
    {
        public ProductCostSetupCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("ProductId Name is Required");
        }
    }
}
