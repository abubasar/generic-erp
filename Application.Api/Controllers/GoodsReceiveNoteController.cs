using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.GoodsReceiveNote;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.SearchRequestModels.Report;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Purchase.GoodsReceiveNotes;
using Application.Services.Services.Purchase.Pdf;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.GoodsReceiveNote;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("purchase")]
    public class GoodsReceiveNoteController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGoodsReceiveNoteService _goodsReceiveNoteService;
        private readonly IPurchasePdfService _purchasePdfService;
        private readonly ITenantService _tenantService;

        public GoodsReceiveNoteController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IGoodsReceiveNoteService goodsReceiveNoteService, IPurchasePdfService purchasePdfService, ITenantService tenantService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _goodsReceiveNoteService = goodsReceiveNoteService;
            _purchasePdfService = purchasePdfService;
            _tenantService = tenantService;
        }

        [Authorize(Permissions.GoodsReceiveNotes.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<GoodsReceiveNoteViewModel>, int>>> Search(GoodsReceiveNoteRequestModel request)
        {
            return await Result<Tuple<List<GoodsReceiveNoteViewModel>, int>>.SuccessAsync(await _goodsReceiveNoteService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.GoodsReceiveNotes.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<GoodsReceiveNoteAggregatorModel>> ReportAggregates(GoodsReceiveNoteRequestModel request)
        {
            return await Result<GoodsReceiveNoteAggregatorModel>.SuccessAsync(await _goodsReceiveNoteService.PrepareGoodsReceiveNoteAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.GoodsReceiveNotes.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<GoodsReceiveNoteViewModel>> GetById(Guid id)
        {
            return await Result<GoodsReceiveNoteViewModel>.SuccessAsync(await _goodsReceiveNoteService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.GoodsReceiveNotes.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _goodsReceiveNoteService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.GoodsReceiveNotes.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] GoodsReceiveNoteCreationDto goodsReceiveNoteCreationDto)
        {
            var goodsReceiveNoteId = await _goodsReceiveNoteService.AddAsync(goodsReceiveNoteCreationDto);
            return await Result<Guid>.SuccessAsync(goodsReceiveNoteId, "Goods Receive Note Added Successfully");
        }

        [Authorize(Permissions.GoodsReceiveNotes.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] GoodsReceiveNoteUpdateDto goodsReceiveNoteUpdateDto)
        {
            var goodsReceiveNoteId = await _goodsReceiveNoteService.UpdateAsync(goodsReceiveNoteUpdateDto);
            return await Result<Guid>.SuccessAsync(goodsReceiveNoteId, "Goods Receive Note Updated Successfully");
        }

        [Authorize(Permissions.GoodsReceiveNotes.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var goodsReceiveNoteId = await _goodsReceiveNoteService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(goodsReceiveNoteId, "Goods Receive Note Deleted Successfully");
        }

        [Authorize(Permissions.GoodsReceiveNotes.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _goodsReceiveNoteService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)GRNStatus.Checked, "Goods Receive Note Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.GoodsReceiveNotes.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _goodsReceiveNoteService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)GRNStatus.Approved, "Goods Receive Note Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.GoodsReceiveNotes.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _goodsReceiveNoteService.UnpostAsync(id, fromStatus), "Goods Receive Note Unposted Successfully");
        }

        [HttpPost]
        [Route("goods-receive-note-summary-print")]
        public virtual async Task<IActionResult> PrintGoodsReceiveNoteSummaryReport(GoodsReceiveNoteRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "Grnno";
            request.GRNStatuses = new List<int> { (int)GRNStatus.Approved, (int)GRNStatus.Invoice_Generated };
            var list = await _goodsReceiveNoteService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintGoodsReceiveNoteReportToPdfAsync(stream, list.Item1.ToList(), "Goods Receive Note Summary", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("goods-receive-note-details-print")]
        public virtual async Task<IActionResult> PrintGoodsReceiveNoteDetailsReport(GoodsReceiveNoteRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "Grnno";
            request.GRNStatuses = new List<int> { (int)GRNStatus.Approved, (int)GRNStatus.Invoice_Generated };
            var list = await _goodsReceiveNoteService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintGoodsReceiveNoteReportToPdfAsync(stream, list.Item1.ToList(), "Goods Receive Note Details", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [Authorize(Permissions.PurchaseModuleReports.GRN_Supplier_Item_Wise)]
        [HttpPost]
        [Route("grn-supplier-item-wise-print")]
        public virtual async Task<IActionResult> PrintGrnSupplierItemWiseReport(GoodsReceiveNoteRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                request.FromDate = request.FromDate.Value.ToLocal().Date;
                request.ToDate = request.ToDate.Value.ToLocal().Date;
            }
            var userName = _workContext.GetUserName() ?? "";
            var list = await _goodsReceiveNoteService.GrnSupplierItemWiseSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var reportTitle = "GRN: Supplier, Item Wise";
                var tables = await _purchasePdfService.PrintGrnSupplierItemWiseReportToPdfAsync(stream, list, reportTitle, request);
                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                           request.FromDate.HasValue ? request.FromDate.Value.Date : null, request.ToDate.HasValue ? request.ToDate.Value.Date : null), tenantData.Name, tenantData.Address, reportTitle);
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

        [Authorize(Permissions.PurchaseModuleReports.GRN_Item_Supplier_Wise)]
        [HttpPost]
        [Route("grn-item-supplier-wise-print")]
        public virtual async Task<IActionResult> PrintGrnItemSupplierWiseReport(GoodsReceiveNoteRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                request.FromDate = request.FromDate.Value.ToLocal().Date;
                request.ToDate = request.ToDate.Value.ToLocal().Date;
            }
            var userName = _workContext.GetUserName() ?? "";
            var list = await _goodsReceiveNoteService.GrnSupplierItemWiseSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var reportTitle = "GRN: Item, Supplier Wise";
                var tables = await _purchasePdfService.PrintGrnItemSupplierWiseReportToPdfAsync(stream, list, reportTitle, request);
                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                           request.FromDate.HasValue ? request.FromDate.Value.Date : null, request.ToDate.HasValue ? request.ToDate.Value.Date : null), tenantData.Name, tenantData.Address, reportTitle);
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

        [Authorize(Permissions.PurchaseModuleReports.GRN_TotalQty_Value_Average_Price)]
        [HttpPost]
        [Route("grn-totalQty-value-Average-price-print")]
        public virtual async Task<IActionResult> PrintGrnTotalQuantityValueAveragePriceReport(GoodsReceiveNoteRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                request.FromDate = request.FromDate.Value.ToLocal().Date;
                request.ToDate = request.ToDate.Value.ToLocal().Date;
            }
            var userName = _workContext.GetUserName() ?? "";
            var list = await _goodsReceiveNoteService.GrnItemSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var reportTitle = "GRN: Total Qty, Value, Average Price";
                var tables = await _purchasePdfService.PrintGrnTotalQuantityValueAveragePriceReportToPdfAsync(stream, list, reportTitle, request);
                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                           request.FromDate.HasValue ? request.FromDate.Value.Date : null, request.ToDate.HasValue ? request.ToDate.Value.Date : null), tenantData.Name, tenantData.Address, reportTitle);
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

        [Authorize(Permissions.PurchaseModuleReports.CurrentStock_PurchaseRate_And_Quantity)]
        [HttpPost]
        [Route("current_stock_purchase_rate_and_quantity")]
        public virtual async Task<IActionResult> PrintProductsWithLatestPurchasePriceAsyncReport(ProductWithLatestPriceRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            if (request.ToDate.HasValue) request.ToDate = request.ToDate.Value.ToLocal().Date;

            var userName = _workContext.GetUserName() ?? "";
            var list = await _goodsReceiveNoteService.GetProductsWithLatestPurchasePriceAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Summary Report of Current Stock Purchase Rate & Quantity";
                var tables = await _purchasePdfService.PrintProductsWithLatestPurchasePriceAsyncReportToPdfAsync(stream, list, headerText, request);
                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                        null, request.ToDate.HasValue ? request.ToDate.Value.Date : null), tenantData.Name, tenantData.Address, headerText);
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
    }
}
