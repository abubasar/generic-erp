using Application.Services.Dtos.Sale.SaleOrder;
using FluentValidation;

namespace Application.Services.Validators.Sale.SaleOrder
{
    public class SaleOrderDetailUpdateDtoValidator : AbstractValidator<SaleOrderDetailUpdateDto>
    {
        public SaleOrderDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new SaleOrderDetailCreationDtoValidator());
        }
    }
}
