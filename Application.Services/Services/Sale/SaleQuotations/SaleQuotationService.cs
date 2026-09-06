using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Sale.SaleQuotation;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using Application.Services.ViewModels.Sale.SaleQuotation;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Sale.SaleQuotations
{
    public class SaleQuotationService : BaseService<SaleQuotation, SaleQuotationCreationDto, SaleQuotationUpdateDto, SaleQuotationRequestModel, SaleQuotationViewModel>, ISaleQuotationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;

        public SaleQuotationService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService, IHubContext<BroadcastHub, IHubClient> hubContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        public async Task<SaleQuotationAggregatorModel> PrepareSaleQuotationAggregatorModel(SaleQuotationRequestModel saleQuotationRequest)
        {
            var saleQuotationQueryable = _unitOfWork.Repository<SaleQuotation>().TableNoTracking().Where(saleQuotationRequest.GetExpression());
            return new SaleQuotationAggregatorModel
            {
                AggregatorSubtotal = await saleQuotationQueryable.SumAsync(x => x.Subtotal),
                AggregatorDiscount = await saleQuotationQueryable.SumAsync(x => x.Discount),
                AggregatorTotal = await saleQuotationQueryable.SumAsync(x => x.Total),
            };
        }

        public async Task<SaleQuotationViewModel> GetByIdAsync(Guid id)
        {
            var saleQuotation = await _unitOfWork.Repository<SaleQuotation>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Customer)
                .Include(blog => blog.SaleQuotationDetails)
                .ThenInclude(post => post.Product).ThenInclude(x => x.MeasurementUnit));
            if (saleQuotation == null) throw new NotFoundResultException("Sale Quotation Not Found With this Id");
            return _mapper.Map<SaleQuotationViewModel>(saleQuotation);
        }

        public new async Task<AddUpdateResponseModel> AddAsync(SaleQuotationCreationDto saleQuotationCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            saleQuotationCreationDto.QuotationDate = saleQuotationCreationDto.QuotationDate.ToLocal();
            var saleQuotation = _mapper.Map<SaleQuotation>(saleQuotationCreationDto);
            var count = _unitOfWork.Repository<SaleQuotation>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            saleQuotation.Id = Guid.NewGuid();
            saleQuotation.QuotationNo = "SQ" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            saleQuotation.FinancialYearId = financialYear.Id;
            saleQuotation.Status = (int)SaleQuotationStatus.Pending;

            foreach (var item in saleQuotation.SaleQuotationDetails)
            {
                item.Id = Guid.NewGuid();
                item.SaleQuotationId = saleQuotation.Id;
                await _unitOfWork.Repository<SaleQuotationDetail>().AddAsync(item);


            }
            await _unitOfWork.Repository<SaleQuotation>().AddAsync(saleQuotation);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, saleQuotation.Id
                , "/sales/sales-quotation/" + saleQuotation.Id, Permissions.SaleQuotations.Check,
                "Sale Quotation " + saleQuotation.QuotationNo + " is ready for Check", (int)SaleQuotationStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return new AddUpdateResponseModel { Id = saleQuotation.Id, Code = saleQuotation.QuotationNo, Status = saleQuotation.Status };
        }

        public new async Task<AddUpdateResponseModel> UpdateAsync(SaleQuotationUpdateDto saleQuotationUpdateDto)
        {
            var dbSaleQuotation = await _unitOfWork.Repository<SaleQuotation>().FindAsync(saleQuotationUpdateDto.Id);
            //when date updated,convert it to local date
            if (saleQuotationUpdateDto.QuotationDate.Equals(dbSaleQuotation.QuotationDate) == false)
            {
                saleQuotationUpdateDto.QuotationDate = saleQuotationUpdateDto.QuotationDate.ToLocal();
            }
            var saleQuotation = _mapper.Map(saleQuotationUpdateDto, dbSaleQuotation);
            if (dbSaleQuotation.Status == (int)SaleQuotationStatus.Pending || dbSaleQuotation.Status == (int)SaleQuotationStatus.Checked)
            {
                await _unitOfWork.Repository<SaleQuotation>().UpdateAsync(saleQuotation);
                foreach (var item in saleQuotation.SaleQuotationDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.SaleQuotationId = saleQuotation.Id;
                        await _unitOfWork.Repository<SaleQuotationDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<SaleQuotationDetail>().UpdateAsync(item);
                }
                //Delete for SaleQuotationDetails
                if (!string.IsNullOrEmpty(saleQuotationUpdateDto.DeletedSaleQuotationDetailIds))
                {
                    foreach (var id in saleQuotationUpdateDto.DeletedSaleQuotationDetailIds.Split(',').Where(x => x != ""))
                    {
                        var saleQuotationDetail = await _unitOfWork.Repository<SaleQuotationDetail>().FindAsync(new Guid(id));
                        saleQuotationDetail.Deleted = true;
                        await _unitOfWork.Repository<SaleQuotationDetail>().UpdateAsync(saleQuotationDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return new AddUpdateResponseModel { Id = saleQuotation.Id, Code = saleQuotation.QuotationNo, Status = saleQuotation.Status };
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var saleQuotation = await _unitOfWork.Repository<SaleQuotation>().FindAsync(x => x.Id == id, x => x.Include(x => x.SaleQuotationDetails));
            if (saleQuotation == null) throw new NotFoundResultException("SaleQuotation Not Found With this Id");
            if (saleQuotation.Status == (int)SaleQuotationStatus.Pending)
            {
                foreach (var item in saleQuotation.SaleQuotationDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<SaleQuotationDetail>().UpdateAsync(item);
                }
                saleQuotation.Deleted = true;
                await _unitOfWork.Repository<SaleQuotation>().UpdateAsync(saleQuotation);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return saleQuotation.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var saleQuotations = _unitOfWork.Repository<SaleQuotation>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await saleQuotations.Where(x => x.Status == (int)SaleQuotationStatus.Pending).CountAsync(),
                CheckedCount = await saleQuotations.Where(x => x.Status == (int)SaleQuotationStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbSaleQuotation = await _unitOfWork.Repository<SaleQuotation>().FindAsync(id);
            if (dbSaleQuotation.Status == (int)SaleQuotationStatus.Pending)
            {
                dbSaleQuotation.Status = (int)SaleQuotationStatus.Checked;
                dbSaleQuotation.CheckedBy = _workContext.GetUserName();
                //update SaleQuotation
                await _unitOfWork.Repository<SaleQuotation>().UpdateAsync(dbSaleQuotation);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSaleQuotation.Id
                       && x.Status == (int)SaleQuotationStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbSaleQuotation.Id
                    , "/sales/sales-quotation/" + dbSaleQuotation.Id, Permissions.SaleQuotations.Approve,
                    "Sale Quotation " + dbSaleQuotation.QuotationNo + " is ready for Approval", (int)SaleQuotationStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Sale Quotation Status has already been checked by " + dbSaleQuotation?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbSaleQuotationInfo = await _unitOfWork.Repository<SaleQuotation>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbSaleQuotationInfo == null)
                throw new NotFoundResultException("Sale Quotation not found.");

            if (financialYear.Id != dbSaleQuotationInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Sale Quotation does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<SaleQuotation>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)SaleQuotationStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)SaleQuotationStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Sale Quotation already approved by {dbSaleQuotationInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbSaleQuotation = await _unitOfWork.Repository<SaleQuotation>().FindAsync(id);

            dbSaleQuotation.Status = (int)SaleQuotationStatus.Approved;
            dbSaleQuotation.ApprovedBy = approvedBy;

            //update SaleQuotation
            await _unitOfWork.Repository<SaleQuotation>().UpdateAsync(dbSaleQuotation);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSaleQuotation.Id
                   && x.Status == (int)SaleQuotationStatus.Checked);

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

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<SaleQuotation>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("SaleQuotation Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Sale Quotation does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)SaleQuotationStatus.Checked && dbInfo.Status != (int)SaleQuotationStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)SaleQuotationStatus.Checked
                ? (int)SaleQuotationStatus.Pending
                : (int)SaleQuotationStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<SaleQuotation>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbSaleQuotation = await _unitOfWork.Repository<SaleQuotation>().FindAsync(id);
            dbSaleQuotation.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)SaleQuotationStatus.Checked:
                    dbSaleQuotation.CheckedBy = "";
                    break;
                case (int)SaleQuotationStatus.Approved:
                    dbSaleQuotation.ApprovedBy = "";
                    break;
            }

            await _unitOfWork.Repository<SaleQuotation>().UpdateAsync(dbSaleQuotation);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbSaleQuotation.Status;
        }


    }
}
