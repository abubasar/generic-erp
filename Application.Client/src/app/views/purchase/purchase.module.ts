import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FlexLayoutModule } from "@angular/flex-layout";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { SharedComponentsModule } from "app/shared/components/shared-components.module";
import { SharedDirectivesModule } from "app/shared/directives/shared-directives.module";
import { SharedMaterialModule } from "app/shared/shared-material.module";
import { GoodsReceiveNoteDetailComponent } from "./components/goods-receive-note/goods-receive-note-detail/goods-receive-note-detail.component";
import { GoodsReceiveNoteFormComponent } from "./components/goods-receive-note/goods-receive-note-form/goods-receive-note-form.component";
import { GoodsReceiveNoteComponent } from "./components/goods-receive-note/goods-receive-note.component";
import { PurchaseOrderListComponent } from "./components/goods-receive-note/purchase-order-list/purchase-order-list.component";
import { GrnDialogComponent } from "./components/po-price-adjustment-after-grn/grn-dialog/grn-dialog.component";
import { PoPriceAdjustmentAfterGrnDetailComponent } from "./components/po-price-adjustment-after-grn/po-price-adjustment-after-grn-detail/po-price-adjustment-after-grn-detail.component";
import { PoPriceAdjustmentAfterGrnFormComponent } from "./components/po-price-adjustment-after-grn/po-price-adjustment-after-grn-form/po-price-adjustment-after-grn-form.component";
import { PoPriceAdjustmentAfterGrnComponent } from "./components/po-price-adjustment-after-grn/po-price-adjustment-after-grn.component";
import { GRNListComponent } from "./components/purchase-invoice/grn-list/grn-list.component";
import { PurchaseInvoiceDetailComponent } from "./components/purchase-invoice/purchase-invoice-detail/purchase-invoice-detail.component";
import { PurchaseInvoiceFormComponent } from "./components/purchase-invoice/purchase-invoice-form/purchase-invoice-form.component";
import { PurchaseInvoiceComponent } from "./components/purchase-invoice/purchase-invoice.component";
import { PurchaseOrderDetailComponent } from "./components/purchase-order/purchase-order-detail/purchase-order-detail.component";
import { PurchaseOrderFormComponent } from "./components/purchase-order/purchase-order-form/purchase-order-form.component";
import { PurchaseOrderComponent } from "./components/purchase-order/purchase-order.component";
import { SupplierTransactionsAgainstPOComponent } from "./components/purchase-order/supplier-transactions-against-po/supplier-transactions-against-po.component";
import { VendorQuotationListComponent } from "./components/purchase-order/vendor-quotation-list/vendor-quotation-list.component";
import { PurchaseRequisitionDetailComponent } from "./components/purchase-requisition/purchase-requisition-detail/purchase-requisition-detail.component";
import { PurchaseRequisitionFormComponent } from "./components/purchase-requisition/purchase-requisition-form/purchase-requisition-form.component";
import { PurchaseRequisitionComponent } from "./components/purchase-requisition/purchase-requisition.component";
import { RfqSentSupplierComponent } from "./components/purchase-requisition/rfq-sent-supplier/rfq-sent-supplier.component";
import { GRNDialogComponent } from "./components/purchase-return/grn-dialog/grn-dialog.component";
import { PurchaseReturnDetailComponent } from "./components/purchase-return/purchase-return-detail/purchase-return-detail.component";
import { PurchaseReturnFormComponent } from "./components/purchase-return/purchase-return-form/purchase-return-form.component";
import { PurchaseReturnComponent } from "./components/purchase-return/purchase-return.component";
import { SendRFQToVendorEmailComponent } from "./components/send-rfq-to-vendor/send-rfq-to-vendor-email/send-rfq-to-vendor-email.component";
import { SendRFQToVendorFormComponent } from "./components/send-rfq-to-vendor/send-rfq-to-vendor-form/send-rfq-to-vendor-form.component";
import { SendRFQToVendorComponent } from "./components/send-rfq-to-vendor/send-rfq-to-vendor.component";
import { PurchaseInvoiceDialogComponent } from "./components/supplier-payment/purchase-invoice-dialog/purchase-invoice-dialog.component";
import { PurchaseInvoiceListDetailComponent } from "./components/supplier-payment/purchase-invoice-dialog/purchase-invoice-list-detail/purchase-invoice-list-detail.component";
import { PurchaseOrderDialogComponent } from "./components/supplier-payment/purchase-order-dialog/purchase-order-dialog.component";
import { SupplierPaymentDetailComponent } from "./components/supplier-payment/supplier-payment-detail/supplier-payment-detail.component";
import { SupplierPaymentFormComponent } from "./components/supplier-payment/supplier-payment-form/supplier-payment-form.component";
import { SupplierPaymentComponent } from "./components/supplier-payment/supplier-payment.component";
import { PurchaseRequisitionListComponent } from "./components/vendor-quotation/purchase-requisition-list/purchase-requisition-list.component";
import { VendorQuotationDetailComponent } from "./components/vendor-quotation/vendor-quotation-detail/vendor-quotation-detail.component";
import { VendorQuotationFormComponent } from "./components/vendor-quotation/vendor-quotation-form/vendor-quotation-form.component";
import { VendorQuotationComponent } from "./components/vendor-quotation/vendor-quotation.component";
import { VendorQuoteComparisonComponent } from "./components/vendor-quotation/vendor-quote-comparison/vendor-quote-comparison.component";
import { PurchaseRoutingModule } from "./purchase-routing.module";
import { SupplierPaymentDialogComponent } from './components/purchase-invoice/supplier-payment-dialog/supplier-payment-dialog.component';
import { SupplierPaymentDialogDetailComponent } from './components/purchase-invoice/supplier-payment-dialog/supplier-payment-dialog-detail/supplier-payment-dialog-detail.component';
import { LcCostEntryComponent } from './components/lc-cost-entry/lc-cost-entry.component';
import { LcCostEntryFormComponent } from './components/lc-cost-entry/lc-cost-entry-form/lc-cost-entry-form.component';
import { LcCostPurchaseOrderDialogComponent } from './components/lc-cost-entry/lc-cost-purchase-order-dialog/lc-cost-purchase-order-dialog.component';
import { LcCostEntryDetailComponent } from './components/lc-cost-entry/lc-cost-entry-detail/lc-cost-entry-detail.component';
import { LcAdjustmentComponent } from './components/lc-adjustment/lc-adjustment.component';
import { LcAdjustmentFormComponent } from './components/lc-adjustment/lc-adjustment-form/lc-adjustment-form.component';
import { LcAdjustmentDetailComponent } from './components/lc-adjustment/lc-adjustment-detail/lc-adjustment-detail.component';
import { LcAdjustmentPurchaseInvoiceDialogComponent } from './components/lc-adjustment/lc-adjustment-purchase-invoice-dialog/lc-adjustment-purchase-invoice-dialog.component';
import { LcAdjustmentPurchaseInvoiceListDetailComponent } from './components/lc-adjustment/lc-adjustment-purchase-invoice-dialog/lc-adjustment-purchase-invoice-list-detail/lc-adjustment-purchase-invoice-list-detail.component';

