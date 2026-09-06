using Application.Services.Dtos.Accounts.PaymentVoucher;
using FluentValidation;

namespace Application.Services.Validators.Accounts.PaymentVoucher
{
    public class PaymentVoucherCreationDtoValidator : AbstractValidator<PaymentVoucherCreationDto>
    {
        public PaymentVoucherCreationDtoValidator()
        {
            RuleFor(x => x.VoucherDate).NotNull().NotEmpty().WithMessage("VoucherDate can not be Empty");
        }
    }
}
