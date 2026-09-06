import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { hasPermission } from "app/shared/guards/has-permission.guard";
import { DeliveryNoteFormComponent } from "./components/delivery-note/delivery-note-form/delivery-note-form.component";
import { DeliveryNoteComponent } from "./components/delivery-note/delivery-note.component";
import { ReceivePaymentFormComponent } from "./components/receive-payment/receive-payment-form/receive-payment-form.component";
import { ReceivePaymentComponent } from "./components/receive-payment/receive-payment.component";
import { SaleInvoiceFormComponent } from "./components/sale-invoice/sale-invoice-form/sale-invoice-form.component";
import { SaleInvoiceComponent } from "./components/sale-invoice/sale-invoice.component";
import { SaleReturnFormComponent } from "./components/sale-return/sale-return-form/sale-return-form.component";
import { SaleReturnComponent } from "./components/sale-return/sale-return.component";
import { SalesOrderFormComponent } from "./components/sales-order/sales-order-form/sales-order-form.component";
import { SalesOrderComponent } from "./components/sales-order/sales-order.component";
import { SaleQuotationFormComponent } from "./components/sales-quotation/sales-quotation-form/sales-quotation-form.component";
import { SalesQuotationComponent } from "./components/sales-quotation/sales-quotation.component";
import { DeliveryNoteResolverService } from "./resolvers/delivery-note-resolver.service";
import { ReceivePaymentResolverService } from "./resolvers/receive-payment-resolver.service";
import { SaleInvoiceResolverService } from "./resolvers/sale-invoice-resolver.service";
import { SaleOrderResolverService } from "./resolvers/sale-order-resolver.service";
import { SaleQuotationResolverService } from "./resolvers/sale-quotation-resolver.service";
import { SaleReturnResolverService } from "./resolvers/sale-return-resolver.service";
import { ReceivePaymentAgainstSaleComponent } from "./components/receive-payment-against-sale/receive-payment-against-sale.component";
import { ReceivePaymentAgainstSaleFormComponent } from "./components/receive-payment-against-sale/receive-payment-against-sale-form/receive-payment-against-sale-form.component";
import { ReceivePaymentAgainstSaleResolverService } from "./resolvers/receive-payment-against-sale-resolver.service";

