using Application.Core.Common;
using Application.Core.PermissionHelpers;
using Application.Services.ViewModels.Auth;

namespace Application.Services.Services.Auth.Common
{
    public interface IAuthService
    {
        Task<Result<LoginViewModel>> Login(string username, string password);
        Task<(string accessToken, string refreshToken, IList<string> permissions)> RefreshTokensAsync(string refreshToken);
        Task<string> DeleteRefreshTokenAsync(string refreshToken);
        Task<Result<List<RoleClaimModel>>> GetAllPermissionsAsync(Guid roleId);
        Task<Result<string>> UpdatePermissionsAsync(PermissionRequest request);
        Task<bool> UserExists(string username);
    }
}
