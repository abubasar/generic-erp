using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.ExcelHelper;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.LCCostEntry;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Purchase.LCCostEntries;
using Application.Services.Services.Purchase.Pdf;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.LCCostEntry;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("purchase")]
    public class LCCostEntryController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILCCostEntryService _lcCostEntryService;
        private readonly IPurchasePdfService _purchasePdfService;
        private readonly ITenantService _tenantService;

        public LCCostEntryController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, ILCCostEntryService lCCostEntryService, IPurchasePdfService purchasePdfService, ITenantService tenantService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _lcCostEntryService = lCCostEntryService;
            _purchasePdfService = purchasePdfService;
            _tenantService = tenantService;
        }

        [Authorize(Permissions.LCCostEntries.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<LCCostEntryViewModel>, int>>> Search(LCCostEntryRequestModel request)
        {
            return await Result<Tuple<List<LCCostEntryViewModel>, int>>.SuccessAsync(await _lcCostEntryService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.LCCostEntries.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<LCCostEntryAggregatorModel>> ReportAggregates(LCCostEntryRequestModel request)
        {
            return await Result<LCCostEntryAggregatorModel>.SuccessAsync(await _lcCostEntryService.PrepareLCCostEntryAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.LCCostEntries.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<LCCostEntryViewModel>> GetById(Guid id)
        {
            return await Result<LCCostEntryViewModel>.SuccessAsync(await _lcCostEntryService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.LCCostEntries.View)]
        [Route("lc-cost-entry-by-poId/{purchaseOrderId}")]
        [HttpGet]
        public async Task<Result<List<LcCostEntryDetailAgainstPoViewModel>>> GetLcCostEntriesByPo(Guid purchaseOrderId)
        {
            return await Result<List<LcCostEntryDetailAgainstPoViewModel>>.SuccessAsync(await _lcCostEntryService.GetLcCostEntriesByPoAsync(purchaseOrderId), "Result Found");
        }

        [Authorize(Permissions.LCCostEntries.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _lcCostEntryService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.LCCostEntries.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] LCCostEntryCreationDto lCCostEntryCreationDto)
        {
            var lCCostEntryId = await _lcCostEntryService.AddAsync(lCCostEntryCreationDto);
            return await Result<Guid>.SuccessAsync(lCCostEntryId, "LC Cost Entry Added Successfully");
        }

        [Authorize(Permissions.LCCostEntries.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] LCCostEntryUpdateDto lCCostEntryUpdateDto)
        {
            var lCCostEntryId = await _lcCostEntryService.UpdateAsync(lCCostEntryUpdateDto);
            return await Result<Guid>.SuccessAsync(lCCostEntryId, "LC Cost Entry Updated Successfully");
        }

        [Authorize(Permissions.LCCostEntries.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var lCCostEntryId = await _lcCostEntryService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(lCCostEntryId, "LC Cost Entry Deleted Successfully");
        }

        [Authorize(Permissions.LCCostEntries.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _lcCostEntryService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)LCCostEntryStatus.Checked, "LC Cost Entry Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.LCCostEntries.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _lcCostEntryService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)LCCostEntryStatus.Approved, "LC Cost Entry Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.LCCostEntries.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _lcCostEntryService.UnpostAsync(id, fromStatus), "LC Cost Entry Unposted Successfully");
        }

        [HttpPost]
        [Route("lc-cost-entry-summary-print")]
        public virtual async Task<IActionResult> PrintLCCostEntrySummaryReport(LCCostEntryRequestModel request)
        {
            var reportTitle = "LC Cost Entry Summary";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "LcNumber";
            request.LCCostEntryStatus = (int)LCCostEntryStatus.Calculated_Within_Landed_Cost;
            var list = await _lcCostEntryService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintLCCostEntryReportToPdf(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("lc-cost-entry-details-print")]
        public virtual async Task<IActionResult> PrintLCCostEntryDetailsReport(LCCostEntryRequestModel request)
        {
            var reportTitle = "LC Cost Entry Details";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "LcNumber";
            request.LCCostEntryStatus = (int)LCCostEntryStatus.Calculated_Within_Landed_Cost;
            var list = await _lcCostEntryService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintLCCostEntryReportToPdf(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [Authorize(Permissions.LCCostEntries.LC_Cost_Entry_Against_PO_Report)]
        [HttpGet("lc-cost-entry-against-po-print/{purchaseOrderId}/{reportType}")]
        public async Task<IActionResult> PrintLcCostEntryAgainstPurchaseOrderReport(Guid purchaseOrderId, int reportType)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            var dbPurchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().FindAsync(x => x.Id == purchaseOrderId);
            if (dbPurchaseOrder == null) throw new BadHttpRequestException("No LC Cost Entries are found against this LC Number");
            var userName = _workContext.GetUserName() ?? "";
            var list = await _lcCostEntryService.GetLcCostEntriesByPoAsync(purchaseOrderId);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "Statement of LC Cost Entries Against LC Number";
                var tables = await _purchasePdfService.PrintLcCostEntryAgainstPurchaseOrderReportToPdf(stream, list, headerText, dbPurchaseOrder.LcNumber, dbPurchaseOrder.Ponumber);

                if (reportType == 2)
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
    }
}
