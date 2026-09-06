namespace Application.Services.Dtos.Purchase.GoodsReceiveNote
{
    public class GoodsReceiveNoteUpdateDto : GoodsReceiveNoteCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedGoodsReceiveNoteDetailIds { get; set; }
        public new ICollection<GoodsReceiveNoteDetailUpdateDto>? GoodsReceiveNoteDetails { get; set; }
    }
}
