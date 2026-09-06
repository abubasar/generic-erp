import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { hasPermission } from "app/shared/guards/has-permission.guard";
import { AccountsChartComponent } from "./components/accounts-chart/accounts-chart.component";
import { FundTransferFormComponent } from "./components/fund-transfer/fund-transfer-form/fund-transfer-form.component";
import { FundTransferComponent } from "./components/fund-transfer/fund-transfer.component";
import { JournalEntryFormComponent } from "./components/journal-entry/journal-entry-form/journal-entry-form.component";
import { JournalEntryComponent } from "./components/journal-entry/journal-entry.component";
import { PaymentVoucherFormComponent } from "./components/payment-voucher/payment-voucher-form/payment-voucher-form.component";
import { PaymentVoucherComponent } from "./components/payment-voucher/payment-voucher.component";
import { ReceiveVoucherFormComponent } from "./components/receive-voucher/receive-voucher-form/receive-voucher-form.component";
import { ReceiveVoucherComponent } from "./components/receive-voucher/receive-voucher.component";
import { VoucherEntryFormComponent } from "./components/voucher-entry/voucher-entry-form/voucher-entry-form.component";
import { VoucherEntryComponent } from "./components/voucher-entry/voucher-entry.component";
import { FundTransferResolverService } from "./resolvers/fund-transfer-resolver.service";
import { JournalEntryResolverService } from "./resolvers/journal-entry-resolver.service";
import { PaymentVoucherResolverService } from "./resolvers/payment-voucher-resolver.service";
import { ReceiveVoucherResolverService } from "./resolvers/receive-voucher-resolver.service";
import { VoucherEntryResolverService } from "./resolvers/voucher-entry-resolver.service";

