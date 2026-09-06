using Application.Services.Dtos.Sale.SaleOrder;
using FluentValidation;

namespace Application.Services.Validators.Sale.SaleOrder
{
    public class SaleOrderDetailCreationDtoValidator : AbstractValidator<SaleOrderDetailCreationDto>
    {
        public SaleOrderDetailCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("Product Id can not be Empty");
            RuleFor(x => x.BagWeight).NotNull().NotEmpty().WithMessage("Bag Size is Required");
            RuleFor(x => x.PrimaryQuantity).NotNull().NotEmpty().WithMessage("Primary Quantity is Required");
            RuleFor(x => x.Quantity).NotNull().NotEmpty().WithMessage("Quantity is Required");
            RuleFor(x => x.Rate).NotNull().NotEmpty().WithMessage("Rate is Required");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount is Required");
        }
    }
}
