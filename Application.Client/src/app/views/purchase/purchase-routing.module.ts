import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { hasPermission } from "app/shared/guards/has-permission.guard";
import { GoodsReceiveNoteFormComponent } from "./components/goods-receive-note/goods-receive-note-form/goods-receive-note-form.component";
import { GoodsReceiveNoteComponent } from "./components/goods-receive-note/goods-receive-note.component";
import { LcAdjustmentFormComponent } from "./components/lc-adjustment/lc-adjustment-form/lc-adjustment-form.component";
import { LcAdjustmentComponent } from "./components/lc-adjustment/lc-adjustment.component";
import { LcCostEntryFormComponent } from "./components/lc-cost-entry/lc-cost-entry-form/lc-cost-entry-form.component";
import { LcCostEntryComponent } from "./components/lc-cost-entry/lc-cost-entry.component";
import { PoPriceAdjustmentAfterGrnFormComponent } from "./components/po-price-adjustment-after-grn/po-price-adjustment-after-grn-form/po-price-adjustment-after-grn-form.component";
import { PoPriceAdjustmentAfterGrnComponent } from "./components/po-price-adjustment-after-grn/po-price-adjustment-after-grn.component";
import { PurchaseInvoiceFormComponent } from "./components/purchase-invoice/purchase-invoice-form/purchase-invoice-form.component";
import { PurchaseInvoiceComponent } from "./components/purchase-invoice/purchase-invoice.component";
import { PurchaseOrderFormComponent } from "./components/purchase-order/purchase-order-form/purchase-order-form.component";
import { PurchaseOrderComponent } from "./components/purchase-order/purchase-order.component";
import { PurchaseRequisitionFormComponent } from "./components/purchase-requisition/purchase-requisition-form/purchase-requisition-form.component";
import { PurchaseRequisitionComponent } from "./components/purchase-requisition/purchase-requisition.component";
import { PurchaseReturnFormComponent } from "./components/purchase-return/purchase-return-form/purchase-return-form.component";
import { PurchaseReturnComponent } from "./components/purchase-return/purchase-return.component";
import { SendRFQToVendorEmailComponent } from "./components/send-rfq-to-vendor/send-rfq-to-vendor-email/send-rfq-to-vendor-email.component";
import { SendRFQToVendorFormComponent } from "./components/send-rfq-to-vendor/send-rfq-to-vendor-form/send-rfq-to-vendor-form.component";
import { SendRFQToVendorComponent } from "./components/send-rfq-to-vendor/send-rfq-to-vendor.component";
import { SupplierPaymentFormComponent } from "./components/supplier-payment/supplier-payment-form/supplier-payment-form.component";
import { SupplierPaymentComponent } from "./components/supplier-payment/supplier-payment.component";
import { VendorQuotationFormComponent } from "./components/vendor-quotation/vendor-quotation-form/vendor-quotation-form.component";
import { VendorQuotationComponent } from "./components/vendor-quotation/vendor-quotation.component";
import { VendorQuoteComparisonComponent } from "./components/vendor-quotation/vendor-quote-comparison/vendor-quote-comparison.component";
import { GoodsReceiveNoteResolverService } from "./resolvers/goods-receive-note-resolver.service";
import { LcAdjustmentResolverService } from "./resolvers/lc-adjustment-resolver.service";
import { LcCostEntryResolverService } from "./resolvers/lc-cost-entry-resolver.service";
import { PoPriceAdjustmentAfterGrnResolverService } from "./resolvers/po-price-adjustment-after-grn-resolver.service";
import { PurchaseInvoiceResolverService } from "./resolvers/purchase-invoice-resolver.service";
import { PurchaseOrderResolverService } from "./resolvers/purchase-order-resolver.service";
import { PurchaseRequisitionResolverService } from "./resolvers/purchase-requisition-resolver.service";
import { PurchaseReturnResolverService } from "./resolvers/purchase-return-resolver.service";
import { SupplierPaymentResolverService } from "./resolvers/supplier-payment-resolver.service";
import { VendorQuotationResolverService } from "./resolvers/vendor-quotation-resolver.service";

