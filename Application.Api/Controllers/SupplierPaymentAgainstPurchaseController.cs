using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.AccountsPayable.SupplierPaymentAgainstPurchase;
using Application.Services.SearchRequestModels.Accounts.AccountsPayable;
using Application.Services.Services.Accounts.AccountsPayable.SupplierPaymentAgainstPurchases;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts.AccountsPayable;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierPaymentAgainstPurchaseController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISupplierPaymentAgainstPurchaseService _supplierPaymentAgainstPurchaseService;
        public SupplierPaymentAgainstPurchaseController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, ISupplierPaymentAgainstPurchaseService supplierPaymentAgainstPurchaseService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _supplierPaymentAgainstPurchaseService = supplierPaymentAgainstPurchaseService;
        }

        [Authorize(Permissions.SupplierPaymentAgainstPurchases.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<SupplierPaymentAgainstPurchaseViewModel>, int>>> Search(SupplierPaymentAgainstPurchaseRequestModel request)
        {
            return await Result<Tuple<List<SupplierPaymentAgainstPurchaseViewModel>, int>>.SuccessAsync(await _supplierPaymentAgainstPurchaseService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.SupplierPaymentAgainstPurchases.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<SupplierPaymentAgainstPurchaseViewModel>> GetById(Guid id)
        {
            return await Result<SupplierPaymentAgainstPurchaseViewModel>.SuccessAsync(await _supplierPaymentAgainstPurchaseService.GetByIdAsync(id), "Result Found");
        }
        [Authorize(Permissions.SupplierPaymentAgainstPurchases.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _supplierPaymentAgainstPurchaseService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.SupplierPaymentAgainstPurchases.Create)]
        [HttpPost]
        public virtual async Task<Result<AddUpdateResponseModel>> Add([FromBody] SupplierPaymentAgainstPurchaseCreationDto supplierPaymentAgainstPurchaseCreationDto)
        {
            var supplierPaymentAgainstPurchaseId = await _supplierPaymentAgainstPurchaseService.AddAsync(supplierPaymentAgainstPurchaseCreationDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(supplierPaymentAgainstPurchaseId, "Supplier Payment Against Purchase Added Successfully");
        }

        [Authorize(Permissions.SupplierPaymentAgainstPurchases.Edit)]
        [HttpPost("/api/supplierPaymentAgainstPurchase/update")]
        public virtual async Task<Result<AddUpdateResponseModel>> Put([FromBody] SupplierPaymentAgainstPurchaseUpdateDto supplierPaymentAgainstPurchaseUpdateDto)
        {
            var supplierPaymentAgainstPurchaseId = await _supplierPaymentAgainstPurchaseService.UpdateAsync(supplierPaymentAgainstPurchaseUpdateDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(supplierPaymentAgainstPurchaseId, "Supplier Payment Against Purchase Updated Successfully");
        }

        [Authorize(Permissions.SupplierPaymentAgainstPurchases.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var supplierPaymentAgainstPurchaseId = await _supplierPaymentAgainstPurchaseService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(supplierPaymentAgainstPurchaseId, "Supplier Payment Against Purchase Deleted Successfully");
        }
        [Authorize(Permissions.SupplierPaymentAgainstPurchases.Check)]
        [HttpPost("/api/supplierPaymentAgainstPurchase/check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _supplierPaymentAgainstPurchaseService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ReceivePaymentStatus.Checked, "Supplier Payment Against Purchase Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }
        [Authorize(Permissions.SupplierPaymentAgainstPurchases.Approve)]
        [HttpPost("/api/supplierPaymentAgainstPurchase/approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _supplierPaymentAgainstPurchaseService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ReceivePaymentStatus.Approved, "Supplier Payment Against Purchase Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }
        [Authorize(Permissions.SupplierPaymentAgainstPurchases.Unpost)]
        [HttpPost("/api/supplierPaymentAgainstPurchase/unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _supplierPaymentAgainstPurchaseService.UnpostAsync(id, fromStatus), "Supplier Payment Against Purchase Unposted Successfully");
        }
    }
}