const routes: Routes = [
  {
    path: "accounts-chart",
    canMatch: [() => hasPermission(["Permissions.Accounts.View"])],
    component: AccountsChartComponent,
    data: {
      module: "Accounts",
      pageTitle: "Accounts Chart Tree",
      breadcrumb: {
        title: "Accounts Chart",
        url: "/accounts/accounts-chart",
      },
    },
  },

  // * journal-entry
  {
    path: "journal-entry",
    canMatch: [() => hasPermission(["Permissions.JournalEntries.View"])],
    component: JournalEntryComponent,
    data: {
      module: "Accounts",
      pageTitle: "Journal Entry List",
      breadcrumb: {
        title: "Journal Entry",
        url: "/accounts/journal-entry",
      },
    },
  },
  {
    path: "journal-entry/add-new",
    canMatch: [() => hasPermission(["Permissions.JournalEntries.Create"])],
    component: JournalEntryFormComponent,
    data: {
      module: "Accounts",
      pageTitle: "Add New Journal Entry",
      breadcrumb: {
        title: "Journal Entry List",
        url: "/accounts/journal-entry",
      },
    },
  },
  {
    path: "journal-entry/:id",
    component: JournalEntryFormComponent,
    data: {
      module: "Accounts",
      pageTitle: "Journal Entry Detail",
      breadcrumb: {
        title: "Journal Entry List",
        url: "/accounts/journal-entry",
      },
    },
    resolve: { journalEntry: JournalEntryResolverService },
  },
  // * fund-transfer
  {
    path: "fund-transfer",
    canMatch: [() => hasPermission(["Permissions.FundTransfers.View"])],
    component: FundTransferComponent,
    data: {
      module: "Accounts",
      pageTitle: "Fund Transfer List",
      breadcrumb: {
        title: "Fund Transfer",
        url: "/accounts/fund-transfer",
      },
    },
  },
  {
    path: "fund-transfer/add-new",
    canMatch: [() => hasPermission(["Permissions.FundTransfers.Create"])],
    component: FundTransferFormComponent,
    data: {
      module: "Accounts",
      pageTitle: "Add New Fund Transfer",
      breadcrumb: {
        title: "Fund Transfer List",
        url: "/accounts/fund-transfer",
      },
    },
  },
  {
    path: "fund-transfer/:id",
    component: FundTransferFormComponent,
    data: {
      module: "Accounts",
      pageTitle: "Fund Transfer Detail",
      breadcrumb: {
        title: "Fund Transfer List",
        url: "/accounts/fund-transfer",
      },
    },
    resolve: { fundTransfer: FundTransferResolverService },
  },

  // *  voucher-entry
  {
    path: "voucher-entry",
    canMatch: [() => hasPermission(["Permissions.VoucherEntries.View"])],
    component: VoucherEntryComponent,
    data: {
      module: "Accounts",
      pageTitle: "Voucher Entry List",
      breadcrumb: {
        title: "Voucher Entry",
        url: "/accounts/voucher-entry",
      },
    },
  },
  {
    path: "voucher-entry/add-new",
    canMatch: [() => hasPermission(["Permissions.VoucherEntries.Create"])],
    component: VoucherEntryFormComponent,
    data: {
      module: "Accounts",
      pageTitle: "Add New Voucher Entry",
      breadcrumb: {
        title: "Voucher Entry List",
        url: "/accounts/voucher-entry",
      },
    },
  },
  {
    path: "voucher-entry/:id",
    component: VoucherEntryFormComponent,
    data: {
      module: "Accounts",
      pageTitle: "Voucher Entry Detail",
      breadcrumb: {
        title: "Voucher Entry List",
        url: "/accounts/voucher-entry",
      },
    },
    resolve: { voucherEntry: VoucherEntryResolverService },
  },
  // *  cash-payment-voucher
  {
    path: "cash-payment-voucher",
    canMatch: [() => hasPermission(["Permissions.PaymentVouchers.View"])],
    component: PaymentVoucherComponent,
    data: {
      module: "Accounts",
      pageTitle: "Cash Payment Voucher List",
      breadcrumb: {
        title: "Cash Payment Voucher",
        url: "/accounts/cash-payment-voucher",
      },
    },
  },
  {
    path: "cash-payment-voucher/add-new",
    canMatch: [() => hasPermission(["Permissions.PaymentVouchers.Create"])],
    component: PaymentVoucherFormComponent,
    data: {
      module: "Accounts",
      pageTitle: "Add New Cash Payment Voucher",
      breadcrumb: {
        title: "Cash Payment Voucher List",
        url: "/accounts/cash-payment-voucher",
      },
    },
  },
  {
    path: "cash-payment-voucher/:id",
    component: PaymentVoucherFormComponent,
    data: {
      module: "Accounts",
      pageTitle: "Cash Payment Voucher Detail",
      breadcrumb: {
        title: "Cash Payment Voucher List",
        url: "/accounts/cash-payment-voucher",
      },
    },
    resolve: { paymentVoucher: PaymentVoucherResolverService },
  },
  // *  cash-receipt-voucher
  {
    path: "cash-receipt-voucher",
    canMatch: [() => hasPermission(["Permissions.ReceiveVouchers.View"])],
    component: ReceiveVoucherComponent,
    data: {
      module: "Accounts",
      pageTitle: "Cash Receipt Voucher List",
      breadcrumb: {
        title: "Cash Receipt Voucher",
        url: "/accounts/cash-receipt-voucher",
      },
    },
  },
  {
    path: "cash-receipt-voucher/add-new",
    canMatch: [() => hasPermission(["Permissions.ReceiveVouchers.Create"])],
    component: ReceiveVoucherFormComponent,
    data: {
      module: "Accounts",
      pageTitle: "Add New Cash Receipt Voucher",
      breadcrumb: {
        title: "Cash Receipt Voucher List",
        url: "/accounts/cash-receipt-voucher",
      },
    },
  },
  {
    path: "cash-receipt-voucher/:id",
    component: ReceiveVoucherFormComponent,
    data: {
      module: "Accounts",
      pageTitle: "Cash Receipt Voucher Detail",
      breadcrumb: {
        title: "Cash Receipt Voucher List",
        url: "/accounts/cash-receipt-voucher",
      },
    },
    resolve: { receiveVoucher: ReceiveVoucherResolverService },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class AccountsRoutingModule {}
