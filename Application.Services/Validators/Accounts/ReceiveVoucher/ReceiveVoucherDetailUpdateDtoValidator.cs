using Application.Services.Dtos.Accounts.ReceiveVoucher;
using FluentValidation;

namespace Application.Services.Validators.Accounts.ReceiveVoucher
{
    public class ReceiveVoucherDetailUpdateDtoValidator : AbstractValidator<ReceiveVoucherDetailUpdateDto>
    {
        public ReceiveVoucherDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new ReceiveVoucherDetailCreationDtoValidator());
        }
    }
}
