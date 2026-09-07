using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Application.Api.Attributes
{
    /// <summary>
    /// Gates an action / controller behind a platform module. Checked before the
    /// permission <see cref="AuthorizeAttribute"/>: a tenant that has not switched
    /// the module on gets "not on your plan" regardless of the user's role.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class RequiresModuleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _moduleKey;

        public RequiresModuleAttribute(string moduleKey) => _moduleKey = moduleKey;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .OfType<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>().Any();
            if (allowAnonymous) return;

            var tenant = context.HttpContext.RequestServices.GetService(typeof(ITenantContext)) as ITenantContext;
            if (tenant is null || !tenant.HasTenant)
                throw new UnauthorizationException("You are not Authorized");

            if (!tenant.HasModule(_moduleKey))
                throw new ModuleNotEnabledException(
                    $"The '{_moduleKey}' module is not enabled for your account.");
        }
    }
}
