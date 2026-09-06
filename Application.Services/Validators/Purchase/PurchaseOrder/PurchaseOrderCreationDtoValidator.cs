using Application.Services.Dtos.Purchase.PurchaseOrder;
using FluentValidation;

namespace Application.Services.Validators.Purchase.PurchaseOrder
{
    public class PurchaseOrderCreationDtoValidator : AbstractValidator<PurchaseOrderCreationDto>
    {
        public PurchaseOrderCreationDtoValidator()
        {
            RuleFor(x => x.SupplierId).NotNull().NotEmpty().WithMessage("SupplierId can not be Empty");
        }
    }
}
