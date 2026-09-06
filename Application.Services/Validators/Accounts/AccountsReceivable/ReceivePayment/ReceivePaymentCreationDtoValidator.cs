using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountsReceivable.ReceivePayment
{
    public class ReceivePaymentCreationDtoValidator : AbstractValidator<ReceivePaymentCreationDto>
    {
        public ReceivePaymentCreationDtoValidator()
        {
            RuleFor(x => x.PaymentDate).NotNull().NotEmpty().WithMessage("PaymentDate is Required");
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage("CustomerId is Required");
            RuleFor(x => x.CostCenterId).NotNull().NotEmpty().WithMessage("CostCenterId is Required");
        }
    }
}
