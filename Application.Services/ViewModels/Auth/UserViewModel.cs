using Application.Services.ViewModels.Configuration;

namespace Application.Services.ViewModels.Auth
{
    public class UserViewModel
    {

        public Guid Id { get; set; }
        public string? Username { get; set; }
        public Guid RoleId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? RoleName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public EmployeeViewModel? Employee { get; set; }



    }
}
