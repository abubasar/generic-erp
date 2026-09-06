using Application.Services.Services.Report.Accounts;
using Application.Services.Services.Report.Dashboard;
using Application.Services.Services.Report.Ledger;
using Application.Services.Services.Report.Pdf;
using Application.Services.Services.Report.Sales;
using Application.Services.Services.Report.Stocks;
using Autofac;

namespace Application.Services.Services.Report
{
    public class ReportModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Scoped
            builder.RegisterType<StockService>().As<IStockService>().InstancePerLifetimeScope();
            builder.RegisterType<LedgerService>().As<ILedgerService>().InstancePerLifetimeScope();
            builder.RegisterType<ReportPdfService>().As<IReportPdfService>().InstancePerLifetimeScope();
            builder.RegisterType<AccountReportService>().As<IAccountReportService>().InstancePerLifetimeScope();
            builder.RegisterType<DashboardService>().As<IDashboardService>().InstancePerLifetimeScope();
            builder.RegisterType<SaleOrderPdfService>().As<ISaleOrderPdfService>().InstancePerLifetimeScope();
            builder.RegisterType<DeliveryNotePdfService>().As<IDeliveryNotePdfService>().InstancePerLifetimeScope();
            builder.RegisterType<SaleInvoicePdfService>().As<ISaleInvoicePdfService>().InstancePerLifetimeScope();
            builder.RegisterType<SaleReturnPdfService>().As<ISaleReturnPdfService>().InstancePerLifetimeScope();
        }
    }
}
