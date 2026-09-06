using Application.Services.Dtos.Accounts.PaymentVoucher;
using FluentValidation;

namespace Application.Services.Validators.Accounts.PaymentVoucher
{
    public class PaymentVoucherUpdateDtoValidator : AbstractValidator<PaymentVoucherUpdateDto>
    {
        public PaymentVoucherUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new PaymentVoucherCreationDtoValidator());
        }
    }
}
