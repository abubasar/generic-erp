using Application.Services.Services.Productions.BillOfMaterials;
using Application.Services.Services.Productions.ManufacturingOrders;
using Application.Services.Services.Productions.Pdf;
using Application.Services.Services.Productions.Productions;
using Autofac;

namespace Application.Services.Services.Productions
{
    public class ProductionModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Scoped
            builder.RegisterType<BillOfMaterialService>().As<IBillOfMaterialService>().InstancePerLifetimeScope();
            builder.RegisterType<ManufacturingOrderService>().As<IManufacturingOrderService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductionService>().As<IProductionService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductionPdfService>().As<IProductionPdfService>().InstancePerLifetimeScope();
        }
    }
}
