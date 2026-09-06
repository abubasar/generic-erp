using Application.Services.Dtos.Accounts.PaymentVoucher;
using FluentValidation;

namespace Application.Services.Validators.Accounts.PaymentVoucher
{
    public class PaymentVoucherDetailUpdateDtoValidator : AbstractValidator<PaymentVoucherDetailUpdateDto>
    {
        public PaymentVoucherDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new PaymentVoucherDetailCreationDtoValidator());
        }
    }
}
