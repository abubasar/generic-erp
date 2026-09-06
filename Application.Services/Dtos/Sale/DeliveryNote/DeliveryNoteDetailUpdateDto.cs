namespace Application.Services.Dtos.Sale.DeliveryNote
{
    public class DeliveryNoteDetailUpdateDto : DeliveryNoteDetailCreationDto
    {
        public Guid? Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string? CreatedBy { get; set; }
    }
}
