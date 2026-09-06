import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";

interface IMenuItem {
  type: "link" | "dropDown" | "icon" | "separator" | "extLink";
  name?: string; // Used as display text for item and title for separator type
  state?: string; // Router state
  permission?: string;
  icon?: string; // Material icon name
  svgIcon?: string; // UI Lib icon name
  tooltip?: string; // Tooltip text
  disabled?: boolean; // If true, item will not be appeared in sidenav.
  sub?: IChildItem[]; // Dropdown items
  badges?: IBadge[];
}
interface IChildItem {
  type?: string;
  name: string; // Display text
  state?: string; // Router state
  permission?: string;
  icon?: string; // Material icon name
  svgIcon?: string; // UI Lib icon name
  sub?: IChildItem[];
}

interface IBadge {
  color: string; // primary/accent/warn/hex color codes(#fff000)
  value: string; // Display text
}

@Injectable()
export class NavigationService {
  iconMenu: IMenuItem[] = [
    {
      name: "DASHBOARD",
      state: "dashboard/analytics",
      type: "link",
      icon: "dashboard",
    },
    {
      name: "Configuration",
      type: "dropDown",
      tooltip: "Configuration",
      icon: "list",
      permission: "Permissions.AccessModules.Configuration",
      sub: [
        {
          name: "Account Type",
          state: "configuration/account-type",
          permission: "Permissions.AccountTypes.View",
        },
        {
          name: "Account",
          state: "configuration/account",
          permission: "Permissions.Accounts.View",
        },

        // {
        //   name: "Company",
        //   state: "configuration/company",
        //   permission: "Permissions.Companies.View",
        // },
        {
          name: "Category",
          state: "configuration/category",
          permission: "Permissions.Categories.View",
        },
        {
          name: "Cost Center",
          state: "configuration/cost-center",
          permission: "Permissions.CostCenters.View",
        },
        {
          name: "Country",
          state: "configuration/country",
          permission: "Permissions.Countries.View",
        },
        {
          name: "Store",
          state: "configuration/store",
          permission: "Permissions.Stores.View",
        },
        {
          name: "Currency",
          state: "configuration/currency",
          permission: "Permissions.Currencies.View",
        },

        {
          name: "Delivery Place",
          state: "configuration/delivery-place",
          permission: "Permissions.DeliveryPlaces.View",
        },

        {
          name: "Email Account",
          state: "configuration/email-account",
          permission: "Permissions.EmailAccounts.View",
        },
        {
          name: "Department",
          state: "configuration/department",
          permission: "Permissions.Departments.View",
        },
        {
          name: "Designation",
          state: "configuration/designation",
          permission: "Permissions.Designations.View",
        },
        {
          name: "Employee",
          state: "configuration/employee",
          permission: "Permissions.Employees.View",
        },
        {
          name: "Financial Year",
          state: "configuration/financial-year",
          permission: "Permissions.FinancialYears.View",
        },
        {
          name: "Fund Transfer Transaction Type",
          state: "configuration/fund-transfer-transaction-type",
          permission: "Permissions.FundTransferTransactionTypes.View",
        },
        {
          name: "Job Location",
          state: "configuration/job-location",
          permission: "Permissions.JobLocations.View",
        },
        {
          name: "Generic",
          state: "configuration/generic",
          permission: "Permissions.Generics.View",
        },
        {
          name: "Pack Size",
          state: "configuration/pack-size",
          permission: "Permissions.PackSizes.View",
        },
        {
          name: "Payment Method",
          state: "configuration/payment-method",
          permission: "Permissions.PaymentMethods.View",
        },
        {
          name: "Payment Mode",
          state: "configuration/payment-mode",
          permission: "Permissions.PaymentModes.View",
        },
        {
          name: "Measurement Unit",
          state: "configuration/measurement-unit",
          permission: "Permissions.MeasurementUnits.View",
        },
        {
          name: "Inventory Type",
          state: "configuration/inventory-type",
          permission: "Permissions.InventoryTypes.View",
        },
        {
          name: "Product Type",
          state: "configuration/product-type",
          permission: "Permissions.ProductTypes.View",
        },
        {
          name: "Manufacturer",
          state: "configuration/manufacturer",
          permission: "Permissions.Manufacturers.View",
        },
        {
          name: "Product",
          state: "configuration/product",
          permission: "Permissions.Products.View",
        },
        {
          name: "Product Cost Setup",
          state: "configuration/product-cost-setup",
          permission: "Permissions.ProductCostSetups.View",
        },

        {
          name: "Shift",
          state: "configuration/shift",
          permission: "Permissions.Shifts.View",
        },

        {
          name: "Machine",
          state: "configuration/machine",
          permission: "Permissions.Machines.View",
        },

        {
          name: "Region",
          state: "configuration/region",
          permission: "Permissions.Regions.View",
        },
        {
          name: "Zone",
          state: "configuration/zone",
          permission: "Permissions.Zones.View",
        },
        {
          name: "Area",
          state: "configuration/area",
          permission: "Permissions.Areas.View",
        },
        {
          name: "Territory",
          state: "configuration/territory",
          permission: "Permissions.Territories.View",
        },
        {
          name: "Customer",
          state: "configuration/customer",
          permission: "Permissions.Customers.View",
        },
        {
          name: "Customer Wise Product Discount",
          state: "configuration/customer-wise-product-discount",
          permission: "Permissions.CustomerWiseProductDiscounts.View",
        },
        {
          name: "Discount Product Wise",
          state: "configuration/discount-product-wise",
          permission: "Permissions.DiscountProductWises.View",
        },
        {
          name: "Supplier",
          state: "configuration/supplier",
          permission: "Permissions.Suppliers.View",
        },
        {
          name: "Role",
          state: "configuration/role",
          permission: "Permissions.Roles.View",
        },
        {
          name: "User",
          state: "configuration/user",
          permission: "Permissions.Users.View",
        },
        // {
        //   name: "File Upload",
        //   state: "configuration/multiple-file-upload",
        //   permission: "Permissions.ReceivePayments.MultipleFileUpload",
        // },
        {
          name: "Tenant",
          state: "configuration/tenant",
          permission: "Permissions.Tenants.View",
        },
      ],
    },
    {
      name: "Purchase",
      type: "dropDown",
      tooltip: "Purchase",
      icon: "list",
      permission: "Permissions.AccessModules.Purchase",
      sub: [
        {
          name: "Purchase Requisition",
          state: "purchase/purchase-requisition",
          permission: "Permissions.PurchaseRequisitions.View",
        },
        {
          name: "Send RFQ to Vendor",
          state: "purchase/send-rfq-to-vendor",
          permission: "Permissions.PurchaseRequisitions.View",
        },
        {
          name: "Manage Vendor Quotation",
          state: "purchase/manage-vendor-quotation",
          permission: "Permissions.VendorQuotations.View",
        },
        {
          name: "Purchase Order (PO)",
          state: "purchase/purchase-order",
          permission: "Permissions.PurchaseOrders.View",
        },
        {
          name: "Goods Receive Note (GRN)",
          state: "purchase/goods-receive-note",
          permission: "Permissions.GoodsReceiveNotes.View",
        },
        {
          name: "Purchase Invoice",
          state: "purchase/purchase-invoice",
          permission: "Permissions.PurchaseInvoices.View",
        },
        {
          name: "Supplier Payment",
          state: "purchase/supplier-payment",
          permission: "Permissions.SupplierPayments.View",
        },
        {
          name: "Purchase Return",
          state: "purchase/purchase-return",
          permission: "Permissions.PurchaseReturns.View",
        },
        {
          name: "PO Price Adjustment After Grn",
          state: "purchase/po-price-adjustment-after-grn",
          permission: "Permissions.PoPriceAdjustmentAfterGrns.View",
        },
        {
          name: "LC Cost Entry",
          state: "purchase/lc-cost-entry",
          permission: "Permissions.LCCostEntries.View",
        },
        {
          name: "LC Adjustment",
          state: "purchase/lc-adjustment",
          permission: "Permissions.LcAdjustments.View",
        },
      ],
    },
    {
      name: "Production",
      type: "dropDown",
      tooltip: "Production",
      icon: "list",
      permission: "Permissions.AccessModules.Production",
      sub: [
        {
          name: "Bill of Material",
          state: "production/bill-of-material",
          permission: "Permissions.BillOfMaterials.View",
        },
        {
          name: "Manufacturing Order",
          state: "production/manufacturing-order",
          permission: "Permissions.ManufacturingOrders.View",
        },
        {
          name: "Production",
          state: "production/production",
          permission: "Permissions.Productions.View",
        },
      ],
    },
    {
      name: "Sales",
      type: "dropDown",
      tooltip: "Sales",
      icon: "list",
      permission: "Permissions.AccessModules.Sales",
      sub: [
        {
          name: "Sales Quotation",
          state: "sales/sales-quotation",
          permission: "Permissions.SaleQuotations.View",
        },
        {
          name: "Sales Order",
          state: "sales/sales-order",
          permission: "Permissions.SaleOrders.View",
        },
        {
          name: "Delivery Note",
          state: "sales/delivery-note",
          permission: "Permissions.DeliveryNotes.View",
        },
        {
          name: "Sale Invoice",
          state: "sales/sale-invoice",
          permission: "Permissions.SaleInvoices.View",
        },
        {
          name: "Receive Payment Against Sale",
          state: "sales/receive-payment-against-sale",
          permission: "Permissions.ReceivePaymentAgainstSales.View",
        },
        {
          name: "Money Receipt",
          state: "sales/money-receipt",
          permission: "Permissions.ReceivePayments.View",
        },
        {
          name: "Sale Return",
          state: "sales/sale-return",
          permission: "Permissions.SaleReturns.View",
        },
      ],
    },
    {
      name: "Inventory",
      type: "dropDown",
      tooltip: "Inventory",
      icon: "list",
      permission: "Permissions.AccessModules.Inventory",
      sub: [
        {
          name: "Stock Adjustment",
          state: "inventory/stock-adjustment",
          permission: "Permissions.StockAdjustments.View",
        },
        {
          name: "Stock Transfer",
          state: "inventory/stock-transfer",
          permission: "Permissions.StockTransfers.View",
        },
      ],
    },
    {
      name: "Accounts",
      type: "dropDown",
      tooltip: "Accounts",
      icon: "list",
      permission: "Permissions.AccessModules.Accounts",
      sub: [
        {
          name: "Chart of Accounts",
          state: "accounts/accounts-chart",
          permission: "Permissions.Accounts.View",
        },
        {
          name: "Fund Transfer",
          state: "accounts/fund-transfer",
          permission: "Permissions.FundTransfers.View",
        },
        // {
        //   name: "Voucher Entry",
        //   state: "accounts/voucher-entry",
        //   permission: "Permissions.VoucherEntries.View",
        // },
        {
          name: "Cash Payment Voucher",
          state: "accounts/cash-payment-voucher",
          permission: "Permissions.PaymentVouchers.View",
        },
        {
          name: "Cash Receipt Voucher",
          state: "accounts/cash-receipt-voucher",
          permission: "Permissions.ReceiveVouchers.View",
        },
        {
          name: "Journal Entry",
          state: "accounts/journal-entry",
          permission: "Permissions.JournalEntries.View",
        },
      ],
    },
    {
      name: "Report",
      type: "dropDown",
      tooltip: "Report",
      icon: "list",
      permission: "Permissions.AccessModules.Report",
      sub: [
        {
          name: "Accounts Module Report",
          state: "report/accounts-module-report",
          permission: "Permissions.AccessReportModules.AccountsModuleReports",
        },
        {
          name: "Purchase Module Report",
          state: "report/purchase-module-report",
          permission: "Permissions.AccessReportModules.PurchaseModuleReports",
        },
        {
          name: "Production Module Report",
          state: "report/production-module-report",
          permission: "Permissions.AccessReportModules.ProductionModuleReports",
        },
        {
          name: "Sales Module Report",
          state: "report/sales-module-report",
          permission: "Permissions.AccessReportModules.SalesModuleReports",
        },
        {
          name: "Sales Module Report",
          state: "report/primary-sales-module-report",
          permission:
            "Permissions.AccessReportModules.PrimarySalesModuleReports",
        },
        {
          name: "Inventory Module Report",
          state: "report/inventory-module-report",
          permission: "Permissions.AccessReportModules.InventoryModuleReports",
        },
        {
          name: "Inventory Module Report",
          state: "report/primary-inventory-module-report",
          permission:
            "Permissions.AccessReportModules.PrimaryInventoryModuleReports",
        },
        // {
        //   name: "Data Import From Excel",
        //   state: "report/excel-upload",
        //   permission: "Permissions.Reports.Stock",
        // },
      ],
    },
  ];

  // Icon menu TITLE at the very top of navigation.
  // This title will appear if any icon type item is present in menu.
  iconTypeMenuTitle = "Frequently Accessed";
  // sets iconMenu as default;
  menuItems = new BehaviorSubject<IMenuItem[]>(this.iconMenu);
  // navigation component has subscribed to this Observable
  menuItems$ = this.menuItems.asObservable();
  constructor() {}

  // Customizer component uses this method to change menu.
  // You can remove this method and customizer component.
  // Or you can customize this method to supply different menu for
  // different user type.
  publishNavigationChange(menuType: string) {
    // switch (menuType) {
    //   case 'separator-menu':
    //     this.menuItems.next(this.separatorMenu);
    //     break;
    //   case 'icon-menu':
    //     this.menuItems.next(this.iconMenu);
    //     break;
    //   default:
    //     this.menuItems.next(this.plainMenu);
    // }
  }
}
