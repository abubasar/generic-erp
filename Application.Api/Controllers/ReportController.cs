using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.StoredProcedureResult;
using Application.Services.SearchRequestModels.Production;
using Application.Services.SearchRequestModels.Report;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Productions.Productions;
using Application.Services.Services.Report.Accounts;
using Application.Services.Services.Report.Ledger;
using Application.Services.Services.Report.Pdf;
using Application.Services.Services.Report.Stocks;
using Application.Services.ViewModels.Report;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("report")]
    public class ReportController : ControllerBase
    {
        private readonly IStockService _stockService;
        private readonly IProductionService _productionService;
        private readonly IReportPdfService _reportPdfService;
        private readonly ILedgerService _ledgerService;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountReportService _accountReportService;
        private readonly ITenantService _tenantService;

        public ReportController(IStockService stockService, IReportPdfService reportPdfService,
            ILedgerService ledgerService, IProductionService productionService,
            IWorkContext workContext, IUnitOfWork unitOfWork, IAccountReportService accountReportService, ITenantService tenantService)
        {
            _stockService = stockService;
            _reportPdfService = reportPdfService;
            _ledgerService = ledgerService;
            _productionService = productionService;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _accountReportService = accountReportService;
            _tenantService = tenantService;
        }

        [Authorize(Permissions.Reports.Stock)]
        [Route("stock")]
        [HttpPost]
        public async Task<Result<Tuple<List<StockViewModel>, int>>> Search(StockRequestModel request)
        {
            return await Result<Tuple<List<StockViewModel>, int>>.SuccessAsync(await _stockService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.Reports.Stock)]
        [Route("low-stock")]
        [HttpPost]
        public async Task<Result<Tuple<List<StockViewModel>, int>>> LowStock(StockRequestModel request)
        {
            return await Result<Tuple<List<StockViewModel>, int>>.SuccessAsync(await _stockService.LowStockAsync(request), "Result Found");
        }

        [Route("stock-quantity/{productId}/{storeId}")]
        [HttpGet]
        public async Task<decimal> GetItemStock(Guid productId, Guid storeId)
        {
            return await _stockService.GetItemStockAsync(productId, storeId);
        }

        [Route("stock-ledger")]
        [HttpPost]
        public async Task<List<StockLedgerReportLine>> PrepareStockLedger(StockLedgerRequestModel request)
        {
            request.FromDate = request.FromDate.ToLocal().Date;
            request.ToDate = request.ToDate.ToLocal().Date;
            return await _stockService.PrepareStockLedger(request);
        }

        [Route("item-stock-ledger")]
        [HttpPost]
        public List<ItemStockLedger> PrepareItemStockLedger(StockLedgerRequestModel request)
        {
            request.FromDate = request.FromDate.ToLocal().Date;
            request.ToDate = request.ToDate.ToLocal().Date;
            return _stockService.PrepareItemStockLedger(request);
        }

        [Route("supplier-ledger")]
        [HttpPost]
        public async Task<IList<SPSupplierLedgerResult>> PrepareSupplierLedger()
        {
            return await _ledgerService.SubsidiaryLedger(Guid.Parse("45E19DC5-8C95-4D1B-9FCB-8758EA23A6CC"), null, null);
        }

        [Route("work-in-process-inventory-stock")]
        [HttpGet]
        public async Task<List<StockViewModel>> PrepareWorkInProcessInventoryStock()
        {
            return await _stockService.PrepareWorkInProcessInventoryStockAsync();
        }

        [Authorize(Permissions.InventoryModuleReports.Stock_Report)]
        [HttpPost]
        [Route("stock/print")]
        public virtual async Task<IActionResult> Print(StockRequestModel request)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                request.Page = -1;
                var userName = _workContext.GetUserName() ?? "";
                var list = await _stockService.SearchAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Product", "Store", "AvailableQty", "StockValue" };
                List<float> columnWidths = new List<float> { 8f, 32f, 30f, 15f, 15f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var stock in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        stock?.ProductName,
                        stock?.StoreName,
                        AvailableQty = Math.Round(stock!.AvailableQty, 2),
                        StockValue = Math.Round(stock!.StockValue, 2)
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var reportTitle = "Stock Report";
                    var table = await _reportPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, reportTitle, numericColumnsForSum: new List<string> { "AvailableQty", "StockValue" }, false, null, null, request.StoreId, request.ProductId, request.InventoryTypeId, request.ProductTypeId);
                    if (request.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(table, SheetNameGenerator.Generate("Sheet1",
                             null, null), tenantData.Name, tenantData.Address, reportTitle);
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

        [Authorize(PrimaryPermissions.PrimaryInventoryModuleReports.Primary_Stock_Report)]
        [HttpPost]
        [Route("primary-stock/print")]
        public virtual async Task<IActionResult> PrintStockReport(StockRequestModel request)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                request.Page = -1;
                var userName = _workContext.GetUserName() ?? "";
                var result = await _stockService.SearchAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Stock Report";
                    var tables = await _reportPdfService.PrintStockReportToPdfAsync(stream, result, headerText, request);
                    if (request.ReportType == 2)
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
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.InventoryModuleReports.Low_Stock_Report)]
        [HttpPost]
        [Route("lowStock/print")]
        public virtual async Task<IActionResult> PrintLowStock(StockRequestModel request)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                request.Page = -1;
                var userName = _workContext.GetUserName() ?? "";
                var list = await _stockService.LowStockAsync(request);
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Product", "Store", "Quantity", "Value" };
                List<float> columnWidths = new List<float> { 8f, 32f, 30f, 15f, 15f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var lowStock in list.Item1)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        lowStock?.ProductName,
                        lowStock?.StoreName,
                        AvailableQty = Math.Round(lowStock!.AvailableQty, 2),
                        StockValue = Math.Round(lowStock!.StockValue, 2)
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var reportTitle = "Low Stock Report";
                    var table = await _reportPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, reportTitle);
                    if (request.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(table, SheetNameGenerator.Generate("Sheet1",
                             null, null), tenantData.Name, tenantData.Address, reportTitle);
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

        [Authorize(Permissions.InventoryModuleReports.Work_In_Process_Inventory_Stock)]
        [HttpGet]
        [Route("work-in-process-inventory-stock/print/{reportType}")]
        public virtual async Task<IActionResult> PrintWorkInProcessInventoryStock(int reportType)
        {
            try
            {
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var list = await _stockService.PrepareWorkInProcessInventoryStockAsync();
                //headers and column Widths
                List<string> headers = new List<string> { "SL", "Product", "AvailableQty", "StockValue" };
                List<float> columnWidths = new List<float> { 8f, 42f, 25f, 25f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var workInProcessInventoryStock in list)
                {
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        workInProcessInventoryStock?.ProductName,
                        AvailableQty = Math.Round(workInProcessInventoryStock!.AvailableQty, 2),
                        StockValue = Math.Round(workInProcessInventoryStock!.StockValue, 2)
                    });
                }
                //........
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var reportTitle = "Work In Process Inventory Stock Report";
                    var table = await _reportPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, reportTitle, numericColumnsForSum: new List<string> { "AvailableQty", "StockValue" });
                    if (reportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(table, SheetNameGenerator.Generate("Sheet1",
                             null, null), tenantData.Name, tenantData.Address, reportTitle);
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

        [Authorize(Permissions.InventoryModuleReports.Finished_Goods_Stock_Report)]
        [HttpPost]
        [Route("finished-goods-stock/print")]
        public virtual async Task<IActionResult> PrintFinishedGoodsStockLedger(FinishedGoodsStockReportRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            request.FromDate = request.FromDate.ToLocal().Date;
            request.ToDate = request.ToDate.ToLocal().Date;
            request.InventoryTypeId = InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid();
            var userName = _workContext.GetUserName() ?? "";
            var list = await _stockService.PrepareFinishedGoodsStockLedger(request);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Finished Goods Stock : Report";
                var tables = await _reportPdfService.PrintFinishedGoodsStockLedgerToPdfAsync(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                         request.FromDate, request.ToDate), tenantData.Name, tenantData.Address, headerText);
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

        [Authorize(PrimaryPermissions.PrimaryInventoryModuleReports.Primary_Finished_Goods_Stock_Report)]
        [HttpPost]
        [Route("primary-finished-goods-stock/print")]
        public virtual async Task<IActionResult> PrintPrimaryFinishedGoodsStockLedger(FinishedGoodsStockReportRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            request.FromDate = request.FromDate.ToLocal().Date;
            request.ToDate = request.ToDate.ToLocal().Date;
            request.InventoryTypeId = InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid();
            var userName = _workContext.GetUserName() ?? "";
            var list = await _stockService.PreparePrimaryFinishedGoodsStockLedger(request);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Finished Goods Stock : Report";
                var tables = await _reportPdfService.PrintPrimaryFinishedGoodsStockLedgerToPdfAsync(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                         request.FromDate, request.ToDate), tenantData.Name, tenantData.Address, headerText);
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

        [Authorize(PrimaryPermissions.PrimaryInventoryModuleReports.Primary_Stock_Report_Whole)]
        [HttpPost]
        [Route("primary-stock-report-whole/print")]
        public virtual async Task<IActionResult> PrintPrimaryStockReportWhole(FinishedGoodsStockReportRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            request.FromDate = request.FromDate.ToLocal().Date;
            request.ToDate = request.ToDate.ToLocal().Date;
            request.InventoryTypeId = InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid();
            var userName = _workContext.GetUserName() ?? "";
            var list = await _stockService.PreparePrimaryFinishedGoodsStockLedger(request);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Stock Report (Whole)";
                var tables = await _reportPdfService.PrintPrimaryStockReportWholeToPdfAsync(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                         request.FromDate, request.ToDate), tenantData.Name, tenantData.Address, headerText);
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

        [Authorize(Permissions.InventoryModuleReports.Raw_Materials_Stock_Report)]
        [HttpPost]
        [Route("raw-materials-stock/print")]
        public virtual async Task<IActionResult> PrintRawMaterialsStockLedger(RawMaterialsStockReportRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            request.FromDate = request.FromDate.ToLocal().Date;
            request.ToDate = request.ToDate.ToLocal().Date;
            request.InventoryTypeId = InventoryTypeConstants.Inventory_Type_Id_Raw_Materials.ToGuid();
            var userName = _workContext.GetUserName() ?? "";
            var list = await _stockService.PrepareRawMaterialsStockLedger(request);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Item/RM Stock : Report";
                var tables = await _reportPdfService.PrintRawMaterialsStockLedgerToPdfAsync(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                         request.FromDate, request.ToDate), tenantData.Name, tenantData.Address, headerText);
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

        [Authorize(Permissions.InventoryModuleReports.Stock_Depot_Product_Wise_Details_Report)]
        [HttpPost]
        [Route("stock-depot-product-wise-details-report/print")]
        public virtual async Task<IActionResult> PrintStockDepotProductWiseDetailsReport(StockLedgerRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            request.FromDate = request.FromDate.ToLocal().Date;
            request.ToDate = request.ToDate.ToLocal().Date;
            var userName = _workContext.GetUserName() ?? "";
            var list = await _stockService.PrepareStockLedger(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = request.IsDepotWiseReport ? "Stock Report : Depot Wise (Details)" : "Stock Report : Product Wise (Details)";
                var tables = await _reportPdfService.PrintStockDepotProductWiseDetailsReportToPdf(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate, request.ToDate), tenantData.Name, tenantData.Address, headerText);
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

        [Authorize(Permissions.InventoryModuleReports.Stock_Depot_Product_Wise_Short_Report)]
        [HttpPost]
        [Route("stock-depot-product-wise-short-report/print")]
        public virtual async Task<IActionResult> PrintStockDepotProductWiseShortReport(StockLedgerRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            request.FromDate = request.FromDate.ToLocal().Date;
            request.ToDate = request.ToDate.ToLocal().Date;
            var userName = _workContext.GetUserName() ?? "";
            var list = await _stockService.PrepareStockLedger(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = request.IsDepotWiseReport ? "Stock Report : Depot Wise (Short)" : "Stock Report : Product Wise (Short)";
                var tables = await _reportPdfService.PrintStockDepotProductWiseShortReportToPdf(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate, request.ToDate), tenantData.Name, tenantData.Address, headerText);
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

        [Authorize(Permissions.InventoryModuleReports.Item_Wise_Stock_Ledger)]
        [HttpPost]
        [Route("item-stock-ledger/print")]
        public virtual async Task<IActionResult> PrintItemStockLedger(StockLedgerRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            request.FromDate = request.FromDate.ToLocal().Date;
            request.ToDate = request.ToDate.ToLocal().Date;
            var list = _stockService.PrepareItemStockLedger(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var reportTitle = "Item Wise Stock Ledger Report";
                var table = await _reportPdfService.PrintItemStockLedgerReportToPdf(stream, list, reportTitle, request);
                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(table, SheetNameGenerator.Generate("Sheet1",
                         request.FromDate, request.ToDate), tenantData.Name, tenantData.Address, reportTitle);
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

        [Authorize(Permissions.ProductionModuleReports.Day_Wise_Production_Summary)]
        [HttpPost]
        [Route("day-wise-production-summary/print")]
        public virtual async Task<IActionResult> DayWiseProductionSummary(ProductionRequestModel request)
        {
            try
            {
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var result = await _productionService.DayWiseProductionSummaryRowColumnDynamic(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Day Wise Production Summary (Metric Ton)";
                    var tables = await _reportPdfService.PrintReportForExpandoObjectToPdfAsync(stream, result.ToList<object>(), headerText,
                        false, request.FromDate, request.ToDate, request.FgstoreId);

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
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }
        [Authorize(Permissions.ProductionModuleReports.Day_Wise_Consumption_Qty)]
        [HttpPost]
        [Route("day-wise-consumption-qty/print")]
        public virtual async Task<IActionResult> DayWiseConsumptionQty(ConsumptionRequestModel request)
        {
            try
            {
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                string measurementUnitName = "";
                if (request.ProductTypeId.HasValue)
                {
                    var product = await _unitOfWork.Repository<Product>().TableNoTracking().Where(x => x.ProductTypeId == request.ProductTypeId).FirstOrDefaultAsync();
                    var measurementUnit = await _unitOfWork.Repository<MeasurementUnit>().TableNoTracking().Where(x => x.Id == product!.MeasurementUnitId).FirstOrDefaultAsync();
                    measurementUnitName = measurementUnit!.Name;
                }
                var result = await _productionService.DayWiseConsumptionQtyRowColumnDynamic(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Day Wise Consumption Summary " + "(" + measurementUnitName + ")";
                    var tables = await _reportPdfService.PrintReportForExpandoObjectToPdfAsync(stream, result.ToList<object>(), headerText,
                        true, request.FromDate, request.ToDate, null, null, null, request.ProductTypeId);

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
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }
        [Authorize(Permissions.ProductionModuleReports.Day_Wise_Consumption_Rate)]
        [HttpPost]
        [Route("day-wise-consumption-rate/print")]
        public virtual async Task<IActionResult> DayWiseConsumptionRate(ConsumptionRequestModel request)
        {
            try
            {
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                string measurementUnitName = "";
                if (request.ProductTypeId.HasValue)
                {
                    var product = await _unitOfWork.Repository<Product>().TableNoTracking().Where(x => x.ProductTypeId == request.ProductTypeId).FirstOrDefaultAsync();
                    var measurementUnit = await _unitOfWork.Repository<MeasurementUnit>().TableNoTracking().Where(x => x.Id == product!.MeasurementUnitId).FirstOrDefaultAsync();
                    measurementUnitName = measurementUnit!.Name;
                }
                var userName = _workContext.GetUserName() ?? "";
                var result = await _productionService.DayWiseConsumptionRateRowColumnDynamic(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Day Wise Consumption Rate (Per " + measurementUnitName + ")";
                    var tables = await _reportPdfService.PrintReportForExpandoObjectForAverageToPdfAsync(stream, result.ToList<object>(), headerText,
                        true, request.FromDate, request.ToDate, null, null, null, request.ProductTypeId);

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
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.SalesModuleReports.Sales_Report_Feed_Total_MO_Wise)]
        [HttpPost]
        [Route("sales-mo-wise/print")]
        public virtual async Task<IActionResult> SalesMoWise(ProductionRequestModel request)
        {
            try
            {
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var result = await _productionService.SalesMoWise(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Sales Report : Feed Total (MO Wise) in MT";
                    var tables = await _reportPdfService.PrintReportForExpandoObject2ToPdfAsync(stream, result, headerText,
                        false, request.FromDate, request.ToDate);

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
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }


        [Authorize(Permissions.SalesModuleReports.Sales_Report_Feed_Total_Customer_Wise)]
        [HttpPost]
        [Route("sales-customer-wise/print")]
        public virtual async Task<IActionResult> SalesCustomerWise(ProductionRequestModel request)
        {
            try
            {
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                {
                    request.FromDate = request.FromDate.Value.ToLocal().Date;
                    request.ToDate = request.ToDate.Value.ToLocal().Date;
                }
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var result = await _productionService.SalesCustomerWise(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Sales Report : Feed Total (Customer Wise) in MT";
                    var tables = await _reportPdfService.PrintReportForExpandoObject2ToPdfAsync(stream, result, headerText,
                        false, request.FromDate, request.ToDate);

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
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.InventoryModuleReports.Finished_Goods_Stock_Summary_Report)]
        [HttpPost]
        [Route("finished-goods-stock-summary/print")]
        public virtual async Task<IActionResult> PrintFinishedGoodsStockSummaryReport(FinishedGoodsStockReportRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            request.FromDate = request.FromDate.ToLocal().Date;
            request.ToDate = request.ToDate.ToLocal().Date;
            request.InventoryTypeId = InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid();
            var userName = _workContext.GetUserName() ?? "";
            var list = await _stockService.PrepareFinishedGoodsStockLedger(request);

            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Finished Goods Stock Summary : Report";
                var tables = await _reportPdfService.PrintFinishedGoodsStockSummaryReportToPdfAsync(stream, list, headerText, request);

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                         request.FromDate, request.ToDate), tenantData.Name, tenantData.Address, headerText);
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


        //private List<string> GetPropertyNames<T>()
        //{
        //    List<string> propertyNames = new List<string>();

        //    PropertyInfo[] propertyInfos = typeof(T).GetProperties();

        //    foreach (var propertyInfo in propertyInfos)
        //    {
        //        propertyNames.Add(propertyInfo.Name);
        //    }

        //    return propertyNames;
        //}

    }
}
