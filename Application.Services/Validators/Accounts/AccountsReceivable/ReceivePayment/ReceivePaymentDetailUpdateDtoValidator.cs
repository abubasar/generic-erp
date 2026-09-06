using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountsReceivable.ReceivePayment
{
    public class ReceivePaymentDetailUpdateDtoValidator : AbstractValidator<ReceivePaymentDetailUpdateDto>
    {
        public ReceivePaymentDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new ReceivePaymentDetailCreationDtoValidator());
        }
    }
}
