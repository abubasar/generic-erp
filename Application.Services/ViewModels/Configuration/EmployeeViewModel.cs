namespace Application.Services.ViewModels.Configuration
{
    public class EmployeeViewModel
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? EmployeeIdNo { get; set; }
        public string? ContactNo { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime JoiningDate { get; set; }
        public int Gender { get; set; }
        public string? GenderName { get; set; }
        public int BloodGroup { get; set; }
        public string? BloodGroupName { get; set; }
        public int MaritalStatus { get; set; }
        public string? MaritalStatusName { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid DesignationId { get; set; }
        public Guid JobLocationId { get; set; }
        public int EmployeeType { get; set; }
        public Guid? RegionId { get; set; }
        public Guid? ZoneId { get; set; }
        public Guid? AreaId { get; set; }
        public virtual DepartmentViewModel? Department { get; set; }
        public virtual DesignationViewModel? Designation { get; set; }
        public virtual JobLocationViewModel? JobLocation { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