@NgModule({
  declarations: [
    PurchaseRequisitionComponent,
    PurchaseRequisitionFormComponent,
    PurchaseRequisitionDetailComponent,
    SendRFQToVendorComponent,
    SendRFQToVendorEmailComponent,
    VendorQuotationComponent,
    PurchaseOrderComponent,
    PurchaseInvoiceComponent,
    VendorQuotationFormComponent,
    VendorQuotationDetailComponent,
    PurchaseRequisitionListComponent,
    VendorQuoteComparisonComponent,
    PurchaseOrderFormComponent,
    VendorQuotationListComponent,
    GoodsReceiveNoteComponent,
    GoodsReceiveNoteFormComponent,
    GoodsReceiveNoteDetailComponent,
    PurchaseOrderListComponent,
    PurchaseInvoiceFormComponent,
    PurchaseInvoiceDetailComponent,
    GRNListComponent,
    SendRFQToVendorFormComponent,
    RfqSentSupplierComponent,
    PurchaseReturnComponent,
    PurchaseReturnFormComponent,
    PurchaseReturnDetailComponent,
    SupplierPaymentComponent,
    SupplierPaymentFormComponent,
    SupplierPaymentDetailComponent,
    GRNDialogComponent,
    PurchaseOrderDetailComponent,
    PoPriceAdjustmentAfterGrnComponent,
    PoPriceAdjustmentAfterGrnFormComponent,
    PoPriceAdjustmentAfterGrnDetailComponent,
    GrnDialogComponent,
    PurchaseOrderDialogComponent,
    SupplierTransactionsAgainstPOComponent,
    PurchaseInvoiceDialogComponent,
    PurchaseInvoiceListDetailComponent,
    SupplierPaymentDialogComponent,
    SupplierPaymentDialogDetailComponent,
    LcCostEntryComponent,
    LcCostEntryFormComponent,
    LcCostPurchaseOrderDialogComponent,
    LcCostEntryDetailComponent,
    LcAdjustmentComponent,
    LcAdjustmentFormComponent,
    LcAdjustmentDetailComponent,
    LcAdjustmentPurchaseInvoiceDialogComponent,
    LcAdjustmentPurchaseInvoiceListDetailComponent,
  ],
  imports: [
    CommonModule,
    PurchaseRoutingModule,
    SharedMaterialModule,
    SharedComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    FlexLayoutModule,
    SharedDirectivesModule,
  ],
})
export class PurchaseModule {}
