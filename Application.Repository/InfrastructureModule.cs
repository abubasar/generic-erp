
using Application.Core.Common;
using Application.Core.Interfaces;
using Autofac;

namespace Application.Infrastructure
{
    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Scoped
            builder.RegisterType<HttpTenantContext>().As<ITenantContext>().InstancePerLifetimeScope();
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(BaseRepository<>)).As(typeof(IBaseRepository<>)).InstancePerDependency();
            builder.RegisterType<MailService>().As<IMailService>().InstancePerLifetimeScope();
            builder.RegisterType<SmsService>().As<ISmsService>().InstancePerLifetimeScope();
            builder.RegisterType<ApiCaller>().As<IApiCaller>().InstancePerLifetimeScope();

        }
    }
}
