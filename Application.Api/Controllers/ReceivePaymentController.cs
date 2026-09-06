using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment;
using Application.Services.SearchRequestModels.Accounts.AccountsReceivable;
using Application.Services.Services.Accounts.AccountsReceivable.ReceivePayments;
using Application.Services.Services.Common;
using Application.Services.Services.Sale.Pdf;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Accounts.AccountsReceivable.ReceivePayment;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceivePaymentController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReceivePaymentService _receivePaymentService;
        private readonly ISalePdfService _salePdfService;

        public ReceivePaymentController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork,
            IReceivePaymentService receivePaymentService, ISalePdfService salePdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _receivePaymentService = receivePaymentService;
            _salePdfService = salePdfService;
        }

        [Authorize(SecondaryPermissions.ReceivePayments.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<ReceivePaymentViewModel>, int>>> Search(ReceivePaymentRequestModel request)
        {
            return await Result<Tuple<List<ReceivePaymentViewModel>, int>>.SuccessAsync(await _receivePaymentService.SearchAsync(request), "Result Found");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<ReceivePaymentAggregatorModel>> ReportAggregates(ReceivePaymentRequestModel request)
        {
            return await Result<ReceivePaymentAggregatorModel>.SuccessAsync(await _receivePaymentService.PrepareReceivePaymentAggregatorModel(request), "Summation Found");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<ReceivePaymentViewModel>> GetById(Guid id)
        {
            return await Result<ReceivePaymentViewModel>.SuccessAsync(await _receivePaymentService.GetByIdAsync(id), "Result Found");
        }
        [Route("status/{id}")]
        [HttpGet]
        public async Task<Result<int>> GetStatusById(Guid id)
        {
            var receivePayment = await _receivePaymentService.GetByIdAsync(id);
            return await Result<int>.SuccessAsync(receivePayment.Status, "Receive Payment Status Found");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _receivePaymentService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ReceivePaymentCreationDto receivePaymentCreationDto)
        {
            var receivePayment = await _receivePaymentService.AddAsync(receivePaymentCreationDto);
            return await Result<Guid>.SuccessAsync(receivePayment, "Money Receipt Added Successfully");
        }
        [Authorize(SecondaryPermissions.ReceivePayments.Create)]
        [HttpPost("add-from-dms-app")]
        public virtual async Task<Result> AddByDMS([FromForm] ReceivePaymentCreationDtoForDmsApp receivePaymentCreationDto)
        {
            var receivePayment = await _receivePaymentService.AddByDmsAppAsync(receivePaymentCreationDto);
            return await Result<Guid>.SuccessAsync(receivePayment, "Money Receipt Added Successfully");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] ReceivePaymentUpdateDto receivePaymentUpdateDto)
        {
            var receivePayment = await _receivePaymentService.UpdateAsync(receivePaymentUpdateDto);
            return await Result<Guid>.SuccessAsync(receivePayment, "Money Receipt Updated Successfully");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var receivePaymentId = await _receivePaymentService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(receivePaymentId, "Money Receipt Deleted Successfully");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _receivePaymentService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ReceivePaymentStatus.Checked, "Money Receipt Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _receivePaymentService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ReceivePaymentStatus.Approved, "Money Receipt Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _receivePaymentService.UnpostAsync(id, fromStatus), "Money Receipt Unposted Successfully");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.SendToCustomer)]
        [HttpPost("send/{moneyReceiptId}")]
        public virtual async Task<Result> SendToCustomer(Guid moneyReceiptId)
        {
            var status = await _receivePaymentService.SendToCustomerAsync(moneyReceiptId);
            if (status) return await Result<int>.SuccessAsync((int)ReceivePaymentStatus.Sent_To_Customer, "Money Receipt Sent Successfully to the Customer");
            else return await Result<string>.FailAsync("Failed To Send", "Something Went Wrong");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.SingleFileUpload)]
        [HttpPost("single_file")]
        public async Task<Result> PostSingleFile([FromForm] ReceivePaymentPictureMappingCreationDto creationDto)
        {
            if (creationDto == null)
            {
                throw new BadRequestException("Invalid Information.");
            }
            var paymentPictureMappingId = await _receivePaymentService.PostSingleFileAsync(creationDto);
            return await Result<Guid>.SuccessAsync(paymentPictureMappingId, "Uploaded Successfully");
        }

        [Authorize(SecondaryPermissions.ReceivePayments.MultipleFileUpload)]
        [HttpPost("receive_payment_picture_mapping")]
        public async Task<ActionResult> AddReceivePaymentPictureMapping([FromForm] List<ReceivePaymentPictureMappingCreationDto> receivePaymentPictureMappingCreationDto)
        {
            if (receivePaymentPictureMappingCreationDto == null)
            {
                throw new BadRequestException("Please, attach your File!!");
            }
            try
            {
                await _receivePaymentService.AddReceivePaymentPictureMappingAsync(receivePaymentPictureMappingCreationDto);
                return Ok();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //this controller will work for download and view image
        [Authorize(SecondaryPermissions.ReceivePayments.SingleFileUpload)]
        [HttpGet("single_file_view_download/{receivePaymentId}")]
        public async Task<IActionResult> DownloadFile(Guid receivePaymentId)
        {
            if (receivePaymentId == Guid.Empty)
            {
                throw new BadRequestException("Invalid Money Receipt.");
            }
            var fileResult = await _receivePaymentService.GetFileById(receivePaymentId);
            if (fileResult == null)
            {
                throw new NotFoundResultException("File not found.");
            }

            return File(fileResult.Value.Stream, MimeTypes.ApplicationPdf, fileResult.Value.FileName);
        }

        [HttpPost]
        [Route("payment-collection-report/print")]
        public virtual async Task<IActionResult> PrintPaymentCollectionReport(ReceivePaymentRequestModel request)
        {
            request.Page = -1;
            request.OrderBy = "Code";
            request.IsAscending = true;
            request.ReceivePaymentStatus = (int)ReceivePaymentStatus.Approved;
            var list = await _receivePaymentService.SearchAsync(request);

            //headers and column Widths
            List<string> headers = new List<string> { "SL", "Date", "ReceiptNo", "Code", "Name", "Cash/Bank", "Description", "Amount" };
            List<float> columnWidths = new List<float> { 4f, 7f, 10f, 8f, 20f, 22f, 19f, 10f };
            // Create a list of data with properties mapped to header text
            List<object> tableData = new List<object>();

            int sl = 0;
            foreach (var payment in list.Item1)
            {
                sl++;
                tableData.Add(new
                {
                    SL = sl,
                    Date = payment.PaymentDate.ToString("dd/MM/yyyy"),
                    ReceiptNo = payment.Code,
                    Code = payment.Customer?.Code,
                    Name = payment.Customer?.Name,
                    CashBank = String.Join(",", payment.ReceivePaymentDetails!.Select(x => x.Account?.Name + " (" + x.Amount + ")").ToArray()),
                    Description = payment.Remark ?? "",
                    Amount = payment.TotalAmount
                });
            }

            //........
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                string reportTitle = "Payment Collection Report";
                await _salePdfService.PrintReportToPdf(stream, headers, columnWidths, tableData, reportTitle, numericColumnsForSum: new List<string> { "Amount" },
                    true, request.FromDate, request.ToDate?.AddDays(1));
                bytes = stream.ToArray();
            }

            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("receive-payment-summary-print")]
        public virtual async Task<IActionResult> PrintReceivePaymentSummaryReport(ReceivePaymentRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "Code";
            request.ReceivePaymentStatuses = new List<int> { (int)ReceivePaymentStatus.Approved, (int)ReceivePaymentStatus.Sent_To_Customer, (int)ReceivePaymentStatus.Occupied };
            var list = await _receivePaymentService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintReceivePaymentReportToPdf(stream, list.Item1.ToList(), "Money Receipt Summary", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("receive-payment-details-print")]
        public virtual async Task<IActionResult> PrintReceivePaymentDetailsReport(ReceivePaymentRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "Code";
            request.ReceivePaymentStatuses = new List<int> { (int)ReceivePaymentStatus.Approved, (int)ReceivePaymentStatus.Sent_To_Customer, (int)ReceivePaymentStatus.Occupied };
            var list = await _receivePaymentService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintReceivePaymentReportToPdf(stream, list.Item1.ToList(), "Money Receipt Details", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }
    }
}
