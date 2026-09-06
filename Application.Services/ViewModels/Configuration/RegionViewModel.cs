namespace Application.Services.ViewModels.Configuration
{
    public class RegionViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
