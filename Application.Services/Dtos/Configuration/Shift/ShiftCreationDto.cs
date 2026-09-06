namespace Application.Services.Dtos.Configuration.Shift
{
    public class ShiftCreationDto
    {
        public string? Name { get; set; }
        public string FromTime { get; set; } = string.Empty;
        public string ToTime { get; set; } = string.Empty;
    }
}
