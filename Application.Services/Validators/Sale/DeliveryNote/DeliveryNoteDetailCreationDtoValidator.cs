using Application.Services.Dtos.Sale.DeliveryNote;
using FluentValidation;

namespace Application.Services.Validators.Sale.DeliveryNote
{
    public class DeliveryNoteDetailCreationDtoValidator : AbstractValidator<DeliveryNoteDetailCreationDto>
    {
        public DeliveryNoteDetailCreationDtoValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty().WithMessage("Product Id can not be Empty");
        }
    }
}
