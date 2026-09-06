using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePaymentAgainstSale;
using Application.Services.SearchRequestModels.Accounts.AccountsReceivable;
using Application.Services.Services.Accounts.AccountsReceivable.ReceivePaymentAgainstSales;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Accounts.AccountsReceivable.ReceivePaymentAgainstSale;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceivePaymentAgainstSaleController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IReceivePaymentAgainstSaleService _receivePaymentAgainstSaleService;
        public ReceivePaymentAgainstSaleController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IReceivePaymentAgainstSaleService receivePaymentAgainstSaleService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _receivePaymentAgainstSaleService = receivePaymentAgainstSaleService;
        }
        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<ReceivePaymentAgainstSaleViewModel>, int>>> Search(ReceivePaymentAgainstSaleRequestModel request)
        {
            return await Result<Tuple<List<ReceivePaymentAgainstSaleViewModel>, int>>.SuccessAsync(await _receivePaymentAgainstSaleService.SearchAsync(request), "Result Found");
        }

        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<ReceivePaymentAgainstSaleAggregatorModel>> ReportAggregates(ReceivePaymentAgainstSaleRequestModel request)
        {
            return await Result<ReceivePaymentAgainstSaleAggregatorModel>.SuccessAsync(await _receivePaymentAgainstSaleService.PrepareReceivePaymentAgainstSaleAggregatorModel(request), "Summation Found");
        }

        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<ReceivePaymentAgainstSaleViewModel>> GetById(Guid id)
        {
            return await Result<ReceivePaymentAgainstSaleViewModel>.SuccessAsync(await _receivePaymentAgainstSaleService.GetByIdAsync(id), "Result Found");
        }
        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _receivePaymentAgainstSaleService.GetPendingCheckedCountAsync(), "Success");
        }
        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.Create)]
        [HttpPost]
        public virtual async Task<Result<AddUpdateResponseModel>> Add([FromBody] ReceivePaymentAgainstSaleCreationDto receivePaymentAgainstSaleCreationDto)
        {
            var receivePaymentAgainstSale = await _receivePaymentAgainstSaleService.AddAsync(receivePaymentAgainstSaleCreationDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(receivePaymentAgainstSale, "Receive Payment Against Sale Added Successfully");
        }
        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.Create)]
        [HttpPost("add-from-dms-app")]
        public virtual async Task<Result> AddByDMS([FromForm] ReceivePaymentAgainstSaleCreationDtoForDmsApp receivePaymentAgainstSaleCreationDto)
        {
            var receivePaymentAgainstSale = await _receivePaymentAgainstSaleService.AddByDmsAppAsync(receivePaymentAgainstSaleCreationDto);
            return await Result<Guid>.SuccessAsync(receivePaymentAgainstSale, "Receive Payment Against Sale Added Successfully");
        }

        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.Edit)]
        [HttpPost("/api/ReceivePaymentAgainstSale/update")]
        public virtual async Task<Result<AddUpdateResponseModel>> Put([FromBody] ReceivePaymentAgainstSaleUpdateDto receivePaymentAgainstSaleUpdateDto)
        {
            var receivePaymentAgainstSale = await _receivePaymentAgainstSaleService.UpdateAsync(receivePaymentAgainstSaleUpdateDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(receivePaymentAgainstSale, "Receive Payment Against Sale Updated Successfully");
        }

        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var receivePaymentAgainstSaleId = await _receivePaymentAgainstSaleService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(receivePaymentAgainstSaleId, "Receive Payment Against Sale Deleted Successfully");
        }

        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.Check)]
        [HttpPost("/api/ReceivePaymentAgainstSale/check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _receivePaymentAgainstSaleService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ReceivePaymentStatus.Checked, "Receive Payment Against Sale Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }
        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.Approve)]
        [HttpPost("/api/ReceivePaymentAgainstSale/approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _receivePaymentAgainstSaleService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ReceivePaymentStatus.Approved, "Receive Payment Against Sale Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }
        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.Unpost)]
        [HttpPost("/api/ReceivePaymentAgainstSale/unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _receivePaymentAgainstSaleService.UnpostAsync(id, fromStatus), "Receive Payment Against Sale Unposted Successfully");
        }
        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.SingleFileUpload)]
        [HttpPost("single_file")]
        public async Task<Result> PostSingleFile([FromForm] ReceivePaymentAgainstSalePictureMappingCreationDto creationDto)
        {
            if (creationDto == null)
            {
                throw new BadRequestException("Invalid Information.");
            }
            var paymentPictureMappingId = await _receivePaymentAgainstSaleService.PostSingleFileAsync(creationDto);
            return await Result<Guid>.SuccessAsync(paymentPictureMappingId, "Uploaded Successfully");
        }
        //this controller will work for download and view image
        [Authorize(PrimaryPermissions.ReceivePaymentAgainstSales.SingleFileUpload)]
        [HttpGet("single_file_view_download/{receivePaymentAgainstSaleId}")]
        public async Task<IActionResult> DownloadFile(Guid receivePaymentAgainstSaleId)
        {
            if (receivePaymentAgainstSaleId == Guid.Empty)
            {
                throw new BadRequestException("Invalid Money Receipt.");
            }
            var fileResult = await _receivePaymentAgainstSaleService.GetFileById(receivePaymentAgainstSaleId);
            if (fileResult == null)
            {
                throw new NotFoundResultException("File not found.");
            }

            return File(fileResult.Value.Stream, MimeTypes.ApplicationPdf, fileResult.Value.FileName);
        }
    }
}
