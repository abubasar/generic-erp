using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePaymentAgainstSale;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountsReceivable.ReceivePaymentAgainstSale
{
    public class ReceivePaymentAgainstSaleCreationDtoValidator : AbstractValidator<ReceivePaymentAgainstSaleCreationDto>
    {
        public ReceivePaymentAgainstSaleCreationDtoValidator()
        {
            RuleFor(x => x.PaymentDate).NotNull().NotEmpty().WithMessage("PaymentDate is Required");
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage("CustomerId is Required");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount is Required");
            RuleFor(x => x.CostCenterId).NotNull().NotEmpty().WithMessage("CostCenterId is Required");
            RuleFor(x => x.ToAccountId).NotNull().NotEmpty().WithMessage("ToAccountId is Required");
        }
    }
}
