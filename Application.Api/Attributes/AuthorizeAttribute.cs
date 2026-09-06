using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Application.Api.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _privilege;

    public AuthorizeAttribute(string privilege = "")
    {
        _privilege = privilege;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous) return;

        var roleId = context.HttpContext.Items["RoleId"];
        if (roleId == null)
            throw new UnauthorizationException("You are not Authorized");

        var db = context.HttpContext.RequestServices.GetService(typeof(IUnitOfWork)) as IUnitOfWork;

        if (!ValidatePrivilege(Guid.Parse(roleId.ToString()), db))
            throw new UnauthorizationException("You are not authorized to access this resource.");
    }

    private bool ValidatePrivilege(Guid? roleId, IUnitOfWork? db)
    {
        if (roleId == null || db == null) return false;

        var roleExist = db.Repository<Role>().TableNoTracking().Any(x => x.Id == roleId);
        if (!roleExist) return false;

        if (string.IsNullOrWhiteSpace(_privilege)) return true;

        return db.Repository<RoleClaim>().TableNoTracking()
            .Any(x => x.RoleId == roleId && x.Value == _privilege);
    }
}