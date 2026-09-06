using Application.Core.Constants;
using Application.Core.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Application.Api.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly JwtSettings _jwtSettings;

        public JwtMiddleware(RequestDelegate next, IOptions<JwtSettings> jwtSettings)
        {
            this._next = next;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                var (userId, roleId, username, tenantId) = ValidateAccessToken(token);
                context.Items["UserId"] = userId;
                context.Items["RoleId"] = roleId;
                context.Items["UserName"] = username;
                context.Items["TenantId"] = tenantId;
            }

            await _next(context);
        }

        private (string? userId, string? roleId, string? username, string? tenantId) ValidateAccessToken(string accessToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));

                tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = key,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                var jwtToken = validatedToken as JwtSecurityToken;

                var userId = jwtToken?.Claims.FirstOrDefault(x => x.Type == JwtConst.UserId)?.Value;
                var roleId = jwtToken?.Claims.FirstOrDefault(x => x.Type == JwtConst.RoleId)?.Value;
                var username = jwtToken?.Claims.FirstOrDefault(x => x.Type == JwtConst.Username)?.Value;
                var tenantId = jwtToken?.Claims.FirstOrDefault(x => x.Type == JwtConst.TenantId)?.Value;
                return (userId, roleId, username, tenantId);
            }
            catch
            {
                return (null, null, null, null);
            }
        }
    }
}