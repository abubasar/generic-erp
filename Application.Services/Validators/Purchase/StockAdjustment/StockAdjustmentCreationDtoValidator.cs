using Application.Services.Dtos.Purchase.StockAdjustment;
using FluentValidation;

namespace Application.Services.Validators.Purchase.StockAdjustment
{
    public class StockAdjustmentCreationDtoValidator : AbstractValidator<StockAdjustmentCreationDto>
    {
        public StockAdjustmentCreationDtoValidator() 
        {
            RuleFor(x => x.AdjustmentDate).NotNull().NotEmpty().WithMessage("AdjustmentDate can not be Empty");
            RuleFor(x => x.StoreId).NotNull().NotEmpty().WithMessage("StoreId can not be Empty");
            RuleFor(x => x.TotalAdjustmentQty).NotNull().NotEmpty().WithMessage("TotalAdjustmentQty can not be Empty");
        }
    }
}
