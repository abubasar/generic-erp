using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Accounts.AccountsPayable.SupplierPaymentAgainstPurchases;
using Application.Services.Services.Accounts.AccountsReceivable.ReceivePaymentAgainstSales;
using Application.Services.Services.Accounts.AccountsReceivable.ReceivePayments;
using Application.Services.Services.Accounts.AccountsReceivable.SaleInvoices;
using Application.Services.Services.Accounts.AccountTypes;
using Application.Services.Services.Accounts.Customers;
using Application.Services.Services.Accounts.FundTransfers;
using Application.Services.Services.Accounts.JournalEntries;
using Application.Services.Services.Accounts.PaymentVouchers;
using Application.Services.Services.Accounts.Pdf;
using Application.Services.Services.Accounts.PdfAccount;
using Application.Services.Services.Accounts.ReceiveVouchers;
using Application.Services.Services.Accounts.Suppliers;
using Application.Services.Services.Accounts.VoucherEntries;
using Autofac;

namespace Application.Services.Services.Accounts
{
    public class AccountsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Scoped
            builder.RegisterType<AccountPdfService>().As<IAccountPdfService>().InstancePerLifetimeScope();
            builder.RegisterType<AccountReportPdfService>().As<IAccountReportPdfService>().InstancePerLifetimeScope();
            builder.RegisterType<AccountTypeService>().As<IAccountTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<AccountService>().As<IAccountService>().InstancePerLifetimeScope();
            builder.RegisterType<SupplierService>().As<ISupplierService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerLifetimeScope();
            builder.RegisterType<SupplierPaymentAgainstPurchaseService>().As<ISupplierPaymentAgainstPurchaseService>().InstancePerLifetimeScope();
            builder.RegisterType<VoucherEntryService>().As<IVoucherEntryService>().InstancePerLifetimeScope();
            builder.RegisterType<JournalEntryService>().As<IJournalEntryService>().InstancePerLifetimeScope();
            builder.RegisterType<ReceivePaymentAgainstSaleService>().As<IReceivePaymentAgainstSaleService>().InstancePerLifetimeScope();
            builder.RegisterType<SaleInvoiceService>().As<ISaleInvoiceService>().InstancePerLifetimeScope();
            builder.RegisterType<ReceivePaymentService>().As<IReceivePaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<FundTransferService>().As<IFundTransferService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentVoucherService>().As<IPaymentVoucherService>().InstancePerLifetimeScope();
            builder.RegisterType<ReceiveVoucherService>().As<IReceiveVoucherService>().InstancePerLifetimeScope();
        }
    }
}
