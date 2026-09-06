import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";

import { FlexLayoutModule } from "@angular/flex-layout";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { SharedComponentsModule } from "app/shared/components/shared-components.module";
import { SharedDirectivesModule } from "app/shared/directives/shared-directives.module";
import { SharedMaterialModule } from "app/shared/shared-material.module";
import { AccountsRoutingModule } from "./accounts-routing.module";
import { AccountsChartComponent } from "./components/accounts-chart/accounts-chart.component";
import { FundTransferFormComponent } from "./components/fund-transfer/fund-transfer-form/fund-transfer-form.component";
import { FundTransferComponent } from "./components/fund-transfer/fund-transfer.component";
import { JournalEntryDetailComponent } from "./components/journal-entry/journal-entry-detail/journal-entry-detail.component";
import { JournalEntryFormComponent } from "./components/journal-entry/journal-entry-form/journal-entry-form.component";
import { JournalEntryComponent } from "./components/journal-entry/journal-entry.component";
import { PaymentVoucherDetailComponent } from "./components/payment-voucher/payment-voucher-detail/payment-voucher-detail.component";
import { PaymentVoucherFormComponent } from "./components/payment-voucher/payment-voucher-form/payment-voucher-form.component";
import { PaymentVoucherComponent } from "./components/payment-voucher/payment-voucher.component";
import { ReceiveVoucherDetailComponent } from "./components/receive-voucher/receive-voucher-detail/receive-voucher-detail.component";
import { ReceiveVoucherFormComponent } from "./components/receive-voucher/receive-voucher-form/receive-voucher-form.component";
import { ReceiveVoucherComponent } from "./components/receive-voucher/receive-voucher.component";
import { VoucherEntryDetailComponent } from "./components/voucher-entry/voucher-entry-detail/voucher-entry-detail.component";
import { VoucherEntryFormComponent } from "./components/voucher-entry/voucher-entry-form/voucher-entry-form.component";
import { VoucherEntryComponent } from "./components/voucher-entry/voucher-entry.component";

@NgModule({
  declarations: [
    AccountsChartComponent,
    VoucherEntryComponent,
    VoucherEntryFormComponent,
    JournalEntryComponent,
    JournalEntryFormComponent,
    VoucherEntryDetailComponent,
    JournalEntryDetailComponent,
    PaymentVoucherComponent,
    PaymentVoucherFormComponent,
    PaymentVoucherDetailComponent,
    FundTransferComponent,
    FundTransferFormComponent,
    ReceiveVoucherComponent,
    ReceiveVoucherFormComponent,
    ReceiveVoucherDetailComponent,
  ],
  imports: [
    CommonModule,
    AccountsRoutingModule,
    SharedMaterialModule,
    SharedComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    FlexLayoutModule,
    SharedDirectivesModule,
  ],
})
export class AccountsModule {}
