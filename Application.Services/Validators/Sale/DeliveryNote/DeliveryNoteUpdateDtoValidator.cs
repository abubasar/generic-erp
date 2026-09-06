using Application.Services.Dtos.Sale.DeliveryNote;
using FluentValidation;

namespace Application.Services.Validators.Sale.DeliveryNote
{
    public class DeliveryNoteUpdateDtoValidator : AbstractValidator<DeliveryNoteUpdateDto>
    {
        public DeliveryNoteUpdateDtoValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
            Include(new DeliveryNoteCreationDtoValidator());
        }
    }
}
