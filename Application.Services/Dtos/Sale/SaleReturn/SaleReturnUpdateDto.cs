namespace Application.Services.Dtos.Sale.SaleReturn
{
    public class SaleReturnUpdateDto : SaleReturnCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedSaleReturnDetailIds { get; set; }
        public new ICollection<SaleReturnDetailUpdateDto>? SaleReturnDetails { get; set; }
    }
}
