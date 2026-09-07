using Application.Core.Common;
using Application.Services.Services.Platform;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers.Platform
{
    /// <summary>Sign-in for the platform-admin console. Separate surface from tenant /api/Auth.</summary>
    [Route("api/platform/auth")]
    [ApiController]
    public class PlatformAuthController : ControllerBase
    {
        private readonly IPlatformAuthService _auth;

        public PlatformAuthController(IPlatformAuthService auth) => _auth = auth;

        public sealed record LoginRequest(string Email, string Password);

        [HttpPost("login")]
        public async Task<Result> Login(LoginRequest request)
        {
            var result = await _auth.LoginAsync(request.Email, request.Password);
            return await Result<PlatformLoginResult>.SuccessAsync(result, "Signed in");
        }
    }
}
