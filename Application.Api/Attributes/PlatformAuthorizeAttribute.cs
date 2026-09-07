using Application.Core.Constants;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Application.Api.Attributes
{
    /// <summary>
    /// Gates an endpoint behind a platform-admin identity (set by
    /// <c>PlatformAuthMiddleware</c>). Independent of the tenant
    /// <c>[Authorize("Permission")]</c> surface — a platform request has no tenant
    /// claims. Optionally requires a minimum <see cref="PlatformRoles"/> rank.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class PlatformAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _minimumRole;

        public PlatformAuthorizeAttribute(string minimumRole = PlatformRoles.ReadOnly) => _minimumRole = minimumRole;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
                return;

            var admin = context.HttpContext.RequestServices.GetService(typeof(IPlatformAdminContext)) as IPlatformAdminContext;
            if (admin is null || !admin.IsAuthenticated)
                throw new UnauthorizationException("Platform admin sign-in required.");

            if (!PlatformRoles.Satisfies(admin.Role, _minimumRole))
                throw new UnauthorizationException($"This action needs the '{_minimumRole}' platform role or higher.");
        }
    }
}
