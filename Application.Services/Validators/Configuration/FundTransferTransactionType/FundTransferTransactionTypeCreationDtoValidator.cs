using Application.Services.Dtos.Configuration.FundTransferTransactionType;
using FluentValidation;

namespace Application.Services.Validators.Configuration.FundTransferTransactionType
{
    public class FundTransferTransactionTypeCreationDtoValidator : AbstractValidator<FundTransferTransactionTypeCreationDto>
    {
        public FundTransferTransactionTypeCreationDtoValidator()
        {
            RuleFor(x => x.Name).NotNull().NotEmpty().WithMessage("FundTransferTransactionType Name is Required");
        }
    }
}
