using Application.Services.Dtos.Accounts.AccountsReceivable.SaleInvoice;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountsReceivable.SaleInvoice
{
    public class SaleInvoiceDetailCreationDtoValidator : AbstractValidator<SaleInvoiceDetailCreationDto>
    {
        public SaleInvoiceDetailCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("ProductId can not be Empty");
            RuleFor(x => x.Quantity).NotNull().NotEmpty().WithMessage("Quantity is Required");
            RuleFor(x => x.Rate).NotNull().NotEmpty().WithMessage("Rate is Required");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount is Required");
        }
    }
}
