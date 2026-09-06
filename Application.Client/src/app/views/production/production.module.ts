import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";

import { FlexLayoutModule } from "@angular/flex-layout";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { SharedComponentsModule } from "app/shared/components/shared-components.module";
import { SharedDirectivesModule } from "app/shared/directives/shared-directives.module";
import { SharedMaterialModule } from "app/shared/shared-material.module";
import { BillOfMaterialDetailComponent } from "./components/bill-of-material/bill-of-material-detail/bill-of-material-detail.component";
import { BillOfMaterialFormComponent } from "./components/bill-of-material/bill-of-material-form/bill-of-material-form.component";
import { BillOfMaterialComponent } from "./components/bill-of-material/bill-of-material.component";
import { BillOfMaterialListComponent } from "./components/manufacturing-order/bill-of-material-list/bill-of-material-list.component";
import { ManufacturingOrderDetailComponent } from "./components/manufacturing-order/manufacturing-order-detail/manufacturing-order-detail.component";
import { ManufacturingOrderFormComponent } from "./components/manufacturing-order/manufacturing-order-form/manufacturing-order-form.component";
import { ManufacturingOrderComponent } from "./components/manufacturing-order/manufacturing-order.component";
import { ManufacturingOrderListComponent } from "./components/production/manufacturing-order-list/manufacturing-order-list.component";
import { ProductionDetailComponent } from "./components/production/production-detail/production-detail.component";
import { ProductionFormComponent } from "./components/production/production-form/production-form.component";
import { ProductionComponent } from "./components/production/production.component";
import { ProductionRoutingModule } from "./production-routing.module";
import {NgxMatTimepickerModule} from "ngx-mat-timepicker";
import { NgxMatDatetimePickerModule, NgxMatNativeDateModule } from "@angular-material-components/datetime-picker";

@NgModule({
  declarations: [
    BillOfMaterialComponent,
    BillOfMaterialDetailComponent,
    BillOfMaterialFormComponent,
    ManufacturingOrderComponent,
    ManufacturingOrderFormComponent,
    ManufacturingOrderDetailComponent,
    BillOfMaterialListComponent,
    ProductionComponent,
    ProductionFormComponent,
    ProductionDetailComponent,
    ManufacturingOrderListComponent,
  ],
  imports: [
    CommonModule,
    ProductionRoutingModule,
    SharedMaterialModule,
    SharedComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    FlexLayoutModule,
    SharedDirectivesModule,
    NgxMatTimepickerModule.setLocale("en-GB"),
    NgxMatDatetimePickerModule,
    NgxMatNativeDateModule,
  ],
})
export class ProductionModule {}
