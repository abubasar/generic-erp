namespace Application.Services.Dtos.Sale.SaleQuotation
{
    public class SaleQuotationDetailUpdateDto : SaleQuotationDetailCreationDto
    {
        public Guid? Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
    }
}
