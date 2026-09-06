
using Application.Services.Services.Configuration.Areas;
using Application.Services.Services.Configuration.Categories;
using Application.Services.Services.Configuration.Companies;
using Application.Services.Services.Configuration.CostCenters;
using Application.Services.Services.Configuration.Countries;
using Application.Services.Services.Configuration.Currencies;
using Application.Services.Services.Configuration.CustomerWiseProductDiscounts;
using Application.Services.Services.Configuration.DeliveryPlaces;
using Application.Services.Services.Configuration.Departments;
using Application.Services.Services.Configuration.Designations;
using Application.Services.Services.Configuration.DiscountProductWises;
using Application.Services.Services.Configuration.EmailAccounts;
using Application.Services.Services.Configuration.Employees;
using Application.Services.Services.Configuration.FinancialYears;
using Application.Services.Services.Configuration.FundTransferTransactionTypes;
using Application.Services.Services.Configuration.Generics;
using Application.Services.Services.Configuration.InventoryTypes;
using Application.Services.Services.Configuration.JobLocations;
using Application.Services.Services.Configuration.Machines;
using Application.Services.Services.Configuration.Manufacturers;
using Application.Services.Services.Configuration.MeasurementUnits;
using Application.Services.Services.Configuration.PackSizes;
using Application.Services.Services.Configuration.PaymentMethods;
using Application.Services.Services.Configuration.PaymentModes;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.Services.Configuration.ProductAudits;
using Application.Services.Services.Configuration.ProductCostSetups;
using Application.Services.Services.Configuration.Products;
using Application.Services.Services.Configuration.ProductTypes;
using Application.Services.Services.Configuration.Regions;
using Application.Services.Services.Configuration.Settings;
using Application.Services.Services.Configuration.Shifts;
using Application.Services.Services.Configuration.Stores;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Configuration.Territories;
using Application.Services.Services.Configuration.Zones;
using Autofac;

namespace Application.Services.Services.Configuration
{
    public class ConfigurationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // Scoped
            builder.RegisterType<CompanyService>().As<ICompanyService>().InstancePerLifetimeScope();
            builder.RegisterType<CountryService>().As<ICountryService>().InstancePerLifetimeScope();
            builder.RegisterType<DepartmentService>().As<IDepartmentService>().InstancePerLifetimeScope();
            builder.RegisterType<GenericService>().As<IGenericServicecs>().InstancePerLifetimeScope();
            builder.RegisterType<JobLocationService>().As<IJobLocationService>().InstancePerLifetimeScope();
            builder.RegisterType<EmailAccountService>().As<IEmailAccountService>().InstancePerLifetimeScope();
            builder.RegisterType<RegionService>().As<IRegionService>().InstancePerLifetimeScope();
            builder.RegisterType<ZoneService>().As<IZoneService>().InstancePerLifetimeScope();
            builder.RegisterType<AreaService>().As<IAreaService>().InstancePerLifetimeScope();
            builder.RegisterType<CurrencyService>().As<ICurrencyService>().InstancePerLifetimeScope();
            builder.RegisterType<CostCenterService>().As<ICostCenterService>().InstancePerLifetimeScope();
            builder.RegisterType<DesignationService>().As<IDesignationService>().InstancePerLifetimeScope();
            builder.RegisterType<EmployeeService>().As<IEmployeeService>().InstancePerLifetimeScope();
            builder.RegisterType<MeasurementUnitService>().As<IMeasurementUnitService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductService>().As<IProductService>().InstancePerLifetimeScope();
            builder.RegisterType<StoreService>().As<IStoreService>().InstancePerLifetimeScope();
            builder.RegisterType<InventoryTypeService>().As<IInventoryTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductTypeService>().As<IProductTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<FinancialYearService>().As<IFinancialYearService>().InstancePerLifetimeScope();
            builder.RegisterType<DeliveryPlaceService>().As<IDeliveryPlaceService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentMethodService>().As<IPaymentMethodService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductCostSetupService>().As<IProductCostSetupService>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerWiseProductDiscountService>().As<ICustomerWiseProductDiscountService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentModeService>().As<IPaymentModeService>().InstancePerLifetimeScope();
            builder.RegisterType<ConfigurationPdfService>().As<IConfigurationPdfService>().InstancePerLifetimeScope();
            builder.RegisterType<ShiftService>().As<IShiftService>().InstancePerLifetimeScope();
            builder.RegisterType<SettingService>().As<ISettingService>().InstancePerLifetimeScope();
            builder.RegisterType<MachineService>().As<IMachineService>().InstancePerLifetimeScope();
            builder.RegisterType<FundTransferTransactionTypeService>().As<IFundTransferTransactionTypeService>().InstancePerLifetimeScope();
            builder.RegisterType<DiscountProductWiseService>().As<IDiscountProductWiseService>().InstancePerLifetimeScope();
            builder.RegisterType<TenantService>().As<ITenantService>().InstancePerLifetimeScope();
            builder.RegisterType<ManufacturerService>().As<IManufacturerService>().InstancePerLifetimeScope();
            builder.RegisterType<TerritoryService>().As<ITerritoryService>().InstancePerLifetimeScope();
            builder.RegisterType<CategoryService>().As<ICategoryService>().InstancePerLifetimeScope();
            builder.RegisterType<PackSizeService>().As<IPackSizeService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductAuditService>().As<IProductAuditService>().InstancePerLifetimeScope();
        }
    }
}
