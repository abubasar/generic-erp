import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FlexLayoutModule } from "@angular/flex-layout";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { SharedComponentsModule } from "app/shared/components/shared-components.module";
import { SharedDirectivesModule } from "app/shared/directives/shared-directives.module";
import { SharedMaterialModule } from "app/shared/shared-material.module";
import { DeliveryNoteDetailComponent } from "./components/delivery-note/delivery-note-detail/delivery-note-detail.component";
import { DeliveryNoteFormComponent } from "./components/delivery-note/delivery-note-form/delivery-note-form.component";
import { DeliveryNoteComponent } from "./components/delivery-note/delivery-note.component";
import { SalesOrderListDetailComponent } from "./components/delivery-note/sales-order-list/sales-order-list-detail/sales-order-list-detail.component";
import { SalesOrderListComponent } from "./components/delivery-note/sales-order-list/sales-order-list.component";
import { DeliveryNoteListComponent } from "./components/sale-invoice/delivery-note-list/delivery-note-list.component";
import { SaleInvoiceDetailComponent } from "./components/sale-invoice/sale-invoice-detail/sale-invoice-detail.component";
import { SaleInvoiceFormComponent } from "./components/sale-invoice/sale-invoice-form/sale-invoice-form.component";
import { SaleInvoiceComponent } from "./components/sale-invoice/sale-invoice.component";
import { DeliveryNoteDialogComponent } from "./components/sale-return/delivery-note-dialog/delivery-note-dialog.component";
import { SaleReturnDetailComponent } from "./components/sale-return/sale-return-detail/sale-return-detail.component";
import { SaleReturnFormComponent } from "./components/sale-return/sale-return-form/sale-return-form.component";
import { SaleReturnComponent } from "./components/sale-return/sale-return.component";
import { MoneyReceiptDetailComponent } from "./components/sales-order/money-receipt-list/money-receipt-detail/money-receipt-detail.component";
import { MoneyReceiptListComponent } from "./components/sales-order/money-receipt-list/money-receipt-list.component";
import { SalesOrderDetailComponent } from "./components/sales-order/sales-order-detail/sales-order-detail.component";
import { SalesOrderFormComponent } from "./components/sales-order/sales-order-form/sales-order-form.component";
import { SalesOrderComponent } from "./components/sales-order/sales-order.component";
import { SalesQuotationListDetailComponent } from "./components/sales-order/sales-quotation-list/sales-quotation-list-detail/sales-quotation-list-detail.component";
import { SalesQuotationListComponent } from "./components/sales-order/sales-quotation-list/sales-quotation-list.component";
import { SaleQuotationDetailComponent } from "./components/sales-quotation/sales-quotation-detail/sales-quotation-detail.component";
import { SaleQuotationFormComponent } from "./components/sales-quotation/sales-quotation-form/sales-quotation-form.component";
import { SalesQuotationComponent } from "./components/sales-quotation/sales-quotation.component";
import { SalesRoutingModule } from "./sales-routing.module";
import { ReceivePaymentComponent } from './components/receive-payment/receive-payment.component';
import { ReceivePaymentFormComponent } from './components/receive-payment/receive-payment-form/receive-payment-form.component';
import { ReceivePaymentDetailComponent } from './components/receive-payment/receive-payment-detail/receive-payment-detail.component';
import { ReceivePaymentAgainstSaleComponent } from "./components/receive-payment-against-sale/receive-payment-against-sale.component";
import { ReceivePaymentAgainstSaleFormComponent } from "./components/receive-payment-against-sale/receive-payment-against-sale-form/receive-payment-against-sale-form.component";
import { SaleInvoiceListComponent } from "./components/receive-payment-against-sale/sale-invoice-list/sale-invoice-list.component";
import { SaleInvoiceListDetailComponent } from "./components/receive-payment-against-sale/sale-invoice-list/sale-invoice-list-detail/sale-invoice-list-detail.component";
import { SaleInvoiceDialogComponent } from './components/sale-return/sale-invoice-dialog/sale-invoice-dialog.component';
@NgModule({
  declarations: [
    SalesQuotationComponent,
    SalesOrderComponent,
    DeliveryNoteComponent,
    SaleQuotationDetailComponent,
    SaleQuotationFormComponent,
    SalesOrderFormComponent,
    SalesOrderDetailComponent,
    DeliveryNoteFormComponent,
    DeliveryNoteDetailComponent,
    SalesQuotationListComponent,
    SalesQuotationListDetailComponent,
    SalesOrderListComponent,
    SalesOrderListDetailComponent,
    SaleReturnComponent,
    SaleReturnFormComponent,
    SaleReturnDetailComponent,
    SaleInvoiceComponent,
    SaleInvoiceFormComponent,
    SaleInvoiceDetailComponent,
    DeliveryNoteListComponent,
    DeliveryNoteDialogComponent,
    MoneyReceiptListComponent,
    MoneyReceiptDetailComponent,
    ReceivePaymentComponent,
    ReceivePaymentFormComponent,
    ReceivePaymentDetailComponent,
    ReceivePaymentAgainstSaleComponent,
    ReceivePaymentAgainstSaleFormComponent,
    SaleInvoiceListComponent,
    SaleInvoiceListDetailComponent,
    SaleInvoiceDialogComponent,
  ],
  imports: [
    CommonModule,
    SalesRoutingModule,
    SharedMaterialModule,
    SharedComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    FlexLayoutModule,
    SharedDirectivesModule,
  ],
})
export class SalesModule {}
