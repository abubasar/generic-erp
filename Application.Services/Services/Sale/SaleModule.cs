using Application.Services.Services.Purchase.PurchaseReturns;
using Application.Services.Services.Sale.DeliveryNotes;
using Application.Services.Services.Sale.Pdf;
using Application.Services.Services.Sale.SaleOrders;
using Application.Services.Services.Sale.SaleQuotations;
using Application.Services.Services.Sale.SaleReturns;
using Autofac;

namespace Application.Services.Services.Sale
{
    public class SaleModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<SaleQuotationService>().As<ISaleQuotationService>().InstancePerLifetimeScope();
            builder.RegisterType<SaleOrderService>().As<ISaleOrderService>().InstancePerLifetimeScope();
            builder.RegisterType<DeliveryNoteService>().As<IDeliveryNoteService>().InstancePerLifetimeScope();
            builder.RegisterType<SaleReturnService>().As<ISaleReturnService>().InstancePerLifetimeScope();
            builder.RegisterType<PurchaseReturnService>().As<IPurchaseReturnService>().InstancePerLifetimeScope();
            builder.RegisterType<SalePdfService>().As<ISalePdfService>().InstancePerLifetimeScope();
        }
    }
}
