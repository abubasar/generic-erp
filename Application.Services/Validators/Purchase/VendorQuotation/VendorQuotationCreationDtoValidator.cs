using Application.Services.Dtos.Purchase.VendorQuotation;
using FluentValidation;

namespace Application.Services.Validators.Purchase.VendorQuotation
{
    public class VendorQuotationCreationDtoValidator : AbstractValidator<VendorQuotationCreationDto>
    {
        public VendorQuotationCreationDtoValidator()
        {
            RuleFor(x => x.RequisitionNo).NotNull().NotEmpty().WithMessage("Requisition Number is Required");
        }
    }
}
