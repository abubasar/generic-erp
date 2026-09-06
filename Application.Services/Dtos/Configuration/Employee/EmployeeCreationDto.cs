namespace Application.Services.Dtos.Configuration.Employee
{
    public class EmployeeCreationDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? EmployeeIdNo { get; set; }
        public string? ContactNo { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime JoiningDate { get; set; }
        public int Gender { get; set; }
        public int BloodGroup { get; set; }
        public int MaritalStatus { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid DesignationId { get; set; }
        public Guid JobLocationId { get; set; }
        public int EmployeeType { get; set; }
        public Guid? RegionId { get; set; }
        public Guid? ZoneId { get; set; }
        public Guid? AreaId { get; set; }
    }
}
