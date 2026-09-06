using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.PermissionHelpers;
using Application.Services.Dtos.Auth;
using Application.Services.Services.Auth.Common;
using Application.Services.ViewModels.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("Login")]
        public async Task<Result> Login(LoginDto request)
        {
            return await _authService.Login(request.Username, request.Password);
        }
        [HttpPost("refresh-token")]
        public async Task<Result> RefreshToken(RefreshTokenDto request)
        {
            var (accessToken, refreshToken, permissions) = await _authService.RefreshTokensAsync(request.RefreshToken);
            return await Result<LoginViewModel>.SuccessAsync(new LoginViewModel { AccessToken = accessToken, RefreshToken = refreshToken, Permissions = permissions }, "OK");
        }
        [HttpPost("delete-refresh-token")]
        public async Task<Result> DeleteRefreshToken(RefreshTokenDto request)
        {
            var tokenId = await _authService.DeleteRefreshTokenAsync(request.RefreshToken);
            return await Result<string>.SuccessAsync(tokenId, "Refresh Token Deleted Successfully");
        }

        [HttpGet("permissions/byrole/{roleId}")]
        [Authorize(Permissions.RoleClaims.View)]
        public async Task<IActionResult> GetPermissionsByRoleIdAsync([FromRoute] Guid roleId)
        {
            var response = await _authService.GetAllPermissionsAsync(roleId);
            return Ok(response);
        }

        [HttpPost("permissions/update")]
        [Authorize(Permissions.RoleClaims.Edit)]
        public async Task<IActionResult> UpdatePermissionsAsync(PermissionRequest request)
        {
            var response = await _authService.UpdatePermissionsAsync(request);
            return Ok(response);
        }
    }
}
