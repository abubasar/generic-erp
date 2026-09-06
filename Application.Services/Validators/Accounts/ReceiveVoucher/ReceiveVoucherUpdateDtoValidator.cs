using Application.Services.Dtos.Accounts.ReceiveVoucher;
using FluentValidation;

namespace Application.Services.Validators.Accounts.ReceiveVoucher
{
    public class ReceiveVoucherUpdateDtoValidator : AbstractValidator<ReceiveVoucherUpdateDto>
    {
        public ReceiveVoucherUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new ReceiveVoucherCreationDtoValidator());
        }
    }
}
