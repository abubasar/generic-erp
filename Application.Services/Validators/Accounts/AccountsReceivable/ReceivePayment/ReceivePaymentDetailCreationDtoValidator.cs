using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountsReceivable.ReceivePayment
{
    public class ReceivePaymentDetailCreationDtoValidator : AbstractValidator<ReceivePaymentDetailCreationDto>
    {
        public ReceivePaymentDetailCreationDtoValidator()
        {
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount can not be Empty");
        }
    }
}
