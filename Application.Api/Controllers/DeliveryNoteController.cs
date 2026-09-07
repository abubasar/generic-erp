using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Sale.DeliveryNote;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Common;
using Application.Services.Services.Sale.DeliveryNotes;
using Application.Services.Services.Sale.Pdf;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.DeliveryNote;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("sales")]
    public class DeliveryNoteController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDeliveryNoteService _deliveryNoteService;
        private readonly ISalePdfService _salePdfService;

        public DeliveryNoteController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IDeliveryNoteService deliveryNoteService, ISalePdfService salePdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _deliveryNoteService = deliveryNoteService;
            _salePdfService = salePdfService;
        }

        [Authorize(SecondaryPermissions.DeliveryNotes.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<DeliveryNoteViewModel>, int>>> Search(DeliveryNoteRequestModel request)
        {
            return await Result<Tuple<List<DeliveryNoteViewModel>, int>>.SuccessAsync(await _deliveryNoteService.SearchAsync(request), "Result Found");
        }

        [Authorize(SecondaryPermissions.DeliveryNotes.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<DeliveryNoteAggregatorModel>> ReportAggregates(DeliveryNoteRequestModel request)
        {
            return await Result<DeliveryNoteAggregatorModel>.SuccessAsync(await _deliveryNoteService.PrepareDeliveryNoteAggregatorModel(request), "Summation Found");
        }

        [Authorize(SecondaryPermissions.DeliveryNotes.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<DeliveryNoteViewModel>> GetById(Guid id)
        {
            return await Result<DeliveryNoteViewModel>.SuccessAsync(await _deliveryNoteService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(SecondaryPermissions.DeliveryNotes.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _deliveryNoteService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(SecondaryPermissions.DeliveryNotes.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] DeliveryNoteCreationDto deliveryNoteCreationDto)
        {
            var deliveryNoteId = await _deliveryNoteService.AddAsync(deliveryNoteCreationDto);
            return await Result<Guid>.SuccessAsync(deliveryNoteId, "Delivery Note Added Successfully");
        }

        [Authorize(SecondaryPermissions.DeliveryNotes.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] DeliveryNoteUpdateDto deliveryNoteUpdateDto)
        {
            var deliveryNoteId = await _deliveryNoteService.UpdateAsync(deliveryNoteUpdateDto);
            return await Result<Guid>.SuccessAsync(deliveryNoteId, "Delivery Note Updated Successfully");
        }

        [Authorize(SecondaryPermissions.DeliveryNotes.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var deliveryNoteId = await _deliveryNoteService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(deliveryNoteId, "Delivery Note Deleted Successfully");
        }

        [Authorize(SecondaryPermissions.DeliveryNotes.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _deliveryNoteService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)DeliveryNoteStatus.Checked, "Delivery Note Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(SecondaryPermissions.DeliveryNotes.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _deliveryNoteService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)DeliveryNoteStatus.Approved, "Delivery Note Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(SecondaryPermissions.DeliveryNotes.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _deliveryNoteService.UnpostAsync(id, fromStatus), "Delivery Note Unposted Successfully");
        }

        [HttpPost]
        [Route("delivery-note-summary-print")]
        public virtual async Task<IActionResult> PrintDeliveryNoteSummaryReport(DeliveryNoteRequestModel request)
        {
            var reportTitle = "Delivery Note Summary";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "DeliveryNoteNo";
            request.DeliveryNoteStatuses = new List<int> { (int)DeliveryNoteStatus.Approved, (int)DeliveryNoteStatus.Invoice_Generated };
            var list = await _deliveryNoteService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintDeliveryNoteReportToPdf(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("delivery-note-details-print")]
        public virtual async Task<IActionResult> PrintDeliveryNoteDetailsReport(DeliveryNoteRequestModel request)
        {
            var reportTitle = "Delivery Note Details";
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "DeliveryNoteNo";
            request.DeliveryNoteStatuses = new List<int> { (int)DeliveryNoteStatus.Approved, (int)DeliveryNoteStatus.Invoice_Generated };
            var list = await _deliveryNoteService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _salePdfService.PrintDeliveryNoteReportToPdf(stream, list.Item1.ToList(), reportTitle, isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpGet("gate-pass/{deliveryNoteId}")]
        public virtual async Task<IActionResult> GatePass(Guid deliveryNoteId)
        {
            var userName = _workContext.GetUserName() ?? "";
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                var headerText = "GATE PASS";
                await _salePdfService.PrintGatePassReportToPdfAsync(stream, deliveryNoteId, userName, headerText);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddA5Footer(bytes, userName);
                return File(bytes, MimeTypes.ApplicationPdf);
            }
        }

    }
}
