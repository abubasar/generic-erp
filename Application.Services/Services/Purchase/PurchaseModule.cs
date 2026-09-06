using Application.Services.Services.Purchase.GoodsReceiveNotes;
using Application.Services.Services.Purchase.LcAdjustments;
using Application.Services.Services.Purchase.LCCostEntries;
using Application.Services.Services.Purchase.Pdf;
using Application.Services.Services.Purchase.PoPriceAdjustmentAfterGrns;
using Application.Services.Services.Purchase.PurchaseInvoices;
using Application.Services.Services.Purchase.PurchaseOrders;
using Application.Services.Services.Purchase.PurchaseRequisitions;
using Application.Services.Services.Purchase.SupplierPayments;
using Application.Services.Services.Purchase.VendorQuotations;
using Autofac;

namespace Application.Services.Services.Purchase
{
    public class PurchaseModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Scoped
            builder.RegisterType<PurchaseRequisitionService>().As<IPurchaseRequisitionService>().InstancePerLifetimeScope();
            builder.RegisterType<VendorQuotationService>().As<IVendorQuotationService>().InstancePerLifetimeScope();
            builder.RegisterType<PurchaseOrderService>().As<IPurchaseOrderService>().InstancePerLifetimeScope();
            builder.RegisterType<GoodsReceiveNoteService>().As<IGoodsReceiveNoteService>().InstancePerLifetimeScope();
            builder.RegisterType<PurchaseInvoiceService>().As<IPurchaseInvoiceService>().InstancePerLifetimeScope();
            builder.RegisterType<PurchasePdfService>().As<IPurchasePdfService>().InstancePerLifetimeScope();
            builder.RegisterType<SupplierPaymentService>().As<ISupplierPaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<PoPriceAdjustmentAfterGrnService>().As<IPoPriceAdjustmentAfterGrnService>().InstancePerLifetimeScope();
            builder.RegisterType<LCCostEntryService>().As<ILCCostEntryService>().InstancePerLifetimeScope();
            builder.RegisterType<LcAdjustmentService>().As<ILcAdjustmentService>().InstancePerLifetimeScope();
        }
    }
}
