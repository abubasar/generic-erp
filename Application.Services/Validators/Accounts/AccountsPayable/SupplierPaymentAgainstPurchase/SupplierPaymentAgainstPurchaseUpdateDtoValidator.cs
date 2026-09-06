using Application.Services.Dtos.Accounts.AccountsPayable.SupplierPaymentAgainstPurchase;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountsPayable.SupplierPaymentAgainstPurchase
{
    public class SupplierPaymentAgainstPurchaseUpdateDtoValidator : AbstractValidator<SupplierPaymentAgainstPurchaseUpdateDto>
    {
        public SupplierPaymentAgainstPurchaseUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new SupplierPaymentAgainstPurchaseCreationDtoValidator());
        }
    }
}
