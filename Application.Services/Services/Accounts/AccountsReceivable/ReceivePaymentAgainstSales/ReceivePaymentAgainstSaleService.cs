using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment;
using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePaymentAgainstSale;
using Application.Services.SearchRequestModels.Accounts.AccountsReceivable;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Accounts.AccountsReceivable.ReceivePaymentAgainstSale;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using static Application.Core.Constants.Permissions;
using static Application.Core.Constants.SecondaryPermissions;

namespace Application.Services.Services.Accounts.AccountsReceivable.ReceivePaymentAgainstSales
{
    public class ReceivePaymentAgainstSaleService : BaseService<ReceivePaymentAgainstSale, ReceivePaymentAgainstSaleCreationDto, ReceivePaymentAgainstSaleUpdateDto, ReceivePaymentAgainstSaleRequestModel, ReceivePaymentAgainstSaleViewModel>, IReceivePaymentAgainstSaleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IAccountService _accountService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly INotificationService _notificationService;
        public ReceivePaymentAgainstSaleService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext,
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

        public async Task<ReceivePaymentAgainstSaleAggregatorModel> PrepareReceivePaymentAgainstSaleAggregatorModel(ReceivePaymentAgainstSaleRequestModel receivePaymentAgainstSaleRequest)
        {
            var receivePaymentAgainstSaleQueryable = _unitOfWork.Repository<ReceivePaymentAgainstSale>().TableNoTracking().Where(receivePaymentAgainstSaleRequest.GetExpression());
            return new ReceivePaymentAgainstSaleAggregatorModel
            {
                AggregatorAmount = await receivePaymentAgainstSaleQueryable.SumAsync(x => x.Amount),

            };
        }

