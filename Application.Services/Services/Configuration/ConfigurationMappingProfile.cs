using Application.Core.Common;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier;
using Application.Services.Dtos.Accounts.AccountType;
using Application.Services.Dtos.Configuration.Area;
using Application.Services.Dtos.Configuration.Category;
using Application.Services.Dtos.Configuration.Company;
using Application.Services.Dtos.Configuration.CostCenter;
using Application.Services.Dtos.Configuration.Country;
using Application.Services.Dtos.Configuration.Currency;
using Application.Services.Dtos.Configuration.CustomerWiseProductDiscount;
using Application.Services.Dtos.Configuration.DeliveryPlace;
using Application.Services.Dtos.Configuration.Department;
using Application.Services.Dtos.Configuration.Designation;
using Application.Services.Dtos.Configuration.DiscountProductWise;
using Application.Services.Dtos.Configuration.EmailAccount;
using Application.Services.Dtos.Configuration.Employee;
using Application.Services.Dtos.Configuration.FinancialYear;
using Application.Services.Dtos.Configuration.FundTransferTransactionType;
using Application.Services.Dtos.Configuration.Generic;
using Application.Services.Dtos.Configuration.InventoryType;
using Application.Services.Dtos.Configuration.JobLocation;
using Application.Services.Dtos.Configuration.Machine;
using Application.Services.Dtos.Configuration.Manufacturer;
using Application.Services.Dtos.Configuration.MeasurementUnit;
using Application.Services.Dtos.Configuration.PackSize;
using Application.Services.Dtos.Configuration.PaymentMethod;
using Application.Services.Dtos.Configuration.PaymentMode;
using Application.Services.Dtos.Configuration.Product;
using Application.Services.Dtos.Configuration.ProductCostSetup;
using Application.Services.Dtos.Configuration.ProductType;
using Application.Services.Dtos.Configuration.Region;
using Application.Services.Dtos.Configuration.Shift;
using Application.Services.Dtos.Configuration.Store;
using Application.Services.Dtos.Configuration.Tenant;
using Application.Services.Dtos.Configuration.Territory;
using Application.Services.Dtos.Configuration.Zone;
using Application.Services.Dtos.Role;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Auth;
using Application.Services.ViewModels.Configuration;
using AutoMapper;

