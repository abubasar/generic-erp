using Application.Api.Attributes;
using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Dtos.Purchase.PurchaseOrder;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Purchase.Pdf;
using Application.Services.Services.Purchase.PurchaseOrders;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PurchaseOrder;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [RequiresModule("purchase")]
    public class PurchaseOrderController : ControllerBase
    {
        protected readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPurchasePdfService _purchasePdfService;

        public PurchaseOrderController(IMapper mapper, IWorkContext workContext, IUnitOfWork unitOfWork, IPurchaseOrderService purchaseOrderService, IPurchasePdfService purchasePdfService)
        {
            _mapper = mapper;
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _purchaseOrderService = purchaseOrderService;
            _purchasePdfService = purchasePdfService;
        }

        [Authorize(Permissions.PurchaseOrders.View)]
        [Route("search")]
        [HttpPost]
        public async Task<Result<Tuple<List<PurchaseOrderViewModel>, int>>> Search(PurchaseOrderRequestModel request)
        {
            return await Result<Tuple<List<PurchaseOrderViewModel>, int>>.SuccessAsync(await _purchaseOrderService.SearchAsync(request), "Result Found");
        }

        [Authorize(Permissions.PurchaseOrders.View)]
        [Route("last-po-details/{supplierId}/{productId}")]
        [HttpPost]
        public async Task<Result<LastPoDetailsViewModel>> GetLastPoDetails(Guid supplierId, Guid productId)
        {
            var result = await _purchaseOrderService.SearchLastPoDetailsAsync(supplierId, productId);
            return await Result<LastPoDetailsViewModel>.SuccessAsync(result, "Result Found");
        }

        [Authorize(Permissions.PurchaseOrders.Closed)]
        [HttpPost("close/{id}")]
        public virtual async Task<Result> Close(Guid id)
        {
            var isClosed = await _purchaseOrderService.CloseAsync(id);
            if (isClosed) return await Result<int>.SuccessAsync((int)PurchaseOrderStatus.Closed, "Purchase Order Closed Successfully");
            else return await Result<string>.FailAsync("Failed To Close", "Something Went Wrong");
        }

        [Authorize(Permissions.PurchaseOrders.View)]
        [Route("report-aggregates")]
        [HttpPost]
        public async Task<Result<PurchaseOrderAggregatorModel>> ReportAggregates(PurchaseOrderRequestModel request)
        {
            return await Result<PurchaseOrderAggregatorModel>.SuccessAsync(await _purchaseOrderService.PreparePurchaseOrderAggregatorModel(request), "Summation Found");
        }

        [Authorize(Permissions.PurchaseOrders.View)]
        [Route("{id}")]
        [HttpGet]
        public async Task<Result<PurchaseOrderViewModel>> GetById(Guid id)
        {
            return await Result<PurchaseOrderViewModel>.SuccessAsync(await _purchaseOrderService.GetByIdAsync(id), "Result Found");
        }

        [Authorize(Permissions.PurchaseOrders.View)]
        [Route("pending-checked")]
        [HttpGet]
        public async Task<Result<PendingCheckedCountModel>> GetPendingCheckedAsync()
        {
            return await Result<PendingCheckedCountModel>.SuccessAsync(await _purchaseOrderService.GetPendingCheckedCountAsync(), "Success");
        }

        [Authorize(Permissions.PurchaseOrders.Create)]
        [HttpPost]
        public virtual async Task<Result> Add([FromBody] PurchaseOrderCreationDto purchaseOrderCreationDto)
        {
            var addUpdateResponseModel = await _purchaseOrderService.AddAsync(purchaseOrderCreationDto);
            return await Result<Guid>.SuccessAsync(addUpdateResponseModel, "Purchase Order Added Successfully");
        }

        [Authorize(Permissions.PurchaseOrders.Edit)]
        [HttpPost("update")]
        public virtual async Task<Result> Put([FromBody] PurchaseOrderUpdateDto purchaseOrderUpdateDto)
        {
            var addUpdateResponseModel = await _purchaseOrderService.UpdateAsync(purchaseOrderUpdateDto);
            return await Result<Guid>.SuccessAsync(addUpdateResponseModel, "Purchase Order Updated Successfully");
        }

        [Authorize(Permissions.PurchaseOrders.Delete)]
        [Route("delete/{id}")]
        [HttpPost]
        public virtual async Task<Result> Delete(Guid id)
        {
            var purchaseOrderId = await _purchaseOrderService.DeleteAsync(id);
            return await Result<Guid>.SuccessAsync(purchaseOrderId, "Purchase Order Deleted Successfully");
        }

        [Authorize(Permissions.PurchaseOrders.Check)]
        [HttpPost("check/{id}")]
        public virtual async Task<Result> Check(Guid id)
        {
            var isVerified = await _purchaseOrderService.CheckAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)PurchaseOrderStatus.Checked, "Purchase Order Checked Successfully");
            else return await Result<string>.FailAsync("Failed To Check", "Something Went Wrong");
        }

        [Authorize(Permissions.PurchaseOrders.Approve)]
        [HttpPost("approve/{id}")]
        public virtual async Task<Result> Approve(Guid id)
        {
            var isVerified = await _purchaseOrderService.ApproveAsync(id);
            if (isVerified) return await Result<int>.SuccessAsync((int)PurchaseOrderStatus.Approved, "Purchase Order Approved Successfully");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.PurchaseOrders.Ready_For_GRN)]
        [HttpPost("ready-for-grn/{id}")]
        public virtual async Task<Result> ReadyForGrn(Guid id)
        {
            var isReadyForGrn = await _purchaseOrderService.ReadyForGrnAsync(id);
            if (isReadyForGrn) return await Result<int>.SuccessAsync((int)PurchaseOrderStatus.Ready_For_GRN, "This Purchase Order is now Ready For GRN");
            else return await Result<string>.FailAsync("Failed To Approve", "Something Went Wrong");
        }

        [Authorize(Permissions.PurchaseOrders.SendToSupplier)]
        [HttpPost("send/{purchaseOrderId}")]
        public virtual async Task<Result> SendToSupplier(Guid purchaseOrderId)
        {
            var status = await _purchaseOrderService.SendToSupplierAsync(purchaseOrderId);
            if (status) return await Result<int>.SuccessAsync((int)PurchaseOrderStatus.SentToSupplier, "Purchase Order Sent Successfully to the Supplier");
            else return await Result<string>.FailAsync("Failed To Send", "Something Went Wrong");
        }

        [Authorize(Permissions.PurchaseOrders.Unpost)]
        [HttpPost("unpost/{id}/{fromStatus}")]
        public virtual async Task<Result> Unpost(Guid id, int fromStatus)
        {
            return await Result<int>.SuccessAsync(await _purchaseOrderService.UnpostAsync(id, fromStatus), "Purchase Order Unposted Successfully");
        }

        [HttpGet("supplier-transactions-against-po/{purhchaseOrderId}")]
        public virtual async Task<Result> GetRfqSentSuppliersByRequisitionId(Guid purhchaseOrderId)
        {
            var result = await _purchaseOrderService.GetSupplierTransactionsAgainstPo(purhchaseOrderId);
            return await Result<List<SupplierTransactionAgainstPoViewModel>>.SuccessAsync(result, "Supplier Transactions against PO");
        }

        [HttpGet("print/{id}")]
        public virtual async Task<IActionResult> Print(Guid id)
        {
            try
            {
                var dbPurchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().Include(x => x.Supplier).Include(x => x.DeliveryPlace).SingleOrDefaultAsync(x => x.Id == id);
                var purchaseOrderDetails = _unitOfWork.Repository<PurchaseOrderDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(x => x.PurchaseOrderId == dbPurchaseOrder.Id).Select(x => new PurchaseOrderView
                {
                    Description = x.Product.Name,
                    Unit = x.Product.MeasurementUnit.Name,
                    Quantity = x.Quantity,
                    Rate = x.Rate,
                    Amount = x.Amount,
                }).ToList();
                byte[] bytes;
                using (var stream = new MemoryStream())
                {
                    await _purchasePdfService.PrintPurchaseOrderInvoiceToPdf(stream, dbPurchaseOrder, purchaseOrderDetails);
                    bytes = stream.ToArray();
                }

                return File(bytes, MimeTypes.ApplicationPdf);
            }
            catch (Exception exception)
            {
                return Ok(exception.ToString());
            }
        }

        [HttpPost]
        [Route("purchase-order-summary-print")]
        public virtual async Task<IActionResult> PrintPurchaseOrderSummaryReport(PurchaseOrderRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = false;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "Ponumber";
            request.PurchaseOrderStatuses = new List<int> { (int)PurchaseOrderStatus.Approved, (int)PurchaseOrderStatus.Item_Partially_Received, (int)PurchaseOrderStatus.Item_Completely_Received, (int)PurchaseOrderStatus.SentToSupplier, (int)PurchaseOrderStatus.Closed, (int)PurchaseOrderStatus.Ready_For_GRN };
            var list = await _purchaseOrderService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintPurchaseOrderSummaryReportToPdf(stream, list.Item1.ToList(), "Purchase Order Summary", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }

        [HttpPost]
        [Route("purchase-order-details-print")]
        public virtual async Task<IActionResult> PrintPurchaseOrderDetailsReport(PurchaseOrderRequestModel request)
        {
            var userName = _workContext.GetUserName() ?? "";
            var isDetails = true;
            request.Page = -1;
            request.IsAscending = true;
            request.OrderBy = "Ponumber";
            request.PurchaseOrderStatuses = new List<int> { (int)PurchaseOrderStatus.Approved, (int)PurchaseOrderStatus.Item_Partially_Received, (int)PurchaseOrderStatus.Item_Completely_Received, (int)PurchaseOrderStatus.SentToSupplier, (int)PurchaseOrderStatus.Closed, (int)PurchaseOrderStatus.Ready_For_GRN };
            var list = await _purchaseOrderService.SearchAsync(request);
            byte[] bytes;
            using (var stream = new MemoryStream())
            {
                await _purchasePdfService.PrintPurchaseOrderSummaryReportToPdf(stream, list.Item1.ToList(), "Purchase Order Details", isDetails, request);
                bytes = stream.ToArray();
                bytes = PdfHelper.AddPageNumbers(bytes, userName);
            }
            return File(bytes, MimeTypes.ApplicationPdf);
        }
    }
}
