import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";

import { FlexLayoutModule } from "@angular/flex-layout";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { SharedComponentsModule } from "app/shared/components/shared-components.module";
import { SharedDirectivesModule } from "app/shared/directives/shared-directives.module";
import { SharedMaterialModule } from "app/shared/shared-material.module";
import { StockAdjustmentDetailComponent } from "./components/stock-adjustment/stock-adjustment-detail/stock-adjustment-detail.component";
import { StockAdjustmentFormComponent } from "./components/stock-adjustment/stock-adjustment-form/stock-adjustment-form.component";
import { StockAdjustmentComponent } from "./components/stock-adjustment/stock-adjustment.component";
import { InventoryRoutingModule } from "./inventory-routing.module";
import { StockTransferComponent } from './components/stock-transfer/stock-transfer.component';
import { StockTransferFormComponent } from './components/stock-transfer/stock-transfer-form/stock-transfer-form.component';
import { StockTransferDetailComponent } from './components/stock-transfer/stock-transfer-detail/stock-transfer-detail.component';

@NgModule({
  declarations: [
    StockAdjustmentComponent,
    StockAdjustmentFormComponent,
    StockAdjustmentDetailComponent,
    StockTransferComponent,
    StockTransferFormComponent,
    StockTransferDetailComponent,
  ],
  imports: [
    CommonModule,
    InventoryRoutingModule,
    SharedMaterialModule,
    SharedComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    FlexLayoutModule,
    SharedDirectivesModule,
  ],
})
export class InventoryModule {}
