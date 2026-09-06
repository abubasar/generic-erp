using Application.Services.Dtos.Accounts.PaymentVoucher;
using FluentValidation;

namespace Application.Services.Validators.Accounts.PaymentVoucher
{
    public class PaymentVoucherDetailCreationDtoValidator : AbstractValidator<PaymentVoucherDetailCreationDto>
    {
        public PaymentVoucherDetailCreationDtoValidator()
        {
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount can not be Empty");
        }
    }
}
