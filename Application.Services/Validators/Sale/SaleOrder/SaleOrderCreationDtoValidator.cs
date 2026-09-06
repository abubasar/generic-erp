using Application.Services.Dtos.Sale.SaleOrder;
using FluentValidation;

namespace Application.Services.Validators.Sale.SaleOrder
{
    public class SaleOrderCreationDtoValidator : AbstractValidator<SaleOrderCreationDto>
    {
        public SaleOrderCreationDtoValidator()
        {
            RuleFor(x => x.DeliveryDate).NotNull().NotEmpty().WithMessage("DeliveryDate is Required");
        }
    }
}
