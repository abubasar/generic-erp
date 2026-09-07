using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Interfaces;
using Application.Services.Dtos.Configuration.DiscountProductWise;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.DiscountProductWises;
using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Sale;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("sales")]
    public class DiscountProductWiseController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDiscountProductWiseService _discountProductWiseService;

        public DiscountProductWiseController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IDiscountProductWiseService discountProductWiseService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _discountProductWiseService = discountProductWiseService;
        }

        [Authorize(SecondaryPermissions.DiscountProductWises.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<DiscountProductWiseViewModel>, int>>> Search(DiscountProductWiseRequestModel request)
        {
            return await Result<Tuple<List<DiscountProductWiseViewModel>, int>>.SuccessAsync(await _discountProductWiseService.SearchAsync(request), "Result Found");
        }

        [Authorize(SecondaryPermissions.DiscountProductWises.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] DiscountProductWiseCreationDto discountProductWiseCreationDto)
        {
            var discountProductWiseId = await _discountProductWiseService.AddAsync(discountProductWiseCreationDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(discountProductWiseId, " Discount Product Wise Added Successfully");
        }

        [Authorize(SecondaryPermissions.DiscountProductWises.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] DiscountProductWiseUpdateDto discountProductWiseUpdateDto)
        {
            var discountProductWiseId = await _discountProductWiseService.UpdateAsync(discountProductWiseUpdateDto);
            return await Result<AddUpdateResponseModel>.SuccessAsync(discountProductWiseId, " Discount Product Wise Updated Successfully");
        }

        [Authorize(SecondaryPermissions.DiscountProductWises.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var discountProductWiseId = await _discountProductWiseService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(discountProductWiseId, " Discount Product Wise Deleted Successfully");
        }

        [Authorize(SecondaryPermissions.DiscountProductWises.View)]
        [Route("discount-per-kg")]
        [HttpPost]
        public virtual async Task<Result> GetOfferProductDiscountPerKg(DiscountProductWiseByProductIdRequestModel request)
        {
            var discount = await _discountProductWiseService.GetDiscountProductWiseByProductId(request);
            return await Result<DiscountProductWiseByDateViewModel>.SuccessAsync(discount, "Discount Product Wise Found Successfully");
        }

        [Authorize(SecondaryPermissions.DiscountProductWises.View)]
        [Route("active-discount")]
        [HttpPost]
        public virtual async Task<Result> GetActiveDiscountProductWise(DiscountProductWiseByDateRequestModel request)
        {
            var discount = await _discountProductWiseService.GetActiveDiscountProductWiseByDate(request);
            return await Result<List<ActiveDiscountProductWiseByDateViewModel>>.SuccessAsync(discount, "Active Discount Product Wise Found Successfully");
        }
    }
}
