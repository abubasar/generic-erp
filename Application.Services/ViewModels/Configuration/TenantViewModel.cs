namespace Application.Services.ViewModels.Configuration
{
    public class TenantViewModel
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? TimeZoneId { get; set; }
        public string? Address { get; set; }
        public string? ContactNo { get; set; }
        public string? Binno { get; set; }
        public string? Email { get; set; }
        public int BusinessType { get; set; }
        public string? Logo { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
