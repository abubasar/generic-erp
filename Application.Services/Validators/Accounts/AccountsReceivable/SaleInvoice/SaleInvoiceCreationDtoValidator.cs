using Application.Services.Dtos.Accounts.AccountsReceivable.SaleInvoice;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountsReceivable.SaleInvoice
{
    public class SaleInvoiceCreationDtoValidator : AbstractValidator<SaleInvoiceCreationDto>
    {
        public SaleInvoiceCreationDtoValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage("CustomerId can not be Empty");
        }
    }
}
