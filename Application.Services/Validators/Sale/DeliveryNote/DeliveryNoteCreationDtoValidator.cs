using Application.Services.Dtos.Sale.DeliveryNote;
using FluentValidation;

namespace Application.Services.Validators.Sale.DeliveryNote
{
    public class DeliveryNoteCreationDtoValidator : AbstractValidator<DeliveryNoteCreationDto>
    {
        public DeliveryNoteCreationDtoValidator()
        {
            RuleFor(x => x.DeliveryDate).NotNull().NotEmpty().WithMessage("DeliveryDate is Required");
        }
    }
}
