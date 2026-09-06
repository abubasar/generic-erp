using Application.Services.Dtos.Purchase.StockAdjustment;
using FluentValidation;

namespace Application.Services.Validators.Purchase.StockAdjustment
{
    public class StockAdjustmentUpdateDtoValidator : AbstractValidator<StockAdjustmentUpdateDto>
    {
        public StockAdjustmentUpdateDtoValidator() 
        {
            Include(new StockAdjustmentCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
