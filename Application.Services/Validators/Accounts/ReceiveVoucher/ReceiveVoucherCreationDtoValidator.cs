using Application.Services.Dtos.Accounts.ReceiveVoucher;
using FluentValidation;

namespace Application.Services.Validators.Accounts.ReceiveVoucher
{
    public class ReceiveVoucherCreationDtoValidator : AbstractValidator<ReceiveVoucherCreationDto>
    {
        public ReceiveVoucherCreationDtoValidator()
        {
            RuleFor(x => x.VoucherDate).NotNull().NotEmpty().WithMessage("VoucherDate can not be Empty");
        }
    }
}
