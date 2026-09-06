using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Purchase.PurchaseRequisition;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.Services.Purchase.Pdf;
using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Purchase.PurchaseRequisitions
{
    public class PurchaseRequisitionService : BaseService<PurchaseRequisition, PurchaseRequisitionCreationDto, PurchaseRequisitionUpdateDto, PurchaseRequisitionRequestModel, PurchaseRequisitionViewModel>, IPurchaseRequisitionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IMailService _mailService;
        private readonly ISmsService _smsService;
        private readonly IPurchasePdfService _purchasePdfService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly ITenantService _tenantService;

        public PurchaseRequisitionService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext,
            INotificationService notificationService, IMailService mailService,
            IPurchasePdfService purchasePdfService, ISmsService smsService,
            IHubContext<BroadcastHub, IHubClient> hubContext, ITenantService tenantService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _mailService = mailService;
            _purchasePdfService = purchasePdfService;
            _smsService = smsService;
            _hubContext = hubContext;
            _tenantService = tenantService;
        }
        public async Task<PurchaseRequisitionViewModel> GetByIdAsync(Guid id)
        {
            var purchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Department)
                .Include(x => x.Store)
                .Include(x => x.PurchaseRequisitionDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (purchaseRequisition == null) throw new NotFoundResultException("Purchase Requisition Not Found With this Id");
            return _mapper.Map<PurchaseRequisitionViewModel>(purchaseRequisition);
        }
        public new async Task<Guid> AddAsync(PurchaseRequisitionCreationDto purchaseRequisitionCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            purchaseRequisitionCreationDto.RequisitionDate = purchaseRequisitionCreationDto.RequisitionDate.ToLocal();
            var purchaseRequisition = _mapper.Map<PurchaseRequisition>(purchaseRequisitionCreationDto);
            var count = _unitOfWork.Repository<PurchaseRequisition>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            purchaseRequisition.Id = Guid.NewGuid();
            purchaseRequisition.RequisitionNo = "PR" + financialYear.Code + "-" + count.ToString().PadLeft(6, '0');
            purchaseRequisition.FinancialYearId = financialYear.Id;
            purchaseRequisition.RequisitionStatus = (int)RequisitionStatus.Pending;
            purchaseRequisition.RequestBy = _workContext.GetUserName()!;

            foreach (var item in purchaseRequisition.PurchaseRequisitionDetails)
            {
                item.Id = Guid.NewGuid();
                item.PurchaseRequisitionId = purchaseRequisition.Id;
                await _unitOfWork.Repository<PurchaseRequisitionDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<PurchaseRequisition>().AddAsync(purchaseRequisition);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, purchaseRequisition.Id
                , "/purchase/purchase-requisition/" + purchaseRequisition.Id, Permissions.PurchaseRequisitions.Check,
                "Purchase Requisition " + purchaseRequisition.RequisitionNo + " is ready for Check", (int)RequisitionStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return purchaseRequisition.Id;

        }
        public new async Task<Guid> UpdateAsync(PurchaseRequisitionUpdateDto purchaseRequisitionUpdateDto)
        {
            var dbPurchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>().FindAsync(purchaseRequisitionUpdateDto.Id);
            //when date updated,convert it to local date
            if (purchaseRequisitionUpdateDto.RequisitionDate.Equals(dbPurchaseRequisition.RequisitionDate) == false)
            {
                purchaseRequisitionUpdateDto.RequisitionDate = purchaseRequisitionUpdateDto.RequisitionDate.ToLocal();
            }
            if (dbPurchaseRequisition == null) throw new NotFoundResultException("PurchaseRequisition Not Found With this Id");
            var purchaseRequisition = _mapper.Map(purchaseRequisitionUpdateDto, dbPurchaseRequisition);
            if (dbPurchaseRequisition.RequisitionStatus == (int)RequisitionStatus.Pending || dbPurchaseRequisition.RequisitionStatus == (int)RequisitionStatus.Checked)
            {
                await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(purchaseRequisition);
                foreach (var item in purchaseRequisition.PurchaseRequisitionDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.PurchaseRequisitionId = purchaseRequisition.Id;
                        await _unitOfWork.Repository<PurchaseRequisitionDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<PurchaseRequisitionDetail>().UpdateAsync(item);

                }

                //Delete for PurchaseRequisitionDetails
                if (!string.IsNullOrEmpty(purchaseRequisitionUpdateDto.DeletedPurchaseRequisitionDetailIds))
                {
                    foreach (var id in purchaseRequisitionUpdateDto.DeletedPurchaseRequisitionDetailIds.Split(',').Where(x => x != ""))
                    {
                        var purchaseRequisitiondetail = await _unitOfWork.Repository<PurchaseRequisitionDetail>().FindAsync(new Guid(id));
                        purchaseRequisitiondetail.Deleted = true;
                        await _unitOfWork.Repository<PurchaseRequisitionDetail>().UpdateAsync(purchaseRequisitiondetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update.Only Pending or Checked Status Allowed to Update");
            return purchaseRequisition.Id;

        }

        public async Task<Guid> PrepareRfqAsync(PurchaseRequisitionUpdateDto purchaseRequisitionUpdateDto)
        {
            var dbPurchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>().FindAsync(purchaseRequisitionUpdateDto.Id);
            var purchaseRequisition = _mapper.Map(purchaseRequisitionUpdateDto, dbPurchaseRequisition);
            purchaseRequisition.RequisitionStatus = (int)RequisitionStatus.RFQ_Ready_For_Send;
            await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(purchaseRequisition);
            foreach (var item in purchaseRequisition.PurchaseRequisitionDetails)
            {
                if (item.Id == Guid.Empty)
                {
                    item.Id = Guid.NewGuid();
                    item.PurchaseRequisitionId = purchaseRequisition.Id;
                    await _unitOfWork.Repository<PurchaseRequisitionDetail>().AddAsync(item);
                }
                else await _unitOfWork.Repository<PurchaseRequisitionDetail>().UpdateAsync(item);

            }

            //Delete for PurchaseRequisitionDetails
            if (!string.IsNullOrEmpty(purchaseRequisitionUpdateDto.DeletedPurchaseRequisitionDetailIds))
            {
                foreach (var id in purchaseRequisitionUpdateDto.DeletedPurchaseRequisitionDetailIds.Split(',').Where(x => x != ""))
                {
                    var purchaseRequisitiondetail = await _unitOfWork.Repository<PurchaseRequisitionDetail>().FindAsync(new Guid(id));
                    purchaseRequisitiondetail.Deleted = true;
                    await _unitOfWork.Repository<PurchaseRequisitionDetail>().UpdateAsync(purchaseRequisitiondetail);
                }
            }
            await _unitOfWork.SaveChangesAsync();
            return purchaseRequisition.Id;

        }


        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var purchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>().FindAsync(x => x.Id == id, x => x.Include(x => x.PurchaseRequisitionDetails));
            if (purchaseRequisition == null) throw new NotFoundResultException("PurchaseRequisition Not Found With this Id");
            if (purchaseRequisition.RequisitionStatus == (int)RequisitionStatus.Pending)
            {
                foreach (var item in purchaseRequisition.PurchaseRequisitionDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<PurchaseRequisitionDetail>().UpdateAsync(item);
                }
                purchaseRequisition.Deleted = true;
                await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(purchaseRequisition);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete.Only Pending Status Allowed to Delete");
            return purchaseRequisition.Id;
        }
        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var purchaseRequisitions = _unitOfWork.Repository<PurchaseRequisition>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await purchaseRequisitions.Where(x => x.RequisitionStatus == (int)RequisitionStatus.Pending).CountAsync(),
                CheckedCount = await purchaseRequisitions.Where(x => x.RequisitionStatus == (int)RequisitionStatus.Checked).CountAsync()
            };
            return response;
        }
        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbPurchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>().FindAsync(id);
            if (dbPurchaseRequisition.RequisitionStatus == (int)RequisitionStatus.Pending)
            {
                dbPurchaseRequisition.RequisitionStatus = (int)RequisitionStatus.Checked;
                dbPurchaseRequisition.CheckedBy = _workContext.GetUserName();
                //update requisition
                await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(dbPurchaseRequisition);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPurchaseRequisition.Id
                       && x.Status == (int)RequisitionStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbPurchaseRequisition.Id
                    , "/purchase/purchase-requisition/" + dbPurchaseRequisition.Id, Permissions.PurchaseRequisitions.Approve,
                    "Purchase Requisition " + dbPurchaseRequisition.RequisitionNo + " is ready for Approval", (int)RequisitionStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Purchase Requisition Status has already been checked by " + dbPurchaseRequisition?.CheckedBy);

        }
        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<PurchaseRequisition>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.RequisitionStatus, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Purchase Requisition not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Purchase Requisition does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<PurchaseRequisition>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.RequisitionStatus == (int)RequisitionStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.RequisitionStatus, (int)RequisitionStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Purchase Requisition already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbPurchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>().FindAsync(id);

            dbPurchaseRequisition.RequisitionStatus = (int)RequisitionStatus.Approved;
            dbPurchaseRequisition.ApprovedBy = approvedBy;

            //update requisition
            await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(dbPurchaseRequisition);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPurchaseRequisition.Id
                   && x.Status == (int)RequisitionStatus.Checked);

            // Save all changes
            bool isApproved = await _unitOfWork.SaveChangesAsync();

            // Commit transaction (ALL OR NOTHING)
            await transaction.CommitAsync();

            // Notify clients
            await _hubContext.Clients.All.BroadcastMessage();
            return isApproved;
        }
        public virtual async Task<int> UnpostAsync(Guid id, int fromStatus)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();

            // Load minimal data (no tracking)
            var dbInfo = await _unitOfWork.Repository<PurchaseRequisition>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.RequisitionStatus })
                .SingleOrDefaultAsync();

            if (dbInfo == null) throw new NotFoundResultException("Purchase Requisition not found.");
            if (financialYear.Id != dbInfo.FinancialYearId) throw new BadRequestException("Unpost Denied: This Purchase Requisition does not belong to the current financial year.");
            if (dbInfo.RequisitionStatus != fromStatus) throw new BadRequestException("Purchase Requisition status has been changed by another user. Please refresh and try again.");
            if (dbInfo.RequisitionStatus != (int)RequisitionStatus.Checked && dbInfo.RequisitionStatus != (int)RequisitionStatus.Approved && dbInfo.RequisitionStatus != (int)RequisitionStatus.RFQ_Ready_For_Send)
                throw new BadRequestException("Only Checked, Approved, or RFQ Ready For Send Purchase Requisition can be unposted.");

            int newStatus;
            if (dbInfo.RequisitionStatus == (int)RequisitionStatus.Checked)
                newStatus = (int)RequisitionStatus.Pending;
            else if (dbInfo.RequisitionStatus == (int)RequisitionStatus.Approved)
                newStatus = (int)RequisitionStatus.Checked;
            else
                newStatus = (int)RequisitionStatus.Approved;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Atomic update with status predicate
            int rowsUpdated = await _unitOfWork.Repository<PurchaseRequisition>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.RequisitionStatus == dbInfo.RequisitionStatus)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.RequisitionStatus, newStatus)
                    .SetProperty(x => x.UnpostedBy, _workContext.GetUserName()));

            if (rowsUpdated == 0) throw new BadRequestException("Purchase Requisition status was already modified by another request. Please refresh and try again.");

            // Reload full entity with tracking inside transaction
            var dbPurchaseRequisition = await _unitOfWork.Repository<PurchaseRequisition>().FindAsync(id);

            dbPurchaseRequisition.RequisitionStatus = newStatus;
            dbPurchaseRequisition.UnpostedBy = _workContext.GetUserName();

            if (dbInfo.RequisitionStatus == (int)RequisitionStatus.Checked)
            {
                dbPurchaseRequisition.CheckedBy = "";
            }
            else if (dbInfo.RequisitionStatus == (int)RequisitionStatus.Approved)
            {
                dbPurchaseRequisition.ApprovedBy = "";
            }
            else
            {
                dbPurchaseRequisition.Transport = 0;
                dbPurchaseRequisition.PaymentMode = 0;
                dbPurchaseRequisition.PaymentTermInDays = 0;
                dbPurchaseRequisition.TermAndCondition = null;
                dbPurchaseRequisition.ImportPurchaseIncoTerm = 0;
                dbPurchaseRequisition.ImportPurchasePaymentTerm = 0;
                dbPurchaseRequisition.CurrencyId = null;
            }

            await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(dbPurchaseRequisition);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbPurchaseRequisition.RequisitionStatus;
        }
        public virtual async Task<int> SendRFQtoSelectedSupplier(SendRFQtoSelectedSupplierDto sendRFQtoSelectedSupplierDto)
        {
            Guid? tenantId = _workContext.GetTenantId();
            TenantViewModel tenantData = await _tenantService.GetByIdAsync(tenantId);

            var purchaseRequisition = _unitOfWork.Repository<PurchaseRequisition>().TableNoTracking().FirstOrDefault(x => x.Id == sendRFQtoSelectedSupplierDto.PurchaseRequisitionId);
            if (purchaseRequisition == null) throw new NotFoundResultException("Not Found");
            var purchaseRequisitionDetails = _unitOfWork.Repository<PurchaseRequisitionDetail>().TableNoTracking().Include(x => x.Product).ThenInclude(x => x.MeasurementUnit).Where(x => x.PurchaseRequisitionId == purchaseRequisition.Id).Select(x => new PurchaseRequisitionView
            {
                Description = x.Product.Name,
                Unit = x.Product.MeasurementUnit.Name,
                Quantity = x.Quantity,
                Rate = x.Rate,
                Amount = x.Amount,
                LastPurchaseRate = x.LastPurchaseRate,
            }).ToList();
            byte[] bytes;
            using (var stream = new MemoryStream())
            {

                foreach (var id in sendRFQtoSelectedSupplierDto.SelecetedSupplierIds)
                {
                    var dbSupplier = await _unitOfWork.Repository<Account>().FindAsync(id);
                    _purchasePdfService.PrintPurchaseRequisitionInvoiceToPdf(stream, dbSupplier, purchaseRequisition, purchaseRequisitionDetails, tenantData);
                    bytes = stream.ToArray();
                    bytes = _purchasePdfService.AddFooter(bytes);
                    if (string.IsNullOrWhiteSpace(dbSupplier.Email)) throw new BadRequestException("Supplier's Email is not available !!");
                    //send email to supplier
                    var mailRequest = new MailRequest()
                    {
                        ToEmail = dbSupplier.Email,
                        Subject = "APFEED Request For Quotation Ref#" + purchaseRequisition.RequisitionNo,
                        Body = $"Dear {dbSupplier.Name} {Environment.NewLine} Here is in attachment a requsest for quotation {purchaseRequisition.RequisitionNo} from {tenantData.Name}. {Environment.NewLine}If you have any questions,please do not hesitate to contact us.{Environment.NewLine} Best Regards",
                        FileName = "request-for-quotation",
                        FileBytes = bytes
                    };
                    await _mailService.SendEmailFromDefaultEmailAddressAsync(mailRequest);
                    //send sms to supplier
                    await _smsService.Send(dbSupplier.ContactNo, "An email has been sent to your email " + dbSupplier.Email + " for RFQ");
                    RfqSent rfqSent = new RfqSent()
                    {
                        Id = Guid.NewGuid(),
                        RequisitionId = purchaseRequisition.Id,
                        SupplierId = dbSupplier.Id,
                    };

                    await _unitOfWork.Repository<RfqSent>().AddAsync(rfqSent);
                    purchaseRequisition.RequisitionStatus = (int)RequisitionStatus.RFQSent;
                    await _unitOfWork.Repository<PurchaseRequisition>().UpdateAsync(purchaseRequisition);
                }


            }
            await _unitOfWork.SaveChangesAsync();
            return purchaseRequisition.RequisitionStatus;

        }

        public virtual async Task<List<RFQSentSupplier>> GetRfqSentSuppliersByRequisitionId(Guid id)
        {
            var list = await _unitOfWork.Repository<RfqSent>().TableNoTracking().Include(x => x.Supplier)
                .Where(x => x.RequisitionId == id)
                .Select(x => new RFQSentSupplier
                {
                    Name = x.Supplier.Name,
                    Email = x.Supplier.Email,
                    ContactNo = x.Supplier.ContactNo
                }).ToListAsync();
            return list;
        }


    }
}




