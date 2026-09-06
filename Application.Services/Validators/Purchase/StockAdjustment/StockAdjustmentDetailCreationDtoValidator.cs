using Application.Services.Dtos.Purchase.StockAdjustment;
using FluentValidation;

namespace Application.Services.Validators.Purchase.StockAdjustment
{
    public class StockAdjustmentDetailCreationDtoValidator : AbstractValidator<StockAdjustmentDetailCreationDto>
    {
        public StockAdjustmentDetailCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("ProductId can not be Empty");
            RuleFor(x => x.AdjustmentQty).NotNull().NotEmpty().WithMessage("AdjustmentQty can not be Empty");
        }
    }
}
