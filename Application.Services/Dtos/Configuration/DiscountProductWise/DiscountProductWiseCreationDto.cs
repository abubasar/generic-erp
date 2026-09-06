namespace Application.Services.Dtos.Configuration.DiscountProductWise
{
    public class DiscountProductWiseCreationDto
    {
        public string? Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<DiscountProductWiseDetailCreationDto>? DiscountProductWiseDetails { get; set; }
    }
}
