using Application.Services.Dtos.Purchase.StockAdjustment;
using FluentValidation;

namespace Application.Services.Validators.Purchase.StockAdjustment
{
    public class StockAdjustmentDetailUpdateDtoValidator : AbstractValidator<StockAdjustmentDetailUpdateDto>
    {
        public StockAdjustmentDetailUpdateDtoValidator()
        {
            Include(new StockAdjustmentDetailCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
