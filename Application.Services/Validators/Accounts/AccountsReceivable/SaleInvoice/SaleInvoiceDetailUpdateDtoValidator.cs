using Application.Services.Dtos.Accounts.AccountsReceivable.SaleInvoice;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountsReceivable.SaleInvoice
{
    public class SaleInvoiceDetailUpdateDtoValidator : AbstractValidator<SaleInvoiceDetailUpdateDto>
    {
        public SaleInvoiceDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new SaleInvoiceDetailCreationDtoValidator());
        }
    }
}
