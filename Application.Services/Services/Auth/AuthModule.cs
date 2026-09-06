using Application.Services.Services.Auth.Common;
using Application.Services.Services.Auth.Roles;
using Application.Services.Services.Auth.Users;
using Autofac;
using Microsoft.AspNetCore.Http;

namespace Application.Services.Services.Auth
{
    public class AuthModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //singleton
            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
            // Scoped
            builder.RegisterType<AuthService>().As<IAuthService>().InstancePerLifetimeScope();
            builder.RegisterType<RoleService>().As<IRoleService>().InstancePerLifetimeScope();
            builder.RegisterType<UserService>().As<IUserService>().InstancePerLifetimeScope();
        }
    }
}
