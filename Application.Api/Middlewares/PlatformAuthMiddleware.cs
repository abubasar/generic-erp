using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Api.Middlewares
{
    /// <summary>
    /// Validates a platform-admin bearer token (audience = platform, carries a
    /// <c>scope=platform</c> claim and NO tenant id) and fills the request-scoped
    /// <see cref="PlatformAdminContext"/>. Never touches <see cref="TenantScope"/>,
    /// so platform requests run un-tenant-scoped. A tenant token fails validation
    /// here (wrong audience) and is simply ignored.
    /// </summary>
    public class PlatformAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly PlatformAuthSettings _settings;

        public PlatformAuthMiddleware(RequestDelegate next, IOptions<PlatformAuthSettings> settings)
        {
            _next = next;
            _settings = settings.Value;
        }

        public async Task Invoke(HttpContext context, PlatformAdminContext platformContext)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(_settings.Secret))
            {
                try
                {
                    var handler = new JwtSecurityTokenHandler();
                    handler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
                        ValidIssuer = _settings.Issuer,
                        ValidAudience = _settings.Audience,
                        ClockSkew = TimeSpan.Zero,
                    }, out var validated);

                    var jwt = (JwtSecurityToken)validated;
                    if (jwt.Claims.FirstOrDefault(c => c.Type == PlatformClaims.Scope)?.Value == PlatformClaims.ScopeValue
                        && Guid.TryParse(jwt.Claims.FirstOrDefault(c => c.Type == PlatformClaims.AdminId)?.Value, out var adminId))
                    {
                        platformContext.Set(
                            adminId,
                            jwt.Claims.FirstOrDefault(c => c.Type == PlatformClaims.Email)?.Value ?? "",
                            jwt.Claims.FirstOrDefault(c => c.Type == PlatformClaims.Name)?.Value,
                            jwt.Claims.FirstOrDefault(c => c.Type == PlatformClaims.Role)?.Value);

                        context.Items["PlatformAdminId"] = adminId.ToString();
                        context.Items["PlatformAdminRole"] = platformContext.Role;
                    }
                }
                catch
                {
                    // Not a valid platform token — leave the context unauthenticated.
                }
            }

            await _next(context);
        }
    }
}
