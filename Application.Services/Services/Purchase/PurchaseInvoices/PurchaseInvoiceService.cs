using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Purchase.PurchaseInvoice;
using Application.Services.SearchRequestModels.Purchase;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Purchase.PurchaseInvoice;
using Application.Services.ViewModels.Report.Purchase;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text;
using static Application.Core.Constants.Permissions;

namespace Application.Services.Services.Purchase.PurchaseInvoices
{
    public class PurchaseInvoiceService : BaseService<PurchaseInvoice, PurchaseInvoiceCreationDto, PurchaseInvoiceUpdateDto, PurchaseInvoiceRequestModel, PurchaseInvoiceViewModel>, IPurchaseInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IAccountService _accountService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;

        public PurchaseInvoiceService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, IAccountService accountService
            , INotificationService notificationService, IHubContext<BroadcastHub, IHubClient> hubContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _accountService = accountService;
        }

        public async Task<PurchaseInvoiceAggregatorModel> PreparePurchaseInvoiceAggregatorModel(PurchaseInvoiceRequestModel purchaseInvoiceRequest)
        {
            var purchaseInvoiceQueryable = _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().Where(purchaseInvoiceRequest.GetExpression());
            return new PurchaseInvoiceAggregatorModel
            {
                AggregatorSubtotal = await purchaseInvoiceQueryable.SumAsync(x => x.Subtotal),
                AggregatorDiscount = await purchaseInvoiceQueryable.SumAsync(x => x.Discount),
                AggregatorTotal = await purchaseInvoiceQueryable.SumAsync(x => x.Total)
            };
        }

        public async Task<PurchaseInvoiceViewModel> GetByIdAsync(Guid id)
        {
            var purchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Supplier).Include(x => x.Store)
                .Include(x => x.PurchaseInvoiceDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (purchaseInvoice == null) throw new NotFoundResultException("Purchase Invoice Not Found With this Id");
            return _mapper.Map<PurchaseInvoiceViewModel>(purchaseInvoice);
        }

        public new async Task<Guid> AddAsync(PurchaseInvoiceCreationDto purchaseInvoiceCreationDto)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            if (purchaseInvoiceCreationDto.IsImportPurchase)
            {
                var purchaseInvoiceExistsAgainstPO = await _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().AnyAsync(x => x.Ponumber.Contains(purchaseInvoiceCreationDto.Ponumber!));
                if (purchaseInvoiceExistsAgainstPO) throw new BadRequestException("You cannot create another purchase invoice for PO #" + purchaseInvoiceCreationDto.Ponumber + " as one already exists.");
            }
            foreach (var grnno in purchaseInvoiceCreationDto.Grnno!.Split(","))
            {
                var purchaseInvoiceExists = await _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().AnyAsync(x => x.Grnno.Contains(grnno));
                if (purchaseInvoiceExists) throw new BadRequestException("You have already created purchase invoice using this grn no #" + grnno);
            }
            purchaseInvoiceCreationDto.InvoiceDate = purchaseInvoiceCreationDto.InvoiceDate.ToLocal();
            var purchaseInvoice = _mapper.Map<PurchaseInvoice>(purchaseInvoiceCreationDto);
            var count = _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).IgnoreQueryFilters().Count() + 1;
            purchaseInvoice.Id = Guid.NewGuid();
            purchaseInvoice.PurchaseInvoiceNo = "PI" + financialYear.Code + "-" + count.ToString().PadLeft(6, '0');
            purchaseInvoice.FinancialYearId = financialYear.Id;
            purchaseInvoice.Status = (int)PurchaseInvoiceStatus.Pending;
            foreach (var item in purchaseInvoice.PurchaseInvoiceDetails)
            {
                item.Id = Guid.NewGuid();
                item.PurchaseInvoiceId = purchaseInvoice.Id;
                await _unitOfWork.Repository<PurchaseInvoiceDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<PurchaseInvoice>().AddAsync(purchaseInvoice);
            //when invoiced,then change grn status
            if (!string.IsNullOrWhiteSpace(purchaseInvoice.Grnno))
            {
                foreach (var grnno in purchaseInvoice.Grnno.Split(","))
                {
                    var goodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().TableNoTracking().SingleOrDefaultAsync(x => x.Grnno == grnno);
                    goodsReceiveNote!.Status = (int)GRNStatus.Invoice_Generated;
                    await _unitOfWork.Repository<GoodsReceiveNote>().UpdateAsync(goodsReceiveNote);
                }

            }
            //when linked with advance supplier payment
            //if (!string.IsNullOrWhiteSpace(purchaseInvoice.SupplierPaymentCode))
            //{
            //    foreach (var paymentCode in purchaseInvoice.SupplierPaymentCode.Split(","))
            //    {
            //        var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().SingleOrDefaultAsync(x => x.Code == paymentCode);
            //        supplierPayment!.Status = (int)SupplierPaymentStatus.Advance_Amount_Linked_With_Purhchase_invoice;
            //        await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(supplierPayment);
            //    }

            //}

            //when linked with advance supplier payment
            if (!string.IsNullOrWhiteSpace(purchaseInvoice.SupplierPaymentCode))
            {
                var advanceAmount = purchaseInvoice.AdvancePaymentAmount;
                foreach (var paymentCode in purchaseInvoice.SupplierPaymentCode.Split(","))
                {
                    var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().SingleOrDefaultAsync(x => x.Code == paymentCode);
                    var payment = 0m;
                    var availablePayment = supplierPayment!.TotalAmount - supplierPayment.UsedAmountInPurchaseInvoice;
                    if (availablePayment <= advanceAmount && availablePayment != 0m)
                    {
                        payment = availablePayment;
                        supplierPayment.UsedAmountInPurchaseInvoice += payment;
                        advanceAmount -= availablePayment;
                    }
                    else if (availablePayment > advanceAmount && availablePayment != 0m)
                    {
                        payment = advanceAmount;
                        supplierPayment.UsedAmountInPurchaseInvoice += advanceAmount;
                        advanceAmount = 0m;
                    }

                    if (supplierPayment.UsedAmountInPurchaseInvoice == supplierPayment!.TotalAmount)
                        supplierPayment!.Status = (int)SupplierPaymentStatus.Fully_Used_With_Invoice;
                    else if (supplierPayment.UsedAmountInPurchaseInvoice > 0 && supplierPayment.UsedAmountInPurchaseInvoice < supplierPayment.TotalAmount)
                        supplierPayment!.Status = (int)SupplierPaymentStatus.Partially_Used_With_Invoice;
                    await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(supplierPayment);
                    var purchaseInvoiceSupplierPaymentMapping = new PurchaseInvoiceSupplierPaymentMapping
                    {
                        Id = Guid.NewGuid(),
                        PurchaseInvoiceId = purchaseInvoice.Id,
                        SupplierPaymentId = supplierPayment.Id,
                        PaymentAmount = payment,
                    };
                    await _unitOfWork.Repository<PurchaseInvoiceSupplierPaymentMapping>().AddAsync(purchaseInvoiceSupplierPaymentMapping);
                }

            }
            //send notification
            await _notificationService.SendNotificationAsync(_unitOfWork, purchaseInvoice.Id
                , "/accounts/accounts-payable/purchase-invoice/" + purchaseInvoice.Id, Permissions.PurchaseInvoices.Check,
                "Purchase Invoice " + purchaseInvoice.PurchaseInvoiceNo + " is ready for Approval", (int)PurchaseInvoiceStatus.Pending);

            await _unitOfWork.SaveChangesAsync();
            await _hubContext.Clients.All.BroadcastMessage();
            return purchaseInvoice.Id;
        }

        public new async Task<Guid> UpdateAsync(PurchaseInvoiceUpdateDto purchaseInvoiceUpdateDto)
        {
            var dbPurchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(purchaseInvoiceUpdateDto.Id);
            if (dbPurchaseInvoice == null) throw new NotFoundResultException("PurchaseInvoice Not Found With this Id");
            var existingAdvanceSupplierPayment = dbPurchaseInvoice.AdvancePaymentAmount;
            var currentAdvanceSupplierPayment = purchaseInvoiceUpdateDto.AdvancePaymentAmount;
            var existingSupplierPaymentCode = dbPurchaseInvoice.SupplierPaymentCode;
            var currentSupplierPaymentCode = purchaseInvoiceUpdateDto.SupplierPaymentCode;
            //when date updated,convert it to local date
            if (purchaseInvoiceUpdateDto.InvoiceDate.Equals(dbPurchaseInvoice.InvoiceDate) == false)
            {
                purchaseInvoiceUpdateDto.InvoiceDate = purchaseInvoiceUpdateDto.InvoiceDate.ToLocal();
            }
            var purchaseInvoice = _mapper.Map(purchaseInvoiceUpdateDto, dbPurchaseInvoice);
            if (dbPurchaseInvoice.Status == (int)PurchaseInvoiceStatus.Pending || dbPurchaseInvoice.Status == (int)PurchaseInvoiceStatus.Checked)
            {

                await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(purchaseInvoice);
                foreach (var item in purchaseInvoice.PurchaseInvoiceDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.PurchaseInvoiceId = purchaseInvoice.Id;
                        await _unitOfWork.Repository<PurchaseInvoiceDetail>().AddAsync(item);
                    }
                    else await _unitOfWork.Repository<PurchaseInvoiceDetail>().UpdateAsync(item);
                }
                //Delete for PurchaseInvoiceDetails
                if (!string.IsNullOrEmpty(purchaseInvoiceUpdateDto.DeletedPurchaseInvoiceDetailIds))
                {
                    foreach (var id in purchaseInvoiceUpdateDto.DeletedPurchaseInvoiceDetailIds.Split(',').Where(x => x != ""))
                    {
                        var purchaseInvoiceDetail = await _unitOfWork.Repository<PurchaseInvoiceDetail>().FindAsync(new Guid(id));
                        purchaseInvoiceDetail.Deleted = true;
                        await _unitOfWork.Repository<PurchaseInvoiceDetail>().UpdateAsync(purchaseInvoiceDetail);
                    }
                }
                //when linked with advance supplier payment
                //if (!string.IsNullOrWhiteSpace(purchaseInvoice.SupplierPaymentCode))
                //{
                //    foreach (var paymentCode in purchaseInvoice.SupplierPaymentCode.Split(","))
                //    {
                //        var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().SingleOrDefaultAsync(x => x.Code == paymentCode);
                //        supplierPayment!.Status = (int)SupplierPaymentStatus.Advance_Amount_Linked_With_Purhchase_invoice;
                //        await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(supplierPayment);
                //    }

                //}


                if (existingAdvanceSupplierPayment != currentAdvanceSupplierPayment && existingSupplierPaymentCode != currentSupplierPaymentCode)
                {
                    //Delete PurchaseInvoiceSupplierPaymentMapping data against this PurchaseInvoiceId
                    await DeletePurchaseInvoiceSupplierPaymentMapping(purchaseInvoice.Id);

                    //when linked with advance supplier payment
                    if (!string.IsNullOrWhiteSpace(purchaseInvoice.SupplierPaymentCode))
                    {
                        var advanceAmount = purchaseInvoice.AdvancePaymentAmount;
                        foreach (var paymentCode in purchaseInvoice.SupplierPaymentCode.Split(","))
                        {
                            var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().SingleOrDefaultAsync(x => x.Code == paymentCode);
                            var payment = 0m;
                            var availablePayment = supplierPayment!.TotalAmount - supplierPayment.UsedAmountInPurchaseInvoice;
                            if (availablePayment <= advanceAmount && availablePayment != 0m)
                            {
                                payment = availablePayment;
                                supplierPayment.UsedAmountInPurchaseInvoice += payment;
                                advanceAmount -= availablePayment;
                            }
                            else if (availablePayment > advanceAmount && availablePayment != 0m)
                            {
                                payment = advanceAmount;
                                supplierPayment.UsedAmountInPurchaseInvoice += advanceAmount;
                                advanceAmount = 0m;
                            }

                            if (supplierPayment.UsedAmountInPurchaseInvoice == supplierPayment!.TotalAmount)
                                supplierPayment!.Status = (int)SupplierPaymentStatus.Fully_Used_With_Invoice;
                            else if (supplierPayment.UsedAmountInPurchaseInvoice > 0 && supplierPayment.UsedAmountInPurchaseInvoice < supplierPayment.TotalAmount)
                                supplierPayment!.Status = (int)SupplierPaymentStatus.Partially_Used_With_Invoice;
                            await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(supplierPayment);
                            var purchaseInvoiceSupplierPaymentMapping = new PurchaseInvoiceSupplierPaymentMapping
                            {
                                Id = Guid.NewGuid(),
                                PurchaseInvoiceId = purchaseInvoice.Id,
                                SupplierPaymentId = supplierPayment.Id,
                                PaymentAmount = payment,
                            };
                            await _unitOfWork.Repository<PurchaseInvoiceSupplierPaymentMapping>().AddAsync(purchaseInvoiceSupplierPaymentMapping);
                        }

                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to Update.Only Pending or Checked Status Allowed to Update");
            return purchaseInvoice.Id;
        }

        private async Task DeletePurchaseInvoiceSupplierPaymentMapping(Guid purchaseInvoiceId)
        {
            var purchaseInvoiceSupplierPaymentMappingData = _unitOfWork.Repository<PurchaseInvoiceSupplierPaymentMapping>().TableNoTracking().Where(x => x.PurchaseInvoiceId == purchaseInvoiceId);
            if (purchaseInvoiceSupplierPaymentMappingData.Any())
            {
                foreach (var purchaseInvoiceSupplierPaymentMapping in purchaseInvoiceSupplierPaymentMappingData)
                {
                    await _unitOfWork.Repository<PurchaseInvoiceSupplierPaymentMapping>().DeleteAsync(purchaseInvoiceSupplierPaymentMapping.Id);
                }
            }
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var purchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(x => x.Id == id, x => x.Include(x => x.PurchaseInvoiceDetails));
            if (purchaseInvoice == null) throw new NotFoundResultException("PurchaseInvoice Not Found With this Id");
            if (purchaseInvoice.Status == (int)PurchaseInvoiceStatus.Pending)
            {
                foreach (var item in purchaseInvoice.PurchaseInvoiceDetails.ToList())
                {
                    await _unitOfWork.Repository<PurchaseInvoiceDetail>().DeleteAsync(item);
                }
                //reverted Purchase Invoice Status
                if (!string.IsNullOrWhiteSpace(purchaseInvoice.Grnno))
                {
                    foreach (var grnno in purchaseInvoice.Grnno.Split(","))
                    {
                        var goodsReceiveNote = await _unitOfWork.Repository<GoodsReceiveNote>().TableNoTracking().SingleOrDefaultAsync(x => x.Grnno == grnno);
                        goodsReceiveNote!.Status = (int)GRNStatus.Approved;
                        await _unitOfWork.Repository<GoodsReceiveNote>().UpdateAsync(goodsReceiveNote);
                    }

                }
                //reverted supplier payment status
                //if (!string.IsNullOrWhiteSpace(purchaseInvoice.SupplierPaymentCode))
                //{
                //    foreach (var paymentCode in purchaseInvoice.SupplierPaymentCode.Split(","))
                //    {
                //        var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().SingleOrDefaultAsync(x => x.Code == paymentCode);
                //        supplierPayment!.Status = (int)SupplierPaymentStatus.Approved;
                //        await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(supplierPayment);
                //    }

                //}
                //reverted supplier payment status
                //if (!string.IsNullOrWhiteSpace(purchaseInvoice.SupplierPaymentCode))
                //{
                //    foreach (var paymentCode in purchaseInvoice.SupplierPaymentCode.Split(","))
                //    {
                //        var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().SingleOrDefaultAsync(x => x.Code == paymentCode);
                //        supplierPayment!.Status = (int)SupplierPaymentStatus.Approved;
                //        await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(supplierPayment);
                //    }

                //}

                //reverted supplier payment status
                if (!string.IsNullOrWhiteSpace(purchaseInvoice.SupplierPaymentCode))
                {
                    foreach (var paymentCode in purchaseInvoice.SupplierPaymentCode.Split(","))
                    {
                        var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().SingleOrDefaultAsync(x => x.Code == paymentCode);
                        var purchaseInvoiceSupplierPaymentMapping = await _unitOfWork.Repository<PurchaseInvoiceSupplierPaymentMapping>().FindAsync(x => x.PurchaseInvoiceId == id && x.SupplierPaymentId == supplierPayment!.Id);
                        if (supplierPayment!.UsedAmountInPurchaseInvoice - purchaseInvoiceSupplierPaymentMapping.PaymentAmount == 0m)
                        {
                            supplierPayment!.Status = (int)SupplierPaymentStatus.Approved;
                            supplierPayment.UsedAmountInPurchaseInvoice -= purchaseInvoiceSupplierPaymentMapping.PaymentAmount;
                        }
                        else if (supplierPayment!.UsedAmountInPurchaseInvoice - purchaseInvoiceSupplierPaymentMapping.PaymentAmount > 0m)
                        {
                            supplierPayment!.Status = (int)SupplierPaymentStatus.Partially_Used_With_Invoice;
                            supplierPayment.UsedAmountInPurchaseInvoice -= purchaseInvoiceSupplierPaymentMapping.PaymentAmount;
                        }
                        await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(supplierPayment);
                        await _unitOfWork.Repository<PurchaseInvoiceSupplierPaymentMapping>().DeleteAsync(purchaseInvoiceSupplierPaymentMapping.Id);
                    }

                }
                purchaseInvoice.SupplierPaymentCode = "";
                purchaseInvoice.NetPayable = purchaseInvoice.NetPayable + purchaseInvoice.AdvancePaymentAmount;
                purchaseInvoice.AdvancePaymentAmount = 0m;

                await _unitOfWork.Repository<PurchaseInvoice>().DeleteAsync(purchaseInvoice);

                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete.Only Pending Status Allowed to Delete");
            return purchaseInvoice.Id;
        }

        //public async Task<Guid> ResetAssociatedAdvanceAmountAsync(Guid id)
        //{
        //    var purchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(x => x.Id == id);
        //    //reverted supplier payment status
        //    if (!string.IsNullOrWhiteSpace(purchaseInvoice.SupplierPaymentCode))
        //    {
        //        foreach (var paymentCode in purchaseInvoice.SupplierPaymentCode.Split(","))
        //        {
        //            var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().SingleOrDefaultAsync(x => x.Code == paymentCode);
        //            supplierPayment!.Status = (int)SupplierPaymentStatus.Approved;
        //            await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(supplierPayment);
        //        }

        //    }
        //    purchaseInvoice.SupplierPaymentCode = "";
        //    purchaseInvoice.NetPayable = purchaseInvoice.NetPayable + purchaseInvoice.AdvancePaymentAmount;
        //    purchaseInvoice.AdvancePaymentAmount = 0m;
        //    await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(purchaseInvoice);
        //    var isSuccessfull = await _unitOfWork.SaveChangesAsync();
        //    return purchaseInvoice.Id;
        //}
        public async Task<Guid> ResetAssociatedAdvanceAmountAsync(Guid id)
        {
            var purchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(x => x.Id == id);
            //reverted supplier payment status
            if (!string.IsNullOrWhiteSpace(purchaseInvoice.SupplierPaymentCode))
            {
                foreach (var paymentCode in purchaseInvoice.SupplierPaymentCode.Split(","))
                {
                    var supplierPayment = await _unitOfWork.Repository<SupplierPayment>().TableNoTracking().SingleOrDefaultAsync(x => x.Code == paymentCode);
                    var purchaseInvoiceSupplierPaymentMapping = await _unitOfWork.Repository<PurchaseInvoiceSupplierPaymentMapping>().FindAsync(x => x.PurchaseInvoiceId == id && x.SupplierPaymentId == supplierPayment!.Id);
                    if (supplierPayment!.UsedAmountInPurchaseInvoice - purchaseInvoiceSupplierPaymentMapping.PaymentAmount == 0m)
                    {
                        supplierPayment!.Status = (int)SupplierPaymentStatus.Approved;
                        supplierPayment.UsedAmountInPurchaseInvoice -= purchaseInvoiceSupplierPaymentMapping.PaymentAmount;
                    }
                    else if (supplierPayment!.UsedAmountInPurchaseInvoice - purchaseInvoiceSupplierPaymentMapping.PaymentAmount > 0m)
                    {
                        supplierPayment!.Status = (int)SupplierPaymentStatus.Partially_Used_With_Invoice;
                        supplierPayment.UsedAmountInPurchaseInvoice -= purchaseInvoiceSupplierPaymentMapping.PaymentAmount;
                    }
                    await _unitOfWork.Repository<SupplierPayment>().UpdateAsync(supplierPayment);
                    await _unitOfWork.Repository<PurchaseInvoiceSupplierPaymentMapping>().DeleteAsync(purchaseInvoiceSupplierPaymentMapping.Id);
                }

            }
            purchaseInvoice.SupplierPaymentCode = "";
            purchaseInvoice.NetPayable = purchaseInvoice.NetPayable + purchaseInvoice.AdvancePaymentAmount;
            purchaseInvoice.AdvancePaymentAmount = 0m;
            await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(purchaseInvoice);
            await _unitOfWork.SaveChangesAsync();
            return purchaseInvoice.Id;
        }

        public virtual async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var purchaseInvoices = _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await purchaseInvoices.Where(x => x.Status == (int)PurchaseInvoiceStatus.Pending).CountAsync(),
                CheckedCount = await purchaseInvoices.Where(x => x.Status == (int)PurchaseInvoiceStatus.Checked).CountAsync()
            };
            return response;

        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbPurchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(id);
            if (dbPurchaseInvoice.Status == (int)PurchaseInvoiceStatus.Pending)
            {
                dbPurchaseInvoice.Status = (int)PurchaseInvoiceStatus.Checked;
                dbPurchaseInvoice.CheckedBy = _workContext.GetUserName();
                await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(dbPurchaseInvoice);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPurchaseInvoice.Id
                       && x.Status == (int)PurchaseInvoiceStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbPurchaseInvoice.Id
                    , "/accounts/accounts-payable/purchase-invoice/" + dbPurchaseInvoice.Id, Permissions.PurchaseInvoices.Approve,
                    "Purchase Invoice " + dbPurchaseInvoice.PurchaseInvoiceNo + " is ready for Approval", (int)PurchaseInvoiceStatus.Checked);
                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Purchase Invoice Status has already been checked by " + dbPurchaseInvoice?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var financialYear = _workContext.GetCurrentFinancialYear();
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<PurchaseInvoice>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Purchase Invoice not found.");

            if (financialYear.Id != dbInfo.FinancialYearId)
                throw new BadRequestException("Approval Denied: This Purchase Invoice does not belong to the current financial year.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<PurchaseInvoice>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)PurchaseInvoiceStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)PurchaseInvoiceStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Purchase Invoice already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbPurchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>()
                .FindAsync(x => x.Id == id, x => x.Include(x => x.PurchaseInvoiceDetails).ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));

            dbPurchaseInvoice.Status = (int)PurchaseInvoiceStatus.Approved;
            dbPurchaseInvoice.ApprovedBy = approvedBy;

            await InsertTransaction(dbPurchaseInvoice);
            await InsertSupplierTransactionAgainstPO(dbPurchaseInvoice);
            foreach (var item in dbPurchaseInvoice.PurchaseInvoiceDetails)
            {
                item.Product = null;
            }
            await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(dbPurchaseInvoice);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbPurchaseInvoice.Id
                   && x.Status == (int)PurchaseInvoiceStatus.Checked);

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
            var dbInfo = await _unitOfWork.Repository<PurchaseInvoice>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.FinancialYearId, x.Status, x.IsLcAdjusted })
                .SingleOrDefaultAsync();

            if (dbInfo == null) throw new NotFoundResultException("Purchase Invoice not found.");
            if (financialYear.Id != dbInfo.FinancialYearId) throw new BadRequestException("Unpost Denied: This Purchase Invoice does not belong to the current financial year.");
            if (dbInfo.Status == (int)PurchaseInvoiceStatus.Approved && dbInfo.IsLcAdjusted) throw new BadRequestException("This purchase Invoice cannot be unposted due to its association with an LC Adjustment.");
            if (dbInfo.Status != fromStatus) throw new BadRequestException("Purchase Invoice status has been changed by another user. Please refresh and try again.");
            if (dbInfo.Status != (int)PurchaseInvoiceStatus.Checked && dbInfo.Status != (int)PurchaseInvoiceStatus.Approved)
                throw new BadRequestException("Only Checked or Approved Purchase Invoice can be unposted.");

            var newStatus = dbInfo.Status == (int)PurchaseInvoiceStatus.Approved
                ? (int)PurchaseInvoiceStatus.Checked
                : (int)PurchaseInvoiceStatus.Pending;

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Atomic update with status predicate
            int rowsUpdated = await _unitOfWork.Repository<PurchaseInvoice>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                    .SetProperty(x => x.UnpostedBy, _workContext.GetUserName()));

            if (rowsUpdated == 0) throw new BadRequestException("Purchase Invoice status was already modified by another request. Please refresh and try again.");

            // Reload full entity with tracking inside transaction
            var dbPurchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().FindAsync(id);

            dbPurchaseInvoice.Status = newStatus;
            dbPurchaseInvoice.UnpostedBy = _workContext.GetUserName();

            switch (dbInfo.Status)
            {
                case (int)PurchaseInvoiceStatus.Checked:
                    dbPurchaseInvoice.CheckedBy = "";
                    break;
                case (int)PurchaseInvoiceStatus.Approved:
                    dbPurchaseInvoice.ApprovedBy = "";
                    await DeleteTransaction(dbPurchaseInvoice.PurchaseInvoiceNo);
                    await RemoveSupplierTransactionAgainstPO(dbPurchaseInvoice);
                    break;
            }

            await _unitOfWork.Repository<PurchaseInvoice>().UpdateAsync(dbPurchaseInvoice);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbPurchaseInvoice.Status;
        }

        private async Task InsertTransaction(PurchaseInvoice purchaseInvoice)
        {
            StringBuilder sb = new StringBuilder();
            //sb.Append("Purchase-");
            sb.Append(purchaseInvoice?.Ponumber);
            var transactionQty = 0;
            var transactionQtyValue = 0m;
            foreach (var item in purchaseInvoice!.PurchaseInvoiceDetails)
            {
                transactionQty += item.Grnquantity;
                transactionQtyValue += (item.Grnquantity * item.Rate);
                sb.Append(" ");
                sb.Append(item?.Product?.Name);
                sb.Append(", ");
                sb.Append(item?.Grnquantity);
                sb.Append(" ");
                sb.Append(item?.Product?.MeasurementUnit?.Name);
                sb.Append(" @");
                sb.Append(item?.Rate);
            }
            StringBuilder contraAccountNamesForCredit = new();
            StringBuilder contraAccountIdsForCredit = new();
            var purchaseClearingAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.PurchaseClearingAccount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForCredit.Append(purchaseClearingAccountName);
            contraAccountIdsForCredit.Append(AccountHeadConstants.PurchaseClearingAccount);

            StringBuilder contraAccountNamesForDebit = new();
            StringBuilder contraAccountIdsForDebit = new();
            var supplierAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == purchaseInvoice.SupplierId).Select(x => x.Name).FirstOrDefaultAsync();
            contraAccountNamesForDebit.Append(supplierAccountName);
            contraAccountIdsForDebit.Append(purchaseInvoice.SupplierId.ToString());

            if (purchaseInvoice.IsImportPurchase)
            {
                var purchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().Include(x => x.PurchaseOrderDetails).SingleOrDefaultAsync(x => x.Ponumber == purchaseInvoice.Ponumber);
                if (purchaseOrder is null) throw new NotFoundResultException("Purchase Order not found associated with this Purchase Invoice " + purchaseInvoice.PurchaseInvoiceNo);
                // for purchase price variance
                var priceVariance = purchaseOrder.Total - purchaseInvoice.Subtotal;
                if (priceVariance > 0)
                {
                    var purchasePriceVarianceAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.PurchasePriceVariance.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForCredit.Append(", ");
                    contraAccountNamesForCredit.Append(purchasePriceVarianceAccountName);
                    contraAccountIdsForCredit.Append(", ");
                    contraAccountIdsForCredit.Append(AccountHeadConstants.PurchasePriceVariance);
                }
                if (priceVariance < 0)
                {
                    var purchasePriceVarianceAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.PurchasePriceVariance.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForDebit.Append(", ");
                    contraAccountNamesForDebit.Append(purchasePriceVarianceAccountName);
                    contraAccountIdsForDebit.Append(", ");
                    contraAccountIdsForDebit.Append(AccountHeadConstants.PurchasePriceVariance);
                }
                //for landed cost variance
                var variance = purchaseOrder.AdditionalLandedCost - purchaseInvoice.AdditionalLandedCost;
                if (variance > 0)
                {
                    var purchasePriceVarianceAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.PurchasePriceVariance.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForCredit.Append(", ");
                    contraAccountNamesForCredit.Append(purchasePriceVarianceAccountName);
                    contraAccountIdsForCredit.Append(", ");
                    contraAccountIdsForCredit.Append(AccountHeadConstants.PurchasePriceVariance);
                }
                if (variance < 0)
                {
                    var purchasePriceVarianceAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.PurchasePriceVariance.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForDebit.Append(", ");
                    contraAccountNamesForDebit.Append(purchasePriceVarianceAccountName);
                    contraAccountIdsForDebit.Append(", ");
                    contraAccountIdsForDebit.Append(AccountHeadConstants.PurchasePriceVariance);
                }
                //for lc cost clearing account
                var lccostClearingAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.LCCostClearingAccount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                contraAccountNamesForCredit.Append(", ");
                contraAccountNamesForCredit.Append(lccostClearingAccountName);
                contraAccountIdsForCredit.Append(", ");
                contraAccountIdsForCredit.Append(AccountHeadConstants.LCCostClearingAccount);

                //LC Cost Entries
                var lcCostEntries = await _unitOfWork.Repository<LccostEntryDetail>().TableNoTracking().Include(x => x.LccostEntry)
                  .Where(x => x.LccostEntry.Status == (int)LCCostEntryStatus.Calculated_Within_Landed_Cost
                   && x.LccostEntry.PurchaseOrderId == purchaseOrder.Id && x.IsIncludedWithinLandedCost).ToListAsync();
                foreach (var lcCostEntry in lcCostEntries)
                {
                    var lcCostDebitAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == lcCostEntry.DebitAccountId).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForDebit.Append(", ");
                    contraAccountNamesForDebit.Append(lcCostDebitAccountName);
                    contraAccountIdsForDebit.Append(", ");
                    contraAccountIdsForDebit.Append(lcCostEntry.DebitAccountId.ToString());
                }

                //purchase account debit
                await _accountService.HitAccount(_unitOfWork, null, purchaseInvoice.PurchaseInvoiceNo, sb.ToString(),
                     Guid.Parse(AccountHeadConstants.PurchaseClearingAccount), purchaseInvoice.Subtotal, 0, purchaseInvoice.InvoiceDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                if (priceVariance > 0)
                {
                    await _accountService.HitAccount(_unitOfWork, null, purchaseInvoice.PurchaseInvoiceNo, sb.ToString(),
                      Guid.Parse(AccountHeadConstants.PurchasePriceVariance), priceVariance, 0, purchaseInvoice.InvoiceDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                }
                if (priceVariance < 0)
                {
                    await _accountService.HitAccount(_unitOfWork, null, purchaseInvoice.PurchaseInvoiceNo, sb.ToString(),
                    Guid.Parse(AccountHeadConstants.PurchasePriceVariance), 0, Math.Abs(priceVariance), purchaseInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }
                //supplier account credit
                await _accountService.HitAccount(_unitOfWork, null, purchaseInvoice.PurchaseInvoiceNo, sb.ToString(),
                     purchaseInvoice.SupplierId, 0, purchaseOrder.Total, purchaseInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.Purchase, transactionQty, transactionQtyValue);
                if (variance > 0)
                {
                    await _accountService.HitAccount(_unitOfWork, null, purchaseInvoice.PurchaseInvoiceNo, sb.ToString(),
                      Guid.Parse(AccountHeadConstants.PurchasePriceVariance), variance, 0, purchaseInvoice.InvoiceDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());
                }
                if (variance < 0)
                {
                    await _accountService.HitAccount(_unitOfWork, null, purchaseInvoice.PurchaseInvoiceNo, sb.ToString(),
                    Guid.Parse(AccountHeadConstants.PurchasePriceVariance), 0, Math.Abs(variance), purchaseInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }
                //Lc cost clearing account debit
                await _accountService.HitAccount(_unitOfWork, null, purchaseInvoice.PurchaseInvoiceNo, sb.ToString(),
                    Guid.Parse(AccountHeadConstants.LCCostClearingAccount), purchaseInvoice.AdditionalLandedCost, 0, purchaseInvoice.InvoiceDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());

                foreach (var lcCostEntry in lcCostEntries)
                {
                    await _accountService.HitAccount(_unitOfWork, null, purchaseInvoice.PurchaseInvoiceNo, sb.ToString(),
                    lcCostEntry.DebitAccountId, 0, lcCostEntry.Amount, purchaseInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }


            }
            else
            {
                //supplier account credit
                await _accountService.HitAccount(_unitOfWork, null, purchaseInvoice.PurchaseInvoiceNo, sb.ToString(),
                     purchaseInvoice.SupplierId, 0, purchaseInvoice.Total, purchaseInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString(), (int)AccountTransactonType.Purchase, transactionQty, transactionQtyValue);
                //purchase discount Account credit
                if (purchaseInvoice.Discount != 0m)
                {
                    var purchaseDiscountAccountName = await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.Id == AccountHeadConstants.PurchaseDiscount.ToGuid()).Select(x => x.Name).FirstOrDefaultAsync();
                    contraAccountNamesForDebit.Append(", ");
                    contraAccountNamesForDebit.Append(purchaseDiscountAccountName);
                    contraAccountIdsForDebit.Append(", ");
                    contraAccountIdsForDebit.Append(AccountHeadConstants.PurchaseDiscount);
                    await _accountService.HitAccount(_unitOfWork, null, purchaseInvoice.PurchaseInvoiceNo, sb.ToString(),
                      Guid.Parse(AccountHeadConstants.PurchaseDiscount), 0, purchaseInvoice.Discount, purchaseInvoice.InvoiceDate, contraAccountIdsForCredit.ToString(), contraAccountNamesForCredit.ToString());
                }

                //purchase account debit
                await _accountService.HitAccount(_unitOfWork, null, purchaseInvoice.PurchaseInvoiceNo, sb.ToString(),
                    Guid.Parse(AccountHeadConstants.PurchaseClearingAccount), purchaseInvoice.Subtotal, 0, purchaseInvoice.InvoiceDate, contraAccountIdsForDebit.ToString(), contraAccountNamesForDebit.ToString());

            }

        }

        private async Task DeleteTransaction(string purchaseInvoiceNo)
        {
            var purchaseInvoice = await _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking()
                     .SingleOrDefaultAsync(x => x.PurchaseInvoiceNo == purchaseInvoiceNo);
            if (purchaseInvoice is null) throw new NotFoundResultException("Purchase Invoice Not Found With this invoice No");
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == purchaseInvoice.PurchaseInvoiceNo);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<Transaction>().DeleteAsync(transaction.Id);
                }
            }
        }

        private async Task InsertSupplierTransactionAgainstPO(PurchaseInvoice purchaseInvoice)
        {
            var purchaseOrder = await _unitOfWork.Repository<PurchaseOrder>().TableNoTracking().SingleOrDefaultAsync(x => x.Ponumber == purchaseInvoice.Ponumber);
            if (purchaseOrder is null) throw new BadRequestException("PO not found associated with this Invoice:" + purchaseInvoice.PurchaseInvoiceNo);
            await _unitOfWork.Repository<SupplierTransactionAgainstPo>().AddAsync(new SupplierTransactionAgainstPo
            {
                Id = Guid.NewGuid(),
                TransactionId = purchaseInvoice.Id,
                TransactionDate = purchaseInvoice.InvoiceDate,
                SupplierInvoiceDate = purchaseInvoice.SupplierInvoiceDate,
                PaymentTermInDays = purchaseInvoice.PaymentTermInDays,
                PurchaseOrderId = purchaseOrder!.Id,
                SupplierId = purchaseInvoice.SupplierId,
                SupplierTransactionType = (int)SupplierTransactionType.Purchase_Invoice,
                Amount = purchaseInvoice.Total,
                Remark = "Purchase Invoice"
            });

        }

        private async Task RemoveSupplierTransactionAgainstPO(PurchaseInvoice purchaseInvoice)
        {
            var transactions = _unitOfWork.Repository<SupplierTransactionAgainstPo>().TableNoTracking().Where(x => x.TransactionId == purchaseInvoice.Id);
            if (transactions.Any())
            {
                foreach (var transaction in transactions)
                {
                    await _unitOfWork.Repository<SupplierTransactionAgainstPo>().DeleteAsync(transaction.Id);
                }
            }
        }

        public async Task<List<PurchaseItemViewModel>> SupplierWisePurchaseItemSearchAsync(PurchaseInvoiceRequestModel request)
        {
            var queryable = _unitOfWork.Repository<PurchaseInvoiceDetail>().TableNoTracking()
                .Include(x => x.PurchaseInvoice).ThenInclude(x => x.Store).AsQueryable();
            queryable = queryable.Where(x => x.PurchaseInvoice.Status >= (int)PurchaseInvoiceStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                queryable = queryable.Where(d => d.PurchaseInvoice.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && d.PurchaseInvoice.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);
            }
            if (request.SupplierId.HasValue)
            {
                queryable = queryable.Where(d => d.PurchaseInvoice.SupplierId == request.SupplierId.Value);
            }
            if (request.ProductId.HasValue)
            {
                queryable = queryable.Where(d => d.ProductId == request.ProductId.Value);
            }
            if (request.StoreId.HasValue)
            {
                queryable = queryable.Where(d => d.PurchaseInvoice.StoreId == request.StoreId.Value);
            }
            var purchaseInvoiceItems = await queryable.Select(d => new PurchaseItemViewModel
            {
                Id = d.Id,
                BillNo = d.PurchaseInvoice.PurchaseInvoiceNo,
                Date = d.PurchaseInvoice.InvoiceDate,
                SupplierName = d.PurchaseInvoice.Supplier.Name,
                StoreName = d.PurchaseInvoice.Store.Name,
                ProductName = d.Product.Name,
                Description = d.PurchaseInvoice.Remark,
                Pono = d.PurchaseInvoice.Ponumber,
                Quantity = d.Grnquantity,
                Rate = d.Rate,
                Value = d.Amount
            }).OrderBy(x => x.Date).ToListAsync();

            return purchaseInvoiceItems;
        }

        public async Task<List<PurchaseInvoiceItemViewModel>> PurchaseInvoiceItemSearchAsync(PurchaseInvoiceRequestModel request)
        {
            var queryable = _unitOfWork.Repository<PurchaseInvoiceDetail>().TableNoTracking()
                .Include(x => x.PurchaseInvoice).AsQueryable();
            queryable = queryable.Where(x => x.PurchaseInvoice.Status >= (int)PurchaseInvoiceStatus.Approved);
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                queryable = queryable.Where(d => d.PurchaseInvoice.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && d.PurchaseInvoice.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);
            }
            var purchaseInvoiceItems = await queryable.GroupBy(x => x.ProductId).Select(d => new PurchaseInvoiceItemViewModel
            {
                Id = d.Key,
                Code = d.First().Product.Code,
                ProductName = d.First().Product.Name,
                ProductTypeName = d.First().Product.ProductType.Name,
                Quantity = d.Sum(x => x.Grnquantity),
                Rate = d.Sum(x => x.Amount) / d.Sum(x => x.Grnquantity),
                Value = d.Sum(x => x.Amount)
            }).OrderBy(x => x.ProductName).ToListAsync();

            return purchaseInvoiceItems;
        }

        public async Task<PurchaseReportViewModel> PurchaseReportSearchAsync(PurchaseInvoiceRequestModel request)
        {
            var purchaseInvoices = _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().Where(x => x.Status >= (int)PurchaseInvoiceStatus.Approved).AsQueryable();
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                purchaseInvoices = purchaseInvoices.Where(d => d.InvoiceDate.Date >= request.FromDate.Value.ToLocal().Date && d.InvoiceDate.Date <= request.ToDate.Value.ToLocal().Date);
            }
            var purchaseReturns = _unitOfWork.Repository<PurchaseReturn>().TableNoTracking().Where(x => x.Status >= (int)PurchaseReturnStatus.Approved).AsQueryable();
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                purchaseReturns = purchaseReturns.Where(d => d.PurchaseReturnDate.Date >= request.FromDate.Value.ToLocal().Date && d.PurchaseReturnDate.Date <= request.ToDate.Value.ToLocal().Date);
            }
            var poPriceAdjustmentsAfterGrn = _unitOfWork.Repository<PoPriceAdjustmentAfterGrn>().TableNoTracking().Where(x => x.Status >= (int)PoPriceAdjustmentAfterGrnStatus.Approved).AsQueryable();
            if (request.FromDate.HasValue && request.ToDate.HasValue)
            {
                poPriceAdjustmentsAfterGrn = poPriceAdjustmentsAfterGrn.Where(d => d.AdjustmentDate.Date >= request.FromDate.Value.ToLocal().Date && d.AdjustmentDate.Date <= request.ToDate.Value.ToLocal().Date);
            }
            var result = new PurchaseReportViewModel()
            {
                TotalPurchase = await purchaseInvoices.SumAsync(x => x.Subtotal),
                TotalFreight = await purchaseInvoices.SumAsync(x => x.TransportationCost),
                TotalPurchaseRetun = await purchaseReturns.SumAsync(x => x.TotalAmount),
                TotalPurchaseDiscount = await purchaseInvoices.SumAsync(x => x.Discount),
                TotalPOPriceAdjustment = await poPriceAdjustmentsAfterGrn.SumAsync(x => x.TotalAmount),
            };

            return result;
        }

    }
}