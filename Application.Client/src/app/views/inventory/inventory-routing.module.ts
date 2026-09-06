import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { hasPermission } from "app/shared/guards/has-permission.guard";
import { StockAdjustmentFormComponent } from "./components/stock-adjustment/stock-adjustment-form/stock-adjustment-form.component";
import { StockAdjustmentComponent } from "./components/stock-adjustment/stock-adjustment.component";
import { StockTransferFormComponent } from "./components/stock-transfer/stock-transfer-form/stock-transfer-form.component";
import { StockTransferComponent } from "./components/stock-transfer/stock-transfer.component";
import { StockAdjustmentResolverService } from "./resolvers/stock-adjustment-resolver.service";
import { StockTransferResolverService } from "./resolvers/stock-transfer-resolver.service";

const routes: Routes = [
  //stock-adjustment
  {
    path: "stock-adjustment",
    canMatch: [() => hasPermission(["Permissions.StockAdjustments.View"])],
    component: StockAdjustmentComponent,
    data: {
      module: "Inventory",
      pageTitle: "Stock Adjustment List",
      breadcrumb: {
        title: "Stock Adjustment",
        url: "/inventory/stock-adjustment",
      },
    },
  },
  {
    path: "stock-adjustment/add-new",
    canMatch: [() => hasPermission(["Permissions.StockAdjustments.Create"])],
    component: StockAdjustmentFormComponent,
    data: {
      module: "Inventory",
      pageTitle: "Add New Stock Adjustment",
      breadcrumb: {
        title: "Stock Adjustment List",
        url: "/inventory/stock-adjustment",
      },
    },
  },
  {
    path: "stock-adjustment/:id",
    component: StockAdjustmentFormComponent,
    data: {
      module: "Inventory",
      pageTitle: "Stock Adjustment Detail",
      breadcrumb: {
        title: "Stock Adjustment List",
        url: "/inventory/stock-adjustment",
      },
    },
    resolve: { stockAdjustment: StockAdjustmentResolverService },
  },
  // * Stock Transfer
  {
    path: "stock-transfer",
    canMatch: [() => hasPermission(["Permissions.StockTransfers.View"])],
    component: StockTransferComponent,
    data: {
      module: "Inventory",
      pageTitle: "Stock Transfer List",
      breadcrumb: {
        title: "Stock Transfer",
        url: "/inventory/stock-transfer",
      },
    },
  },
  {
    path: "stock-transfer/add-new",
    canMatch: [() => hasPermission(["Permissions.StockTransfers.Create"])],
    component: StockTransferFormComponent,
    data: {
      module: "Inventory",
      pageTitle: "Add New Stock Transfer",
      breadcrumb: {
        title: "Stock Transfer List",
        url: "/inventory/stock-transfer",
      },
    },
  },
  {
    path: "stock-transfer/:id",
    component: StockTransferFormComponent,
    data: {
      module: "Inventory",
      pageTitle: "Stock Transfer Detail",
      breadcrumb: {
        title: "Stock Transfer List",
        url: "/inventory/stock-transfer",
      },
    },
    resolve: { stockTransfer: StockTransferResolverService },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class InventoryRoutingModule {}
