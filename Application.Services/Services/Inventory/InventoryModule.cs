using Application.Services.Services.Inventory.Pdf;
using Application.Services.Services.Inventory.StockAdjustments;
using Application.Services.Services.Inventory.StockTransfers;
using Autofac;

namespace Application.Services.Services.Inventory
{
    public class InventoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Scoped
            builder.RegisterType<StockTransferService>().As<IStockTransferService>().InstancePerLifetimeScope();
            builder.RegisterType<StockAdjustmentService>().As<IStockAdjustmentService>().InstancePerLifetimeScope();
            builder.RegisterType<InventoryPdfService>().As<IInventoryPdfService>().InstancePerLifetimeScope();
        }
    }
}
