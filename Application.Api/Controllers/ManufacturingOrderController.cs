using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Production.ManufacturingOrder;
using Application.Services.SearchRequestModels.Production;
using Application.Services.Services.Common;
using Application.Services.Services.Productions.ManufacturingOrders;
using Application.Services.ViewModels.Production;
using Application.Services.ViewModels.Production.ManufacturingOrder;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("production")]
    public class ManufacturingOrderController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IManufacturingOrderService _manufacturingOrderService;

        public ManufacturingOrderController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IManufacturingOrderService manufacturingOrderService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _manufacturingOrderService = manufacturingOrderService;
        }

        [Authorize(Permissions.ManufacturingOrders.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<ManufacturingOrderViewModel>, int>>> Search(ManufacturingOrderRequestModel request)
        {
            return await Result<Tuple<List<ManufacturingOrderViewModel>, int>>.SuccessAsync(await _manufacturingOrderService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.ManufacturingOrders.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<ManufacturingOrderAggregatorModel>> ReportAggregates(ManufacturingOrderRequestModel request)
        {
            return await Result<ManufacturingOrderAggregatorModel>.SuccessAsync(await _manufacturingOrderService.PrepareManufacturingOrderAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.ManufacturingOrders.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<ManufacturingOrderViewModel>> GetById(Guid id)
        {
            return await Result<ManufacturingOrderViewModel>.SuccessAsync(await _manufacturingOrderService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.ManufacturingOrders.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _manufacturingOrderService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.ManufacturingOrders.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] ManufacturingOrderCreationDto manufacturingOrderCreationDto)
        {
            var manufacturingOrderId = await _manufacturingOrderService.AddAsync(manufacturingOrderCreationDto);
            return await Result<Guid>.SuccessAsync(manufacturingOrderId, "Manufacturing Order Added Successfully");
        }

        [Authorize(Permissions.ManufacturingOrders.Edit)]
        [HttpPost("/api/manufacturingOrder/update")]
        public virtual async Task<Result> Put([FromBody] ManufacturingOrderUpdateDto manufacturingOrderUpdateDto)
        {
            var manufacturingOrderId = await _manufacturingOrderService.UpdateAsync(manufacturingOrderUpdateDto);
            return await Result<Guid>.SuccessAsync(manufacturingOrderId, "Manufacturing Order Updated Successfully");
        }

        [Authorize(Permissions.ManufacturingOrders.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var manufacturingOrderId = await _manufacturingOrderService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(manufacturingOrderId, "Manufacturing Order Deleted Successfully");
        }

        [Authorize(Permissions.ManufacturingOrders.Check)]
        [HttpPost("/api/manufacturingOrder/check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _manufacturingOrderService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ManufacturingOrderStatus.Checked, "Manufacturing Order Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.ManufacturingOrders.Approve)]
        [HttpPost("/api/manufacturingOrder/approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _manufacturingOrderService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)ManufacturingOrderStatus.Approved, "Manufacturing Order Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.ManufacturingOrders.Unpost)]
        [HttpPost("/api/manufacturingOrder/unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _manufacturingOrderService.UnpostAsync(id, fromStatus), "Manufacturing Order Unposted Successfully");
        }
    }
}
