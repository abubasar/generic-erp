using Application.Services.Dtos.Sale.DeliveryNote;
using FluentValidation;

namespace Application.Services.Validators.Sale.DeliveryNote
{
    public class DeliveryNoteDetailUpdateDtoValidator : AbstractValidator<DeliveryNoteDetailUpdateDto>
    {
        public DeliveryNoteDetailUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new DeliveryNoteDetailCreationDtoValidator());
        }
    }
}
