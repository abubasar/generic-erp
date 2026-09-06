namespace Application.Services.Dtos.Configuration.Zone
{
    public class ZoneCreationDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid RegionId { get; set; }
    }
}
