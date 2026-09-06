using Application.Services.Dtos.Sale.SaleOrder;
using FluentValidation;

namespace Application.Services.Validators.Sale.SaleOrder
{
    public class SaleOrderUpdateDtoValidator : AbstractValidator<SaleOrderUpdateDto>
    {
        public SaleOrderUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new SaleOrderCreationDtoValidator());
        }
    }
}
