using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Accounts.AccountsPayable.SupplierPaymentAgainstPurchase;
using Application.Services.SearchRequestModels.Accounts.AccountsPayable;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts.AccountsPayable;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static iTextSharp.text.pdf.AcroFields;

namespace Application.Services.Services.Accounts.AccountsPayable.SupplierPaymentAgainstPurchases
{
    public class SupplierPaymentAgainstPurchaseService : BaseService<SupplierPaymentAgainstPurchase, SupplierPaymentAgainstPurchaseCreationDto, SupplierPaymentAgainstPurchaseUpdateDto, SupplierPaymentAgainstPurchaseRequestModel, SupplierPaymentAgainstPurchaseViewModel>, ISupplierPaymentAgainstPurchaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IAccountService _accountService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly INotificationService _notificationService;

        public SupplierPaymentAgainstPurchaseService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext,
            IAccountService accountService, IHubContext<BroadcastHub, IHubClient> hubContext
            , INotificationService notificationService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _accountService = accountService;
            _hubContext = hubContext;
            _notificationService = notificationService;
        }




        public async Task<SupplierPaymentAgainstPurchaseViewModel> GetByIdAsync(Guid id)
        {
            var supplierPaymentAgainstPurchase = await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.CostCenter).Include(x => x.FromAccount).Include(x => x.PurchaseInvoice).ThenInclude(x => x.Supplier));
            if (supplierPaymentAgainstPurchase == null) throw new NotFoundResultException("Supplier Payment Against Purchase Not Found With this Id");
            return _mapper.Map<SupplierPaymentAgainstPurchaseViewModel>(supplierPaymentAgainstPurchase);
        }
        public new async Task<AddUpdateResponseModel> AddAsync(SupplierPaymentAgainstPurchaseCreationDto supplierPaymentAgainstPurchaseCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var model = _mapper.Map<SupplierPaymentAgainstPurchase>(supplierPaymentAgainstPurchaseCreationDto);
            var count = _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            model.Id = Guid.NewGuid();
            model.Code = "SPAP" + financialYear.Code + "-" + count.ToString().PadLeft(6, '0');
            model.FinancialYearId = financialYear.Id;
            model.Status = (int)SupplierPaymentStatus.Pending;
            await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().AddAsync(model);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, model.Id
                , "/accounts/accounts-payable/supplier-payment-against-purchase/" + model.Id, Permissions.SaleInvoices.Check,
                "Supplier Payment Against Purchase " + model.Code + " is ready for Approval", (int)SupplierPaymentStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            return new AddUpdateResponseModel { Id = model.Id, Code = model.Code, Status = model.Status };
        }
        public new async Task<AddUpdateResponseModel> UpdateAsync(SupplierPaymentAgainstPurchaseUpdateDto supplierPaymentAgainstPurchaseUpdateDto)
        {
            var dbSupplierPaymentAgainstPurchase = await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().FindAsync(supplierPaymentAgainstPurchaseUpdateDto.Id);
            if (dbSupplierPaymentAgainstPurchase == null) throw new NotFoundResultException("Supplier Payment Against Purchase Not Found With this Id");
            var supplierPaymentAgainstPurchase = _mapper.Map(supplierPaymentAgainstPurchaseUpdateDto, dbSupplierPaymentAgainstPurchase);
            if (dbSupplierPaymentAgainstPurchase.Status == (int)SupplierPaymentStatus.Pending || dbSupplierPaymentAgainstPurchase.Status == (int)SupplierPaymentStatus.Checked)
            {
                await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().UpdateAsync(supplierPaymentAgainstPurchase);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update.Only Pending or Checked Status Allowed to Update");
            return new AddUpdateResponseModel { Id = supplierPaymentAgainstPurchase.Id, Code = supplierPaymentAgainstPurchase.Code, Status = supplierPaymentAgainstPurchase.Status };
        }
        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var supplierPaymentAgainstPurchase = await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().FindAsync(x => x.Id == id);
            if (supplierPaymentAgainstPurchase == null) throw new NotFoundResultException("Supplier Payment Against Purchase Not Found With this Id");
            if (supplierPaymentAgainstPurchase.Status == (int)SupplierPaymentStatus.Pending)
            {
                supplierPaymentAgainstPurchase.Deleted = true;
                await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().UpdateAsync(supplierPaymentAgainstPurchase);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete.Only Pending Status Allowed to Delete");
            return supplierPaymentAgainstPurchase.Id;
        }
        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var supplierPaymentAgainstPurchases = _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await supplierPaymentAgainstPurchases.Where(x => x.Status == (int)SupplierPaymentStatus.Pending).CountAsync(),
                CheckedCount = await supplierPaymentAgainstPurchases.Where(x => x.Status == (int)SupplierPaymentStatus.Checked).CountAsync()
            };
            return response;
        }
        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbSupplierPaymentAgainstPurchase = await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().FindAsync(id);
            dbSupplierPaymentAgainstPurchase.Status = (int)SupplierPaymentStatus.Checked;
            dbSupplierPaymentAgainstPurchase.CheckedBy = _workContext.GetUserName();
            await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().UpdateAsync(dbSupplierPaymentAgainstPurchase);
            //remove pending notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSupplierPaymentAgainstPurchase.Id
                   && x.Status == (int)SupplierPaymentStatus.Pending);
            //send notification for approval
            await _notificationService.SendNotificationAsync(_unitOfWork, dbSupplierPaymentAgainstPurchase.Id
                , "/accounts/accounts-payable/supplier-payment-against-purchase/" + dbSupplierPaymentAgainstPurchase.Id, Permissions.SaleInvoices.Approve,
                "Supplier Payment Against Purchase " + dbSupplierPaymentAgainstPurchase.Code + " is ready for Approval", (int)SupplierPaymentStatus.Checked);
            bool isChecked = await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            return isChecked;
        }
        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Supplier Payment Against Purchase not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Supplier Payment Against Purchase does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)SupplierPaymentStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)SupplierPaymentStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Supplier Payment Against Purchase already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbSupplierPaymentAgainstPurchase = await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.PurchaseInvoice).ThenInclude(x => x.Supplier));

            dbSupplierPaymentAgainstPurchase.Status = (int)SupplierPaymentStatus.Approved;
            dbSupplierPaymentAgainstPurchase.ApprovedBy = approvedBy;
            //change purchase invoice status
            dbSupplierPaymentAgainstPurchase.PurchaseInvoice.Status = (int)PurchaseInvoiceStatus.Fully_Paid;

            await InsertTransaction(dbSupplierPaymentAgainstPurchase);
            await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().UpdateAsync(dbSupplierPaymentAgainstPurchase);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbSupplierPaymentAgainstPurchase.Id
                   && x.Status == (int)SupplierPaymentStatus.Checked);

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
            var unpostedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("SupplierPaymentAgainstPurchase Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Supplier Payment Against Purchase does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)SupplierPaymentStatus.Checked && dbInfo.Status != (int)SupplierPaymentStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)SupplierPaymentStatus.Checked
                ? (int)SupplierPaymentStatus.Pending
                : (int)SupplierPaymentStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                    .SetProperty(x => x.UnpostedBy, unpostedBy)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbSupplierPaymentAgainstPurchase = await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.PurchaseInvoice).ThenInclude(x => x.Supplier));
            dbSupplierPaymentAgainstPurchase.Status = newStatus;
            dbSupplierPaymentAgainstPurchase.UnpostedBy = unpostedBy;

            switch (dbInfo.Status)
            {
                case (int)SupplierPaymentStatus.Checked:
                    break;
                case (int)SupplierPaymentStatus.Approved:
                    //change purchase invoice status
                    var purchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().SingleOrDefaultAsync(x => x.Id == dbSupplierPaymentAgainstPurchase.PurchaseInvoiceId);
                    purchaseInvoice!.Status = (int)PurchaseInvoiceStatus.Approved;
                    await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(purchaseInvoice);
                    await DeleteTransaction(dbSupplierPaymentAgainstPurchase.Code);
                    break;
            }

            await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().UpdateAsync(dbSupplierPaymentAgainstPurchase);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbSupplierPaymentAgainstPurchase.Status;
        }
        private async Task InsertTransaction(SupplierPaymentAgainstPurchase model)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Paid to ");
            sb.Append(model?.PurchaseInvoice?.Supplier?.Name);
            sb.Append(" Against ");
            sb.Append(model?.PurchaseInvoice?.PurchaseInvoiceNo);
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var supplierAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == model!.SupplierId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(supplierAccountName);
            contraAccountIdsForCredit.Append(model!.SupplierId.ToString());
            var fromAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == model!.FromAccountId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(fromAccountName);
            contraAccountIdsForDebit.Append(model!.FromAccountId.ToString());
            //Transaction Entries start here
            //supplier account debit
            await _accountService.HitAccount(_unitOfWork, model!.CostCenterId, model.Code, sb.ToString(),
                 model.SupplierId, model.Amount, 0, model.PaymentDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
            //cash/bank account credit
            await _accountService.HitAccount(_unitOfWork, model.CostCenterId, model.Code, sb.ToString(),
                model.FromAccountId, 0, model.Amount, model.PaymentDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());

        }

        private async Task DeleteTransaction(string code)
        {
            var supplierPaymentAgainstPurchase = await _unitOfWork.Repository<SupplierPaymentAgainstPurchase>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.Code == code);
            if (supplierPaymentAgainstPurchase is null) throw new NotFoundResultException("Supplier Payment Against Purchase Not Found With this invoice No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == supplierPaymentAgainstPurchase.Code);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction);
                }
            }
        }
    }
}
