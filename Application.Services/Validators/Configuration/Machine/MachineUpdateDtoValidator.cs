using Application.Services.Dtos.Configuration.Machine;
using FluentValidation;

namespace Application.Services.Validators.Configuration.Machine
{
    public class MachineUpdateDtoValidator : AbstractValidator<MachineUpdateDto>
    {
        public MachineUpdateDtoValidator() 
        {
            Include(new MachineCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
