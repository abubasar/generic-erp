using Application.Services.Dtos.Purchase.PurchaseOrder;
using FluentValidation;

namespace Application.Services.Validators.Purchase.PurchaseOrder
{
    public class PurchaseOrderUpdateDtoValidator : AbstractValidator<PurchaseOrderUpdateDto>
    {
        public PurchaseOrderUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
