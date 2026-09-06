namespace Application.Services.Dtos.Sale.DeliveryNote
{
    public class DeliveryNoteUpdateDto : DeliveryNoteCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedDeliveryNoteDetailIds { get; set; }
        public new ICollection<DeliveryNoteDetailUpdateDto>? DeliveryNoteDetails { get; set; }
    }
}
