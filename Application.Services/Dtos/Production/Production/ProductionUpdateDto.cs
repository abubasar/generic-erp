namespace Application.Services.Dtos.Production.Production
{
    public class ProductionUpdateDto : ProductionCreationDto
    {
        public Guid Id { get; set; }
        public string DeletedProductionDetailIds { get; set; } = string.Empty;
        public new ICollection<ProductionDetailUpdateDto>? ProductionDetails { get; set; }
    }
}