namespace Application.Services.Services.Configuration
{
    public class ConfigurationMappingProfile : Profile
    {
        public ConfigurationMappingProfile()
        {
            //Role
            CreateMap<RoleCreationDto, Role>();
            CreateMap<RoleUpdateDto, Role>();
            CreateMap<Role, RoleViewModel>();
            //Company
            CreateMap<CompanyCreationDto, Company>();
            CreateMap<CompanyUpdateDto, Company>();
            CreateMap<Company, CompanyViewModel>();
            //Country
            CreateMap<CountryCreationDto, Country>();
            CreateMap<CountryUpdateDto, Country>();
            CreateMap<Country, CountryViewModel>();
            //ProductCostSetup
            CreateMap<ProductCostSetupCreationDto, ProductCostSetup>();
            CreateMap<ProductCostSetupUpdateDto, ProductCostSetup>();
            CreateMap<ProductCostSetup, ProductCostSetupViewModel>();
            //Store
            CreateMap<StoreCreationDto, Store>();
            CreateMap<StoreUpdateDto, Store>();
            CreateMap<Store, StoreViewModel>();
            //Department
            CreateMap<DepartmentCreationDto, Department>();
            CreateMap<DepartmentUpdateDto, Department>();
            CreateMap<Department, DepartmentViewModel>();
            //Generic
            CreateMap<GenericCreationDto, Generic>();
            CreateMap<GenericUpdateDto, Generic>();
            CreateMap<Generic, GenericViewModel>();
            //JobLocation
            CreateMap<JobLocationCreationDto, JobLocation>();
            CreateMap<JobLocationUpdateDto, JobLocation>();
            CreateMap<JobLocation, JobLocationViewModel>();
            //EmailAccount
            CreateMap<EmailAccountCreationDto, EmailAccount>();
            CreateMap<EmailAccountUpdateDto, EmailAccount>();
            CreateMap<EmailAccount, EmailAccountViewModel>();
            //Region
            CreateMap<RegionCreationDto, Region>();
            CreateMap<RegionUpdateDto, Region>();
            CreateMap<Region, RegionViewModel>();
            //Zone
            CreateMap<ZoneCreationDto, Zone>();
            CreateMap<ZoneUpdateDto, Zone>();
            CreateMap<Zone, ZoneViewModel>();
            //Area
            CreateMap<AreaCreationDto, Area>();
            CreateMap<AreaUpdateDto, Area>();
            CreateMap<Area, AreaViewModel>();
            //Region
            CreateMap<CurrencyCreationDto, Currency>();
            CreateMap<CurrencyUpdateDto, Currency>();
            CreateMap<Currency, CurrencyViewModel>();
            //CostCenter
            CreateMap<CostCenterCreationDto, CostCenter>();
            CreateMap<CostCenterUpdateDto, CostCenter>();
            CreateMap<CostCenter, CostCenterViewModel>();
            //Designation
            CreateMap<DesignationCreationDto, Designation>();
            CreateMap<DesignationUpdateDto, Designation>();
            CreateMap<Designation, DesignationViewModel>();
            //Employee
            CreateMap<EmployeeCreationDto, Employee>();
            CreateMap<EmployeeUpdateDto, Employee>();
            CreateMap<Employee, EmployeeViewModel>()
               .ForMember(dest => dest.GenderName, opt => opt.MapFrom(s => Enum.GetName(typeof(Gender), s.Gender)))
               .ForMember(dest => dest.BloodGroupName, opt => opt.MapFrom(s => Enum.GetName(typeof(BloodGroup), s.BloodGroup)))
               .ForMember(dest => dest.MaritalStatusName, opt => opt.MapFrom(s => Enum.GetName(typeof(MaritalStatus), s.MaritalStatus)))
               .ForMember(dest => dest.JoiningDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.JoiningDate)))
               .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.DateOfBirth)));
            //MeasurementUnit
            CreateMap<MeasurementUnitCreationDto, MeasurementUnit>();
            CreateMap<MeasurementUnitUpdateDto, MeasurementUnit>();
            CreateMap<MeasurementUnit, MeasurementUnitViewModel>();
            //Inventory Type
            CreateMap<InventoryTypeCreationDto, InventoryType>();
            CreateMap<InventoryTypeUpdateDto, InventoryType>();
            CreateMap<InventoryType, InventoryTypeViewModel>();
            //Product Type
            CreateMap<ProductTypeCreationDto, ProductType>();
            CreateMap<ProductTypeUpdateDto, ProductType>();
            CreateMap<ProductType, ProductTypeViewModel>();
            //Product
            CreateMap<ProductCreationDto, Product>();//Add
            CreateMap<ProductUpdateDto, Product>();//Update
            CreateMap<Product, ProductViewModel>();//Search Response
            //FinancialYear
            CreateMap<FinancialYearCreationDto, FinancialYear>();//Add
            CreateMap<FinancialYearUpdateDto, FinancialYear>();//Update
            CreateMap<FinancialYear, FinancialYearViewModel>();//Search Response
            //DeliveryPlace
            CreateMap<DeliveryPlaceCreationDto, DeliveryPlace>();//Add
            CreateMap<DeliveryPlaceUpdateDto, DeliveryPlace>();//Update
            CreateMap<DeliveryPlace, DeliveryPlaceViewModel>();//Search Response
            //PaymentMethod
            CreateMap<PaymentMethodCreationDto, PaymentMethod>();//Add
            CreateMap<PaymentMethodUpdateDto, PaymentMethod>();//Update
            CreateMap<PaymentMethod, PaymentMethodViewModel>();//Search Response
            //PaymentMode
            CreateMap<PaymentModeCreationDto, Core.Entities.PaymentMode>();
            CreateMap<PaymentModeUpdateDto, Core.Entities.PaymentMode>();
            CreateMap<Core.Entities.PaymentMode, PaymentModeViewModel>();

            //User
            CreateMap<User, UserViewModel>()
                .ForMember(d => d.RoleName, opt => opt.MapFrom(s => s.Role.Name));
            //Account Type
            CreateMap<AccountTypeCreationDto, AccountType>();
            CreateMap<AccountTypeUpdateDto, AccountType>();
            CreateMap<AccountType, AccountTypeViewModel>();
            //Account
            CreateMap<AccountCreationDto, Account>();
            CreateMap<AccountUpdateDto, Account>();
            CreateMap<Account, AccountViewModel>()
                 .ForMember(d => d.AccountTypeName, opt => opt.MapFrom(s => s.AccountType.Name))
                  .ForMember(d => d.ParentName, opt => opt.MapFrom(s => s.Parent.Name));
            //Supplier Account
            CreateMap<SupplierCreationDto, Account>();
            CreateMap<SupplierUpdateDto, Account>();
            CreateMap<Account, SupplierViewModel>();
            //Customer Account
            CreateMap<CustomerCreationDto, Account>();
            CreateMap<CustomerUpdateDto, Account>();
            CreateMap<Account, CustomerViewModel>();
            //supplier and customer bank details
            CreateMap<BankAccountCreationDto, BankAccount>();
            CreateMap<BankAccountUpdateDto, BankAccount>();
            CreateMap<BankAccount, BankAccountViewModel>();
            //Customer Wise Product Discount
            CreateMap<CustomerWiseProductDiscountCreationDto, CustomerWiseProductDiscount>();//Add
            CreateMap<CustomerWiseProductDiscountUpdateDto, CustomerWiseProductDiscount>();//Update
            CreateMap<CustomerWiseProductDiscount, CustomerWiseProductDiscountViewModel>()//Search Response
                                                                                          // .ForMember(dest => dest.ApplicableDate, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.ApplicableDate)))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Customer Wise Product Discount Detail
            CreateMap<CustomerWiseProductDiscountDetailCreationDto, CustomerWiseProductDiscountDetail>();//Add
            CreateMap<CustomerWiseProductDiscountDetailUpdateDto, CustomerWiseProductDiscountDetail>();//Update
            CreateMap<CustomerWiseProductDiscountDetail, CustomerWiseProductDiscountDetailViewModel>();//Search Response
            //Shift
            CreateMap<ShiftCreationDto, Shift>()
                 .ForMember(dest => dest.FromTime, opt => opt.MapFrom(s => TimeSpanHelper.ParseTime(s.FromTime)))
                 .ForMember(dest => dest.ToTime, opt => opt.MapFrom(s => TimeSpanHelper.ParseTime(s.ToTime)));
            CreateMap<ShiftUpdateDto, Shift>()
                 .ForMember(dest => dest.FromTime, opt => opt.MapFrom(s => TimeSpanHelper.ParseTime(s.FromTime)))
                 .ForMember(dest => dest.ToTime, opt => opt.MapFrom(s => TimeSpanHelper.ParseTime(s.ToTime)));
            CreateMap<Shift, ShiftViewModel>()
                .ForMember(dest => dest.FromTime, opt => opt.MapFrom(s => DateTime.MinValue.Add(s.FromTime).ToString("hh:mm tt")))
                 .ForMember(dest => dest.ToTime, opt => opt.MapFrom(s => DateTime.MinValue.Add(s.ToTime).ToString("hh:mm tt")))
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Machine
            CreateMap<MachineCreationDto, Machine>();
            CreateMap<MachineUpdateDto, Machine>();
            CreateMap<Machine, MachineViewModel>();
            //FundTransferTransactionType
            CreateMap<FundTransferTransactionTypeCreationDto, FundTransferTransactionType>();
            CreateMap<FundTransferTransactionTypeUpdateDto, FundTransferTransactionType>();
            CreateMap<FundTransferTransactionType, FundTransferTransactionTypeViewModel>();

            //Discount Product Wise
            CreateMap<DiscountProductWiseCreationDto, DiscountProductWise>();//Add
            CreateMap<DiscountProductWiseUpdateDto, DiscountProductWise>();//Update
            CreateMap<DiscountProductWise, DiscountProductWiseViewModel>()//Search Response
                .ForMember(dest => dest.CreatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.CreatedOn)))
                .ForMember(dest => dest.UpdatedOn, opt => opt.MapFrom(s => DateTimeHelper.UtcToLocal(s.UpdatedOn)));
            //Discount Product Wise Detail
            CreateMap<DiscountProductWiseDetailCreationDto, DiscountProductWiseDetail>();//Add
            CreateMap<DiscountProductWiseDetailUpdateDto, DiscountProductWiseDetail>();//Update
            CreateMap<DiscountProductWiseDetail, DiscountProductWiseDetailViewModel>();//Search Response

            //Tenant
            CreateMap<TenantCreationDto, Tenant>();
            CreateMap<TenantUpdateDto, Tenant>();
            CreateMap<Tenant, TenantViewModel>();

            //Manufacturer
            CreateMap<ManufacturerCreationDto, Manufacturer>();//Add
            CreateMap<ManufacturerUpdateDto, Manufacturer>();//Update
            CreateMap<Manufacturer, ManufacturerViewModel>();//Search Response

            //Territory
            CreateMap<TerritoryCreationDto, Territory>();
            CreateMap<TerritoryUpdateDto, Territory>();
            CreateMap<Territory, TerritoryViewModel>();

            //Category
            CreateMap<CategoryCreationDto, Category>();
            CreateMap<CategoryUpdateDto, Category>();
            CreateMap<Category, CategoryViewModel>();

            //PackSize
            CreateMap<PackSizeCreationDto, PackSize>();
            CreateMap<PackSizeUpdateDto, PackSize>();
            CreateMap<PackSize, PackSizeViewModel>();
        }
    }
}
