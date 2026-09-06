import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";

import { FlexLayoutModule } from "@angular/flex-layout";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { SharedComponentsModule } from "app/shared/components/shared-components.module";
import { SharedDirectivesModule } from "app/shared/directives/shared-directives.module";
import { SharedMaterialModule } from "app/shared/shared-material.module";
import { NgxMatTimepickerModule } from "ngx-mat-timepicker";
import { AccountTypeFormComponent } from "./components/account-type/account-type-form/account-type-form.component";
import { AccountTypeComponent } from "./components/account-type/account-type.component";
import { AccountFormComponent } from "./components/account/account-form/account-form.component";
import { AccountComponent } from "./components/account/account.component";
import { AreaFormComponent } from "./components/area/area-form/area-form.component";
import { AreaComponent } from "./components/area/area.component";
import { CompanyFormComponent } from "./components/company/company-form/company-form.component";
import { CompanyComponent } from "./components/company/company.component";
import { CostCenterFormComponent } from "./components/cost-center/cost-center-form/cost-center-form.component";
import { CostCenterComponent } from "./components/cost-center/cost-center.component";
import { CurrencyFormComponent } from "./components/currency/currency-form/currency-form.component";
import { CurrencyComponent } from "./components/currency/currency.component";
import { CustomerWiseProductDiscountAddFormComponent } from "./components/customer-wise-product-discount/customer-wise-product-discount-add-form/customer-wise-product-discount-add-form.component";
import { CustomerWiseProductDiscountDetailComponent } from "./components/customer-wise-product-discount/customer-wise-product-discount-detail/customer-wise-product-discount-detail.component";
import { CustomerWiseProductDiscountEditFormComponent } from "./components/customer-wise-product-discount/customer-wise-product-discount-edit-form/customer-wise-product-discount-edit-form.component";
import { CustomerWiseProductDiscountComponent } from "./components/customer-wise-product-discount/customer-wise-product-discount.component";
import { CustomerBankAccountComponent } from "./components/customer/customer-bank-account/customer-bank-account.component";
import { CustomerFormComponent } from "./components/customer/customer-form/customer-form.component";
import { CustomerComponent } from "./components/customer/customer.component";
import { DeliveryPlaceFormComponent } from "./components/delivery-place/delivery-place-form/delivery-place-form.component";
import { DeliveryPlaceComponent } from "./components/delivery-place/delivery-place.component";
import { DepartmentFormComponent } from "./components/department/department-form/department-form.component";
import { DepartmentComponent } from "./components/department/department.component";
import { DesignationFormComponent } from "./components/designation/designation-form/designation-form.component";
import { DesignationComponent } from "./components/designation/designation.component";
import { DiscountProductWiseComponent } from "./components/discount-product-wise/discount-product-wise.component";
import { EmailAccountFormComponent } from "./components/email-account/email-account-form/email-account-form.component";
import { EmailAccountComponent } from "./components/email-account/email-account.component";
import { EmployeeFormComponent } from "./components/employee/employee-form/employee-form.component";
import { EmployeeComponent } from "./components/employee/employee.component";
import { FinancialYearFormComponent } from "./components/financial-year/financial-year-form/financial-year-form.component";
import { FinancialYearComponent } from "./components/financial-year/financial-year.component";
import { FundTransferTransactionTypeFormComponent } from "./components/fund-transfer-transaction-type/fund-transfer-transaction-type-form/fund-transfer-transaction-type-form.component";
import { FundTransferTransactionTypeComponent } from "./components/fund-transfer-transaction-type/fund-transfer-transaction-type.component";
import { InventoryTypeFormComponent } from "./components/inventory-type/inventory-type-form/inventory-type-form.component";
import { InventoryTypeComponent } from "./components/inventory-type/inventory-type.component";
import { JobLocationFormComponent } from "./components/job-location/job-location-form/job-location-form.component";
import { JobLocationComponent } from "./components/job-location/job-location.component";
import { MachineFormComponent } from "./components/machine/machine-form/machine-form.component";
import { MachineComponent } from "./components/machine/machine.component";
import { MeasurementUnitFormComponent } from "./components/measurement-unit/measurement-unit-form/measurement-unit-form.component";
import { MeasurementUnitComponent } from "./components/measurement-unit/measurement-unit.component";
import { PaymentMethodFormComponent } from "./components/payment-method/payment-method-form/payment-method-form.component";
import { PaymentMethodComponent } from "./components/payment-method/payment-method.component";
import { PaymentModeFormComponent } from "./components/payment-mode/payment-mode-form/payment-mode-form.component";
import { PaymentModeComponent } from "./components/payment-mode/payment-mode.component";
import { ProductCostSetupFormComponent } from "./components/product-cost-setup/product-cost-setup-form/product-cost-setup-form.component";
import { ProductCostSetupComponent } from "./components/product-cost-setup/product-cost-setup.component";
import { ProductTypeFormComponent } from "./components/product-type/product-type-form/product-type-form.component";
import { ProductTypeComponent } from "./components/product-type/product-type.component";
import { ProductFormComponent } from "./components/product/product-form/product-form.component";
import { ProductComponent } from "./components/product/product.component";
import { RegionFormComponent } from "./components/region/region-form/region-form.component";
import { RegionComponent } from "./components/region/region.component";
import { RoleFormComponent } from "./components/role/role-form/role-form.component";
import { RolePermissionFormComponent } from "./components/role/role-permission-form/role-permission-form.component";
import { RoleComponent } from "./components/role/role.component";
import { ShiftFormComponent } from "./components/shift/shift-form/shift-form.component";
import { ShiftComponent } from "./components/shift/shift.component";
import { StoreFormComponent } from "./components/store/store-form/store-form.component";
import { StoreComponent } from "./components/store/store.component";
import { SupplierBankAccountComponent } from "./components/supplier/supplier-bank-account/supplier-bank-account.component";
import { SupplierFormComponent } from "./components/supplier/supplier-form/supplier-form.component";
import { SupplierComponent } from "./components/supplier/supplier.component";
import { UserFormComponent } from "./components/user/user-form/user-form.component";
import { UserComponent } from "./components/user/user.component";
import { ZoneFormComponent } from "./components/zone/zone-form/zone-form.component";
import { ZoneComponent } from "./components/zone/zone.component";
import { ConfigurationRoutingModule } from "./configuration-routing.module";

