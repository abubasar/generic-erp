using Application.Services.Dtos.Purchase.PurchaseInvoice;
using FluentValidation;

namespace Application.Services.Validators.Purchase.PurchaseInvoice
{
    public class PurchaseInvoiceUpdateDtoValidator : AbstractValidator<PurchaseInvoiceUpdateDto>
    {
        public PurchaseInvoiceUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
