namespace Application.Services.Dtos.Configuration.DiscountProductWise
{
    public class DiscountProductWiseUpdateDto : DiscountProductWiseCreationDto
    {
        public Guid Id { get; set; }
        public string? DeletedDiscountProductWiseDetailIds { get; set; }
        public new ICollection<DiscountProductWiseDetailUpdateDto>? DiscountProductWiseDetails { get; set; }
    }
}
