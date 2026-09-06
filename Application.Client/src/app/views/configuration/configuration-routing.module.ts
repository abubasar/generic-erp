import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { hasPermission } from "app/shared/guards/has-permission.guard";
import { AccountTypeComponent } from "./components/account-type/account-type.component";
import { AccountComponent } from "./components/account/account.component";
import { AreaComponent } from "./components/area/area.component";
import { CategoryComponent } from "./components/category/category.component";
import { CompanyComponent } from "./components/company/company.component";
import { CostCenterComponent } from "./components/cost-center/cost-center.component";
import { CountryComponent } from "./components/country/country.component";
import { CurrencyComponent } from "./components/currency/currency.component";
import { CustomerWiseProductDiscountAddFormComponent } from "./components/customer-wise-product-discount/customer-wise-product-discount-add-form/customer-wise-product-discount-add-form.component";
import { CustomerWiseProductDiscountEditFormComponent } from "./components/customer-wise-product-discount/customer-wise-product-discount-edit-form/customer-wise-product-discount-edit-form.component";
import { CustomerWiseProductDiscountComponent } from "./components/customer-wise-product-discount/customer-wise-product-discount.component";
import { CustomerComponent } from "./components/customer/customer.component";
import { DeliveryPlaceComponent } from "./components/delivery-place/delivery-place.component";
import { DepartmentComponent } from "./components/department/department.component";
import { DesignationComponent } from "./components/designation/designation.component";
import { DiscountProductWiseComponent } from "./components/discount-product-wise/discount-product-wise.component";
import { EmailAccountComponent } from "./components/email-account/email-account.component";
import { EmployeeComponent } from "./components/employee/employee.component";
import { FinancialYearComponent } from "./components/financial-year/financial-year.component";
import { FundTransferTransactionTypeComponent } from "./components/fund-transfer-transaction-type/fund-transfer-transaction-type.component";
import { GenericComponent } from "./components/generic/generic.component";
import { InventoryTypeComponent } from "./components/inventory-type/inventory-type.component";
import { JobLocationComponent } from "./components/job-location/job-location.component";
import { MachineComponent } from "./components/machine/machine.component";
import { ManufacturerComponent } from "./components/manufacturer/manufacturer.component";
import { MeasurementUnitComponent } from "./components/measurement-unit/measurement-unit.component";
import { PackSizeComponent } from "./components/pack-size/pack-size.component";
import { PaymentMethodComponent } from "./components/payment-method/payment-method.component";
import { PaymentModeComponent } from "./components/payment-mode/payment-mode.component";
import { ProductCostSetupComponent } from "./components/product-cost-setup/product-cost-setup.component";
import { ProductTypeComponent } from "./components/product-type/product-type.component";
import { ProductComponent } from "./components/product/product.component";
import { RegionComponent } from "./components/region/region.component";
import { RoleComponent } from "./components/role/role.component";
import { ShiftComponent } from "./components/shift/shift.component";
import { StoreComponent } from "./components/store/store.component";
import { SupplierComponent } from "./components/supplier/supplier.component";
import { TenantComponent } from "./components/tenant/tenant.component";
import { TerritoryComponent } from "./components/territory/territory.component";
import { UserComponent } from "./components/user/user.component";
import { ZoneComponent } from "./components/zone/zone.component";
import { CustomerWiseProductDiscountResolverService } from "./resolvers/customer-wise-product-discount-resolver.service";

