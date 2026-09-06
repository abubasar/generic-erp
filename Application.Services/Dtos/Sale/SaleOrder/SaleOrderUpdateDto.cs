namespace Application.Services.Dtos.Sale.SaleOrder
{
    public class SaleOrderUpdateDto : SaleOrderCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedSaleOrderDetailIds { get; set; }
        public new ICollection<SaleOrderDetailUpdateDto>? SaleOrderDetails { get; set; }
    }
}
