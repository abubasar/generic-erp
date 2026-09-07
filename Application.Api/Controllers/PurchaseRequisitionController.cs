using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.PurchaseRequisition;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Purchase.PurchaseRequisitions;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("purchase")]
    public class PurchaseRequisitionController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPurchaseRequisitionService _purchaseRequisitionService;
        public PurchaseRequisitionController(IPurchaseRequisitionService purchaseRequisitionService,
           IWorkContext workContext,
            IUnitOfWork unitOfWork, IMapper mapper)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _purchaseRequisitionService = purchaseRequisitionService;
            _mapper = mapper;
        }
        [Authorize(Permissions.PurchaseRequisitions.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<PurchaseRequisitionViewModel>, int>>> Search(PurchaseRequisitionRequestModel request)
        {
            return await Result<Tuple<List<PurchaseRequisitionViewModel>, int>>.SuccessAsync(await _purchaseRequisitionService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.PurchaseRequisitions.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<PurchaseRequisitionViewModel>> GetById(Guid id)
        {
            return await Result<PurchaseRequisitionViewModel>.SuccessAsync(await _purchaseRequisitionService.GetByIdAsync(id), "Result Found");
        }
        [Authorize(Permissions.PurchaseRequisitions.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _purchaseRequisitionService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.PurchaseRequisitions.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] PurchaseRequisitionCreationDto purchaseRequisitionCreationDto)
        {
            var purchaseRequisitionId = await _purchaseRequisitionService.AddAsync(purchaseRequisitionCreationDto);
            return await Result<Guid>.SuccessAsync(purchaseRequisitionId, "Purchase Requisition Added Successfully");
        }
        [Authorize(Permissions.PurchaseRequisitions.Edit)]
        [HttpPost("/api/purchaseRequisition/update")]
        public virtual async Task<Result> Put([FromBody] PurchaseRequisitionUpdateDto purchaseRequisitionUpdateDto)
        {
            var purchaseRequisitionId = await _purchaseRequisitionService.UpdateAsync(purchaseRequisitionUpdateDto);
            return await Result<Guid>.SuccessAsync(purchaseRequisitionId, "PurchaseRequisition Updated Successfully");
        }
        [Authorize(Permissions.PurchaseRequisitions.Edit)]
        [HttpPost("/api/purchaseRequisition/prepare-rfq")]
        public virtual async Task<Result> PrepareRequsitionForRfq([FromBody] PurchaseRequisitionUpdateDto purchaseRequisitionUpdateDto)
        {
            var purchaseRequisitionId = await _purchaseRequisitionService.PrepareRfqAsync(purchaseRequisitionUpdateDto);
            return await Result<Guid>.SuccessAsync(purchaseRequisitionId, "RFQ Ready For Send");
        }
        [Authorize(Permissions.PurchaseRequisitions.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var purchaseRequisitionId = await _purchaseRequisitionService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(purchaseRequisitionId, "PurchaseRequisition Deleted Successfully");


        }
        [Authorize(Permissions.PurchaseRequisitions.Check)]
        [HttpPost("/api/purchaseRequisition/check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _purchaseRequisitionService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)RequisitionStatus.Checked, "Purchase Requisition Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }
        [Authorize(Permissions.PurchaseRequisitions.Approve)]
        [HttpPost("/api/purchaseRequisition/approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _purchaseRequisitionService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)RequisitionStatus.Approved, "Purchase Requisition Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }
        [Authorize(Permissions.PurchaseRequisitions.Unpost)]
        [HttpPost("/api/purchaseRequisition/unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _purchaseRequisitionService.UnpostAsync(id, fromStatus), "Purchase Requisition Unposted Successfully");
        }
        [Authorize(Permissions.PurchaseRequisitions.SendRFQ)]
        [HttpPost("/api/purchaseRequisition/send-rfq")]
        public virtual async Task<Result> SendRFQtoSelectedVendor([FromBody] SendRFQtoSelectedSupplierDto sendRFQtoSelectedSupplierDto)
        {
            return await Result<int>.SuccessAsync(await _purchaseRequisitionService.SendRFQtoSelectedSupplier(sendRFQtoSelectedSupplierDto), "RFQ Sent Successfully");
        }
        [HttpGet("/api/purchaseRequisition/rfq-sent-suppliers/{id}")]
        public virtual async Task<Result> GetRfqSentSuppliersByRequisitionId(Guid id)
        {
            var result = await _purchaseRequisitionService.GetRfqSentSuppliersByRequisitionId(id);
            return await Result<List<RFQSentSupplier>>.SuccessAsync(result, "RFQ Sent Suppliers");
        }

    }
}
