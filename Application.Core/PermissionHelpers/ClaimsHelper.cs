using Application.Core.Constants;
using Application.Core.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel;
using System.Reflection;

namespace Application.Core.PermissionHelpers
{
    public static class ClaimsHelper
    {
        public static void GetAllPermissions(this List<RoleClaimModel> allPermissions, int businessType)
        {
            if (businessType == (int)BusinessType.Primary)
            {
                var primaryModules = typeof(PrimaryPermissions).GetNestedTypes();

                foreach (var primaryModule in primaryModules)
                {
                    string moduleName = string.Empty;
                    string moduleDescription = string.Empty;

                    if (primaryModule.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                        .FirstOrDefault() is DisplayNameAttribute displayNameAttribute)
                    {
                        moduleName = displayNameAttribute.DisplayName;
                    }

                    if (primaryModule.GetCustomAttributes(typeof(DescriptionAttribute), true)
                        .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                    {
                        moduleDescription = descriptionAttribute.Description;
                    }

                    var fields = primaryModule.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                    foreach (var fi in fields)
                    {
                        object? propertyValue = fi.GetValue(null);

                        if (propertyValue is not null)
                        {
                            allPermissions.Add(new RoleClaimModel { Value = propertyValue?.ToString(), Type = ApplicationClaimTypes.Permission, Group = moduleName, Description = moduleDescription });
                        }
                    }
                }
            }
            if (businessType == (int)BusinessType.Secondary)
            {
                var secondaryModules = typeof(SecondaryPermissions).GetNestedTypes();

                foreach (var secondaryModule in secondaryModules)
                {
                    string moduleName = string.Empty;
                    string moduleDescription = string.Empty;

                    if (secondaryModule.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                        .FirstOrDefault() is DisplayNameAttribute displayNameAttribute)
                    {
                        moduleName = displayNameAttribute.DisplayName;
                    }

                    if (secondaryModule.GetCustomAttributes(typeof(DescriptionAttribute), true)
                        .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                    {
                        moduleDescription = descriptionAttribute.Description;
                    }

                    var fields = secondaryModule.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                    foreach (var fi in fields)
                    {
                        object? propertyValue = fi.GetValue(null);

                        if (propertyValue is not null)
                        {
                            allPermissions.Add(new RoleClaimModel { Value = propertyValue?.ToString(), Type = ApplicationClaimTypes.Permission, Group = moduleName, Description = moduleDescription });
                        }
                    }
                }
            }
            var modules = typeof(Permissions).GetNestedTypes();

            foreach (var module in modules)
            {
                string moduleName = string.Empty;
                string moduleDescription = string.Empty;

                if (module.GetCustomAttributes(typeof(DisplayNameAttribute), true)
                    .FirstOrDefault() is DisplayNameAttribute displayNameAttribute)
                {
                    moduleName = displayNameAttribute.DisplayName;
                }

                if (module.GetCustomAttributes(typeof(DescriptionAttribute), true)
                    .FirstOrDefault() is DescriptionAttribute descriptionAttribute)
                {
                    moduleDescription = descriptionAttribute.Description;
                }

                var fields = module.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                foreach (var fi in fields)
                {
                    object? propertyValue = fi.GetValue(null);

                    if (propertyValue is not null)
                    {
                        allPermissions.Add(new RoleClaimModel { Value = propertyValue?.ToString(), Type = ApplicationClaimTypes.Permission, Group = moduleName, Description = moduleDescription });
                    }
                }
            }
        }
        public static string? GetUserId(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }
            var claim = httpContext.Items["UserId"];
            return claim?.ToString();
        }
        public static string? GetUserName(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }
            var claim = httpContext.Items["UserName"];
            return claim?.ToString();
        }
        public static Guid? GetTenantId(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }
            var claim = httpContext?.Items["TenantId"]?.ToString();
            if (claim is null) return null;
            return Guid.Parse(claim);
        }
        public static string? GetDomain(this HttpContext httpContext)
        {
            if (httpContext == null)
            {
                throw new ArgumentNullException(nameof(httpContext));
            }
            return httpContext.Request.Host.Host;
        }


    }
}
