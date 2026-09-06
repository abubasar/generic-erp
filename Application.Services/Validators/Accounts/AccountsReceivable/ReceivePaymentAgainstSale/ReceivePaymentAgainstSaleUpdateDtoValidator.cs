using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePaymentAgainstSale;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountsReceivable.ReceivePaymentAgainstSale
{
    public class ReceivePaymentAgainstSaleUpdateDtoValidator : AbstractValidator<ReceivePaymentAgainstSaleUpdateDto>
    {
        public ReceivePaymentAgainstSaleUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new ReceivePaymentAgainstSaleCreationDtoValidator());
        }
    }
}
