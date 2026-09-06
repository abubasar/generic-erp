using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.FundTransferTransactionType;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.FundTransferTransactionTypes;
using Application.Services.Services.Configuration.Pdf;
using Application.Services.ViewModels.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FundTransferTransactionTypeController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFundTransferTransactionTypeService _fundTransferTransactionTypeService;
        private readonly IConfigurationPdfService _configurationPdfService;
        public FundTransferTransactionTypeController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IFundTransferTransactionTypeService fundTransferTransactionTypeService, IConfigurationPdfService configurationPdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _fundTransferTransactionTypeService = fundTransferTransactionTypeService;
            _configurationPdfService = configurationPdfService;
        }
        [Authorize(Permissions.FundTransferTransactionTypes.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<FundTransferTransactionTypeViewModel>, int>>> Search(FundTransferTransactionTypeRequestModel request)
        {
            return await Result<Tuple<List<FundTransferTransactionTypeViewModel>, int>>.SuccessAsync(await _fundTransferTransactionTypeService.SearchAsync(request), "Result Found");
        }
        [Authorize(Permissions.FundTransferTransactionTypes.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] FundTransferTransactionTypeCreationDto fundTransferTransactionTypeCreationDto)
        {
            var fundTransferTransactionTypeId = await _fundTransferTransactionTypeService.AddAsync(fundTransferTransactionTypeCreationDto);
            return await Result<Guid>.SuccessAsync(fundTransferTransactionTypeId, "Fund Transfer Transaction Type Added Successfully");
        }
        [Authorize(Permissions.FundTransferTransactionTypes.Edit)]
        [HttpPost("/api/fundTransferTransactionType/update")]
        public virtual async Task<Result> Put([FromBody] FundTransferTransactionTypeUpdateDto fundTransferTransactionTypeUpdateDto)
        {
            var fundTransferTransactionTypeId = await _fundTransferTransactionTypeService.UpdateAsync(fundTransferTransactionTypeUpdateDto);
            return await Result<Guid>.SuccessAsync(fundTransferTransactionTypeId, "Fund Transfer Transaction Type Updated Successfully");
        }
        [Authorize(Permissions.FundTransferTransactionTypes.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var fundTransferTransactionTypeId = await _fundTransferTransactionTypeService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(fundTransferTransactionTypeId, "Fund Transfer Transaction Type Deleted Successfully");
        }
    }
}
