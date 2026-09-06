using Application.Services.Dtos.Accounts.AccountsReceivable.SaleInvoice;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountsReceivable.SaleInvoice
{
    public class SaleInvoiceUpdateDtoValidator : AbstractValidator<SaleInvoiceUpdateDto>
    {
        public SaleInvoiceUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new SaleInvoiceCreationDtoValidator());
        }
    }
}