import { DiscountProductWiseAddFormComponent } from "./components/discount-product-wise/discount-product-wise-add-form/discount-product-wise-add-form.component";
import { DiscountProductWiseDetailComponent } from "./components/discount-product-wise/discount-product-wise-detail/discount-product-wise-detail.component";
import { DiscountProductWiseEditFormComponent } from "./components/discount-product-wise/discount-product-wise-edit-form/discount-product-wise-edit-form.component";
import { TenantComponent } from './components/tenant/tenant.component';
import { TenantFormComponent } from './components/tenant/tenant-form/tenant-form.component';
import { CountryComponent } from './components/country/country.component';
import { GenericComponent } from './components/generic/generic.component';
import { CountryFormComponent } from './components/country/country-form/country-form.component';
import { GenericFormComponent } from './components/generic/generic-form/generic-form.component';
import { ManufacturerComponent } from './components/manufacturer/manufacturer.component';
import { ManufacturerFormComponent } from './components/manufacturer/manufacturer-form/manufacturer-form.component';
import { TerritoryComponent } from './components/territory/territory.component';
import { TerritoryFormComponent } from './components/territory/territory-form/territory-form.component';
import { PackSizeComponent } from './components/pack-size/pack-size.component';
import { CategoryComponent } from './components/category/category.component';
import { CategoryFormComponent } from './components/category/category-form/category-form.component';
import { PackSizeFormComponent } from './components/pack-size/pack-size-form/pack-size-form.component';

@NgModule({
  declarations: [
    CompanyComponent,
    CompanyFormComponent,
    DepartmentComponent,
    DepartmentFormComponent,
    MeasurementUnitComponent,
    MeasurementUnitFormComponent,
    ProductComponent,
    ProductFormComponent,
    StoreComponent,
    StoreFormComponent,
    RoleComponent,
    RoleFormComponent,
    UserComponent,
    UserFormComponent,
    RolePermissionFormComponent,
    InventoryTypeComponent,
    ProductTypeComponent,
    InventoryTypeFormComponent,
    ProductTypeFormComponent,
    AccountTypeComponent,
    AccountComponent,
    SupplierComponent,
    AccountTypeFormComponent,
    AccountFormComponent,
    SupplierFormComponent,
    DesignationComponent,
    DesignationFormComponent,
    EmployeeComponent,
    EmployeeFormComponent,
    JobLocationComponent,
    JobLocationFormComponent,
    RegionComponent,
    RegionFormComponent,
    ZoneComponent,
    ZoneFormComponent,
    CustomerComponent,
    CustomerFormComponent,
    CostCenterComponent,
    CostCenterFormComponent,
    CurrencyComponent,
    CurrencyFormComponent,
    EmailAccountComponent,
    EmailAccountFormComponent,
    FinancialYearComponent,
    FinancialYearFormComponent,
    DeliveryPlaceComponent,
    DeliveryPlaceFormComponent,
    PaymentMethodComponent,
    PaymentMethodFormComponent,
    SupplierBankAccountComponent,
    CustomerBankAccountComponent,
    ProductCostSetupComponent,
    ProductCostSetupFormComponent,
    CustomerWiseProductDiscountComponent,
    CustomerWiseProductDiscountDetailComponent,
    CustomerWiseProductDiscountAddFormComponent,
    CustomerWiseProductDiscountEditFormComponent,
    PaymentModeComponent,
    PaymentModeFormComponent,
    AreaComponent,
    AreaFormComponent,
    ShiftComponent,
    ShiftFormComponent,
    MachineComponent,
    MachineFormComponent,
    FundTransferTransactionTypeComponent,
    FundTransferTransactionTypeFormComponent,
    DiscountProductWiseComponent,
    DiscountProductWiseDetailComponent,
    DiscountProductWiseAddFormComponent,
    DiscountProductWiseEditFormComponent,
    TenantComponent,
    TenantFormComponent,
    CountryComponent,
    GenericComponent,
    CountryFormComponent,
    GenericFormComponent,
    ManufacturerComponent,
    ManufacturerFormComponent,
    TerritoryComponent,
    TerritoryFormComponent,
    PackSizeComponent,
    CategoryComponent,
    CategoryFormComponent,
    PackSizeFormComponent
  ],
  imports: [
    CommonModule,
    ConfigurationRoutingModule,
    SharedMaterialModule,
    SharedComponentsModule,
    FormsModule,
    ReactiveFormsModule,
    FlexLayoutModule,
    SharedDirectivesModule,
    NgxMatTimepickerModule.setLocale("en-GB"),
  ],
})
export class ConfigurationModule {}
