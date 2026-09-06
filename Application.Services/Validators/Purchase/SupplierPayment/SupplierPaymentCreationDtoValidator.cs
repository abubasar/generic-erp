using Application.Services.Dtos.Purchase.SupplierPayment;
using FluentValidation;

namespace Application.Services.Validators.Purchase.SupplierPayment
{
    public class SupplierPaymentCreationDtoValidator : AbstractValidator<SupplierPaymentCreationDto>
    {
        public SupplierPaymentCreationDtoValidator()
        {
            RuleFor(x => x.PaymentDate).NotNull().NotEmpty().WithMessage("PaymentDate is Required");
            RuleFor(x => x.SupplierId).NotNull().NotEmpty().WithMessage("SupplierId is Required");
            RuleFor(x => x.CostCenterId).NotNull().NotEmpty().WithMessage("CostCenterId is Required");
        }
    }
}
