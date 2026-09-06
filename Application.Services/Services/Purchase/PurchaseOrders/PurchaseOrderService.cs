using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Purchase.PurchaseOrder;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Purchase.Pdf;
using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PurchaseOrder;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Purchase.PurchaseOrders
{
    public class PurchaseOrderService : BaseService<PurchaseOrder, PurchaseOrderCreationDto, PurchaseOrderUpdateDto, PurchaseOrderRequestModel, PurchaseOrderViewModel>, IPurchaseOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IMailService _mailService;
        private readonly IPurchasePdfService _purchasePdfService;
        private readonly ISmsService _smsService;
        private readonly ITenantService _tenantService;

        public PurchaseOrderService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext
            , INotificationService notificationService, IHubContext<BroadcastHub, IHubClient> hubContext,
            IMailService mailService, IPurchasePdfService purchasePdfService, ISmsService smsService, ITenantService tenantService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _mailService = mailService;
            _purchasePdfService = purchasePdfService;
            _smsService = smsService;
            _tenantService = tenantService;
        }

        public async Task<PurchaseOrderAggregatorModel> PreparePurchaseOrderAggregatorModel(PurchaseOrderRequestModel purchaseOrderRequest)
        {
            var purchaseOrderQueryable = _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().Where(purchaseOrderRequest.GetExpression());
            return new PurchaseOrderAggregatorModel
            {
                AggregatorTotal = await purchaseOrderQueryable.SumAsync(x => x.Total)
            };
        }

        public async Task<PurchaseOrderViewModel> GetByIdAsync(Guid id)
        {
            var purchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Supplier).Include(x => x.Store)
                .Include(x => x.PurchaseOrderDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (purchaseOrder == null) throw new NotFoundResultException("Purchase Order Not Found With this Id");
            return _mapper.Map<PurchaseOrderViewModel>(purchaseOrder);
        }

        public async Task<LastPoDetailsViewModel> SearchLastPoDetailsAsync(Guid supplierId, Guid productId)
        {
            var purchaseOrderDetail = await _unitOfWork.Repository<PurchaseOrderDetail>().TableNoTracking().Include(x => x.PurchaseOrder).Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).OrderByDescending(x => x.CreatedOn).FirstOrDefaultAsync(x => x.PurchaseOrder.SupplierId == supplierId && x.ProductId == productId && x.PurchaseOrder.Status >= (int)PurchaseOrderStatus.Approved);
            if (purchaseOrderDetail == null) throw new NotFoundResultException("Purchase Order Not Found against this Supplier");
            var result = new LastPoDetailsViewModel
            {
                PoId = purchaseOrderDetail.PurchaseOrderId,
                ProductId = purchaseOrderDetail.Id,
                ProductName = purchaseOrderDetail.Product.Name,
                PoNumber = purchaseOrderDetail.PurchaseOrder.Ponumber,
                OrderedQuantity = purchaseOrderDetail.Quantity,
                ReceivedQuantity = purchaseOrderDetail.ReceivedQuantity,
                Rate = purchaseOrderDetail.Rate,
                Status = purchaseOrderDetail.PurchaseOrder.Status,
            };
            return result;
        }

        public new async Task<Guid> AddAsync(PurchaseOrderCreationDto purchaseOrderCreationDto)
        {
            if (purchaseOrderCreationDto.IsImportPurchase && !string.IsNullOrEmpty(purchaseOrderCreationDto.LcNumber))
            {
                var isLcNumberExist = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().AnyAsync(x => x.LcNumber == purchaseOrderCreationDto.LcNumber);
                if (isLcNumberExist) throw new BadRequestException("This LC Number has already been utilized. Please try another one.");
            }
            var financialYear = _workContext.GetCurrentFinancialYear();
            purchaseOrderCreationDto.Podate = purchaseOrderCreationDto.Podate.ToLocal();
            purchaseOrderCreationDto.DeliveryDate = purchaseOrderCreationDto.DeliveryDate.ToLocal();
            var purchaseOrder = _mapper.Map<PurchaseOrder>(purchaseOrderCreationDto);
            var count = _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            purchaseOrder.Id = Guid.NewGuid();
            purchaseOrder.Ponumber = "PO" + financialYear.Code + "-" + count.ToString().PadLeft(6, '0');
            purchaseOrder.FinancialYearId = financialYear.Id;
            purchaseOrder.Status = (int)PurchaseOrderStatus.Pending;
            purchaseOrder.PurchaseOrderDetails = purchaseOrder.PurchaseOrderDetails.Where(x => x.Quantity > 0).ToList();
            foreach (var item in purchaseOrder.PurchaseOrderDetails)
            {
                item.Id = Guid.NewGuid();
                item.PurchaseOrderId = purchaseOrder.Id;
                await _unitOfWork.Repository<PurchaseOrderDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<PurchaseOrder>().AddAsync(purchaseOrder);
            //update purchase Requisition
            if (!string.IsNullOrEmpty(purchaseOrder.RequisitionNo))
            {
                await ProcessPurchaseRequisitionMappingAsync(purchaseOrder);
            }
            //update vendor quotation
            if (!string.IsNullOrWhiteSpace(purchaseOrderCreationDto.QuotationNo))
            {
                var dbVendorQuotation = await _unitOfWork.Repository<VendorQuotation>().TableNoTracking().SingleOrDefaultAsync(x => x.QuotationNo == purchaseOrderCreationDto.QuotationNo);
                if (dbVendorQuotation is null) throw new NotFoundResultException("Vendor Quotation Not Found");
                dbVendorQuotation.Status = (int)VendorQuotationStatus.OrderCreated;
                await _unitOfWork.Repository<VendorQuotation>().UpdateAsync(dbVendorQuotation);
            }
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, purchaseOrder.Id
                , "/purchase/purchase-order/" + purchaseOrder.Id, Permissions.PurchaseOrders.Check,
                "Purchase Order " + purchaseOrder.Ponumber + " is ready for check", (int)PurchaseOrderStatus.Pending);


            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            return purchaseOrder.Id;
        }

        public new async Task<Guid> UpdateAsync(PurchaseOrderUpdateDto purchaseOrderUpdateDto)
        {
            var dbPurchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().FindAsync(purchaseOrderUpdateDto.Id);
            if (purchaseOrderUpdateDto.IsImportPurchase && !string.IsNullOrEmpty(purchaseOrderUpdateDto.LcNumber))
            {
                if (purchaseOrderUpdateDto.LcNumber.Equals(dbPurchaseOrder.LcNumber) == false)
                {
                    var isLcNumberExist = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().AnyAsync(x => x.LcNumber == purchaseOrderUpdateDto.LcNumber);
                    if (isLcNumberExist) throw new BadRequestException("This LC Number has already been utilized. Please try another one.");
                }
            }
            //when date updated,convert it to local date
            if (purchaseOrderUpdateDto.Podate.Equals(dbPurchaseOrder.Podate) == false)
            {
                purchaseOrderUpdateDto.Podate = purchaseOrderUpdateDto.Podate.ToLocal();
            }
            if (purchaseOrderUpdateDto.DeliveryDate.Equals(dbPurchaseOrder.DeliveryDate) == false)
            {
                purchaseOrderUpdateDto.DeliveryDate = purchaseOrderUpdateDto.DeliveryDate.ToLocal();
            }
            var purchaseOrder = _mapper.Map(purchaseOrderUpdateDto, dbPurchaseOrder);
            if (dbPurchaseOrder.Status == (int)PurchaseOrderStatus.Pending || dbPurchaseOrder.Status == (int)PurchaseOrderStatus.Checked)
            {
                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(purchaseOrder);
                foreach (var item in purchaseOrder.PurchaseOrderDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.PurchaseOrderId = purchaseOrder.Id;
                        await _unitOfWork.Repository<PurchaseOrderDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<PurchaseOrderDetail>().UpdateAsync(item);
                }
                //Delete for PurchaseOrderDetails
                if (!string.IsNullOrEmpty(purchaseOrderUpdateDto.DeletedPurchaseOrderDetailIds))
                {
                    foreach (var id in purchaseOrderUpdateDto.DeletedPurchaseOrderDetailIds.Split(',').Where(x => x != ""))
                    {
                        var purchaseOrderDetail = await _unitOfWork.Repository<PurchaseOrderDetail>().FindAsync(new Guid(id));
                        purchaseOrderDetail.Deleted = true;
                        await _unitOfWork.Repository<PurchaseOrderDetail>().UpdateAsync(purchaseOrderDetail);
                    }
                }
                if (!string.IsNullOrEmpty(purchaseOrder.RequisitionNo)) await UpdatePurchaseRequisitionMappingAsync(purchaseOrder);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return purchaseOrder.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var purchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().FindAsync(x => x.Id == id, x => x.Include(x => x.PurchaseOrderDetails));
            if (purchaseOrder == null) throw new NotFoundResultException("PurchaseOrder Not Found With this Id");
            if (purchaseOrder.Status == (int)PurchaseOrderStatus.Pending && purchaseOrder.IsImportPurchase)
            {
                var hasLcCostEntry = await _unitOfWork.Repository<LccostEntry>().TableNoTracking().AnyAsync(x => x.PurchaseOrderId == id);
                if (hasLcCostEntry) throw new BadRequestException("Action denied: This PO is linked to an LC Cost Entry.");
            }

            if (!string.IsNullOrEmpty(purchaseOrder.RequisitionNo))
            {
                await ReversePurchaseRequisitionMappingAsync(purchaseOrder);
            }

            if (purchaseOrder.Status == (int)PurchaseOrderStatus.Pending)
            {
                foreach (var item in purchaseOrder.PurchaseOrderDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<PurchaseOrderDetail>().UpdateAsync(item);
                }
                purchaseOrder.Deleted = true;
                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(purchaseOrder);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");

            return purchaseOrder.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var purchaseOrders = _unitOfWork.Repository<PurchaseOrder>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await purchaseOrders.Where(x => x.Status == (int)PurchaseOrderStatus.Pending).CountAsync(),
                CheckedCount = await purchaseOrders.Where(x => x.Status == (int)PurchaseOrderStatus.Checked).CountAsync()
            };
            return response;

        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbPurchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().FindAsync(id);
            if (dbPurchaseOrder.Status == (int)PurchaseOrderStatus.Pending)
            {
                dbPurchaseOrder.Status = (int)PurchaseOrderStatus.Checked;
                dbPurchaseOrder.CheckedBy = _workContext.GetUserName();
                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(dbPurchaseOrder);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPurchaseOrder.Id
                       && x.Status == (int)PurchaseOrderStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbPurchaseOrder.Id
                    , "/purchase/purchase-order/" + dbPurchaseOrder.Id, Permissions.PurchaseRequisitions.Approve,
                    "Purchase Order " + dbPurchaseOrder.Ponumber + " is ready for Approval", (int)PurchaseOrderStatus.Checked);
                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Purchase Order Status has already been checked by " + dbPurchaseOrder?.CheckedBy);

        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<PurchaseOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Purchase Order not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Purchase Order does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<PurchaseOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)PurchaseOrderStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)PurchaseOrderStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Purchase Order already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbPurchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().FindAsync(id);

            dbPurchaseOrder.Status = (int)PurchaseOrderStatus.Approved;
            dbPurchaseOrder.ApprovedBy = approvedBy;

            await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(dbPurchaseOrder);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPurchaseOrder.Id
                   && x.Status == (int)PurchaseOrderStatus.Checked);

            // Save all changes
            bool isApproved = await _unitOfWork.SaveChangesAsync();

            // Commit transaction (ALL OR NOTHING)
            await transaction.CommitAsync();

            // Notify clients
            await _hubContext.Clients.All.BroadcastMessage();
            return isApproved;
        }
        public virtual async Task<bool> ReadyForGrnAsync(Guid id)
        {
            var dbPurchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().Include(x => x.PurchaseOrderDetails).SingleOrDefaultAsync(x => x.Id == id);
            if (dbPurchaseOrder is null) throw new NotFoundResultException("Purchase Order not found");
            if (dbPurchaseOrder.IsImportPurchase && (dbPurchaseOrder.Status == (int)PurchaseOrderStatus.Approved))
            {
                var countPendingLcCostEntry = await _unitOfWork.Repository<LccostEntry>().TableNoTracking().CountAsync(x => x.PurchaseOrderId ==
                dbPurchaseOrder.Id && x.Status == (int)LCCostEntryStatus.Pending);
                if (countPendingLcCostEntry > 0) throw new BadRequestException($"You have {countPendingLcCostEntry} pending LC Cost Entry associated with this PO.Please Take action First");

                var countCheckedLcCostEntry = await _unitOfWork.Repository<LccostEntry>().TableNoTracking().CountAsync(x => x.PurchaseOrderId ==
                dbPurchaseOrder.Id && x.Status == (int)LCCostEntryStatus.Checked);
                if (countCheckedLcCostEntry > 0) throw new BadRequestException($"You have {countCheckedLcCostEntry} checked LC Cost Entry associated with this PO.Please Take action First");

                var lcCostEntries = await _unitOfWork.Repository<LccostEntry>().TableNoTracking().Include(x => x.LccostEntryDetails)
                   .Where(x => x.Status == (int)LCCostEntryStatus.Approved
                    && x.PurchaseOrderId == dbPurchaseOrder.Id).ToListAsync();
                if (!lcCostEntries.Any()) throw new BadRequestException("You have 0 LC Cost Entry associated with this PO. Please Input LC Cost Entry First");
                var totalAdditionalLandedCost = 0m;
                foreach (var lccostEntry in lcCostEntries)
                {
                    totalAdditionalLandedCost += lccostEntry.LccostEntryDetails.Where(x => x.IsIncludedWithinLandedCost).Sum(x => x.Amount);
                    lccostEntry.Status = (int)LCCostEntryStatus.Calculated_Within_Landed_Cost;
                    await _unitOfWork.Repository<LccostEntry>().UpdateAsync(lccostEntry);
                }
                dbPurchaseOrder.AdditionalLandedCost = totalAdditionalLandedCost;
                dbPurchaseOrder!.Status = (int)PurchaseOrderStatus.Ready_For_GRN;
                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(dbPurchaseOrder);
                //remove checked notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPurchaseOrder.Id
                       && x.Status == (int)PurchaseOrderStatus.Approved);
                bool isReadyForGrn = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isReadyForGrn;
            }
            else throw new BadRequestException("Not Possible to Change Status. Only Approved Status Allowed to Change into Ready For GRN.");
        }

        public virtual async Task<bool> SendToSupplierAsync(Guid id)
        {
            var dbPurchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().Include(x => x.Supplier).SingleOrDefaultAsync(x => x.Id == id);
            //send email to supplier
            if (string.IsNullOrWhiteSpace(dbPurchaseOrder!.Supplier.Email) || string.IsNullOrWhiteSpace(dbPurchaseOrder!.Supplier.ContactNo))
            {
                throw new BadRequestException("Supplier Email/Contact No Not Found");
            }
            dbPurchaseOrder!.Status = (int)PurchaseOrderStatus.SentToSupplier;
            await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(dbPurchaseOrder);

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
                Guid? tenantId = _workContext.GetTenantId();
                TenantViewModel tenantData = await _tenantService.GetByIdAsync(tenantId);

                await _purchasePdfService.PrintPurchaseOrderInvoiceToPdf(stream, dbPurchaseOrder, purchaseOrderDetails);
                bytes = stream.ToArray();
                bytes = _purchasePdfService.AddFooter(bytes);
                var mailRequest = new MailRequest()
                {
                    ToEmail = dbPurchaseOrder.Supplier.Email,
                    Subject = "APFEED Purchase Order Ref#" + dbPurchaseOrder.Ponumber,
                    Body = $"Dear {dbPurchaseOrder.Supplier.Name} {Environment.NewLine} Here is in attachment a purchase order {dbPurchaseOrder.Ponumber} from {tenantData.Name}. {Environment.NewLine}If you have any questions,please do not hesitate to contact us.{Environment.NewLine} Best Regards",
                    FileName = "purchase-order",
                    FileBytes = bytes
                };
                await _mailService.SendEmailFromDefaultEmailAddressAsync(mailRequest);
            }
            //send email to supplier end
            await _smsService.Send(dbPurchaseOrder.Supplier.ContactNo, "An email has been sent to your email " + dbPurchaseOrder.Supplier.Email + " providing Purchase Order-APFEED");
            bool isApproved = await _unitOfWork.SaveChangesAsync();
            return isApproved;

        }

        public virtual async Task<bool> CloseAsync(Guid id)
        {
            var dbPurchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().FindAsync(id);
            if (dbPurchaseOrder.Status == (int)PurchaseOrderStatus.Item_Partially_Received)
            {
                dbPurchaseOrder.Status = (int)PurchaseOrderStatus.Closed;
                await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(dbPurchaseOrder);
                bool isClose = await _unitOfWork.SaveChangesAsync();
                return isClose;
            }
            else throw new BadRequestException("You won't be able to close this Purchase Order");
        }

        public virtual async Task<int> UnpostAsync(Guid id, int fromStatus)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();

            // Load minimal data (no tracking)
            var dbInfo = await _unitOfWork.Repository<PurchaseOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.IsImportPurchase, x.Ponumber })
                .SingleOrDefaultAsync();

            if (dbInfo == null) throw new NotFoundResultException("Purchase Order not found.");
            if (financialYear.Id != dbInfo.FinancialYearId) throw new BadRequestException("Unpost Denied: This Purchase Order does not belong to the current financial year.");
            if (dbInfo.Status != fromStatus) throw new BadRequestException("Purchase Order status has been changed by another user. Please refresh and try again.");
            if (dbInfo.Status != (int)PurchaseOrderStatus.Checked && dbInfo.Status != (int)PurchaseOrderStatus.Approved && dbInfo.Status != (int)PurchaseOrderStatus.Ready_For_GRN)
                throw new BadRequestException("Only Checked, Approved, or Ready For GRN Purchase Order can be unposted.");

            int newStatus;
            if (dbInfo.Status == (int)PurchaseOrderStatus.Checked)
                newStatus = (int)PurchaseOrderStatus.Pending;
            else if (dbInfo.Status == (int)PurchaseOrderStatus.Approved)
                newStatus = (int)PurchaseOrderStatus.Checked;
            else
                newStatus = (int)PurchaseOrderStatus.Approved;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Atomic update with status predicate
            int rowsUpdated = await _unitOfWork.Repository<PurchaseOrder>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus));

            if (rowsUpdated == 0) throw new BadRequestException("Purchase Order status was already modified by another request. Please refresh and try again.");

            // Pre-check: if Approved && IsImportPurchase, reverse LcCostEntries (before reloading full entity)
            if (dbInfo.Status == (int)PurchaseOrderStatus.Approved && dbInfo.IsImportPurchase)
            {
                var dbLcCostEntries = await _unitOfWork.Repository<LccostEntry>().TableNoTracking().Where(x => x.PurchaseOrderId == id && x.Status == (int)LCCostEntryStatus.Approved).ToListAsync();
                foreach (var lcCostEntry in dbLcCostEntries)
                {
                    if (financialYear.Id != lcCostEntry.FinancialYearId) throw new BadRequestException($"Unpost Denied: LC Cost Entry of PO {dbInfo.Ponumber} does not belong to the current financial year.");
                    lcCostEntry.Status = (int)LCCostEntryStatus.Checked;
                    lcCostEntry.ApprovedBy = "";
                    await DeleteLcCostEntryTransaction(lcCostEntry.Code);
                    await _unitOfWork.Repository<LccostEntry>().UpdateAsync(lcCostEntry);
                }
            }

            // Reload full entity with tracking inside transaction
            var dbPurchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().FindAsync(id);

            dbPurchaseOrder.Status = newStatus;

            if (dbInfo.Status == (int)PurchaseOrderStatus.Checked)
            {
                dbPurchaseOrder.CheckedBy = "";
            }
            else if (dbInfo.Status == (int)PurchaseOrderStatus.Approved)
            {
                dbPurchaseOrder.ApprovedBy = "";
            }
            else
            {
                var lcCostEntries = await _unitOfWork.Repository<LccostEntry>().TableNoTracking()
                    .Where(x => x.Status == (int)LCCostEntryStatus.Calculated_Within_Landed_Cost
                     && x.PurchaseOrderId == dbPurchaseOrder.Id).ToListAsync();
                foreach (var lcCostEntry in lcCostEntries)
                {
                    lcCostEntry.Status = (int)LCCostEntryStatus.Approved;
                    await _unitOfWork.Repository<LccostEntry>().UpdateAsync(lcCostEntry);
                }
                dbPurchaseOrder.AdditionalLandedCost = 0m;
            }

            await _unitOfWork.Repository<PurchaseOrder>().UpdateAsync(dbPurchaseOrder);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbPurchaseOrder.Status;
        }

        private async Task DeleteLcCostEntryTransaction(string code)
        {
            var dbLcCostEntry = await _unitOfWork.Repository<LccostEntry>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.Code == code);
            if (dbLcCostEntry is null) throw new NotFoundResultException("LC Cost Entry Not Found With this Code");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == dbLcCostEntry.Code);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }
        }

        public virtual async Task<List<SupplierTransactionAgainstPoViewModel>> GetSupplierTransactionsAgainstPo(Guid id)
        {
            var result = new List<SupplierTransactionAgainstPoViewModel>();
            var list = await _unitOfWork.Repository<SupplierTransactionAgainstPo>().TableNoTracking().Include(x => x.Supplier).Include(x => x.PurchaseOrder)
                .Where(x => x.PurchaseOrderId == id).OrderBy(x => x.CreatedOn).ToListAsync();

            decimal runningBalance = 0;
            foreach (var x in list)
            {
                runningBalance += x.Amount;
                result.Add(new SupplierTransactionAgainstPoViewModel
                {
                    Id = x.Id,
                    TransactionId = x.TransactionId,
                    TransactionDate = x.TransactionDate.ToString("dd/MM/yyyy"),
                    SupplierInvoiceDate = x.SupplierInvoiceDate.HasValue ? x.SupplierInvoiceDate.Value.ToString("dd/MM/yyyy") : "",
                    PaymentTermInDays = x.PaymentTermInDays,
                    Ponumber = x.PurchaseOrder.Ponumber,
                    SupplierName = x.Supplier.Name,
                    SupplierTransactionTypeName = Enum.GetName(typeof(SupplierTransactionType), x.SupplierTransactionType),
                    Amount = x.SupplierTransactionType == (int)SupplierTransactionType.PO_Price_Adjustment_After_GRN ? x.Amount : Math.Abs(x.Amount),
                    Balance = runningBalance,
                    Remark = x.Remark
                });
            }

            return result;
        }
        private async Task ProcessPurchaseRequisitionMappingAsync(PurchaseOrder purchaseOrder)
        {
            var dbPurchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>()
                .TableNoTracking()
                .Include(x => x.PurchaseRequisitionDetails).ThenInclude(x => x.Product)
                .SingleOrDefaultAsync(x => x.RequisitionNo == purchaseOrder.RequisitionNo);

            if (dbPurchaseRequisition is null)
                throw new NotFoundResultException("Purchase Requisition Not Found");

            foreach (var item in purchaseOrder.PurchaseOrderDetails)
            {
                var purchaseRequisitionItem = dbPurchaseRequisition.PurchaseRequisitionDetails
                    .FirstOrDefault(x => x.ProductId == item.ProductId);

                if (purchaseRequisitionItem is null)
                    throw new NotFoundResultException($"Purchase Requisition detail not found for product {item.ProductId}");

                var mappingItem = _unitOfWork.Repository<PurchaseRequisitionPurchaseOrderMapping>().TableNoTracking().Where(x => x.PurchaseOrderId == purchaseOrder.Id && x.ProductId == item.ProductId);
                var mappingQty = mappingItem.Sum(x => x.PurchaseOrderQuantity);

                var availableQty = 0M;
                if (mappingQty > 0) availableQty = purchaseRequisitionItem.Quantity - purchaseRequisitionItem.OrderedQuantity + mappingQty;
                else availableQty = purchaseRequisitionItem.Quantity - purchaseRequisitionItem.OrderedQuantity;

                if (item.Quantity > availableQty) throw new BadRequestException($"Ordered quantity for product {purchaseRequisitionItem.Product?.Name} exceeds the requisitioned quantity {availableQty}");

                var purchaseRequisitionPurchaseOrderMappingItem = new PurchaseRequisitionPurchaseOrderMapping
                {
                    Id = Guid.NewGuid(),
                    PurchaseRequisitionId = dbPurchaseRequisition.Id,
                    PurchaseOrderId = purchaseOrder.Id,
                    ProductId = item.ProductId,
                    PurchaseOrderQuantity = item.Quantity,
                    PurchaseOrderRate = item.Rate,
                    RequisitionQuantity = purchaseRequisitionItem.Quantity
                };

                await _unitOfWork.Repository<PurchaseRequisitionPurchaseOrderMapping>().AddAsync(purchaseRequisitionPurchaseOrderMappingItem);

                purchaseRequisitionItem.OrderedQuantity += item.Quantity;
                await _unitOfWork.Repository<PurchaseRequisitionDetail>().UpdateAsync(purchaseRequisitionItem);
            }

            var totalRequested = dbPurchaseRequisition.PurchaseRequisitionDetails.Sum(x => x.Quantity);
            var totalOrdered = dbPurchaseRequisition.PurchaseRequisitionDetails.Sum(x => x.OrderedQuantity);

            dbPurchaseRequisition.RequisitionStatus = totalRequested == totalOrdered
                ? (int)RequisitionStatus.Order_Complete
                : (int)RequisitionStatus.Order_Partial;

            await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(dbPurchaseRequisition);
        }

        private async Task ReversePurchaseRequisitionMappingAsync(PurchaseOrder purchaseOrder)
        {
            var dbPurchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>()
                .TableNoTracking()
                .Include(x => x.PurchaseRequisitionDetails).ThenInclude(x => x.Product)
                .SingleOrDefaultAsync(x => x.RequisitionNo == purchaseOrder.RequisitionNo);

            if (dbPurchaseRequisition == null)
                throw new NotFoundResultException("Purchase Requisition Not Found");

            // Reverse ordered quantities on requisition details
            foreach (var item in purchaseOrder.PurchaseOrderDetails)
            {
                var purchaseRequisitionItem = dbPurchaseRequisition.PurchaseRequisitionDetails
                    .FirstOrDefault(x => x.ProductId == item.ProductId);

                if (purchaseRequisitionItem is null)
                    throw new NotFoundResultException($"Purchase Requisition detail not found for product {item.ProductId}");

                purchaseRequisitionItem.OrderedQuantity -= item.Quantity;
                if (purchaseRequisitionItem.OrderedQuantity < 0) purchaseRequisitionItem.OrderedQuantity = 0; // guard against negative values

                await _unitOfWork.Repository<PurchaseRequisitionDetail>().UpdateAsync(purchaseRequisitionItem);
            }

            // Determine if other purchase orders exist for this requisition
            var isPurchaseRequisitionExist = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking()
                .AnyAsync(x => x.RequisitionNo == purchaseOrder.RequisitionNo && x.Ponumber != purchaseOrder.Ponumber);

            dbPurchaseRequisition.RequisitionStatus = isPurchaseRequisitionExist
                ? (int)RequisitionStatus.Order_Partial
                : (int)RequisitionStatus.Approved;

            await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(dbPurchaseRequisition);

            // Remove the mapping entries for this purchase order
            var purchaseRequisitionPurchaseOrderMappings = _unitOfWork.Repository<PurchaseRequisitionPurchaseOrderMapping>()
                .TableNoTracking()
                .Where(x => x.PurchaseOrderId == purchaseOrder.Id);

            foreach (var mapping in purchaseRequisitionPurchaseOrderMappings)
            {
                await _unitOfWork.Repository<PurchaseRequisitionPurchaseOrderMapping>().DeleteAsync(mapping.Id);
            }
        }
        private async Task UpdatePurchaseRequisitionMappingAsync(PurchaseOrder purchaseOrder)
        {
            var dbPurchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>()
                .TableNoTracking()
                .Include(x => x.PurchaseRequisitionDetails).ThenInclude(x => x.Product)
                .SingleOrDefaultAsync(x => x.RequisitionNo == purchaseOrder.RequisitionNo);

            if (dbPurchaseRequisition is null)
                throw new NotFoundResultException("Purchase Requisition Not Found");

            foreach (var item in purchaseOrder.PurchaseOrderDetails)
            {
                var purchaseRequisitionItem = dbPurchaseRequisition.PurchaseRequisitionDetails
                    .FirstOrDefault(x => x.ProductId == item.ProductId);

                if (purchaseRequisitionItem is null)
                    throw new NotFoundResultException($"Purchase Requisition detail not found for product {item.ProductId}");

                var mappingItem = _unitOfWork.Repository<PurchaseRequisitionPurchaseOrderMapping>().TableNoTracking().Where(x => x.PurchaseOrderId == purchaseOrder.Id && x.ProductId == item.ProductId);
                var mappingQty = mappingItem.Sum(x => x.PurchaseOrderQuantity);

                var availableQty = 0M;
                if (mappingQty > 0)
                {
                    purchaseRequisitionItem.OrderedQuantity -= mappingQty;
                    availableQty = purchaseRequisitionItem.Quantity - purchaseRequisitionItem.OrderedQuantity;
                }
                else
                {
                    availableQty = purchaseRequisitionItem.Quantity - purchaseRequisitionItem.OrderedQuantity;
                }
                purchaseRequisitionItem.OrderedQuantity += item.Quantity;

                if (item.Quantity > availableQty) throw new BadRequestException($"Ordered quantity for product {purchaseRequisitionItem.Product?.Name} exceeds the requisitioned quantity {availableQty}");
                if (mappingItem.Any()) await _unitOfWork.Repository<PurchaseRequisitionPurchaseOrderMapping>().DeleteAsync(mappingItem.First().Id);
                var purchaseRequisitionPurchaseOrderMappingItem = new PurchaseRequisitionPurchaseOrderMapping
                {
                    Id = Guid.NewGuid(),
                    PurchaseRequisitionId = dbPurchaseRequisition.Id,
                    PurchaseOrderId = purchaseOrder.Id,
                    ProductId = item.ProductId,
                    PurchaseOrderQuantity = item.Quantity,
                    PurchaseOrderRate = item.Rate,
                    RequisitionQuantity = purchaseRequisitionItem.Quantity
                };

                await _unitOfWork.Repository<PurchaseRequisitionPurchaseOrderMapping>().AddAsync(purchaseRequisitionPurchaseOrderMappingItem);
                await _unitOfWork.Repository<PurchaseRequisitionDetail>().UpdateAsync(purchaseRequisitionItem);
            }

            var totalRequested = dbPurchaseRequisition.PurchaseRequisitionDetails.Sum(x => x.Quantity);
            var totalOrdered = dbPurchaseRequisition.PurchaseRequisitionDetails.Sum(x => x.OrderedQuantity);

            dbPurchaseRequisition.RequisitionStatus = totalRequested == totalOrdered
                ? (int)RequisitionStatus.Order_Complete
                : (int)RequisitionStatus.Order_Partial;

            await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(dbPurchaseRequisition);
        }
    }
}
