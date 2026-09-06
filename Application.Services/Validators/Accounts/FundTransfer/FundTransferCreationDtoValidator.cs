using Application.Services.Dtos.Accounts.FundTransfer;
using FluentValidation;

namespace Application.Services.Validators.Accounts.FundTransfer
{
    public class FundTransferCreationDtoValidator : AbstractValidator<FundTransferCreationDto>
    {
        public FundTransferCreationDtoValidator()
        {
            RuleFor(x => x.FundTransferDate).NotNull().NotEmpty().WithMessage("FundTransferDate can not be Empty");
            RuleFor(x => x.CostCenterId).NotNull().NotEmpty().WithMessage("CostCenterId can not be Empty");
            RuleFor(x => x.TransferFromAccountId).NotNull().NotEmpty().WithMessage("TransferFromAccountId can not be Empty");
            RuleFor(x => x.TransferToAccountId).NotNull().NotEmpty().WithMessage("TransferToAccountId can not be Empty");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount can not be Empty");
        }
    }
}
