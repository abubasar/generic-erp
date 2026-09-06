using Application.Services.Dtos.Purchase.PurchaseOrder;
using FluentValidation;

namespace Application.Services.Validators.Purchase.PurchaseOrder
{
    public class PurchaseOrderDetailUpdateDtoValidator : AbstractValidator<PurchaseOrderDetailUpdateDto>
    {
        public PurchaseOrderDetailUpdateDtoValidator()
        {
            Include(new PurchaseOrderDetailCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
