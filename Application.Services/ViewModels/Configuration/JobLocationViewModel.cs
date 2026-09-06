namespace Application.Services.ViewModels.Configuration
{
    public class JobLocationViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = String.Empty;
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = String.Empty;
    }
}
