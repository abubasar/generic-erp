namespace Application.Services.Dtos.Sale.SaleOrder
{
    public class SaleOrderDetailUpdateDto : SaleOrderDetailCreationDto
    {
        public Guid? Id { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
    }
}
