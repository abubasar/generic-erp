using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.ExcelHelper;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.VoucherEntry;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Accounts.PdfAccount;
using Application.Services.Services.Accounts.VoucherEntries;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("accounts")]
    public class VoucherEntryController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IVoucherEntryService _voucherEntryService;
        private readonly IAccountService _accountService;
        private readonly IAccountReportPdfService _accountReportPdfService;
        private readonly ITenantService _tenantService;

        public VoucherEntryController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork,
            IVoucherEntryService voucherEntryService, IAccountService accountService, IAccountReportPdfService accountReportPdfService, ITenantService tenantService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _voucherEntryService = voucherEntryService;
            _accountService = accountService;
            _accountReportPdfService = accountReportPdfService;
            _tenantService = tenantService;
        }

        [Authorize(Permissions.VoucherEntries.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<VoucherEntryViewModel>, int>>> Search(VoucherEntryRequestModel request)
        {
            return await Result<Tuple<List<VoucherEntryViewModel>, int>>.SuccessAsync(await _voucherEntryService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.VoucherEntries.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<VoucherEntryViewModel>> GetById(Guid id)
        {
            return await Result<VoucherEntryViewModel>.SuccessAsync(await _voucherEntryService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.VoucherEntries.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _voucherEntryService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.VoucherEntries.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] VoucherEntryCreationDto voucherEntryCreationDto)
        {
            var voucherEntryId = await _voucherEntryService.AddAsync(voucherEntryCreationDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(voucherEntryId, "Voucher Entry Added Successfully");
        }

        [Authorize(Permissions.VoucherEntries.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] VoucherEntryUpdateDto voucherEntryUpdateDto)
        {
            var voucherEntryId = await _voucherEntryService.UpdateAsync(voucherEntryUpdateDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(voucherEntryId, "Voucher Entry Updated Successfully");
        }

        [Authorize(Permissions.VoucherEntries.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var voucherEntryId = await _voucherEntryService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(voucherEntryId, "Voucher Entry Deleted Successfully");
        }

        [Authorize(Permissions.VoucherEntries.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _voucherEntryService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)VoucherEntryStatus.Checked, "Voucher Entry Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.VoucherEntries.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _voucherEntryService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)VoucherEntryStatus.Approved, "Voucher Entry Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.VoucherEntries.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _voucherEntryService.UnpostAsync(id, fromStatus), "Voucher Entry Unposted Successfully");
        }

        [Authorize(Permissions.AccountsModuleReports.Payment_Report)]
        [HttpPost]
        [Route("payment-report/print")]
        public virtual async Task<IActionResult> PrintPaymentReport(VoucherEntryRequestModel request)
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
                var supplierName = "";
                var costCenterName = "";
                if (request.AccountId.HasValue) supplierName = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == request.AccountId.Value)?.Name;
                if (request.CostCenterId.HasValue) costCenterName = _unitOfWork.Repository<CostCenter>().TableNoTracking().FirstOrDefault(x => x.Id == request.CostCenterId.Value)?.Name;
                var list = await _accountService.PreparePaymentReport(request.FromDate, request.ToDate, request.AccountTransactionType, request.CostCenterId, request.AccountId, request.PaymentModeId);
                //headers and column Widths
                List<string> headers = new List<string> { "Sl", "Date", "VoucherNo", "Code", "Name", "Cash/Bank", "Description", "Amount" };
                List<float> columnWidths = new List<float> { 4f, 7f, 10f, 8f, 20f, 22f, 19f, 10f };
                // Create a list of data with properties mapped to header text
                List<object> tableData = new List<object>();
                int sl = 0;
                foreach (var transaction in list)
                {
                    var account = await _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == transaction.AccountId);
                    sl++;
                    tableData.Add(new
                    {
                        SL = sl,
                        Date = transaction.TransactionDate.ToString("dd/MM/yyyy"),
                        VoucherNo = transaction.Vnumber,
                        account?.Code,
                        account?.Name,
                        //CashBankAccountName = await _accountService.GetContraAccountName(transaction),
                        CashBankAccountName = transaction.ContraAccountNames,
                        Particulars = transaction.Description,
                        Amount = transaction.Debit
                    });
                }

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var headerText = "Payment Report";
                    var tables = await _accountReportPdfService.PrintReportToPdfAsync(stream, headers, columnWidths, tableData, headerText, numericColumnsForSum: new List<string> { "Amount" },
                        true, request.FromDate, request.ToDate, supplierName, costCenterName);
                    if (request.ReportType == 2)
                    {
                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, headerText);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        //bytes = PdfHelper.AddPageNumbers(bytes, userName);
                        return File(bytes, MimeTypes.ApplicationPdf);
                    }
                }
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [Authorize(Permissions.AccountsModuleReports.Collection_Report)]
        [HttpPost]
        [Route("payment-collection-report/print")]
        public virtual async Task<IActionResult> PrintPaymentCollectionReport(PaymentCollectionRequestModel request)
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

                var list = await _accountService.PrepareCollectionReport(request, tenantData.BusinessType);

                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    var reportTitle = "Collection Report";

                    var tables = await _accountReportPdfService.PrintPaymentCollectionReportToPdfAsync(stream, list, request, reportTitle);

                    if (request.ReportType == 2)
                    {

                        var excelBytes = ExcelGenerator.GenerateExcelByteArrayFromItextTable(tables.filterTable, tables.dataTable, SheetNameGenerator.Generate("Sheet1",
                            request.FromDate.HasValue ? request.FromDate.Value : null, request.ToDate.HasValue ? request.ToDate.Value : null), tenantData.Name, tenantData.Address, reportTitle);
                        return File(excelBytes, MimeTypes.TextXlsx);
                    }
                    else
                    {
                        bytes = stream.ToArray();
                        //bytes = PdfHelper.AddPageNumbers(bytes, userName);
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
