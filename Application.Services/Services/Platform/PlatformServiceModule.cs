using Application.Core.Common;
using Application.Core.Interfaces;
using Autofac;

namespace Application.Services.Services.Platform
{
    /// <summary>Autofac wiring for the platform-admin console (separate surface from the tenant services).</summary>
    public class PlatformServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PlatformAdminContext>().AsSelf().As<IPlatformAdminContext>().InstancePerLifetimeScope();
            builder.RegisterType<PlatformAuditWriter>().As<IPlatformAuditWriter>().InstancePerLifetimeScope();
            builder.RegisterType<PlatformAuthService>().As<IPlatformAuthService>().InstancePerLifetimeScope();
            builder.RegisterType<PlatformTenantService>().As<IPlatformTenantService>().InstancePerLifetimeScope();
            builder.RegisterType<PlatformCatalogService>().As<IPlatformCatalogService>().InstancePerLifetimeScope();
            builder.RegisterType<PlatformUsageService>().As<IPlatformUsageService>().InstancePerLifetimeScope();
        }
    }
}
