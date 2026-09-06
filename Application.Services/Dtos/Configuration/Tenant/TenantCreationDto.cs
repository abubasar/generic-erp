namespace Application.Services.Dtos.Configuration.Tenant
{
    public class TenantCreationDto
    {
        public string? Code { get; set; }
        public string? Name { get; set; }
        public string? TimeZoneId { get; set; }
        public string? Address { get; set; }
        public string? ContactNo { get; set; }
        public string? Binno { get; set; }
        public string? Email { get; set; }
    }
}
