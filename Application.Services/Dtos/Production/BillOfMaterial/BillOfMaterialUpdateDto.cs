namespace Application.Services.Dtos.Production.BillOfMaterial
{
    public class BillOfMaterialUpdateDto : BillOfMaterialCreationDto
    {
        public Guid Id { get; set; }
        public string DeletedBillOfMaterialDetailIds { get; set; } = string.Empty;
        public new ICollection<BillOfMaterialDetailUpdateDto>? BillOfMaterialDetails { get; set; }
    }
}