const routes: Routes = [
  // * LC Cost Entry
  {
    path: "lc-cost-entry",
    canMatch: [() => hasPermission(["Permissions.LCCostEntries.View"])],
    component: LcCostEntryComponent,
    data: {
      module: "Purchase",
      pageTitle: "LC Cost Entry List",
      breadcrumb: {
        title: "LC Cost Entry",
        url: "/purchase/lc-cost-entry",
      },
    },
  },
  {
    path: "lc-cost-entry/add-new",
    canMatch: [() => hasPermission(["Permissions.LCCostEntries.Create"])],
    component: LcCostEntryFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Add New LC Cost Entry",
      breadcrumb: {
        title: "LC Cost Entry List",
        url: "/purchase/lc-cost-entry",
      },
    },
  },
  {
    path: "lc-cost-entry/:id",
    component: LcCostEntryFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "LC Cost Entry Detail",
      breadcrumb: {
        title: "LC Cost Entry List",
        url: "/purchase/lc-cost-entry",
      },
    },
    resolve: { lcCostEntry: LcCostEntryResolverService },
  },

  // * LC LC Adjustment
  {
    path: "lc-adjustment",
    canMatch: [() => hasPermission(["Permissions.LcAdjustments.View"])],
    component: LcAdjustmentComponent,
    data: {
      module: "Purchase",
      pageTitle: "LC Adjustment List",
      breadcrumb: {
        title: "LC Adjustment",
        url: "/purchase/lc-adjustment",
      },
    },
  },
  {
    path: "lc-adjustment/add-new",
    canMatch: [() => hasPermission(["Permissions.LcAdjustments.Create"])],
    component: LcAdjustmentFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Add New LC Adjustment",
      breadcrumb: {
        title: "LC Adjustment List",
        url: "/purchase/lc-adjustment",
      },
    },
  },
  {
    path: "lc-adjustment/:id",
    component: LcAdjustmentFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "LC Adjustment Detail",
      breadcrumb: {
        title: "LC Adjustment List",
        url: "/purchase/lc-adjustment",
      },
    },
    resolve: { lcAdjustment: LcAdjustmentResolverService },
  },

  // * purchase-requisition
  {
    path: "purchase-requisition",
    canMatch: [() => hasPermission(["Permissions.PurchaseRequisitions.View"])],
    component: PurchaseRequisitionComponent,
    data: {
      module: "Purchase",
      pageTitle: "Purchase Requisition List",
      breadcrumb: {
        title: "Purchase Requisition",
        url: "/purchase/purchase-requisition",
      },
    },
  },
  {
    path: "purchase-requisition/add-new",
    canMatch: [
      () => hasPermission(["Permissions.PurchaseRequisitions.Create"]),
    ],
    component: PurchaseRequisitionFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Add New Purchase Requisition",
      breadcrumb: {
        title: "Purchase Requisition List",
        url: "/purchase/purchase-requisition",
      },
    },
  },
  {
    path: "purchase-requisition/:id",
    component: PurchaseRequisitionFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Purchase Requisition Detail",
      breadcrumb: {
        title: "Purchase Requisition List",
        url: "/purchase/purchase-requisition",
      },
    },
    resolve: { purchaseRequisition: PurchaseRequisitionResolverService },
  },
  // * send-rfq-to-vendor
  {
    path: "send-rfq-to-vendor",
    canMatch: [() => hasPermission(["Permissions.PurchaseRequisitions.View"])],
    component: SendRFQToVendorComponent,
    data: {
      module: "Purchase",
      pageTitle: "Send RFQ to Vendor List",
      breadcrumb: {
        title: "Send RFQ to Vendor",
        url: "/purchase/send-rfq-to-vendor",
      },
    },
  },
  {
    path: "send-rfq-to-vendor/email/:id",
    component: SendRFQToVendorEmailComponent,
    data: {
      module: "Purchase",
      pageTitle: "Send RFQ to Vendor",
      breadcrumb: {
        title: "Send RFQ to Vendor List",
        url: "/purchase/send-rfq-to-vendor",
      },
    },
  },
  {
    path: "send-rfq-to-vendor/:id",
    component: SendRFQToVendorFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Send RFQ to Vendor Detail",
      breadcrumb: {
        title: "Send RFQ to Vendor List",
        url: "/purchase/send-rfq-to-vendor",
      },
    },
    resolve: { purchaseRequisition: PurchaseRequisitionResolverService },
  },

  // * manage-vendor-quotation
  {
    path: "manage-vendor-quotation",
    canMatch: [() => hasPermission(["Permissions.VendorQuotations.View"])],
    component: VendorQuotationComponent,
    data: {
      module: "Purchase",
      pageTitle: "Manage Vendor Quotation List",
      breadcrumb: {
        title: "Manage Vendor Quotation",
        url: "/purchase/manage-vendor-quotation",
      },
    },
  },
  {
    path: "manage-vendor-quotation/add-new",
    canMatch: [() => hasPermission(["Permissions.VendorQuotations.Create"])],
    component: VendorQuotationFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Add New Vendor Quotation",
      breadcrumb: {
        title: "Vendor Quotation List",
        url: "/purchase/manage-vendor-quotation",
      },
    },
  },
  {
    path: "manage-vendor-quotation/compare",
    component: VendorQuoteComparisonComponent,
    data: {
      module: "Purchase",
      pageTitle: "Vendor Quotation Compare",
      breadcrumb: {
        title: "Vendor Quotation List",
        url: "/purchase/manage-vendor-quotation",
      },
    },
  },
  {
    path: "manage-vendor-quotation/:id",
    component: VendorQuotationFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Vendor Quotation Detail",
      breadcrumb: {
        title: "Vendor Quotation List",
        url: "/purchase/manage-vendor-quotation",
      },
    },
    resolve: { vendorQuotation: VendorQuotationResolverService },
  },

  // * purchase-order
  {
    path: "purchase-order",
    canMatch: [() => hasPermission(["Permissions.PurchaseOrders.View"])],
    component: PurchaseOrderComponent,
    data: {
      module: "Purchase",
      pageTitle: "Purchase Order List",
      breadcrumb: {
        title: "Purchase Order",
        url: "/purchase/purchase-order",
      },
    },
  },
  {
    path: "purchase-order/add-new",
    canMatch: [() => hasPermission(["Permissions.PurchaseOrders.Create"])],
    component: PurchaseOrderFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Add New Purchase Order",
      breadcrumb: {
        title: "Purchase Order List",
        url: "/purchase/purchase-order",
      },
    },
  },
  {
    path: "purchase-order/:id",
    component: PurchaseOrderFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Purchase Order Detail",
      breadcrumb: {
        title: "Purchase Order List",
        url: "/purchase/purchase-order",
      },
    },
    resolve: { purchaseOrder: PurchaseOrderResolverService },
  },

  // * goods-receive-note
  {
    path: "goods-receive-note",
    canMatch: [() => hasPermission(["Permissions.GoodsReceiveNotes.View"])],
    component: GoodsReceiveNoteComponent,
    data: {
      module: "Purchase",
      pageTitle: "Goods Receive Note List",
      breadcrumb: {
        title: "Goods Receive Note",
        url: "/purchase/goods-receive-note",
      },
    },
  },
  {
    path: "goods-receive-note/add-new",
    canMatch: [() => hasPermission(["Permissions.GoodsReceiveNotes.Create"])],
    component: GoodsReceiveNoteFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Add New Goods Receive Note",
      breadcrumb: {
        title: "Goods Receive Note List",
        url: "/purchase/goods-receive-note",
      },
    },
  },
  {
    path: "goods-receive-note/:id",
    component: GoodsReceiveNoteFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Goods Receive Note Detail",
      breadcrumb: {
        title: "Goods Receive Note List",
        url: "/purchase/goods-receive-note",
      },
    },
    resolve: { goodsReceiveNote: GoodsReceiveNoteResolverService },
  },

  // * purchase-invoice
  {
    path: "purchase-invoice",
    canMatch: [() => hasPermission(["Permissions.PurchaseInvoices.View"])],
    component: PurchaseInvoiceComponent,
    data: {
      module: "Purchase",
      pageTitle: "Purchase Invoice List",
      breadcrumb: {
        title: "Purchase Invoice",
        url: "/purchase/purchase-invoice",
      },
    },
  },
  {
    path: "purchase-invoice/add-new",
    canMatch: [() => hasPermission(["Permissions.PurchaseInvoices.Create"])],
    component: PurchaseInvoiceFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Add New Purchase Invoice",
      breadcrumb: {
        title: "Purchase Invoice List",
        url: "/purchase/purchase-invoice",
      },
    },
  },
  {
    path: "purchase-invoice/:id",
    component: PurchaseInvoiceFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Purchase Invoice Detail",
      breadcrumb: {
        title: "Purchase Invoice List",
        url: "/purchase/purchase-invoice",
      },
    },
    resolve: { purchaseInvoice: PurchaseInvoiceResolverService },
  },

  // * purchase-return
  {
    path: "purchase-return",
    canMatch: [() => hasPermission(["Permissions.PurchaseReturns.View"])],
    component: PurchaseReturnComponent,
    data: {
      module: "Purchase",
      pageTitle: "Purchase Return List",
      breadcrumb: {
        title: "Purchase Return",
        url: "/purchase/purchase-return",
      },
    },
  },
  {
    path: "purchase-return/add-new",
    canMatch: [() => hasPermission(["Permissions.PurchaseReturns.Create"])],
    component: PurchaseReturnFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Add New Purchase Return",
      breadcrumb: {
        title: "Purchase Return List",
        url: "/purchase/purchase-return",
      },
    },
  },
  {
    path: "purchase-return/:id",
    component: PurchaseReturnFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Purchase Return Detail",
      breadcrumb: {
        title: "Purchase Return List",
        url: "/purchase/purchase-return",
      },
    },
    resolve: { purchaseReturn: PurchaseReturnResolverService },
  },

  // * po-price-adjustment-after-grn
  {
    path: "po-price-adjustment-after-grn",
    canMatch: [
      () => hasPermission(["Permissions.PoPriceAdjustmentAfterGrns.View"]),
    ],
    component: PoPriceAdjustmentAfterGrnComponent,
    data: {
      module: "Purchase",
      pageTitle: "PO Price Adjustment After Grn List",
      breadcrumb: {
        title: "PO Price Adjustment After Grn",
        url: "/purchase/po-price-adjustment-after-grn",
      },
    },
  },
  {
    path: "po-price-adjustment-after-grn/add-new",
    canMatch: [
      () => hasPermission(["Permissions.PoPriceAdjustmentAfterGrns.Create"]),
    ],
    component: PoPriceAdjustmentAfterGrnFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Add New PO Price Adjustment After Grn",
      breadcrumb: {
        title: "PO Price Adjustment After Grn List",
        url: "/purchase/po-price-adjustment-after-grn",
      },
    },
  },
  {
    path: "po-price-adjustment-after-grn/:id",
    component: PoPriceAdjustmentAfterGrnFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "PO Price Adjustment After Grn Detail",
      breadcrumb: {
        title: "PO Price Adjustment After Grn List",
        url: "/purchase/po-price-adjustment-after-grn",
      },
    },
    resolve: {
      poPriceAdjustmentAfterGrn: PoPriceAdjustmentAfterGrnResolverService,
    },
  },

  // * supplier-payment
  {
    path: "supplier-payment",
    canMatch: [() => hasPermission(["Permissions.SupplierPayments.View"])],
    component: SupplierPaymentComponent,
    data: {
      module: "Purchase",
      pageTitle: "Supplier Payment List",
      breadcrumb: {
        title: "Supplier Payment",
        url: "/purchase/supplier-payment",
      },
    },
  },
  {
    path: "supplier-payment/add-new",
    canMatch: [() => hasPermission(["Permissions.SupplierPayments.Create"])],
    component: SupplierPaymentFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Add New Supplier Payment",
      breadcrumb: {
        title: "Supplier Payment List",
        url: "/purchase/supplier-payment",
      },
    },
  },
  {
    path: "supplier-payment/:id",
    component: SupplierPaymentFormComponent,
    data: {
      module: "Purchase",
      pageTitle: "Supplier Payment Detail",
      breadcrumb: {
        title: "Supplier Payment List",
        url: "/purchase/supplier-payment",
      },
    },
    resolve: { supplierPayment: SupplierPaymentResolverService },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class PurchaseRoutingModule {}
