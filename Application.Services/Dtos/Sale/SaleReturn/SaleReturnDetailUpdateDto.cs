namespace Application.Services.Dtos.Sale.SaleReturn
{
    public class SaleReturnDetailUpdateDto : SaleReturnDetailCreationDto
    {
        public Guid? Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
    }
}
