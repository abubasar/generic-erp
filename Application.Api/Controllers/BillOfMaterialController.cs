using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Production.BillOfMaterial;
using Application.Services.SearchRequestModels.Production;
using Application.Services.Services.Common;
using Application.Services.Services.Productions.BillOfMaterials;
using Application.Services.ViewModels.Production;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("production")]
    public class BillOfMaterialController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBillOfMaterialService _billOfMaterialService;

        public BillOfMaterialController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IBillOfMaterialService billOfMaterialService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _billOfMaterialService = billOfMaterialService;
        }

        [Authorize(Permissions.BillOfMaterials.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<BillOfMaterialViewModel>, int>>> Search(BillOfMaterialRequestModel request)
        {
            return await Result<Tuple<List<BillOfMaterialViewModel>, int>>.SuccessAsync(await _billOfMaterialService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.BillOfMaterials.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<BillOfMaterialViewModel>> GetById(Guid id)
        {
            return await Result<BillOfMaterialViewModel>.SuccessAsync(await _billOfMaterialService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.BillOfMaterials.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _billOfMaterialService.GetPendingCheckedCountAsync(), "Success");
        }
        [Authorize(Permissions.BillOfMaterials.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] BillOfMaterialCreationDto billOfMaterialCreationDto)
        {
            var billOfMaterialId = await _billOfMaterialService.AddAsync(billOfMaterialCreationDto);
            return await Result<Guid>.SuccessAsync(billOfMaterialId, "Bill of Material Added Successfully");
        }
        [Authorize(Permissions.BillOfMaterials.Edit)]
        [HttpPost("/api/billOfMaterial/update")]
        public virtual async Task<Result> Put([FromBody] BillOfMaterialUpdateDto billOfMaterialUpdateDto)
        {
            var billOfMaterialId = await _billOfMaterialService.UpdateAsync(billOfMaterialUpdateDto);
            return await Result<Guid>.SuccessAsync(billOfMaterialId, "Bill of Material Updated Successfully");
        }
        [Authorize(Permissions.BillOfMaterials.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var billOfMaterialId = await _billOfMaterialService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(billOfMaterialId, "Bill of Material Deleted Successfully");
        }
        [Authorize(Permissions.BillOfMaterials.Check)]
        [HttpPost("/api/billOfMaterial/check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _billOfMaterialService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)BOMStatus.Checked, "Bill of Material Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }
        [Authorize(Permissions.BillOfMaterials.Approve)]
        [HttpPost("/api/billOfMaterial/approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _billOfMaterialService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)BOMStatus.Approved, "Bill of Material Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }
        [Authorize(Permissions.BillOfMaterials.Unpost)]
        [HttpPost("/api/billOfMaterial/unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _billOfMaterialService.UnpostAsync(id, fromStatus), "Bill of Material Unposted Successfully");
        }
    }
}
