namespace Application.Services.Dtos.Configuration.Generic
{
    public class GenericCreationDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid ProductTypeId { get; set; }
    }
}
