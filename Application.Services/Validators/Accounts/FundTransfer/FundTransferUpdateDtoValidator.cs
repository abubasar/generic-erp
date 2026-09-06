using Application.Services.Dtos.Accounts.FundTransfer;
using FluentValidation;

namespace Application.Services.Validators.Accounts.FundTransfer
{
    public class FundTransferUpdateDtoValidator : AbstractValidator<FundTransferUpdateDto>
    {
        public FundTransferUpdateDtoValidator()
        {
            Include(new FundTransferCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
