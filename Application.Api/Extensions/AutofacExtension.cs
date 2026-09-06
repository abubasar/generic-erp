using Application.Infrastructure;
using Application.Services.Services.Accounts;
using Application.Services.Services.Auth;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration;
using Application.Services.Services.Inventory;
using Application.Services.Services.Productions;
using Application.Services.Services.Purchase;
using Application.Services.Services.Report;
using Application.Services.Services.Sale;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace Application.Api.Extensions
{
    public static class AutofacExtension
    {
        public static void AddAutofac(this ConfigureHostBuilder host)
        {
            host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
             {
                 containerBuilder.RegisterModule<InfrastructureModule>();
                 containerBuilder.RegisterModule<AuthModule>();
                 containerBuilder.RegisterModule<ConfigurationModule>();
                 containerBuilder.RegisterModule<PurchaseModule>();
                 containerBuilder.RegisterModule<ProductionModule>();
                 containerBuilder.RegisterModule<SaleModule>();
                 containerBuilder.RegisterModule<AccountsModule>();
                 containerBuilder.RegisterModule<ReportModule>();
                 containerBuilder.RegisterModule<CommonModule>();
                 containerBuilder.RegisterModule<InventoryModule>();
             });
        }
    }
}