const routes: Routes = [
  // * sales-quotation
  {
    path: "sales-quotation",
    canMatch: [() => hasPermission(["Permissions.SaleQuotations.View"])],
    component: SalesQuotationComponent,
    data: {
      module: "Sales",
      pageTitle: "Sales Quotation List",
      breadcrumb: {
        title: "Sales Quotation",
        url: "/sales/sales-quotation",
      },
    },
  },
  {
    path: "sales-quotation/add-new",
    canMatch: [() => hasPermission(["Permissions.SaleQuotations.Create"])],
    component: SaleQuotationFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Add New Sales Quotation",
      breadcrumb: {
        title: "Sales Quotation List",
        url: "/sales/sales-quotation",
      },
    },
  },
  {
    path: "sales-quotation/:id",
    component: SaleQuotationFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Sales Quotation Detail",
      breadcrumb: {
        title: "Sales Quotation List",
        url: "/sales/sales-quotation",
      },
    },
    resolve: { saleQuotation: SaleQuotationResolverService },
  },
  // * sales-order
  {
    path: "sales-order",
    canMatch: [() => hasPermission(["Permissions.SaleOrders.View"])],
    component: SalesOrderComponent,
    data: {
      module: "Sales",
      pageTitle: "Sales Order List",
      breadcrumb: {
        title: "Sales Order",
        url: "/sales/sales-order",
      },
    },
  },
  {
    path: "sales-order/add-new",
    canMatch: [() => hasPermission(["Permissions.SaleOrders.Create"])],
    component: SalesOrderFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Add New Sales Order",
      breadcrumb: {
        title: "Sales Order List",
        url: "/sales/sales-order",
      },
    },
  },
  {
    path: "sales-order/:id",
    component: SalesOrderFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Sales Order Detail",
      breadcrumb: {
        title: "Sales Order List",
        url: "/sales/sales-order",
      },
    },
    resolve: { saleOrder: SaleOrderResolverService },
  },
  // * delivery-note
  {
    path: "delivery-note",
    canMatch: [() => hasPermission(["Permissions.DeliveryNotes.View"])],
    component: DeliveryNoteComponent,
    data: {
      module: "Sales",
      pageTitle: "Delivery Note List",
      breadcrumb: {
        title: "Delivery Note",
        url: "/sales/delivery-note",
      },
    },
  },
  {
    path: "delivery-note/add-new",
    canMatch: [() => hasPermission(["Permissions.DeliveryNotes.Create"])],
    component: DeliveryNoteFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Add New Delivery Note",
      breadcrumb: {
        title: "Delivery Note List",
        url: "/sales/delivery-note",
      },
    },
  },
  {
    path: "delivery-note/:id",
    component: DeliveryNoteFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Delivery Note Detail",
      breadcrumb: {
        title: "Delivery Note List",
        url: "/sales/delivery-note",
      },
    },
    resolve: { deliveryNote: DeliveryNoteResolverService },
  },
  // * sale-invoice
  {
    path: "sale-invoice",
    canMatch: [() => hasPermission(["Permissions.SaleInvoices.View"])],
    component: SaleInvoiceComponent,
    data: {
      module: "Sales",
      pageTitle: "Sale Invoice List",
      breadcrumb: {
        title: "Sale Invoice",
        url: "/sales/sale-invoice",
      },
    },
  },
  {
    path: "sale-invoice/add-new",
    canMatch: [() => hasPermission(["Permissions.SaleInvoices.Create"])],
    component: SaleInvoiceFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Add New Sale Invoice",
      breadcrumb: {
        title: "Sale Invoice List",
        url: "/sales/sale-invoice",
      },
    },
  },
  {
    path: "sale-invoice/:id",
    component: SaleInvoiceFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Sale Invoice Detail",
      breadcrumb: {
        title: "Sale Invoice List",
        url: "/sales/sale-invoice",
      },
    },
    resolve: { saleInvoice: SaleInvoiceResolverService },
  },

   // * receive-payment-against-sale
   {
    path: "receive-payment-against-sale",
    component: ReceivePaymentAgainstSaleComponent,
    data: {
      module: "Sales",
      pageTitle: "Receive Payment Against Sale List",
      breadcrumb: {
        title: "Receive Payment Against Sale",
        url: "/sales/receive-payment-against-sale",
      },
    },
  },
  {
    path: "receive-payment-against-sale/add-new",
    component: ReceivePaymentAgainstSaleFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Add New Receive Payment Against Sale",
      breadcrumb: {
        title: "Receive Payment Against Sale List",
        url: "/sales/receive-payment-against-sale",
      },
    },
  },
  {
    path: "receive-payment-against-sale/:id",
    component: ReceivePaymentAgainstSaleFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Receive Payment Against Sale Detail",
      breadcrumb: {
        title: "Receive Payment Against Sale List",
        url: "/sales/receive-payment-against-sale",
      },
    },
    resolve: {
      receivePaymentAgainstSale: ReceivePaymentAgainstSaleResolverService,
    },
  },
  // * money-receipt
  {
    path: "money-receipt",
    canMatch: [() => hasPermission(["Permissions.ReceivePayments.View"])],
    component: ReceivePaymentComponent,
    data: {
      module: "Sales",
      pageTitle: "Money Receipt List",
      breadcrumb: {
        title: "Money Receipt",
        url: "/sales/money-receipt",
      },
    },
  },
  {
    path: "money-receipt/add-new",
    canMatch: [() => hasPermission(["Permissions.ReceivePayments.Create"])],
    component: ReceivePaymentFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Add New Money Receipt",
      breadcrumb: {
        title: "Money Receipt List",
        url: "/sales/money-receipt",
      },
    },
  },
  {
    path: "money-receipt/:id",
    component: ReceivePaymentFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Money Receipt Detail",
      breadcrumb: {
        title: "Money Receipt List",
        url: "/sales/money-receipt",
      },
    },
    resolve: { receivePayment: ReceivePaymentResolverService },
  },
  // * Sale Return  */
  {
    path: "sale-return",
    canMatch: [() => hasPermission(["Permissions.SaleReturns.View"])],
    component: SaleReturnComponent,
    data: {
      module: "Sales",
      pageTitle: "Sale Return List",
      breadcrumb: {
        title: "Sale Return",
        url: "/sales/sale-return",
      },
    },
  },
  {
    path: "sale-return/add-new",
    canMatch: [() => hasPermission(["Permissions.SaleReturns.Create"])],
    component: SaleReturnFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Add New Sale Return",
      breadcrumb: {
        title: "Sale Return List",
        url: "/sales/sale-return",
      },
    },
  },
  {
    path: "sale-return/:id",
    component: SaleReturnFormComponent,
    data: {
      module: "Sales",
      pageTitle: "Sale Return Detail",
      breadcrumb: {
        title: "Sale Return List",
        url: "/sales/sale-return",
      },
    },
    resolve: { saleReturn: SaleReturnResolverService },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class SalesRoutingModule {}
