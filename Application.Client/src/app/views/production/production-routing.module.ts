import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { hasPermission } from "app/shared/guards/has-permission.guard";
import { BillOfMaterialFormComponent } from "./components/bill-of-material/bill-of-material-form/bill-of-material-form.component";
import { BillOfMaterialComponent } from "./components/bill-of-material/bill-of-material.component";
import { ManufacturingOrderFormComponent } from "./components/manufacturing-order/manufacturing-order-form/manufacturing-order-form.component";
import { ManufacturingOrderComponent } from "./components/manufacturing-order/manufacturing-order.component";
import { ProductionFormComponent } from "./components/production/production-form/production-form.component";
import { ProductionComponent } from "./components/production/production.component";
import { BillOfMaterialResolverService } from "./resolvers/bill-of-material-resolver.service";
import { ManufacturingOrderResolverService } from "./resolvers/manufacturing-order-resolver.service";
import { ProductionResolverService } from "./resolvers/production-resolver.service";

const routes: Routes = [
  //bill-of-material
  {
    path: "bill-of-material",
    canMatch: [() => hasPermission(["Permissions.BillOfMaterials.View"])],
    component: BillOfMaterialComponent,
    data: {
      module: "Production",
      pageTitle: "Bill of Material List",
      breadcrumb: {
        title: "Bill of Material",
        url: "/production/bill-of-material",
      },
    },
  },
  {
    path: "bill-of-material/add-new",
    canMatch: [() => hasPermission(["Permissions.BillOfMaterials.Create"])],
    component: BillOfMaterialFormComponent,
    data: {
      module: "Production",
      pageTitle: "Add New Bill of Material",
      breadcrumb: {
        title: "Bill of Material List",
        url: "/production/bill-of-material",
      },
    },
  },
  {
    path: "bill-of-material/:id",
    component: BillOfMaterialFormComponent,
    data: {
      module: "Production",
      pageTitle: "Bill of Material Detail",
      breadcrumb: {
        title: "Bill of Material List",
        url: "/production/bill-of-material",
      },
    },
    resolve: { billOfMaterial: BillOfMaterialResolverService },
  },
  //manufacturing-order
  {
    path: "manufacturing-order",
    canMatch: [() => hasPermission(["Permissions.ManufacturingOrders.View"])],
    component: ManufacturingOrderComponent,
    data: {
      module: "Production",
      pageTitle: "Manufacturing Order List",
      breadcrumb: {
        title: "Manufacturing Order",
        url: "/production/manufacturing-order",
      },
    },
  },
  {
    path: "manufacturing-order/add-new",
    canMatch: [() => hasPermission(["Permissions.ManufacturingOrders.Create"])],
    component: ManufacturingOrderFormComponent,
    data: {
      module: "Production",
      pageTitle: "Add New Manufacturing Order",
      breadcrumb: {
        title: "Manufacturing Order List",
        url: "/production/manufacturing-order",
      },
    },
  },
  {
    path: "manufacturing-order/:id",
    component: ManufacturingOrderFormComponent,
    data: {
      module: "Production",
      pageTitle: "Manufacturing Order Detail",
      breadcrumb: {
        title: "Manufacturing Order List",
        url: "/production/manufacturing-order",
      },
    },
    resolve: { manufacturingOrder: ManufacturingOrderResolverService },
  },
  //production
  {
    path: "production",
    canMatch: [() => hasPermission(["Permissions.Productions.View"])],
    component: ProductionComponent,
    data: {
      module: "Production",
      pageTitle: "Production List",
      breadcrumb: {
        title: "Production",
        url: "/production/production",
      },
    },
  },
  {
    path: "production/add-new",
    canMatch: [() => hasPermission(["Permissions.Productions.Create"])],
    component: ProductionFormComponent,
    data: {
      module: "Production",
      pageTitle: "Add New Production",
      breadcrumb: {
        title: "Production List",
        url: "/production/production",
      },
    },
  },
  {
    path: "production/:id",
    component: ProductionFormComponent,
    data: {
      module: "Production",
      pageTitle: "Production Detail",
      breadcrumb: {
        title: "Production List",
        url: "/production/production",
      },
    },
    resolve: { production: ProductionResolverService },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ProductionRoutingModule {}
