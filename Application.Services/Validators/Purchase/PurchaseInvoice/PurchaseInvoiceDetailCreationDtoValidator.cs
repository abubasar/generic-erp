using Application.Services.Dtos.Purchase.PurchaseInvoice;
using FluentValidation;

namespace Application.Services.Validators.Purchase.PurchaseInvoice
{
    public class PurchaseInvoiceDetailCreationDtoValidator : AbstractValidator<PurchaseInvoiceDetailCreationDto>
    {
        public PurchaseInvoiceDetailCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("Product Id can not be Empty");
            RuleFor(x => x.Grnquantity).NotNull().NotEmpty().WithMessage("GRN Quantity is Required");
            RuleFor(x => x.Rate).NotNull().NotEmpty().WithMessage("Rate is Required");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount is Required");
        }
    }
}
