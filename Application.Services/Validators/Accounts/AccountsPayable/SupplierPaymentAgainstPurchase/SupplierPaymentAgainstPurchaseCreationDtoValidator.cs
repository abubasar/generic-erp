using Application.Services.Dtos.Accounts.AccountsPayable.SupplierPaymentAgainstPurchase;
using FluentValidation;

namespace Application.Services.Validators.Accounts.AccountsPayable.SupplierPaymentAgainstPurchase
{
    public class SupplierPaymentAgainstPurchaseCreationDtoValidator : AbstractValidator<SupplierPaymentAgainstPurchaseCreationDto>
    {
        public SupplierPaymentAgainstPurchaseCreationDtoValidator()
        {
            RuleFor(x => x.PaymentDate).NotNull().NotEmpty().WithMessage("PaymentDate is Required");
            RuleFor(x => x.PurchaseInvoiceId).NotNull().NotEmpty().WithMessage("PurchaseInvoiceId is Required");
            RuleFor(x => x.SupplierId).NotNull().NotEmpty().WithMessage("SupplierId is Required");
            RuleFor(x => x.Amount).NotNull().NotEmpty().WithMessage("Amount is Required");
            RuleFor(x => x.CostCenterId).NotNull().NotEmpty().WithMessage("CostCenterId is Required");
            RuleFor(x => x.FromAccountId).NotNull().NotEmpty().WithMessage("FromAccountId is Required");
        }
    }
}
