
namespace Application.Core.PermissionHelpers
{
    public class PermissionRequest
    {
        public Guid RoleId { get; set; }

        public IList<RoleClaimModel> RoleClaims { get; set; }=new List<RoleClaimModel>();
    }
}
