using Application.Services.Dtos.Configuration.FundTransferTransactionType;
using FluentValidation;

namespace Application.Services.Validators.Configuration.FundTransferTransactionType
{
    public class FundTransferTransactionTypeUpdateDtoValidator : AbstractValidator<FundTransferTransactionTypeUpdateDto>
    {
        public FundTransferTransactionTypeUpdateDtoValidator()
        {
            Include(new FundTransferTransactionTypeCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
