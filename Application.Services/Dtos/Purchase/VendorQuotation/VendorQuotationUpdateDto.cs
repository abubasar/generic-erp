namespace Application.Services.Dtos.Purchase.VendorQuotation
{
    public class VendorQuotationUpdateDto : VendorQuotationCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedVendorQuotationDetailIds { get; set; }
        public new ICollection<VendorQuotationDetailUpdateDto>? VendorQuotationDetails { get; set; }
    }
}