        public async Task<ReceivePaymentAgainstSaleViewModel> GetByIdAsync(Guid id)
        {
            var receivePaymentAgainstSale = await _unitOfWork.Repository<ReceivePaymentAgainstSale>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.CostCenter).Include(x => x.ToAccount).Include(x => x.Customer));
            if (receivePaymentAgainstSale == null) throw new NotFoundResultException("Receive Payment Against Sale Not Found With this Id");
            return _mapper.Map<ReceivePaymentAgainstSaleViewModel>(receivePaymentAgainstSale);
        }
        public new async Task<AddUpdateResponseModel> AddAsync(ReceivePaymentAgainstSaleCreationDto receivePaymentAgainstSaleCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            receivePaymentAgainstSaleCreationDto.PaymentDate = receivePaymentAgainstSaleCreationDto.PaymentDate.ToLocal();
            var model = _mapper.Map<ReceivePaymentAgainstSale>(receivePaymentAgainstSaleCreationDto);
            var count = _unitOfWork.Repository<ReceivePaymentAgainstSale>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            model.Id = Guid.NewGuid();
            model.Code = "MRAS" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            model.FinancialYearId = financialYear.Id;
            model.Status = (int)ReceivePaymentStatus.Pending;
            var customer = await _unitOfWork.Repository<Account>().FindAsync(receivePaymentAgainstSaleCreationDto.CustomerId);
            model.CustomerMarketingOfficerId = customer?.CustomerMarketingOfficerId;
            model.InvoiceNo = "";
            await _unitOfWork.Repository<ReceivePaymentAgainstSale>().AddAsync(model);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, model.Id
                , "/accounts/accounts-receivable/receive-payment-against-sale/" + model.Id, PrimaryPermissions.ReceivePaymentAgainstSales.Check,
                "Receive Payment Against Sale " + model.Code + " is ready for Approval", (int)ReceivePaymentStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            return new AddUpdateResponseModel { Id = model.Id, Code = model.Code, Status = model.Status };
        }
        public async Task<Guid> AddByDmsAppAsync(ReceivePaymentAgainstSaleCreationDtoForDmsApp receivePaymentAgainstSaleCreationDto)
        {
            //comment
            var financialYear = _workContext.GetCurrentFinancialYear();
            receivePaymentAgainstSaleCreationDto.PaymentDate = receivePaymentAgainstSaleCreationDto.PaymentDate.ToLocal();
            var receivePaymentAgainstSale = new ReceivePaymentAgainstSale();
            var count = _unitOfWork.Repository<ReceivePaymentAgainstSale>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            receivePaymentAgainstSale.Id = Guid.NewGuid();
            receivePaymentAgainstSale.Code = "MRAS" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            receivePaymentAgainstSale.FinancialYearId = financialYear.Id;
            receivePaymentAgainstSale.Status = (int)ReceivePaymentStatus.Pending;
            var customer = await _unitOfWork.Repository<Account>().FindAsync(receivePaymentAgainstSaleCreationDto.CustomerId);
            receivePaymentAgainstSale.CustomerMarketingOfficerId = customer?.CustomerMarketingOfficerId;
            //manual mapping entity from dto
            receivePaymentAgainstSale.PaymentDate = receivePaymentAgainstSaleCreationDto.PaymentDate;
            receivePaymentAgainstSale.InvoiceNo = "";
            receivePaymentAgainstSale.CustomerId = receivePaymentAgainstSaleCreationDto.CustomerId;
            receivePaymentAgainstSale.Amount = receivePaymentAgainstSaleCreationDto.Amount;
            receivePaymentAgainstSale.CostCenterId = receivePaymentAgainstSaleCreationDto.CostCenterId;
            receivePaymentAgainstSale.ToAccountId = receivePaymentAgainstSaleCreationDto.ToAccountId;
            receivePaymentAgainstSale.Remark = receivePaymentAgainstSaleCreationDto.Remark;
            await _unitOfWork.Repository<ReceivePaymentAgainstSale>().AddAsync(receivePaymentAgainstSale);
            //
            await AddReceiptAsync(new ReceivePaymentAgainstSalePictureMappingCreationDto { FileDetails = receivePaymentAgainstSaleCreationDto.FileDetails, ReceivePaymentAgainstSaleId = receivePaymentAgainstSale.Id });

            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, receivePaymentAgainstSale.Id
                , "/accounts/accounts-receivable/receive-payment-against-sale/" + receivePaymentAgainstSale.Id, PrimaryPermissions.ReceivePaymentAgainstSales.Check,
                "Receive Payment Against Sale " + receivePaymentAgainstSale.Code + " is ready for Check", (int)ReceivePaymentStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return receivePaymentAgainstSale.Id;
        }

        public new async Task<AddUpdateResponseModel> UpdateAsync(ReceivePaymentAgainstSaleUpdateDto receivePaymentAgainstSaleUpdateDto)
        {
            var dbReceivePaymentAgainstSale = await _unitOfWork.Repository<ReceivePaymentAgainstSale>().FindAsync(receivePaymentAgainstSaleUpdateDto.Id);
            if (dbReceivePaymentAgainstSale == null) throw new NotFoundResultException("Receive Payment Against Sale Not Found With this Id");
            //when date updated,convert it to local date
            if (receivePaymentAgainstSaleUpdateDto.PaymentDate.Equals(dbReceivePaymentAgainstSale.PaymentDate) == false)
            {
                receivePaymentAgainstSaleUpdateDto.PaymentDate = receivePaymentAgainstSaleUpdateDto.PaymentDate.ToLocal();
            }
            var receivePaymentAgainstSale = _mapper.Map(receivePaymentAgainstSaleUpdateDto, dbReceivePaymentAgainstSale);
            if (dbReceivePaymentAgainstSale.Status == (int)ReceivePaymentStatus.Pending || dbReceivePaymentAgainstSale.Status == (int)ReceivePaymentStatus.Checked)
            {
                await _unitOfWork.Repository<ReceivePaymentAgainstSale>().UpdateAsync(receivePaymentAgainstSale);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update.Only Pending or Checked Status Allowed to Update");
            return new AddUpdateResponseModel { Id = receivePaymentAgainstSale.Id, Code = receivePaymentAgainstSale.Code, Status = receivePaymentAgainstSale.Status };
        }
        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var receivePaymentAgainstSale = await _unitOfWork.Repository<ReceivePaymentAgainstSale>().FindAsync(x => x.Id == id);
            if (receivePaymentAgainstSale == null) throw new NotFoundResultException("Receive Payment Against Sale Not Found With this Id");
            if (receivePaymentAgainstSale.Status == (int)ReceivePaymentStatus.Pending)
            {
                receivePaymentAgainstSale.Deleted = true;
                await _unitOfWork.Repository<ReceivePaymentAgainstSale>().UpdateAsync(receivePaymentAgainstSale);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete.Only Pending Status Allowed to Delete");
            return receivePaymentAgainstSale.Id;
        }
        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var receivePaymentAgainstSales = _unitOfWork.Repository<ReceivePaymentAgainstSale>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await receivePaymentAgainstSales.Where(x => x.Status == (int)ReceivePaymentStatus.Pending).CountAsync(),
                CheckedCount = await receivePaymentAgainstSales.Where(x => x.Status == (int)ReceivePaymentStatus.Checked).CountAsync()
            };
            return response;
        }
        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var dbReceivePaymentAgainstSale = await _unitOfWork.Repository<ReceivePaymentAgainstSale>().FindAsync(id);
            if (financialYear.Id != dbReceivePaymentAgainstSale.FinancialYearId) throw new BadRequestException("Checked Denied: This Receive Payment Against Sale does not belong to the current financial year.");
            if (dbReceivePaymentAgainstSale.Status == (int)ReceivePaymentStatus.Pending)
            {
                dbReceivePaymentAgainstSale.Status = (int)ReceivePaymentStatus.Checked;
                dbReceivePaymentAgainstSale.CheckedBy = _workContext.GetUserName();
                await _unitOfWork.Repository<ReceivePaymentAgainstSale>().UpdateAsync(dbReceivePaymentAgainstSale);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbReceivePaymentAgainstSale.Id
                       && x.Status == (int)ReceivePaymentStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbReceivePaymentAgainstSale.Id
                    , "/accounts/accounts-receivable/receive-payment-against-sale/" + dbReceivePaymentAgainstSale.Id, PrimaryPermissions.ReceivePaymentAgainstSales.Approve,
                    "Receive Payment Against Sale " + dbReceivePaymentAgainstSale.Code + " is ready for Approval", (int)ReceivePaymentStatus.Checked);
                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Receive Payment Against Sale's Status has already been checked by " + dbReceivePaymentAgainstSale?.CheckedBy);
        }
        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<ReceivePaymentAgainstSale>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Receive Payment Against Sale not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Receive Payment Against Sale does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<ReceivePaymentAgainstSale>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)ReceivePaymentStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)ReceivePaymentStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Receive Payment Against Sale already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbReceivePaymentAgainstSale = await _unitOfWork.Repository<ReceivePaymentAgainstSale>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.Customer));

            dbReceivePaymentAgainstSale.Status = (int)ReceivePaymentStatus.Approved;
            dbReceivePaymentAgainstSale.ApprovedBy = approvedBy;

            var saleInvoices = await ReceiveAgainstInvoiceFIFO(dbReceivePaymentAgainstSale);
            dbReceivePaymentAgainstSale.InvoiceNo = string.Join(",", saleInvoices.Select(x => x.InvoiceNo));
            //update SaleInvoice
            await InsertTransaction(dbReceivePaymentAgainstSale);
            await _unitOfWork.Repository<ReceivePaymentAgainstSale>().UpdateAsync(dbReceivePaymentAgainstSale);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbReceivePaymentAgainstSale.Id
                   && x.Status == (int)ReceivePaymentStatus.Checked);

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
            var dbInfo = await _unitOfWork.Repository<ReceivePaymentAgainstSale>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("ReceivePaymentAgainstSale Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Receive Payment Against Sale does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)ReceivePaymentStatus.Checked && dbInfo.Status != (int)ReceivePaymentStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)ReceivePaymentStatus.Checked
                ? (int)ReceivePaymentStatus.Pending
                : (int)ReceivePaymentStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<ReceivePaymentAgainstSale>()
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
            var dbReceivePaymentAgainstSale = await _unitOfWork.Repository<ReceivePaymentAgainstSale>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.Customer));
            dbReceivePaymentAgainstSale.Status = newStatus;
            dbReceivePaymentAgainstSale.UnpostedBy = unpostedBy;

            switch (dbInfo.Status)
            {
                case (int)ReceivePaymentStatus.Checked:
                    break;
                case (int)ReceivePaymentStatus.Approved:
                    //Reverse Invoice Payment
                    await ReverseInvoicePayment(dbReceivePaymentAgainstSale);
                    dbReceivePaymentAgainstSale.InvoiceNo = "";
                    //Delete Transaction Entry
                    await DeleteTransaction(dbReceivePaymentAgainstSale.Code);
                    break;
            }

            await _unitOfWork.Repository<ReceivePaymentAgainstSale>().UpdateAsync(dbReceivePaymentAgainstSale);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbReceivePaymentAgainstSale.Status;
        }

        private async Task<List<SaleInvoice>> ReceiveAgainstInvoiceFIFO(ReceivePaymentAgainstSale receivePaymentAgainstSale)
        {
            List<SaleInvoice> saleInvoices = new();
            var neededAmount = receivePaymentAgainstSale.Amount;
            var amountCalculated = 0M;
            var used = 0M;
            var currentIndex = 0;
            var outstandingInvoices = _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Where(x => x.CustomerId == receivePaymentAgainstSale.CustomerId && x.AvailableReceivable != 0m)
                     .OrderBy(x => x.CreatedOn).ToArray();
            if (!outstandingInvoices.Any()) throw new NotFoundResultException($"There is no outstanding invoices for this customer");
            while (neededAmount != amountCalculated)
            {
                var invoice = outstandingInvoices[currentIndex];
                var mapping = new ReceivePaymentAgainstSaleSaleInvoiceMapping
                {
                    Id = Guid.NewGuid(),
                    ReceivePaymentAgainstSaleId = receivePaymentAgainstSale.Id,
                    SaleInvoiceId = invoice.Id,
                    PreviousAvailableReceivable = invoice.AvailableReceivable
                };
                if (invoice.AvailableReceivable > (neededAmount - amountCalculated))
                {
                    used = (neededAmount - amountCalculated);
                    invoice.AvailableReceivable = invoice.AvailableReceivable - (neededAmount - amountCalculated);
                    amountCalculated = amountCalculated + (neededAmount - amountCalculated);
                }
                else if (invoice.AvailableReceivable == (neededAmount - amountCalculated))
                {
                    used = invoice.AvailableReceivable;
                    amountCalculated += invoice.AvailableReceivable;
                    invoice.AvailableReceivable = 0M;
                }
                else if (invoice.AvailableReceivable < (neededAmount - amountCalculated))
                {
                    used = invoice.AvailableReceivable;
                    amountCalculated += invoice.AvailableReceivable;
                    invoice.AvailableReceivable = 0M;
                }
                mapping.AmountUsed = used;
                invoice.Paid += used;
                saleInvoices.Add(invoice);
                await _unitOfWork.Repository<ReceivePaymentAgainstSaleSaleInvoiceMapping>().AddAsync(mapping);
                await _unitOfWork.Repository<SaleInvoice>().UpdateAsync(invoice);
                currentIndex++;

            }
            return saleInvoices;
        }
        private async Task ReverseInvoicePayment(ReceivePaymentAgainstSale dbReceivePaymentAgainstSale)
        {
            var mappings = await _unitOfWork.Repository<ReceivePaymentAgainstSaleSaleInvoiceMapping>().TableNoTracking()
                .Where(x => x.ReceivePaymentAgainstSaleId == dbReceivePaymentAgainstSale.Id).ToListAsync();
            foreach (var mapping in mappings)
            {
                var invoice = await _unitOfWork.Repository<SaleInvoice>().FindAsync(mapping.SaleInvoiceId);
                if (invoice == null) throw new NotFoundResultException("Invoice Not Found Associated with this Collection!!");
                invoice.AvailableReceivable += mapping.AmountUsed;
                invoice.Paid -= mapping.AmountUsed;
                await _unitOfWork.Repository<SaleInvoice>().UpdateAsync(invoice);
            }
            await _unitOfWork.Repository<ReceivePaymentAgainstSaleSaleInvoiceMapping>().DeleteAsync(mappings);
        }

        private async Task InsertTransaction(ReceivePaymentAgainstSale model)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Money Receipt from ");
            sb.Append(model?.Customer?.Name);
            sb.Append(" Against ");
            sb.Append(model?.InvoiceNo);
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var toAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == model!.ToAccountId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(toAccountName);
            contraAccountIdsForCredit.Append(model!.ToAccountId.ToString());
            var customerAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == model!.CustomerId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(customerAccountName);
            contraAccountIdsForDebit.Append(model!.CustomerId.ToString());
            //account entry
            //cash/bank account debit
            await _accountService.HitAccount(_unitOfWork, model!.CostCenterId, model.Code, sb.ToString(),
                 model.ToAccountId, model.Amount, 0, model.PaymentDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
            //customer account credit
            await _accountService.HitAccount(_unitOfWork, null, model.Code, sb.ToString(),
                model.CustomerId, 0, model.Amount, model.PaymentDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());

        }

        private async Task DeleteTransaction(string code)
        {
            var receivePaymentAgainstSale = await _unitOfWork.Repository<ReceivePaymentAgainstSale>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.Code == code);
            if (receivePaymentAgainstSale is null) throw new NotFoundResultException("Receive Payment Against Sale Not Found With this invoice No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == receivePaymentAgainstSale.Code);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }
        }

        private async Task AddReceiptAsync(ReceivePaymentAgainstSalePictureMappingCreationDto creationDto)
        {
            if (creationDto.FileDetails is null)
                throw new BadRequestException("Please Select File!");
            var picture = new Picture()
            {
                Id = Guid.NewGuid(),
                FileName = creationDto.FileDetails?.FileName,
            };
            using (var stream = new MemoryStream())
            {
                creationDto.FileDetails!.CopyTo(stream);
                if (creationDto.FileDetails!.ContentType == "application/pdf")
                {
                    picture.FileData = stream.ToArray();
                }
                else
                {
                    picture.FileData = ConvertImageToPdf(stream.ToArray()).ToArray();
                }

            }
            var receivePaymentPictureMapping = new ReceivePaymentPictureMapping()
            {
                Id = Guid.NewGuid(),
                PictureId = picture.Id,
                ReceivePaymentId = creationDto.ReceivePaymentAgainstSaleId,
            };
            await _unitOfWork.Repository<Picture>().AddAsync(picture);
            await _unitOfWork.Repository<ReceivePaymentPictureMapping>().AddAsync(receivePaymentPictureMapping);
        }

        public async Task<Guid> PostSingleFileAsync(ReceivePaymentAgainstSalePictureMappingCreationDto creationDto)
        {
            if (creationDto.FileDetails is null)
                throw new BadRequestException("Please Select File!");

            var dbReceivePaymentAgainstSale = await _unitOfWork.Repository<ReceivePaymentAgainstSale>().FindAsync(x => x.Id == creationDto.ReceivePaymentAgainstSaleId);
            if (dbReceivePaymentAgainstSale.Status >= (int)ReceivePaymentStatus.Approved) throw new BadRequestException("You won't be able to upload Image because this money receipt has already been approved");
            //Previous file deletion Start
            var existingReceivePaymentPictureMapping = await _unitOfWork.Repository<ReceivePaymentPictureMapping>().TableNoTracking().FirstOrDefaultAsync(x => x.ReceivePaymentId == creationDto.ReceivePaymentAgainstSaleId);
            if (existingReceivePaymentPictureMapping is not null)
            {
                var existingPicture = await _unitOfWork.Repository<Picture>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == existingReceivePaymentPictureMapping!.PictureId);
                await _unitOfWork.Repository<ReceivePaymentPictureMapping>().DeleteAsync(existingReceivePaymentPictureMapping.Id);
                await _unitOfWork.Repository<Picture>().DeleteAsync(existingPicture!.Id);
            }
            //End
            var picture = new Picture()
            {
                Id = Guid.NewGuid(),
                FileName = creationDto.FileDetails?.FileName,
            };
            using (var stream = new MemoryStream())
            {
                creationDto.FileDetails!.CopyTo(stream);
                if (creationDto.FileDetails!.ContentType == MimeTypes.ApplicationPdf)
                {
                    picture.FileData = stream.ToArray();
                }
                else
                {
                    picture.FileData = ConvertImageToPdf(stream.ToArray()).ToArray();
                }

            }
            var receivePaymentPictureMapping = new ReceivePaymentPictureMapping()
            {
                Id = Guid.NewGuid(),
                PictureId = picture.Id,
                ReceivePaymentId = creationDto.ReceivePaymentAgainstSaleId,
            };
            await _unitOfWork.Repository<Picture>().AddAsync(picture);
            await _unitOfWork.Repository<ReceivePaymentPictureMapping>().AddAsync(receivePaymentPictureMapping);
            await _unitOfWork.SaveChangesAsync();
            return receivePaymentPictureMapping.Id;
        }
        private byte[] ConvertImageToPdf(byte[] imageBytes)
        {
            using (var ms = new MemoryStream())
            {
                // Define the page size and margins
                var pageSize = PageSize.A4;
                float margin = 25;

                // Create a new PDF document with the specified page size and margins
                using (var document = new Document(pageSize, margin, margin, margin, margin))
                {
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    try
                    {
                        // Convert the image byte array to an iTextSharp image
                        var image = iTextSharp.text.Image.GetInstance(imageBytes);

                        // Get the width and height of the page, considering the margins
                        float maxWidth = pageSize.Width - margin * 2;
                        float maxHeight = pageSize.Height - margin * 2;

                        // Calculate the scaling factor to maintain the aspect ratio and fit the image within the page
                        float widthScale = maxWidth / image.Width;
                        float heightScale = maxHeight / image.Height;
                        float scale = Math.Min(widthScale, heightScale);

                        // Scale the image proportionally
                        image.ScalePercent(scale * 100);

                        // Center the image on the page
                        //image.SetAbsolutePosition(
                        //    (pageSize.Width - image.ScaledWidth) / 2,
                        //    (pageSize.Height - image.ScaledHeight) / 2
                        //);
                        // Position the image at the top of the page, considering margins
                        image.SetAbsolutePosition(
                            (pageSize.Width - image.ScaledWidth) / 2,
                            pageSize.Height - margin - image.ScaledHeight
                        );
                        // Add the image to the PDF document
                        document.Add(image);
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions related to image processing
                        Console.WriteLine($"Error adding image to PDF: {ex.Message}");
                        throw;
                    }
                    finally
                    {
                        document.Close();
                    }
                }

                // Return the PDF byte array
                return ms.ToArray();
            }
        }
        public async Task<(Stream Stream, string FileName)?> GetFileById(Guid receivePaymentAgainstSaleId)
        {
            var receivePaymentPictureMapping = await _unitOfWork.Repository<ReceivePaymentPictureMapping>().TableNoTracking().FirstOrDefaultAsync(x => x.ReceivePaymentId == receivePaymentAgainstSaleId);
            if (receivePaymentPictureMapping == null) throw new BadRequestException("No file exists for this money receipt.");
            var file = await _unitOfWork.Repository<Picture>().TableNoTracking()
                          .FirstOrDefaultAsync(x => x.Id == receivePaymentPictureMapping!.PictureId);
            if (file == null)
            {
                throw new NotFoundResultException("File not found.");
            }
            //file.FileData is of type byte[] because of varbinary(MAX)
            return (new MemoryStream(file.FileData), file.FileName);
        }
    }
}
