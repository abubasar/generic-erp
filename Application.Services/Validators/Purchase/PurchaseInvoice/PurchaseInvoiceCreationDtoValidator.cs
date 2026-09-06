using Application.Services.Dtos.Purchase.PurchaseInvoice;
using FluentValidation;

namespace Application.Services.Validators.Purchase.PurchaseInvoice
{
    public class PurchaseInvoiceCreationDtoValidator : AbstractValidator<PurchaseInvoiceCreationDto>
    {
        public PurchaseInvoiceCreationDtoValidator()
        {
            RuleFor(x => x.SupplierId).NotNull().NotEmpty().WithMessage("SupplierId can not be Empty");
        }
    }
}
