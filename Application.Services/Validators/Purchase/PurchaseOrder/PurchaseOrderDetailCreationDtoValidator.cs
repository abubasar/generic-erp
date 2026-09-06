using Application.Services.Dtos.Purchase.PurchaseOrder;
using FluentValidation;

namespace Application.Services.Validators.Purchase.PurchaseOrder
{
    public class PurchaseOrderDetailCreationDtoValidator : AbstractValidator<PurchaseOrderDetailCreationDto>
    {
        public PurchaseOrderDetailCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("Product Id can not be Empty");
            RuleFor(x => x.Quantity).NotNull().NotEmpty().WithMessage("Quantity is Required");
            RuleFor(x => x.Rate).NotNull().NotEmpty().WithMessage("Rate is Required");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount is Required");
        }
    }
}
