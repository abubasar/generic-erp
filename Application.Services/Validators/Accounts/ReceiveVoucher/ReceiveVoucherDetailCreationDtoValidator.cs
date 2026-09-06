using Application.Services.Dtos.Accounts.ReceiveVoucher;
using FluentValidation;

namespace Application.Services.Validators.Accounts.ReceiveVoucher
{
    public class ReceiveVoucherDetailCreationDtoValidator : AbstractValidator<ReceiveVoucherDetailCreationDto>
    {
        public ReceiveVoucherDetailCreationDtoValidator()
        {
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount can not be Empty");
        }
    }
}
