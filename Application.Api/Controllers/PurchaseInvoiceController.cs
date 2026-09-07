using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.PurchaseInvoice;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Purchase.Pdf;
using Application.Services.Services.Purchase.PurchaseInvoices;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PurchaseInvoice;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("purchase")]
    public class PurchaseInvoiceController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPurchaseInvoiceService _purchaseInvoiceService;
        private readonly IPurchasePdfService _purchasePdfService;
        private readonly ITenantService _tenantService;

        public PurchaseInvoiceController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IPurchaseInvoiceService purchaseInvoiceService, IPurchasePdfService purchasePdfService, ITenantService tenantService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _purchaseInvoiceService = purchaseInvoiceService;
            _purchasePdfService = purchasePdfService;
            _tenantService = tenantService;
        }

        [Authorize(Permissions.PurchaseInvoices.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<PurchaseInvoiceViewModel>, int>>> Search(PurchaseInvoiceRequestModel request)
        {
            return await Result<Tuple<List<PurchaseInvoiceViewModel>, int>>.SuccessAsync(await _purchaseInvoiceService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.PurchaseInvoices.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<PurchaseInvoiceAggregatorModel>> ReportAggregates(PurchaseInvoiceRequestModel request)
        {
            return await Result<PurchaseInvoiceAggregatorModel>.SuccessAsync(await _purchaseInvoiceService.PreparePurchaseInvoiceAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.PurchaseInvoices.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<PurchaseInvoiceViewModel>> GetById(Guid id)
        {
            return await Result<PurchaseInvoiceViewModel>.SuccessAsync(await _purchaseInvoiceService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.PurchaseInvoices.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _purchaseInvoiceService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.PurchaseInvoices.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] PurchaseInvoiceCreationDto purchaseInvoiceCreationDto)
        {
            var purchaseInvoiceId = await _purchaseInvoiceService.AddAsync(purchaseInvoiceCreationDto);
            return await Result<Guid>.SuccessAsync(purchaseInvoiceId, "Purchase Invoice Added Successfully");
        }

        [Authorize(Permissions.PurchaseInvoices.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] PurchaseInvoiceUpdateDto purchaseInvoiceUpdateDto)
        {
            var purchaseInvoiceId = await _purchaseInvoiceService.UpdateAsync(purchaseInvoiceUpdateDto);
            return await Result<Guid>.SuccessAsync(purchaseInvoiceId, "Purchase Invoice Updated Successfully");
        }

        [Authorize(Permissions.PurchaseInvoices.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var purchaseInvoiceId = await _purchaseInvoiceService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(purchaseInvoiceId, "Purchase Invoice Deleted Successfully");
        }

        [Authorize(Permissions.PurchaseInvoices.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _purchaseInvoiceService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)PurchaseInvoiceStatus.Checked, "Purchase Invoice Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.PurchaseInvoices.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _purchaseInvoiceService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)PurchaseInvoiceStatus.Approved, "Purchase Invoice Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.PurchaseInvoices.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _purchaseInvoiceService.UnpostAsync(id, fromStatus), "Purchase Invoice Unposted Successfully");
        }

        [Authorize(Permissions.PurchaseInvoices.Edit)]
        [HttpPost("reset-advance-amount/{id}")]
        public virtual async Task<Result> ResetAssociatedAdvanceAmount(Guid id)
        {
            return await Result<Guid>.SuccessAsync(await _purchaseInvoiceService.ResetAssociatedAdvanceAmountAsync(id), "Advance Amount Reset Successfully");
        }

        [Authorize(Permissions.PurchaseModuleReports.Purchase_Supplier_Item_Wise)]
        [HttpPost]
        [Route("supplier-wise-purchase-item-print")]
        public virtual async Task<IActionResult> PrintSupplierWisePurchaseItemReport(PurchaseInvoiceRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _purchaseInvoiceService.SupplierWisePurchaseItemSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var reportTitle = "Purchase: Supplier, Item Wise";
                var table = await _purchasePdfService.PrintSupplierWisePurchaseItemReportToPdfAsync(stream, list, reportTitle,
                   request.FromDate, request.ToDate?.AddDays(1));
                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(table, SheetNameGenerator.Generate("Sheet1",
                           request.FromDate.HasValue ? request.FromDate.Value.ToLocal().Date : null, request.ToDate.HasValue ? request.ToDate.Value.ToLocal().Date : null), tenantData.Name, tenantData.Address, reportTitle);
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

        [Authorize(Permissions.PurchaseModuleReports.Purchase_Item_Supplier_Wise)]
        [HttpPost]
        [Route("purchase-item-wise-supplier-print")]
        public virtual async Task<IActionResult> PrintPurchaseItemWiseSupplierReport(PurchaseInvoiceRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _purchaseInvoiceService.SupplierWisePurchaseItemSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var reportTitle = "Purchase: Item, Supplier Wise";
                var table = await _purchasePdfService.PrintPurchaseItemWiseSupplierReportToPdfAsync(stream, list, reportTitle,
                    request.FromDate, request.ToDate?.AddDays(1));
                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(table, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value.ToLocal().Date : null, request.ToDate.HasValue ? request.ToDate.Value.ToLocal().Date : null), tenantData.Name, tenantData.Address, reportTitle);
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
        [Route("purchase-invoice-details-print")]
        public virtual async Task<IActionResult> PrintPurchaseInvoiceDetailsReport(PurchaseInvoiceRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "PurchaseInvoiceNo";
            request.PurchaseInvoiceStatuses = new List<int> { (int)PurchaseInvoiceStatus.Approved, (int)PurchaseInvoiceStatus.Partially_Paid, (int)PurchaseInvoiceStatus.Fully_Paid };
            var list = await _purchaseInvoiceService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintPurchaseInvoiceReportToPdfAsync(stream, list.Item1.ToList(), "Purchase Invoice Details", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("purchase-invoice-summary-print")]
        public virtual async Task<IActionResult> PrintPurchaseInvoiceSummaryReport(PurchaseInvoiceRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "PurchaseInvoiceNo";
            request.PurchaseInvoiceStatuses = new List<int> { (int)PurchaseInvoiceStatus.Approved, (int)PurchaseInvoiceStatus.Fully_Paid, (int)PurchaseInvoiceStatus.Partially_Paid };
            var list = await _purchaseInvoiceService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintPurchaseInvoiceReportToPdfAsync(stream, list.Item1.ToList(), "Purchase Invoice Summary", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [Authorize(Permissions.PurchaseModuleReports.Purchase_TotalQty_Value_Average_Price)]
        [HttpPost]
        [Route("purchase-invoice-item-print")]
        public virtual async Task<IActionResult> PrintPurchaseInvoiceItemReport(PurchaseInvoiceRequestModel requestModel)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _purchaseInvoiceService.PurchaseInvoiceItemSearchAsync(requestModel);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var reportTitle = "Purchase: Total Qty, Value, Average Price";
                var table = await _purchasePdfService.PrintPurchaseInvoiceItemReportToPdf(stream, list, reportTitle, requestModel.FromDate, requestModel.ToDate?.AddDays(1));
                if (requestModel.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(table, SheetNameGenerator.Generate("Sheet1",
                        requestModel.FromDate.HasValue ? requestModel.FromDate.Value.ToLocal().Date : null, requestModel.ToDate.HasValue ? requestModel.ToDate.Value.ToLocal().Date : null), tenantData.Name, tenantData.Address, reportTitle);
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

        [Authorize(Permissions.PurchaseModuleReports.Purchase_Report)]
        [HttpPost]
        [Route("purchase-report-print")]
        public virtual async Task<IActionResult> PrintPurchaseReport(PurchaseInvoiceRequestModel request)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var userName = _workContext.GetUserName() ?? "";
            var list = await _purchaseInvoiceService.PurchaseReportSearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var reportTitle = "Purchase Report";
                var table = await _purchasePdfService.PrintPurchaseReportToPdfAsync(stream, list, reportTitle, request.FromDate, request.ToDate?.AddDays(1));

                if (request.ReportType == 2)
                {
                    var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(table, SheetNameGenerator.Generate("Sheet1",
                        request.FromDate.HasValue ? request.FromDate.Value.ToLocal().Date : null, request.ToDate.HasValue ? request.ToDate.Value.ToLocal().Date : null), tenantData.Name, tenantData.Address, reportTitle);
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
