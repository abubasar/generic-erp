namespace Application.Services.ViewModels.Configuration
{
    public class AreaViewModel
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public Guid ZoneId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public virtual ZoneViewModel? Zone { get; set; }
    }
}
