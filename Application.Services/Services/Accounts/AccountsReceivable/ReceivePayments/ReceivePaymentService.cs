using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Accounts.AccountsReceivable.ReceivePayment;
using Application.Services.SearchRequestModels.Accounts.AccountsReceivable;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Accounts.AccountsReceivable.ReceivePayment;
using Application.Services.ViewModels.Purchase;
using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;
using static Application.Core.Constants.Permissions;
using static iTextSharp.text.pdf.AcroFields;

namespace Application.Services.Services.Accounts.AccountsReceivable.ReceivePayments
{
    public class ReceivePaymentService : BaseService<ReceivePayment, ReceivePaymentCreationDto, ReceivePaymentUpdateDto, ReceivePaymentRequestModel, ReceivePaymentViewModel>, IReceivePaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
        private readonly IAccountService _accountService;
        private readonly IConfiguration _configuration;
        private readonly ISmsService _smsService;
        private readonly IApiCaller _apiCaller;

        public ReceivePaymentService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService,
            IHubContext<BroadcastHub, IHubClient> hubContext, IAccountService accountService, IConfiguration configuration,
            ISmsService smsService, IApiCaller apiCaller) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _accountService = accountService;
            _configuration = configuration;
            _smsService = smsService;
            _apiCaller = apiCaller;
        }

        public async Task<ReceivePaymentAggregatorModel> PrepareReceivePaymentAggregatorModel(ReceivePaymentRequestModel receivePaymentRequest)
        {
            var receivePaymentQueryable = _unitOfWork.Repository<ReceivePayment>().TableNoTracking().Where(receivePaymentRequest.GetExpression());
            return new ReceivePaymentAggregatorModel
            {
                AggregatorTotalAmount = await receivePaymentQueryable.SumAsync(x => x.TotalAmount),

            };
        }

        public async Task<ReceivePaymentViewModel> GetByIdAsync(Guid id)
        {

            //test
            //var pictures = await _unitOfWork.Repository<Picture>().TableNoTracking().ToListAsync();
            //foreach (var picture in pictures)
            //{
            //    if (picture.FileData is null) continue;
            //    var extension = Path.GetExtension(picture.FileName)?.ToLowerInvariant();
            //    HashSet<string> validExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".apng", ".gif", ".svg", ".webp" };
            //    if (validExtensions.Contains(extension!.ToLower()))
            //    {
            //        var pdfByteArray = ConvertImageToPdf(picture);
            //        picture.FileData = pdfByteArray;
            //        await _unitOfWork.Repository<Picture>().UpdateAsync(picture);
            //    }

            //}
            //await _unitOfWork.SaveChangesAsync();
            //



            var receivePayment = await _unitOfWork.Repository<ReceivePayment>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.CostCenter).Include(x => x.Customer).Include(x => x.PaymentMode)
                .Include(x => x.ReceivePaymentDetails).ThenInclude(x => x.Account));
            if (receivePayment == null) throw new NotFoundResultException("Receive Payment Not Found With this Id");
            return _mapper.Map<ReceivePaymentViewModel>(receivePayment);
        }

        public new async Task<Guid> AddAsync(ReceivePaymentCreationDto receivePaymentCreationDto)
        {
            if (!string.IsNullOrEmpty(receivePaymentCreationDto.TransactionNumber))
            {
                var existingTransaction = await _unitOfWork.Repository<ReceivePayment>().TableNoTracking().AnyAsync(x => x.TransactionNumber == receivePaymentCreationDto.TransactionNumber);
                if (existingTransaction) throw new BadRequestException("This transaction number has already been utilized. Please try another one.");
            }
            //comment
            var financialYear = _workContext.GetCurrentFinancialYear();
            receivePaymentCreationDto.PaymentDate = receivePaymentCreationDto.PaymentDate.ToLocal();
            var receivePayment = _mapper.Map<ReceivePayment>(receivePaymentCreationDto);
            var count = _unitOfWork.Repository<ReceivePayment>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            receivePayment.Id = Guid.NewGuid();
            receivePayment.Code = "MR" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            receivePayment.FinancialYearId = financialYear.Id;
            receivePayment.Status = (int)ReceivePaymentStatus.Pending;
            var customer = await _unitOfWork.Repository<Account>().FindAsync(receivePaymentCreationDto.CustomerId);
            receivePayment.CustomerMarketingOfficerId = customer?.CustomerMarketingOfficerId;
            foreach (var item in receivePayment.ReceivePaymentDetails)
            {
                item.Id = Guid.NewGuid();
                item.ReceivePaymentId = receivePayment.Id;
                await _unitOfWork.Repository<ReceivePaymentDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<ReceivePayment>().AddAsync(receivePayment);
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, receivePayment.Id
                , "/accounts/accounts-receivable/money-receipt/" + receivePayment.Id, SecondaryPermissions.ReceivePayments.Check,
                "Receive Payment " + receivePayment.Code + " is ready for Check", (int)ReceivePaymentStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return receivePayment.Id;
        }

        public async Task<Guid> AddByDmsAppAsync(ReceivePaymentCreationDtoForDmsApp receivePaymentCreationDto)
        {
            if (!string.IsNullOrEmpty(receivePaymentCreationDto.TransactionNumber))
            {
                var existingTransaction = await _unitOfWork.Repository<ReceivePayment>().TableNoTracking().AnyAsync(x => x.TransactionNumber == receivePaymentCreationDto.TransactionNumber);
                if (existingTransaction) throw new BadRequestException("This transaction number has already been utilized. Please try another one.");
            }
            //comment
            var financialYear = _workContext.GetCurrentFinancialYear();
            receivePaymentCreationDto.PaymentDate = receivePaymentCreationDto.PaymentDate.ToLocal();
            var receivePayment = new ReceivePayment();
            var count = _unitOfWork.Repository<ReceivePayment>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            receivePayment.Id = Guid.NewGuid();
            receivePayment.Code = "MR" + financialYear.Code + "-" + count.ToString().PadLeft(7, '0');
            receivePayment.FinancialYearId = financialYear.Id;
            receivePayment.Status = (int)ReceivePaymentStatus.Pending;
            var customer = await _unitOfWork.Repository<Account>().FindAsync(receivePaymentCreationDto.CustomerId);
            receivePayment.CustomerMarketingOfficerId = customer?.CustomerMarketingOfficerId;
            //manual mapping entity from dto
            receivePayment.PaymentDate = receivePaymentCreationDto.PaymentDate;
            receivePayment.CustomerId = receivePaymentCreationDto.CustomerId;
            receivePayment.FundTransferTransactionTypeId = receivePaymentCreationDto.FundTransferTransactionTypeId;
            receivePayment.TransactionNumber = receivePaymentCreationDto.TransactionNumber;
            receivePayment.PaymentModeId = receivePaymentCreationDto.PaymentModeId;
            receivePayment.TotalAmount = receivePaymentCreationDto.TotalAmount;
            receivePayment.FeedSalesPurpose = receivePaymentCreationDto.FeedSalesPurpose;
            receivePayment.CreditRecoveryPurpose = receivePaymentCreationDto.CreditRecoveryPurpose;
            receivePayment.CostCenterId = receivePaymentCreationDto.CostCenterId;
            receivePayment.Remark = receivePaymentCreationDto.Remark;
            await _unitOfWork.Repository<ReceivePayment>().AddAsync(receivePayment);
            //
            var receivePaymentDetail = new ReceivePaymentDetail();
            receivePaymentDetail.Id = Guid.NewGuid();
            receivePaymentDetail.ReceivePaymentId = receivePayment.Id;
            receivePaymentDetail.AccountId = receivePaymentCreationDto.AccountId;
            receivePaymentDetail.AccountDescription = receivePaymentCreationDto.AccountDescription;
            receivePaymentDetail.Amount = receivePaymentCreationDto.Amount;
            await _unitOfWork.Repository<ReceivePaymentDetail>().AddAsync(receivePaymentDetail);
            await AddReceiptAsync(new ReceivePaymentPictureMappingCreationDto { FileDetails = receivePaymentCreationDto.FileDetails, ReceivePaymentId = receivePayment.Id });

            //var succeeded= await _apiCaller.AddReceiptAsync(new MoneyReceiptUploadDto { FileDetails = receivePaymentCreationDto.FileDetails, ReceivePaymentId = receivePayment.Id });
            //if (!succeeded) throw new BadRequestException("Picture Uplaod Failed");
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, receivePayment.Id
                , "/accounts/accounts-receivable/money-receipt/" + receivePayment.Id, SecondaryPermissions.ReceivePayments.Check,
                "Receive Payment " + receivePayment.Code + " is ready for Check", (int)ReceivePaymentStatus.Pending);
            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            //send notification end
            return receivePayment.Id;
        }

        public new async Task<Guid> UpdateAsync(ReceivePaymentUpdateDto receivePaymentUpdateDto)
        {
            var dbReceivePayment = await _unitOfWork.Repository<ReceivePayment>().FindAsync(receivePaymentUpdateDto.Id);
            if (!string.IsNullOrEmpty(receivePaymentUpdateDto.TransactionNumber))
            {
                if (receivePaymentUpdateDto.TransactionNumber.Equals(dbReceivePayment.TransactionNumber) == false)
                {
                    var existingTransaction = await _unitOfWork.Repository<ReceivePayment>().TableNoTracking().AnyAsync(x => x.TransactionNumber == receivePaymentUpdateDto.TransactionNumber);
                    if (existingTransaction) throw new BadRequestException("This transaction number has already been utilized. Please try another one.");
                }
            }
            //when date updated,convert it to local date
            if (receivePaymentUpdateDto.PaymentDate.Equals(dbReceivePayment.PaymentDate) == false)
            {
                receivePaymentUpdateDto.PaymentDate = receivePaymentUpdateDto.PaymentDate.ToLocal();
            }
            if (dbReceivePayment == null) throw new NotFoundResultException("ReceivePayment Not Found With this Id");
            var receivePayment = _mapper.Map(receivePaymentUpdateDto, dbReceivePayment);
            if (dbReceivePayment.Status == (int)ReceivePaymentStatus.Pending || dbReceivePayment.Status == (int)ReceivePaymentStatus.Checked)
            {
                await _unitOfWork.Repository<ReceivePayment>().UpdateAsync(receivePayment);
                foreach (var item in receivePayment.ReceivePaymentDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.ReceivePaymentId = receivePayment.Id;
                        await _unitOfWork.Repository<ReceivePaymentDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<ReceivePaymentDetail>().UpdateAsync(item);
                }
                //Delete for ReceivePaymentDetails
                if (!string.IsNullOrEmpty(receivePaymentUpdateDto.DeletedReceivePaymentDetailIds))
                {
                    foreach (var id in receivePaymentUpdateDto.DeletedReceivePaymentDetailIds.Split(',').Where(x => x != ""))
                    {
                        var receivePaymentDetail = await _unitOfWork.Repository<ReceivePaymentDetail>().FindAsync(new Guid(id));
                        receivePaymentDetail.Deleted = true;
                        await _unitOfWork.Repository<ReceivePaymentDetail>().UpdateAsync(receivePaymentDetail);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update.Only Pending or Checked Status Allowed to Update");
            return receivePayment.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var receivePayment = await _unitOfWork.Repository<ReceivePayment>().FindAsync(x => x.Id == id, x => x.Include(x => x.ReceivePaymentDetails));
            if (receivePayment == null) throw new NotFoundResultException("ReceivePayment Not Found With this Id");
            if (receivePayment.Status == (int)ReceivePaymentStatus.Pending)
            {
                foreach (var item in receivePayment.ReceivePaymentDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<ReceivePaymentDetail>().UpdateAsync(item);
                }
                receivePayment.Deleted = true;
                await _unitOfWork.Repository<ReceivePayment>().UpdateAsync(receivePayment);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete.Only Pending Status Allowed to Delete");
            return receivePayment.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var receivePayments = _unitOfWork.Repository<ReceivePayment>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await receivePayments.Where(x => x.Status == (int)ReceivePaymentStatus.Pending).CountAsync(),
                CheckedCount = await receivePayments.Where(x => x.Status == (int)ReceivePaymentStatus.Checked).CountAsync()
            };
            return response;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbReceivePayment = await _unitOfWork.Repository<ReceivePayment>().FindAsync(id);
            dbReceivePayment.Status = (int)ReceivePaymentStatus.Checked;
            dbReceivePayment.CheckedBy = _workContext.GetUserName();
            //update ReceivePayment
            await _unitOfWork.Repository<ReceivePayment>().UpdateAsync(dbReceivePayment);
            //remove pending notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbReceivePayment.Id
                   && x.Status == (int)ReceivePaymentStatus.Pending);
            //send notification for approval
            await _notificationService.SendNotificationAsync(_unitOfWork, dbReceivePayment.Id
                , "/accounts/accounts-receivable/money-receipt/" + dbReceivePayment.Id, SecondaryPermissions.ReceivePayments.Approve,
                "Receive Payment " + dbReceivePayment.Code + " is ready for Approval", (int)ReceivePaymentStatus.Checked);

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
            var dbInfo = await _unitOfWork.Repository<ReceivePayment>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Money Receipt not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Money Receipt does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<ReceivePayment>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)ReceivePaymentStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)ReceivePaymentStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Money Receipt already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbReceivePayment = await _unitOfWork.Repository<ReceivePayment>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.ReceivePaymentDetails));

            dbReceivePayment.Status = (int)ReceivePaymentStatus.Approved;
            dbReceivePayment.ApprovedBy = approvedBy;

            //update ReceivePayment
            await InsertTransaction(dbReceivePayment);
            await _unitOfWork.Repository<ReceivePayment>().UpdateAsync(dbReceivePayment);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbReceivePayment.Id
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
            var dbInfo = await _unitOfWork.Repository<ReceivePayment>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("ReceivePayment Not Found With this Id");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Unpost Denied: This Money Receipt does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)ReceivePaymentStatus.Checked && dbInfo.Status != (int)ReceivePaymentStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)ReceivePaymentStatus.Checked
                ? (int)ReceivePaymentStatus.Pending
                : (int)ReceivePaymentStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<ReceivePayment>()
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
            var dbReceivePayment = await _unitOfWork.Repository<ReceivePayment>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.ReceivePaymentDetails));
            dbReceivePayment.Status = newStatus;
            dbReceivePayment.UnpostedBy = unpostedBy;

            switch (dbInfo.Status)
            {
                case (int)ReceivePaymentStatus.Checked:
                    dbReceivePayment.CheckedBy = "";
                    break;
                case (int)ReceivePaymentStatus.Approved:
                    dbReceivePayment.ApprovedBy = "";
                    await DeleteTransaction(dbReceivePayment.Code);
                    break;
            }

            await _unitOfWork.Repository<ReceivePayment>().UpdateAsync(dbReceivePayment);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbReceivePayment.Status;
        }

        public virtual async Task<bool> SendToCustomerAsync(Guid id)
        {
            var receivePayment = await _unitOfWork.Repository<ReceivePayment>().TableNoTracking().Include(x => x.Customer).SingleOrDefaultAsync(x => x.Id == id);
            if (string.IsNullOrWhiteSpace(receivePayment!.Customer.ContactNo))
            {
                throw new BadRequestException("Customer Contact No Not Found");
            }
            //send email
            //if (!string.IsNullOrWhiteSpace(saleInvoice!.Customer.Email))
            //{

            //    var mailRequest = new MailRequest()
            //    {
            //        ToEmail = saleInvoice.Customer.Email,
            //        Subject = "Your APFEED Invoice Is Here",
            //        Body = meassage
            //    };
            //    await _mailService.SendEmailFromDefaultEmailAddressAsync(mailRequest);
            //}
            //send sms
            bool isSuccess = false;
            var isSmsSendAllowed = _configuration.GetValue<bool>("SmsSend:Allowed");
            if (isSmsSendAllowed)
            {
                var SmsSendBaseUrl = _configuration.GetValue<string>("SmsSend:BaseUrl");
                var meassage = $"Deposited TK. {receivePayment.TotalAmount} to AP FEED.Thanks." +
                        $"More Details:{SmsSendBaseUrl}/mr/{receivePayment.Id}";
                await _smsService.Send(receivePayment.Customer.ContactNo, meassage);
                receivePayment!.Status = (int)ReceivePaymentStatus.Sent_To_Customer;
                await _unitOfWork.Repository<ReceivePayment>().UpdateAsync(receivePayment);
                isSuccess = await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Sms Send not Allowed!!");
            return isSuccess;

        }

        private async Task InsertTransaction(ReceivePayment receivePayment)
        {
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var counter = 0M;
            foreach (var item in receivePayment.ReceivePaymentDetails)
            {
                var accountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == item.AccountId).Select(x => x.Name).FirstOrDefaultAsync();
                if (counter > 0)
                {
                    contraAccountIdsForCredit.Append(", ");
                    contraAccountNamesForCredit.Append(", ");
                }
                contraAccountNamesForCredit.Append(accountName);
                contraAccountIdsForCredit.Append(item.AccountId.ToString());
                counter++;
            }
            var customerAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == receivePayment.CustomerId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(customerAccountName);
            contraAccountIdsForDebit.Append(receivePayment.CustomerId.ToString());

            if (receivePayment.ReceivePaymentDetails.Sum(x => x.Amount) != receivePayment.TotalAmount) throw new BadRequestException("Debit Credit Not Equal!");
            //cash/back account debit
            foreach (var item in receivePayment.ReceivePaymentDetails)
            {
                await _accountService.HitAccount(_unitOfWork, receivePayment.CostCenterId, receivePayment.Code, receivePayment.Remark,
                item.AccountId, item.Amount, 0.0M, receivePayment.PaymentDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
            }
            //customer account credit
            await _accountService.HitAccount(_unitOfWork, receivePayment.CostCenterId, receivePayment.Code, receivePayment.Remark,
                receivePayment.CustomerId, 0.0M, receivePayment.TotalAmount, receivePayment.PaymentDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.CustomerReceipt, 0, 0m, receivePayment.PaymentModeId);
        }

        private async Task DeleteTransaction(string code)
        {
            var receivePayment = await _unitOfWork.Repository<ReceivePayment>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.Code == code);
            if (receivePayment is null) throw new NotFoundResultException("Supplier Payment Not Found With this Code No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == receivePayment.Code);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }
        }
        private async Task AddReceiptAsync(ReceivePaymentPictureMappingCreationDto creationDto)
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
                ReceivePaymentId = creationDto.ReceivePaymentId,
            };
            await _unitOfWork.Repository<Picture>().AddAsync(picture);
            await _unitOfWork.Repository<ReceivePaymentPictureMapping>().AddAsync(receivePaymentPictureMapping);
        }
        public async Task<Guid> PostSingleFileAsync(ReceivePaymentPictureMappingCreationDto creationDto)
        {
            if (creationDto.FileDetails is null)
                throw new BadRequestException("Please Select File!");

            var dbReceivePayment = await _unitOfWork.Repository<ReceivePayment>().FindAsync(x => x.Id == creationDto.ReceivePaymentId);
            if (dbReceivePayment.Status >= (int)ReceivePaymentStatus.Approved) throw new BadRequestException("You won't be able to upload Image because this money receipt has already been approved");
            //Previous file deletion Start
            var existingReceivePaymentPictureMapping = await _unitOfWork.Repository<ReceivePaymentPictureMapping>().TableNoTracking().FirstOrDefaultAsync(x => x.ReceivePaymentId == creationDto.ReceivePaymentId);
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
                ReceivePaymentId = creationDto.ReceivePaymentId,
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
        public async Task AddReceivePaymentPictureMappingAsync(List<ReceivePaymentPictureMappingCreationDto> receivePaymentPictureMappingCreationDto)
        {
            try
            {
                foreach (var file in receivePaymentPictureMappingCreationDto)
                {
                    var picture = new Picture()
                    {
                        Id = Guid.NewGuid(),
                        FileName = file?.FileDetails?.FileName,
                    };
                    var extension = Path.GetExtension(picture.FileName)?.ToLowerInvariant();
                    HashSet<string> validExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".apng", ".gif", ".svg", ".webp" };
                    if (validExtensions.Contains(extension!.ToLower()))
                    {
                        using (var stream = new MemoryStream())
                        {
                            file?.FileDetails?.CopyTo(stream);
                            picture.FileData = stream.ToArray();
                        }
                        await _unitOfWork.Repository<Picture>().AddAsync(picture);

                        var receivePaymentPictureMapping = new ReceivePaymentPictureMapping()
                        {
                            Id = Guid.NewGuid(),
                            PictureId = picture.Id,
                            ReceivePaymentId = file!.ReceivePaymentId,
                        };
                        await _unitOfWork.Repository<ReceivePaymentPictureMapping>().AddAsync(receivePaymentPictureMapping);
                    }
                    else throw new BadRequestException("Please, Upload Valid Image");
                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(Stream Stream, string FileName)?> GetFileById(Guid receivePaymentId)
        {
            var receivePaymentPictureMapping = await _unitOfWork.Repository<ReceivePaymentPictureMapping>().TableNoTracking().FirstOrDefaultAsync(x => x.ReceivePaymentId == receivePaymentId);
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
