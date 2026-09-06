using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Sale.SaleQuotation;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Common;
using Application.Services.Services.Sale.SaleQuotations;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.SaleQuotation;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SaleQuotationController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISaleQuotationService _saleQuotationService;

        public SaleQuotationController(IWorkContext workContext, IUnitOfWork unitOfWork, IMapper mapper, ISaleQuotationService saleQuotationService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _saleQuotationService = saleQuotationService;
        }

        [Authorize(Permissions.SaleQuotations.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<SaleQuotationViewModel>, int>>> Search(SaleQuotationRequestModel request)
        {
            return await Result<Tuple<List<SaleQuotationViewModel>, int>>.SuccessAsync(await _saleQuotationService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.SaleQuotations.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<SaleQuotationAggregatorModel>> ReportAggregates(SaleQuotationRequestModel request)
        {
            return await Result<SaleQuotationAggregatorModel>.SuccessAsync(await _saleQuotationService.PrepareSaleQuotationAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.SaleQuotations.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<SaleQuotationViewModel>> GetById(Guid id)
        {
            return await Result<SaleQuotationViewModel>.SuccessAsync(await _saleQuotationService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.SaleQuotations.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _saleQuotationService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.SaleQuotations.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] SaleQuotationCreationDto saleQuotationCreationDto)
        {
            var saleQuotationId = await _saleQuotationService.AddAsync(saleQuotationCreationDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(saleQuotationId, "Sale Quotation Added Successfully");
        }

        [Authorize(Permissions.SaleQuotations.Edit)]
        [HttpPost("/api/saleQuotation/update")]
        public virtual async Task<Result> Put([FromBody] SaleQuotationUpdateDto saleQuotationUpdateDto)
        {
            var saleQuotationId = await _saleQuotationService.UpdateAsync(saleQuotationUpdateDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(saleQuotationId, "Sale Quotation Updated Successfully");
        }

        [Authorize(Permissions.SaleQuotations.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var saleQuotationId = await _saleQuotationService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(saleQuotationId, "Sale Quotation Deleted Successfully");
        }

        [Authorize(Permissions.SaleQuotations.Check)]
        [HttpPost("/api/saleQuotation/check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _saleQuotationService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)SaleQuotationStatus.Checked, "Sale Quotation Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.SaleQuotations.Approve)]
        [HttpPost("/api/saleQuotation/approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _saleQuotationService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)SaleQuotationStatus.Approved, "Sale Quotation Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.SaleQuotations.Unpost)]
        [HttpPost("/api/saleQuotation/unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _saleQuotationService.UnpostAsync(id, fromStatus), "Sale Quotation Unposted Successfully");
        }
    }
}
