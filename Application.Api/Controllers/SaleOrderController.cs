using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Sale.SaleOrder;
using Application.Services.SearchRequestModels.Report;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Sale.Pdf;
using Application.Services.Services.Sale.SaleOrders;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.SaleOrder;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleOrderController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISaleOrderService _saleOrderService;
        private readonly ISalePdfService _salePdfService;
        private readonly ITenantService _tenantService;

        public SaleOrderController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, ISaleOrderService saleOrderService, ISalePdfService salePdfService, ITenantService tenantService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _saleOrderService = saleOrderService;
            _salePdfService = salePdfService;
            _tenantService = tenantService;
        }

        [Authorize(Permissions.SaleOrders.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<SaleOrderViewModel>, int>>> Search(SaleOrderRequestModel request)
        {
            return await Result<Tuple<List<SaleOrderViewModel>, int>>.SuccessAsync(await _saleOrderService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.SaleOrders.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<SaleOrderAggregatorModel>> ReportAggregates(SaleOrderRequestModel request)
        {
            return await Result<SaleOrderAggregatorModel>.SuccessAsync(await _saleOrderService.PrepareSaleOrderAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.SaleOrders.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<IActionResult> GetById(Guid id)
        {
            var dbSaleOrder = await _saleOrderService.GetByIdAsync(id);
            //ETag implementation start
            var eTag = HashFactory.GetHash(dbSaleOrder);
            HttpContext.Response.Headers.Add("ETag", eTag);
            if (HttpContext.Request.Headers.ContainsKey("If-None-Match") &&
                HttpContext.Request.Headers["If-None-Match"].RemoveQuotes() == eTag)
                return new StatusCodeResult(304);
            //ETag implementation end
            return Ok(await Result<SaleOrderViewModel>.SuccessAsync(dbSaleOrder, "Result Found", eTag));
        }

        [Authorize(Permissions.SaleOrders.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _saleOrderService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.SaleOrders.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] SaleOrderCreationDto saleOrderCreationDto)
        {
            var saleOrderId = await _saleOrderService.AddAsync(saleOrderCreationDto);
            return await Result<Guid>.SuccessAsync(saleOrderId, "Sale Order Added Successfully");
        }

        [Authorize(Permissions.SaleOrders.Edit)]
        [HttpPost("/api/saleOrder/update")]
        public virtual async Task<IActionResult> Put([FromBody] SaleOrderUpdateDto saleOrderUpdateDto)
        {
            //ETag implementation start
            var dbSaleOrder = await _saleOrderService.GetByIdAsync(saleOrderUpdateDto.Id);
            var eTag = HashFactory.GetHash(dbSaleOrder);
            HttpContext.Response.Headers.Add("ETag", eTag);
            if (!HttpContext.Request.Headers.ContainsKey("If-None-Match") ||
                HttpContext.Request.Headers["If-None-Match"].RemoveQuotes() != eTag)
            {
                return new StatusCodeResult(412);
            }
            //ETag implementation end
            else
            {
                var saleOrderId = await _saleOrderService.UpdateAsync(saleOrderUpdateDto);
                return Ok(await Result<Guid>.SuccessAsync(saleOrderId, "Sale Order Updated Successfully"));
            }


        }

        [Authorize(Permissions.SaleOrders.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var saleOrderId = await _saleOrderService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(saleOrderId, "Sale Order Deleted Successfully");
        }

        [Authorize(Permissions.SaleOrders.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _saleOrderService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)SaleOrderStatus.Checked, "Sale Order Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.SaleOrders.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _saleOrderService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)SaleOrderStatus.Approved, "Sale Order Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.SaleOrders.Closed)]
        [HttpPost("close/{id}")]
        public virtual async Task<Result> Close(Guid id)
        {
            var isClosed = await _saleOrderService.CloseAsync(id);
            if (isClosed) return await Result<int>.SuccessAsync((int)SaleOrderStatus.Closed, "Sale Order Closed Successfully");
            else return await Result<string>.FailAsync("Failed To Close", "Something Went Wrong");
        }

        [Authorize(Permissions.SaleOrders.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _saleOrderService.UnpostAsync(id, fromStatus), "Sale Order Unposted Successfully");
        }

        [HttpPost]
        [Route("sale-order-summary-print")]
        public virtual async Task<IActionResult> PrintSaleOrderSummaryReport(SaleOrderRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "SaleOrderNo";
            request.SaleOrderStatuses = new List<int> { (int)SaleOrderStatus.Approved, (int)SaleOrderStatus.Item_Partially_Delivered, (int)SaleOrderStatus.Item_Completely_Delivered, (int)SaleOrderStatus.Closed, (int)SaleOrderStatus.Partially_Invoiced, (int)SaleOrderStatus.Fully_Invoiced };
            var list = await _saleOrderService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintSaleOrderReportToPdf(stream, list.Item1.ToList(), "Sale Order Summary", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("sale-order-details-print")]
        public virtual async Task<IActionResult> PrintSaleOrderDetailsReport(SaleOrderRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "SaleOrderNo";
            request.SaleOrderStatuses = new List<int> { (int)SaleOrderStatus.Approved, (int)SaleOrderStatus.Item_Partially_Delivered, (int)SaleOrderStatus.Item_Completely_Delivered, (int)SaleOrderStatus.Closed, (int)SaleOrderStatus.Partially_Invoiced, (int)SaleOrderStatus.Fully_Invoiced };
            var list = await _saleOrderService.SearchAsync(request);
            byte[] bytes;

            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintSaleOrderReportToPdf(stream, list.Item1.ToList(), "Sale Order Details", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [Authorize(Permissions.SalesModuleReports.Transit_Sales_Report)]
        [HttpPost]
        [Route("transit-sales-report-print")]
        public virtual async Task<IActionResult> TransitSalesReportPrint(SalesOrderItemRequestModel request)
        {
            try
            {
                if (request.ToDate.HasValue) request.ToDate = request.ToDate.Value.ToLocal().Date;
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var list = await _saleOrderService.GetSaleOrderedDataWithoutDeliveryItemAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Transit Sales Report";
                    var tables = await _salePdfService.PrintTransitSalesReportToPdfAsync(stream, list, headerText, request);
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

        [Authorize(Permissions.SalesModuleReports.Sales_Order_History_Report)]
        [HttpPost]
        [Route("sales-order-history-report-print")]
        public virtual async Task<IActionResult> SalesOrderHistoryReportPrint(SalesOrderItemRequestModel request)
        {
            try
            {
                if (request.ToDate.HasValue) request.ToDate = request.ToDate.Value.ToLocal().Date;
                if (request.FromDate.HasValue) request.FromDate = request.FromDate.Value.ToLocal().Date;
                Guid? tenantId = _workContext.GetTenantId();
                var tenantData = await _tenantService.GetByIdAsync(tenantId);
                var userName = _workContext.GetUserName() ?? "";
                var list = await _saleOrderService.GetSaleOrderedDataWithoutDeliveryItemAsync(request);
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Sales Order History";
                    var tables = await _salePdfService.PrintSalesOrderHistoryReportToPdfAsync(stream, list, headerText, request);
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
    }
}
