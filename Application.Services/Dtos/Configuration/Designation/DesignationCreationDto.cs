namespace Application.Services.Dtos.Configuration.Designation
{
    public class DesignationCreationDto
    {
        public string Name { get; set; } = String.Empty;
        public Guid DepartmentId { get; set; }
    }
}
