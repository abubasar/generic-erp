using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Production.BillOfMaterial;
using Application.Services.SearchRequestModels.Production;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Production;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Productions.BillOfMaterials
{
    public class BillOfMaterialService : BaseService<BillOfMaterial, BillOfMaterialCreationDto, BillOfMaterialUpdateDto, BillOfMaterialRequestModel, BillOfMaterialViewModel>, IBillOfMaterialService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        public BillOfMaterialService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService, IHubContext<BroadcastHub, IHubClient> hubContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }
        public async Task<BillOfMaterialViewModel> GetByIdAsync(Guid id)
        {
            var billOfMaterial = await _unitOfWork.Repository<BillOfMaterial>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.FinishedProduct).Include(x => x.BillOfMaterialDetails.OrderBy(x => x.RawMaterial.Name))
                .ThenInclude(x => x.RawMaterial).ThenInclude(x => x.MeasurementUnit));
            if (billOfMaterial == null) throw new NotFoundResultException("Bill Of Material Not Found With this Id");
            return _mapper.Map<BillOfMaterialViewModel>(billOfMaterial);
        }
        public new async Task<Guid> AddAsync(BillOfMaterialCreationDto billOfMaterialCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var billOfMaterial = _mapper.Map<BillOfMaterial>(billOfMaterialCreationDto);
            var count = _unitOfWork.Repository<BillOfMaterial>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            billOfMaterial.Id = Guid.NewGuid();
            billOfMaterial.BomNo = "BOM" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            billOfMaterial.FinancialYearId = financialYear.Id;
            billOfMaterial.Status = (int)BOMStatus.Pending;

            foreach (var item in billOfMaterial.BillOfMaterialDetails)
            {
                item.Id = Guid.NewGuid();
                item.BillOfMaterialId = billOfMaterial.Id;
                await _unitOfWork.Repository<BillOfMaterialDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<BillOfMaterial>().AddAsync(billOfMaterial);

            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, billOfMaterial.Id
                , "/production/bill-of-material/" + billOfMaterial.Id, Permissions.BillOfMaterials.Check,
                "Bill of Material " + billOfMaterial.BomNo + " is ready for Check", (int)BOMStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return billOfMaterial.Id;
        }
        public new async Task<Guid> UpdateAsync(BillOfMaterialUpdateDto billOfMaterialUpdateDto)
        {
            var dbBillOfMaterial = await _unitOfWork.Repository<BillOfMaterial>().FindAsync(billOfMaterialUpdateDto.Id);
            var billOfMaterial = _mapper.Map(billOfMaterialUpdateDto, dbBillOfMaterial);
            if (dbBillOfMaterial.Status == (int)BOMStatus.Pending || dbBillOfMaterial.Status == (int)BOMStatus.Checked)
            {
                await _unitOfWork.Repository<BillOfMaterial>().UpdateAsync(billOfMaterial);
                foreach (var item in billOfMaterial.BillOfMaterialDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.BillOfMaterialId = billOfMaterial.Id;
                        await _unitOfWork.Repository<BillOfMaterialDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<BillOfMaterialDetail>().UpdateAsync(item);
                }
                //Delete for BillOfMaterialDetails
                if (!string.IsNullOrEmpty(billOfMaterialUpdateDto.DeletedBillOfMaterialDetailIds))
                {
                    foreach (var id in billOfMaterialUpdateDto.DeletedBillOfMaterialDetailIds.Split(',').Where(x => x != ""))
                    {
                        var billOfMaterialDetail = await _unitOfWork.Repository<BillOfMaterialDetail>().FindAsync(new Guid(id));
                        billOfMaterialDetail.Deleted = true;
                        await _unitOfWork.Repository<BillOfMaterialDetail>().UpdateAsync(billOfMaterialDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            return billOfMaterial.Id;
        }
        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var billOfMaterial = await _unitOfWork.Repository<BillOfMaterial>().FindAsync(x => x.Id == id, x => x.Include(x => x.BillOfMaterialDetails));
            if (billOfMaterial == null) throw new NotFoundResultException("BillOfMaterial Not Found With this Id");
            if (billOfMaterial.Status == (int)BOMStatus.Pending)
            {
                foreach (var item in billOfMaterial.BillOfMaterialDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<BillOfMaterialDetail>().UpdateAsync(item);
                }
                billOfMaterial.Deleted = true;
                await _unitOfWork.Repository<BillOfMaterial>().UpdateAsync(billOfMaterial);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return billOfMaterial.Id;
        }
        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var billOfMaterials = _unitOfWork.Repository<BillOfMaterial>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await billOfMaterials.Where(x => x.Status == (int)BOMStatus.Pending).CountAsync(),
                CheckedCount = await billOfMaterials.Where(x => x.Status == (int)BOMStatus.Checked).CountAsync()
            };
            return response;
        }
        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbBillOfMaterial = await _unitOfWork.Repository<BillOfMaterial>().FindAsync(id);
            if (dbBillOfMaterial.Status == (int)BOMStatus.Pending)
            {
                dbBillOfMaterial.Status = (int)BOMStatus.Checked;
                dbBillOfMaterial.CheckedBy = _workContext.GetUserName();
                //update BillOfMaterial
                await _unitOfWork.Repository<BillOfMaterial>().UpdateAsync(dbBillOfMaterial);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbBillOfMaterial.Id
                       && x.Status == (int)BOMStatus.Pending);

                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbBillOfMaterial.Id
                    , "/production/bill-of-material/" + dbBillOfMaterial.Id, Permissions.BillOfMaterials.Approve,
                    "Bill of Material " + dbBillOfMaterial.BomNo + " is ready for Approval", (int)BOMStatus.Checked);

                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Bom Status has already been checked by " + dbBillOfMaterial?.CheckedBy);


        }
        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<BillOfMaterial>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Bill Of Material not found.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<BillOfMaterial>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)BOMStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)BOMStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Bom already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbBillOfMaterial = await _unitOfWork.Repository<BillOfMaterial>().FindAsync(id);

            dbBillOfMaterial.Status = (int)BOMStatus.Approved;
            dbBillOfMaterial.ApprovedBy = approvedBy;

            //update BillOfMaterial
            await _unitOfWork.Repository<BillOfMaterial>().UpdateAsync(dbBillOfMaterial);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbBillOfMaterial.Id
                   && x.Status == (int)BOMStatus.Checked);

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
            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<BillOfMaterial>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("BillOfMaterial Not Found With this Id");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)BOMStatus.Checked && dbInfo.Status != (int)BOMStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)BOMStatus.Checked
                ? (int)BOMStatus.Pending
                : (int)BOMStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<BillOfMaterial>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbBillOfMaterial = await _unitOfWork.Repository<BillOfMaterial>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.BillOfMaterialDetails));
            dbBillOfMaterial.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)BOMStatus.Checked:
                    dbBillOfMaterial.CheckedBy = "";
                    break;
                case (int)BOMStatus.Approved:
                    dbBillOfMaterial.ApprovedBy = "";
                    break;
            }

            await _unitOfWork.Repository<BillOfMaterial>().UpdateAsync(dbBillOfMaterial);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbBillOfMaterial.Status;
        }
    }
}
