using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.AccountsReceivable.SaleInvoice;
using Application.Services.SearchRequestModels.Accounts.AccountsReceivable;
using Application.Services.SearchRequestModels.Report;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Accounts.AccountsReceivable.SaleInvoices;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Report.Pdf;
using Application.Services.Services.Sale.Pdf;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Accounts.AccountsReceivable.SaleInvoice;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("sales")]
    public class SaleInvoiceController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISaleInvoiceService _saleInvoiceService;
        private readonly ISalePdfService _salePdfService;
        private readonly IReportPdfService _reportPdfService;
        private readonly ITenantService _tenantService;


        public SaleInvoiceController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork,
            ISaleInvoiceService saleInvoiceService, ISalePdfService salePdfService, IReportPdfService reportPdfService, ITenantService tenantService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _saleInvoiceService = saleInvoiceService;
            _salePdfService = salePdfService;
            _reportPdfService = reportPdfService;
            _tenantService = tenantService;
        }

        [Authorize(Permissions.SaleInvoices.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<SaleInvoiceViewModel>, int>>> Search(SaleInvoiceRequestModel request)
        {
            return await Result<Tuple<List<SaleInvoiceViewModel>, int>>.SuccessAsync(await _saleInvoiceService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.SaleInvoices.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<SaleInvoiceAggregatorModel>> ReportAggregates(SaleInvoiceRequestModel request)
        {
            return await Result<SaleInvoiceAggregatorModel>.SuccessAsync(await _saleInvoiceService.PrepareSaleInvoiceAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.SaleInvoices.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<SaleInvoiceViewModel>> GetById(Guid id)
        {
            return await Result<SaleInvoiceViewModel>.SuccessAsync(await _saleInvoiceService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.SaleInvoices.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _saleInvoiceService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.SaleInvoices.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] SaleInvoiceCreationDto saleInvoiceCreationDto)
        {
            var saleInvoiceId = await _saleInvoiceService.AddAsync(saleInvoiceCreationDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(saleInvoiceId, "Sale Invoice Added Successfully");
        }

        [Authorize(Permissions.SaleInvoices.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] SaleInvoiceUpdateDto saleInvoiceUpdateDto)
        {
            var saleInvoiceId = await _saleInvoiceService.UpdateAsync(saleInvoiceUpdateDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(saleInvoiceId, "Sale Invoice Updated Successfully");
        }

        [Authorize(Permissions.SaleInvoices.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var saleInvoiceId = await _saleInvoiceService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(saleInvoiceId, "Sale Invoice Deleted Successfully");
        }

        [Authorize(Permissions.SaleInvoices.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _saleInvoiceService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)SaleInvoiceStatus.Checked, "Sale Invoice Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.SaleInvoices.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _saleInvoiceService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)SaleInvoiceStatus.Approved, "Sale Invoice Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.SaleInvoices.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _saleInvoiceService.UnpostAsync(id, fromStatus), "Sale Invoice Unposted Successfully");
        }

        [Authorize(Permissions.SaleInvoices.SendToCustomer)]
        [HttpPost("send/{saleInvoiceId}")]
        public virtual async Task<Result> SendToCustomer(Guid saleInvoiceId)
        {
            var status = await _saleInvoiceService.SendToCustomerAsync(saleInvoiceId);
            if (status) return await Result<int>.SuccessAsync((int)SaleInvoiceStatus.SentToCustomer, "Sale Invoice Sent Successfully to the Customer");
            else return await Result<string>.FailAsync("Failed To Send", "Something Went Wrong");
        }

        [HttpPost("customer-discount-report")]
        public IActionResult CustomerMonthlyDiscountReport(CustomerDiscountRequestModel requestModel)
        {
            return Ok(_saleInvoiceService.GetCustomerMonthlyDiscountReport(requestModel.Year, requestModel.Month, requestModel.CustomerId));
        }

        [Authorize(Permissions.SalesModuleReports.Customer_Discount_Report_Month_Wise)]
        [HttpPost("customer-discount-report-print")]
        public async Task<IActionResult> PrintCustomerMonthlyDiscountReport(CustomerDiscountRequestModel requestModel)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var customerName = "";
            if (requestModel.CustomerId.HasValue) customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == requestModel.CustomerId.Value)?.Name;
            var list = _saleInvoiceService.GetCustomerMonthlyDiscountReport(requestModel.Year, requestModel.Month, requestModel.CustomerId);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Customer Discount Report (Month Wise)";
                var tables = await _salePdfService.PrintCustomerMonthlyDiscountReportToPdf(stream, list, headerText, customerName,
                    requestModel.Year, requestModel.Month);

                if (requestModel.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1", null, null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }
        [Authorize(Permissions.SalesModuleReports.Customer_Discount_Report_Date_Wise)]
        [HttpPost("customer-date-wise-discount-report-print")]
        public async Task<IActionResult> PrintCustomerDateWiseDiscountReport(CustomerDiscountRequestModel requestModel)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var customerName = "";
            requestModel.FromDate = requestModel.FromDate.ToLocal().Date;
            requestModel.ToDate = requestModel.ToDate.ToLocal().Date;
            if (requestModel.CustomerId.HasValue) customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == requestModel.CustomerId.Value)?.Name;
            var list = _saleInvoiceService.GetCustomerDateWiseDiscountReport(requestModel);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Customer Discount Report (Date Wise)";
                var tables = await _salePdfService.PrintCustomerDateWiseDiscountReportToPdf(stream, list, headerText, requestModel);

                if (requestModel.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1", null, null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }

        [Authorize(Permissions.SalesModuleReports.Sales_Report_Warehouse_Wise)]
        [HttpPost]
        [Route("warehouse-wise-sale-invoice-report-print")]
        public virtual async Task<IActionResult> PrintWarehouseWiseSaleInvoiceReport(SaleInvoiceRequestModel request)
        {
            try
            {
                request.Page = -1;
                request.IsAscending = true;
                request.OrderBy = "CreatedOn";
                request.SaleInvoiceStatuses = new List<int> { (int)SaleInvoiceStatus.Approved, (int)SaleInvoiceStatus.SentToCustomer };
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var list = await _saleInvoiceService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Sales : Feed Mill / Warehouse Wise";
                    var tables = await _salePdfService.PrintWarehouseWiseSaleInvoiceReportToPdf(stream, list.Item1.ToList(), headerText, request);

                    if (request.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value.AddDays(1) : null, request.ToDate.HasValue ? request.ToDate.Value.AddDays(1) : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.SalesModuleReports.Sales_Customer_Item_Wise)]
        [HttpPost]
        [Route("sales-customer-item-wise-print")]
        public virtual async Task<IActionResult> PrintSalesCustomerItemWiseReport(SaleInvoiceRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var customerName = "";
            var marketingOfficerName = "";
            var storeName = "";
            var customerZoneName = "";
            var customerAreaName = "";
            if (request.CustomerId.HasValue) customerName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerId.Value)?.Name;
            if (request.CustomerMarketingOfficerId.HasValue) marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerMarketingOfficerId.Value)?.FullName;
            if (request.StoreId.HasValue) storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.StoreId)?.Name;
            if (request.CustomerZoneId.HasValue) customerZoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerZoneId)!.Name;
            if (request.CustomerAreaId.HasValue) customerAreaName = _unitOfWork.Repository<Area>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerAreaId)?.Name;
            var list = await _saleInvoiceService.CustomerWiseSalesItemSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Sales: Customer, Item Wise";
                var tables = await _salePdfService.PrintSalesCustomerItemWiseReportToPdf(stream, list, headerText,
                    request.FromDate, request.ToDate?.AddDays(1), customerName, marketingOfficerName, storeName, customerZoneName, customerAreaName);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value.AddDays(1) : null, request.ToDate.HasValue ? request.ToDate.Value.AddDays(1) : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }

        [Authorize(Permissions.SalesModuleReports.Sales_Item_Customer_Wise)]
        [HttpPost]
        [Route("sales-item-customer-wise-print")]
        public virtual async Task<IActionResult> PrintSalesItemCustomerWiseReport(SaleInvoiceRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var productName = "";
            var marketingOfficerName = "";
            var storeName = "";
            var customerZoneName = "";
            var customerAreaName = "";
            if (request.ProductId.HasValue) productName = _unitOfWork.Repository<Product>().TableNoTracking().FirstOrDefault(x => x.Id == request.ProductId.Value)?.Name;
            if (request.CustomerMarketingOfficerId.HasValue) marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerMarketingOfficerId.Value)?.FullName;
            if (request.StoreId.HasValue) storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.StoreId)?.Name;
            if (request.CustomerZoneId.HasValue) customerZoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerZoneId)?.Name;
            if (request.CustomerAreaId.HasValue) customerAreaName = _unitOfWork.Repository<Area>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerAreaId)?.Name;
            var list = await _saleInvoiceService.CustomerWiseSalesItemSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Sales: Item, Customer Wise";
                var tables = await _salePdfService.PrintSalesItemCustomerWiseReportToPdf(stream, list, headerText,
                    request.FromDate, request.ToDate?.AddDays(1), productName, marketingOfficerName, storeName, customerZoneName, customerAreaName);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value.AddDays(1) : null, request.ToDate.HasValue ? request.ToDate.Value.AddDays(1) : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }

        [Authorize(Permissions.SalesModuleReports.Sale_Total_Product_Wise)]
        [HttpPost]
        [Route("sale-total-product-wise-print")]
        public virtual async Task<IActionResult> PrintSaleTotalProductWiseReport(SaleInvoiceRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var productName = "";
            var marketingOfficerName = "";
            var storeName = "";
            var customerZoneName = "";
            var customerAreaName = "";
            if (request.ProductId.HasValue) productName = _unitOfWork.Repository<Product>().TableNoTracking().FirstOrDefault(x => x.Id == request.ProductId.Value)?.Name;
            if (request.CustomerMarketingOfficerId.HasValue) marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerMarketingOfficerId.Value)?.FullName;
            if (request.StoreId.HasValue) storeName = _unitOfWork.Repository<Store>().TableNoTracking().FirstOrDefault(x => x.Id == request.StoreId)?.Name;
            if (request.CustomerZoneId.HasValue) customerZoneName = _unitOfWork.Repository<Zone>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerZoneId)?.Name;
            if (request.CustomerAreaId.HasValue) customerAreaName = _unitOfWork.Repository<Area>().TableNoTracking().FirstOrDefault(x => x.Id == request.CustomerAreaId)?.Name;
            var list = await _saleInvoiceService.CustomerWiseSalesItemSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Sale Total : Product Wise";
                var tables = await _salePdfService.PrintSaleTotalProductWiseReportToPdf(stream, list, headerText,
                    request.FromDate, request.ToDate?.AddDays(1), productName, marketingOfficerName, storeName, customerZoneName, customerAreaName);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value.AddDays(1) : null, request.ToDate.HasValue ? request.ToDate.Value.AddDays(1) : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }

        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_Sale_Total_Product_Wise)]
        [HttpPost]
        [Route("primary-sale-total-product-wise-print")]
        public virtual async Task<IActionResult> PrintPrimarySaleTotalProductWiseReport(SalesItemDetailRequestModel request)
        {
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                request.FromDate = request.FromDate.Value.ToLocal().Date;
                request.ToDate = request.ToDate.Value.ToLocal().Date;
            }
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _saleInvoiceService.SalesItemSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Product Wise Sales";
                var tables = await _salePdfService.PrintPrimarySaleTotalProductWiseReportToPdf(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }

        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_Sale_Total_Product_Wise_Multi_Territory)]
        [HttpPost]
        [Route("primary-sale-total-product-wise-multi-territory-print")]
        public virtual async Task<IActionResult> PrintPrimarySaleTotalProductWiseMultiTerritoryReport(SalesItemDetailRequestModel request)
        {
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                request.FromDate = request.FromDate.Value.ToLocal().Date;
                request.ToDate = request.ToDate.Value.ToLocal().Date;
            }
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _saleInvoiceService.SalesItemSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Territory Wise Product Sales";
                var tables = await _salePdfService.PrintPrimarySaleTotalProductWiseMultiTerritoryReportToPdf(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }

        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_Sale_Total_Product_Wise_Multi_Officer)]
        [HttpPost]
        [Route("primary-sale-total-product-wise-multi-officer-print")]
        public virtual async Task<IActionResult> PrintPrimarySaleTotalProductWiseMultiOfficerReport(SalesItemDetailRequestModel request)
        {
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                request.FromDate = request.FromDate.Value.ToLocal().Date;
                request.ToDate = request.ToDate.Value.ToLocal().Date;
            }
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _saleInvoiceService.SalesItemSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "MO Wise Product Sales";
                var tables = await _salePdfService.PrintPrimarySaleTotalProductWiseMultiOfficerReportToPdf(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }


        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_Customer_Wise_Product_Wise_Sales_Quantity)]
        [HttpPost]
        [Route("primary-customer-wise-product-wise-sales-quantity-print")]
        public virtual async Task<IActionResult> PrintPrimaryCustomerWiseProductWiseSalesQuantityReport(SalesItemDetailRequestModel request)
        {
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                request.FromDate = request.FromDate.Value.ToLocal().Date;
                request.ToDate = request.ToDate.Value.ToLocal().Date;
            }
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _saleInvoiceService.SalesItemSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Customer Wise Product Wise Sales Quantity";
                var tables = await _salePdfService.PrintPrimaryCustomerWiseProductWiseSalesQuantityReportToPdf(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }


        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_Sale_Aging_Report)]
        [HttpPost]
        [Route("primary-sale-aging-report-print")]
        public virtual async Task<IActionResult> PrintPrimarySaleAgingReport(SalesItemDetailRequestModel request)
        {
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                request.FromDate = request.FromDate.Value.ToLocal().Date;
                request.ToDate = request.ToDate.Value.ToLocal().Date;
            }
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _saleInvoiceService.SalesAgingDataSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Aging Report";
                var tables = await _salePdfService.PrintPrimarySaleAgingReportToPdf(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }
        [Authorize(PrimaryPermissions.PrimarySalesModuleReports.Primary_Temporary_Sale_Aging_Report)]
        [HttpPost]
        [Route("primary-temporary-sale-aging-report-print")]
        public virtual async Task<IActionResult> PrintPrimaryTemporarySaleAgingReport(SalesItemDetailRequestModel request)
        {
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                request.FromDate = request.FromDate.Value.ToLocal().Date;
                request.ToDate = request.ToDate.Value.ToLocal().Date;
            }
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _saleInvoiceService.SalesAgingDataSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Aging Report (Temporary)";
                var tables = await _salePdfService.PrintPrimaryTemporarySaleAgingReportToPdf(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }

        [HttpPost]
        [Route("sale-invoice-summary-print")]
        public virtual async Task<IActionResult> PrintSaleInvoiceSummaryReport(SaleInvoiceRequestModel request)
        {
            var reportTitle = "Sale Invoice Summary";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "InvoiceNo";
            request.SaleInvoiceStatuses = new List<int> { (int)SaleInvoiceStatus.Approved, (int)SaleInvoiceStatus.SentToCustomer };
            var list = await _saleInvoiceService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintSaleInvoiceReportToPdf(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("sale-invoice-details-print")]
        public virtual async Task<IActionResult> PrintSaleInvoiceDetailsReport(SaleInvoiceRequestModel request)
        {
            var reportTitle = "Sale Invoice Details";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "InvoiceNo";
            request.SaleInvoiceStatuses = new List<int> { (int)SaleInvoiceStatus.Approved, (int)SaleInvoiceStatus.SentToCustomer };
            var list = await _saleInvoiceService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintSaleInvoiceReportToPdf(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [Authorize(Permissions.SalesModuleReports.Sales_Report)]
        [HttpPost]
        [Route("sales-report-print")]
        public virtual async Task<IActionResult> PrintSalesReport(SaleInvoiceRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _saleInvoiceService.SalesReportSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Sales Report";
                var tables = await _salePdfService.PrintSalesReportToPdf(stream, list, headerText, request.FromDate, request.ToDate?.AddDays(1));

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value.AddDays(1) : null, request.ToDate.HasValue ? request.ToDate.Value.AddDays(1) : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }

        [Authorize(Permissions.SalesModuleReports.Sale_Total_Month_Wise)]
        [HttpPost]
        [Route("sales-total-month-wise-print")]
        public virtual async Task<IActionResult> PrintSalesTotalMonthWiseReport(SaleInvoiceRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _saleInvoiceService.SalesTotalMonthWiseSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Sale Total : Month Wise";
                var tables = await _salePdfService.PrintSalesTotalMonthWiseReportToPdf(stream, list, headerText, request.FromDate, request.ToDate?.AddDays(1));

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value.AddDays(1) : null, request.ToDate.HasValue ? request.ToDate.Value.AddDays(1) : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }

        [Authorize(Permissions.SalesModuleReports.Sale_Total_Date_Wise)]
        [HttpPost]
        [Route("sales-total-date-wise-print")]
        public virtual async Task<IActionResult> PrintSalesTotalDateWiseReport(SaleInvoiceRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _saleInvoiceService.SalesTotalDateWiseSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Sale Total : Date Wise";
                var tables = await _salePdfService.PrintSalesTotalDateWiseReportToPdf(stream, list, headerText, request.FromDate, request.ToDate?.AddDays(1));

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value.AddDays(1) : null, request.ToDate.HasValue ? request.ToDate.Value.AddDays(1) : null), tenantData.Name, tenantData.Address, headerText);
                    return File(excelBytes, MimeTypes.TextXlsx);
                }
                else
                {
                    bytes = stream.ToArray();
                    bytes = PdfHelper.AddPageNumbers(bytes, userName);
                    return File(bytes, MimeTypes.ApplicationPdf);
                }
            }
        }
        [Authorize(Permissions.SalesModuleReports.Sales_Report_Month_Wise)]
        [HttpPost]
        [Route("sales-report-month-wise/print/{reportType}")]
        public virtual async Task<IActionResult> SalesReportMonthWise(int reportType)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var result = await _saleInvoiceService.MonthWiseProductSales();
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var year = DateTime.Now.Year - 1 + "-" + DateTime.Now.Year;
                    var headerText = "Sales Report : Month Wise(" + year + ")";
                    var tables = await _reportPdfService.PrintReportForExpandoObjectToPdfAsync(stream, result.ToList<object>(), headerText,
                       false, null, null);

                    if (reportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                                null, null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.SalesModuleReports.Customer_Ledger_Feed_Wise)]
        [HttpPost]
        [Route("customer-ledger-feed-wise-print")]
        public virtual async Task<IActionResult> CustomerLedgerFeedWiseReportPrint(CustomerLedgerFeedWiseRequestModel requestModel)
        {
            try
            {
                if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                {
                    requestModel.FromDate = requestModel.FromDate.Value.ToLocal().Date;
                    requestModel.ToDate = requestModel.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var list = await _saleInvoiceService.CustomerLedgerFeedWiseSearchAsync(requestModel);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Customer Ledger : Feed";
                    var tables = await _salePdfService.PrintCustomerLedgerFeedWiseReportToPdfAsync(stream, list, headerText, requestModel);

                    if (requestModel.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                                requestModel.FromDate.HasValue ? requestModel.FromDate.Value : null, requestModel.ToDate.HasValue ? requestModel.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }
    }
}