const routes: Routes = [
  {
    path: "account",
    canMatch: [() => hasPermission(["Permissions.Accounts.View"])],
    component: AccountComponent,
    data: {
      module: "Configuration",
      pageTitle: "Account List",
      breadcrumb: {
        title: "Account",
        url: "/configuration/account",
      },
    },
  },

  {
    path: "account-type",
    canMatch: [() => hasPermission(["Permissions.AccountTypes.View"])],
    component: AccountTypeComponent,
    data: {
      module: "Configuration",
      pageTitle: "Account Type List",
      breadcrumb: {
        title: "Account Type",
        url: "/configuration/account-type",
      },
    },
  },
  {
    path: "area",
    canMatch: [() => hasPermission(["Permissions.Areas.View"])],
    component: AreaComponent,
    data: {
      module: "Configuration",
      pageTitle: "Area List",
      breadcrumb: {
        title: "Area",
        url: "/configuration/area",
      },
    },
  },
  {
    path: "territory",
    canMatch: [() => hasPermission(["Permissions.Territories.View"])],
    component: TerritoryComponent,
    data: {
      module: "Configuration",
      pageTitle: "Territory List",
      breadcrumb: {
        title: "Territory",
        url: "/configuration/territory",
      },
    },
  },
  {
    path: "category",
    canMatch: [() => hasPermission(["Permissions.Categories.View"])],
    component: CategoryComponent,
    data: {
      module: "Configuration",
      pageTitle: "Category List",
      breadcrumb: {
        title: "Category",
        url: "/configuration/category",
      },
    },
  },
  {
    path: "company",
    canMatch: [() => hasPermission(["Permissions.Companies.View"])],
    component: CompanyComponent,
    data: {
      module: "Configuration",
      pageTitle: "Company List",
      breadcrumb: {
        title: "Company",
        url: "/configuration/company",
      },
    },
  },
  {
    path: "cost-center",
    canMatch: [() => hasPermission(["Permissions.CostCenters.View"])],
    component: CostCenterComponent,
    data: {
      module: "Configuration",
      pageTitle: "Cost Center List",
      breadcrumb: {
        title: "Cost Center",
        url: "/configuration/cost-center",
      },
    },
  },
  {
    path: "country",
    canMatch: [() => hasPermission(["Permissions.Countries.View"])],
    component: CountryComponent,
    data: {
      module: "Configuration",
      pageTitle: "Country List",
      breadcrumb: {
        title: "Country",
        url: "/configuration/country",
      },
    },
  },
  {
    path: "currency",
    canMatch: [() => hasPermission(["Permissions.Currencies.View"])],
    component: CurrencyComponent,
    data: {
      module: "Configuration",
      pageTitle: "Currency List",
      breadcrumb: {
        title: "Currency",
        url: "/configuration/currency",
      },
    },
  },
  {
    path: "customer",
    canMatch: [() => hasPermission(["Permissions.Customers.View"])],
    component: CustomerComponent,
    data: {
      module: "Configuration",
      pageTitle: "Customer List",
      breadcrumb: {
        title: "Customer",
        url: "/configuration/customer",
      },
    },
  },
  //* customer-wise-product-discount
  {
    path: "customer-wise-product-discount",
    canMatch: [
      () => hasPermission(["Permissions.CustomerWiseProductDiscounts.View"]),
    ],
    component: CustomerWiseProductDiscountComponent,
    data: {
      module: "Configuration",
      pageTitle: "Customer Wise Product Discount List",
      breadcrumb: {
        title: "Customer Wise Product Discount",
        url: "/configuration/customer-wise-product-discount",
      },
    },
  },
  {
    path: "customer-wise-product-discount/add-new",
    canMatch: [
      () => hasPermission(["Permissions.CustomerWiseProductDiscounts.Create"]),
    ],
    component: CustomerWiseProductDiscountAddFormComponent,
    data: {
      module: "Configuration",
      pageTitle: "Add New Customer Wise Product Discount",
      breadcrumb: {
        title: "Customer Wise Product Discount Order List",
        url: "/configuration/customer-wise-product-discount",
      },
    },
  },
  {
    path: "customer-wise-product-discount/:id",
    component: CustomerWiseProductDiscountEditFormComponent,
    data: {
      module: "Configuration",
      pageTitle: "Customer Wise Product Discount Detail",
      breadcrumb: {
        title: "Customer Wise Product Discount List",
        url: "/configuration/customer-wise-product-discount",
      },
    },
    resolve: {
      customerWiseProductDiscount: CustomerWiseProductDiscountResolverService,
    },
  },
  {
    path: "discount-product-wise",
    canMatch: [() => hasPermission(["Permissions.DiscountProductWises.View"])],
    component: DiscountProductWiseComponent,
    data: {
      module: "Configuration",
      pageTitle: "Discount Product Wise List",
      breadcrumb: {
        title: "Discount Product Wise",
        url: "/configuration/discount-product-wise",
      },
    },
  },
  {
    path: "delivery-place",
    canMatch: [() => hasPermission(["Permissions.DeliveryPlaces.View"])],
    component: DeliveryPlaceComponent,
    data: {
      module: "Configuration",
      pageTitle: "Delivery Place List",
      breadcrumb: {
        title: "Delivery Place",
        url: "/configuration/delivery-place",
      },
    },
  },
  {
    path: "department",
    canMatch: [() => hasPermission(["Permissions.Departments.View"])],
    component: DepartmentComponent,
    data: {
      module: "Configuration",
      pageTitle: "Department List",
      breadcrumb: {
        title: "Department",
        url: "/configuration/department",
      },
    },
  },
  {
    path: "designation",
    canMatch: [() => hasPermission(["Permissions.Designations.View"])],
    component: DesignationComponent,
    data: {
      module: "Configuration",
      pageTitle: "Designation List",
      breadcrumb: {
        title: "Designation",
        url: "/configuration/designation",
      },
    },
  },
  {
    path: "email-account",
    canMatch: [() => hasPermission(["Permissions.EmailAccounts.View"])],
    component: EmailAccountComponent,
    data: {
      module: "Configuration",
      pageTitle: "Email Account List",
      breadcrumb: {
        title: "Email Account",
        url: "/configuration/email-account",
      },
    },
  },
  {
    path: "employee",
    canMatch: [() => hasPermission(["Permissions.Employees.View"])],
    component: EmployeeComponent,
    data: {
      module: "Configuration",
      pageTitle: "Employee List",
      breadcrumb: {
        title: "Employee",
        url: "/configuration/employee",
      },
    },
  },
  {
    path: "financial-year",
    canMatch: [() => hasPermission(["Permissions.FinancialYears.View"])],
    component: FinancialYearComponent,
    data: {
      module: "Configuration",
      pageTitle: "Financial Year List",
      breadcrumb: {
        title: "Financial Year",
        url: "/configuration/financial-year",
      },
    },
  },
  {
    path: "fund-transfer-transaction-type",
    canMatch: [
      () => hasPermission(["Permissions.FundTransferTransactionTypes.View"]),
    ],
    component: FundTransferTransactionTypeComponent,
    data: {
      module: "Configuration",
      pageTitle: "Fund Transfer Transaction Type List",
      breadcrumb: {
        title: "Fund Transfer Transaction Type",
        url: "/configuration/fund-transfer-transaction-type",
      },
    },
  },
  {
    path: "generic",
    canMatch: [() => hasPermission(["Permissions.Generics.View"])],
    component: GenericComponent,
    data: {
      module: "Configuration",
      pageTitle: "Generic List",
      breadcrumb: {
        title: "Generic",
        url: "/configuration/generic",
      },
    },
  },
  {
    path: "inventory-type",
    canMatch: [() => hasPermission(["Permissions.InventoryTypes.View"])],
    component: InventoryTypeComponent,
    data: {
      module: "Configuration",
      pageTitle: "Inventory Type List",
      breadcrumb: {
        title: "Inventory Type",
        url: "/configuration/inventory-type",
      },
    },
  },
  {
    path: "job-location",
    canMatch: [() => hasPermission(["Permissions.JobLocations.View"])],
    component: JobLocationComponent,
    data: {
      module: "Configuration",
      pageTitle: "Job Location List",
      breadcrumb: {
        title: "Job Location",
        url: "/configuration/job-location",
      },
    },
  },
  {
    path: "measurement-unit",
    canMatch: [() => hasPermission(["Permissions.MeasurementUnits.View"])],
    component: MeasurementUnitComponent,
    data: {
      module: "Configuration",
      pageTitle: "Measurement Unit List",
      breadcrumb: {
        title: "Measurement Unit",
        url: "/configuration/measurement-unit",
      },
    },
  },
  {
    path: "pack-size",
    canMatch: [() => hasPermission(["Permissions.PackSizes.View"])],
    component: PackSizeComponent,
    data: {
      module: "Configuration",
      pageTitle: "Pack Size List",
      breadcrumb: {
        title: "Pack Size",
        url: "/configuration/pack-size",
      },
    },
  },
  {
    path: "payment-method",
    canMatch: [() => hasPermission(["Permissions.PaymentMethods.View"])],
    component: PaymentMethodComponent,
    data: {
      module: "Configuration",
      pageTitle: "Payment Method List",
      breadcrumb: {
        title: "Payment Method",
        url: "/configuration/payment-method",
      },
    },
  },
  {
    path: "payment-mode",
    canMatch: [() => hasPermission(["Permissions.PaymentModes.View"])],
    component: PaymentModeComponent,
    data: {
      module: "Configuration",
      pageTitle: "Payment Mode List",
      breadcrumb: {
        title: "Payment Mode",
        url: "/configuration/payment-mode",
      },
    },
  },
  {
    path: "manufacturer",
    canMatch: [() => hasPermission(["Permissions.Manufacturers.View"])],
    component: ManufacturerComponent,
    data: {
      module: "Configuration",
      pageTitle: "Manufacturer List",
      breadcrumb: {
        title: "Manufacturer",
        url: "/configuration/manufacturer",
      },
    },
  },
  {
    path: "product",
    canMatch: [() => hasPermission(["Permissions.Products.View"])],
    component: ProductComponent,
    data: {
      module: "Configuration",
      pageTitle: "Product List",
      breadcrumb: {
        title: "Product",
        url: "/configuration/product",
      },
    },
  },
  {
    path: "product-cost-setup",
    canMatch: [() => hasPermission(["Permissions.ProductCostSetups.View"])],
    component: ProductCostSetupComponent,
    data: {
      module: "Configuration",
      pageTitle: "Product Cost Setup List",
      breadcrumb: {
        title: "Product Cost Setup",
        url: "/configuration/product-cost-setup",
      },
    },
  },
  {
    path: "product-type",
    canMatch: [() => hasPermission(["Permissions.ProductTypes.View"])],
    component: ProductTypeComponent,
    data: {
      module: "Configuration",
      pageTitle: "Product Type List",
      breadcrumb: {
        title: "Product Type",
        url: "/configuration/product-type",
      },
    },
  },
  {
    path: "region",
    canMatch: [() => hasPermission(["Permissions.Regions.View"])],
    component: RegionComponent,
    data: {
      module: "Configuration",
      pageTitle: "Region List",
      breadcrumb: {
        title: "Region",
        url: "/configuration/region",
      },
    },
  },
  {
    path: "role",
    canMatch: [() => hasPermission(["Permissions.Roles.View"])],
    component: RoleComponent,
    data: {
      module: "Configuration",
      pageTitle: "Role List",
      breadcrumb: {
        title: "Role",
        url: "/configuration/role",
      },
    },
  },
  {
    path: "store",
    canMatch: [() => hasPermission(["Permissions.Stores.View"])],
    component: StoreComponent,
    data: {
      module: "Configuration",
      pageTitle: "Store List",
      breadcrumb: {
        title: "Store",
        url: "/configuration/store",
      },
    },
  },
  {
    path: "supplier",
    canMatch: [() => hasPermission(["Permissions.Suppliers.View"])],
    component: SupplierComponent,
    data: {
      module: "Configuration",
      pageTitle: "Supplier List",
      breadcrumb: {
        title: "Supplier",
        url: "/configuration/supplier",
      },
    },
  },
  {
    path: "tenant",
    canMatch: [() => hasPermission(["Permissions.Tenants.View"])],
    component: TenantComponent,
    data: {
      module: "Configuration",
      pageTitle: "Tenant List",
      breadcrumb: {
        title: "Tenant",
        url: "/configuration/tenant",
      },
    },
  },
  {
    path: "user",
    canMatch: [() => hasPermission(["Permissions.Users.View"])],
    component: UserComponent,
    data: {
      module: "Configuration",
      pageTitle: "User List",
      breadcrumb: {
        title: "User",
        url: "/configuration/user",
      },
    },
  },
  {
    path: "zone",
    canMatch: [() => hasPermission(["Permissions.Zones.View"])],
    component: ZoneComponent,
    data: {
      module: "Configuration",
      pageTitle: "Zone List",
      breadcrumb: {
        title: "Zone",
        url: "/configuration/zone",
      },
    },
  },
  {
    path: "shift",
    canMatch: [() => hasPermission(["Permissions.Shifts.View"])],
    component: ShiftComponent,
    data: {
      module: "Configuration",
      pageTitle: "Shift List",
      breadcrumb: {
        title: "Shift",
        url: "/configuration/shift",
      },
    },
  },
  {
    path: "machine",
    canMatch: [() => hasPermission(["Permissions.Machines.View"])],
    component: MachineComponent,
    data: {
      module: "Configuration",
      pageTitle: "Machine List",
      breadcrumb: {
        title: "Machine",
        url: "/configuration/machine",
      },
    },
  },
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class ConfigurationRoutingModule {}
