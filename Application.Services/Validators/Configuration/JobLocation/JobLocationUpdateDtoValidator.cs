using Application.Services.Dtos.Configuration.JobLocation;
using FluentValidation;

namespace Application.Services.Validators.Configuration.JobLocation
{
    public class JobLocationUpdateDtoValidator : AbstractValidator<JobLocationUpdateDto>
    {
        public JobLocationUpdateDtoValidator()
        {
            Include(new JobLocationCreationDtoValidator());
            RuleFor(x => x.Id).NotNull().NotEmpty().WithMessage("Id can not be Empty");
        }
    }
}
