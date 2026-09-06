using Application.Services.Dtos.Purchase.PurchaseInvoice;
using FluentValidation;

namespace Application.Services.Validators.Purchase.PurchaseInvoice
{
    public class PurchaseInvoiceDetailUpdateDtoValidator : AbstractValidator<PurchaseInvoiceDetailUpdateDto>
    {
        public PurchaseInvoiceDetailUpdateDtoValidator()
        {
            Include(new PurchaseInvoiceDetailCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
