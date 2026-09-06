namespace Application.Services.Dtos.Sale.SaleQuotation
{
    public class SaleQuotationUpdateDto : SaleQuotationCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedSaleQuotationDetailIds { get; set; }
        public new ICollection<SaleQuotationDetailUpdateDto>? SaleQuotationDetails { get; set; }
    }
}
