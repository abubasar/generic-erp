using Application.Services.Dtos.Configuration.DeliveryPlace;
using FluentValidation;

namespace Application.Services.Validators.Configuration.DeliveryPlace
{
    public class DeliveryPlaceUpdateDtoValidator : AbstractValidator<DeliveryPlaceUpdateDto>
    {
        public DeliveryPlaceUpdateDtoValidator()
        {
            Include(new DeliveryPlaceCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
