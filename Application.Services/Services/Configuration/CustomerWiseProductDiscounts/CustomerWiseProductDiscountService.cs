using Application.Core.Common;
using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Core.SignalR;
using Application.Services.Dtos.Configuration.CustomerWiseProductDiscount;
using Application.Services.SearchRequestModels.Configuration;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Configuration;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Sale;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Configuration.CustomerWiseProductDiscounts
{
    public class CustomerWiseProductDiscountService : BaseService<CustomerWiseProductDiscount, CustomerWiseProductDiscountCreationDto, CustomerWiseProductDiscountUpdateDto, CustomerWiseProductDiscountRequestModel, CustomerWiseProductDiscountViewModel>, ICustomerWiseProductDiscountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;

        public CustomerWiseProductDiscountService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, INotificationService notificationService, IHubContext<BroadcastHub, IHubClient> hubContext) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _notificationService = notificationService;
            _hubContext = hubContext;
        }

        public new async Task<AddUpdateResponseModel> AddAsync(CustomerWiseProductDiscountCreationDto customerWiseProductDiscountCreationDto)
        {
            var isActive = await _unitOfWork.Repository<CustomerWiseProductDiscount>().TableNoTracking().AnyAsync(x => x.CustomerId == customerWiseProductDiscountCreationDto.CustomerId && x.IsActive == true);
            if (isActive)
            {
                throw new BadRequestException("Customer already has an active discount. Please deactivate it before adding a new one.");
            }
            customerWiseProductDiscountCreationDto.ApplicableDate = customerWiseProductDiscountCreationDto.ApplicableDate.ToLocal();
            var customerWiseProductDiscount = _mapper.Map<CustomerWiseProductDiscount>(customerWiseProductDiscountCreationDto);
            customerWiseProductDiscount.Id = Guid.NewGuid();
            customerWiseProductDiscount.Status = (int)CustomerWiseProductDiscountStatus.Pending;
            foreach (var item in customerWiseProductDiscount.CustomerWiseProductDiscountDetails)
            {
                item.Id = Guid.NewGuid();
                item.CustomerWiseProductDiscountId = customerWiseProductDiscount.Id;
                item.CustomerId = customerWiseProductDiscount.CustomerId;
                await _unitOfWork.Repository<CustomerWiseProductDiscountDetail>().AddAsync(item);
            }
            await _unitOfWork.Repository<CustomerWiseProductDiscount>().AddAsync(customerWiseProductDiscount);
            await _unitOfWork.SaveChangesAsync();

            return new AddUpdateResponseModel { Id = customerWiseProductDiscount.Id, Status = customerWiseProductDiscount.Status };
        }

        public new async Task<AddUpdateResponseModel> UpdateAsync(CustomerWiseProductDiscountUpdateDto customerWiseProductDiscountUpdateDto)
        {
            var isActive = await _unitOfWork.Repository<CustomerWiseProductDiscount>().TableNoTracking().AnyAsync(x => x.CustomerId == customerWiseProductDiscountUpdateDto.CustomerId && x.Id != customerWiseProductDiscountUpdateDto.Id && x.IsActive == true);
            if (isActive)
            {
                throw new BadRequestException("Customer already has an active discount. Please deactivate it before reactivating this one.");
            }
            var dbCustomerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>().FindAsync(customerWiseProductDiscountUpdateDto.Id);
            //when date updated,convert it to local date
            if (customerWiseProductDiscountUpdateDto.ApplicableDate.Equals(dbCustomerWiseProductDiscount.ApplicableDate) == false)
            {
                customerWiseProductDiscountUpdateDto.ApplicableDate = customerWiseProductDiscountUpdateDto.ApplicableDate.ToLocal();
            }
            var customerWiseProductDiscount = _mapper.Map(customerWiseProductDiscountUpdateDto, dbCustomerWiseProductDiscount);
            if (dbCustomerWiseProductDiscount.Status == (int)CustomerWiseProductDiscountStatus.Pending || dbCustomerWiseProductDiscount.Status == (int)CustomerWiseProductDiscountStatus.Checked)
            {
                await _unitOfWork.Repository<CustomerWiseProductDiscount>().UpdateAsync(customerWiseProductDiscount);
                foreach (var item in customerWiseProductDiscount.CustomerWiseProductDiscountDetails)
                {
                    if (item.Id == Guid.Empty)
                    {
                        item.Id = Guid.NewGuid();
                        item.CustomerWiseProductDiscountId = customerWiseProductDiscount.Id;
                        item.CustomerId = customerWiseProductDiscount.CustomerId;
                        await _unitOfWork.Repository<CustomerWiseProductDiscountDetail>().AddAsync(item);
                    }
                    else
                    {
                        item.CustomerId = customerWiseProductDiscount.CustomerId;
                        await _unitOfWork.Repository<CustomerWiseProductDiscountDetail>().UpdateAsync(item);
                    }
                }
                //Delete for CustomerWiseProductDiscountDetails
                if (!string.IsNullOrEmpty(customerWiseProductDiscountUpdateDto.DeletedCustomerWiseProductDiscountDetailIds))
                {
                    foreach (var id in customerWiseProductDiscountUpdateDto.DeletedCustomerWiseProductDiscountDetailIds.Split(',').Where(x => x != ""))
                    {
                        var customerWiseProductDiscountDetail = await _unitOfWork.Repository<CustomerWiseProductDiscountDetail>().FindAsync(new Guid(id));
                        customerWiseProductDiscountDetail.Deleted = true;
                        await _unitOfWork.Repository<CustomerWiseProductDiscountDetail>().UpdateAsync(customerWiseProductDiscountDetail);
                    }
                }
            }
            else throw new BadRequestException("Not Possible to Update. Only Pending or Checked Status Allowed to Update.");
            await _unitOfWork.SaveChangesAsync();
            return new AddUpdateResponseModel { Id = customerWiseProductDiscount.Id };
        }

        public async Task<CustomerWiseProductDiscountViewModel> GetByIdAsync(Guid id)
        {
            var customerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>().FindAsync(x => x.Id == id, x =>
                x.Include(x => x.Customer)
                .Include(x => x.CustomerWiseProductDiscountDetails)
                .ThenInclude(x => x.Product).ThenInclude(x => x.MeasurementUnit));
            if (customerWiseProductDiscount == null) throw new NotFoundResultException("Customer Wise Product Discount Not Found With this Id");
            return _mapper.Map<CustomerWiseProductDiscountViewModel>(customerWiseProductDiscount);
        }

        public async Task<PendingCheckedCountModel> GetPendingCheckedCountAsync()
        {
            var customerWiseProductDiscounts = _unitOfWork.Repository<CustomerWiseProductDiscount>().TableNoTracking();
            var response = new PendingCheckedCountModel
            {
                PendingCount = await customerWiseProductDiscounts.Where(x => x.Status == (int)CustomerWiseProductDiscountStatus.Pending).CountAsync(),
                CheckedCount = await customerWiseProductDiscounts.Where(x => x.Status == (int)CustomerWiseProductDiscountStatus.Checked).CountAsync()
            };
            return response;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var customerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>().FindAsync(x => x.Id == id, x => x.Include(x => x.CustomerWiseProductDiscountDetails));
            if (customerWiseProductDiscount == null) throw new NotFoundResultException("Customer Wise Product Discount Not Found With this Id");
            if (customerWiseProductDiscount.Status == (int)CustomerWiseProductDiscountStatus.Pending)
            {
                foreach (var item in customerWiseProductDiscount.CustomerWiseProductDiscountDetails.ToList())
                {
                    item.Deleted = true;
                    await _unitOfWork.Repository<CustomerWiseProductDiscountDetail>().UpdateAsync(item);
                }
                customerWiseProductDiscount.Deleted = true;
                await _unitOfWork.Repository<CustomerWiseProductDiscount>().UpdateAsync(customerWiseProductDiscount);
                await _unitOfWork.SaveChangesAsync();
            }
            else throw new BadRequestException("Not Possible to delete. Only Pending Status Allowed to Delete.");
            return customerWiseProductDiscount.Id;
        }

        public async Task<CustomerInvoiceDiscountViewModel?> GetCustomerDiscountByProductId(Guid customerId, Guid productId)
        {
            var customerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>()
                                                        .TableNoTracking().SingleOrDefaultAsync(x => x.CustomerId == customerId && x.IsActive);
            if (customerWiseProductDiscount is not null)
            {
                var discountDetail = await _unitOfWork.Repository<CustomerWiseProductDiscountDetail>()
                                                  .TableNoTracking()
                                                  .SingleAsync(x => x.CustomerWiseProductDiscountId == customerWiseProductDiscount.Id
                                                  && x.ProductId == productId);
                return new CustomerInvoiceDiscountViewModel
                {
                    CustomerWiseProductDiscountId = customerWiseProductDiscount.Id,
                    InvoiceDiscountPerUnit = discountDetail.InvoiceDiscount,
                    CashDiscountPerUnit = discountDetail.CashDiscount,
                    SpecialDiscountPerUnit = discountDetail.SpecialDiscount
                };
            }
            return new CustomerInvoiceDiscountViewModel
            {
                CustomerWiseProductDiscountId = null,
                InvoiceDiscountPerUnit = 0,
                CashDiscountPerUnit = 0,
                SpecialDiscountPerUnit = 0
            };
        }
        public async Task<List<ActiveCustomerWiseProductDiscountByCustomerIdViewModel>> GetActiveCustomerDiscountByCustomerId(Guid customerId)
        {
            List<ActiveCustomerWiseProductDiscountByCustomerIdViewModel> result = new();
            var customerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>().TableNoTracking().SingleOrDefaultAsync(x => x.CustomerId == customerId && x.IsActive && x.Status == (int)CustomerWiseProductDiscountStatus.Approved);
            if (customerWiseProductDiscount is not null)
            {
                var queryable = _unitOfWork.Repository<CustomerWiseProductDiscountDetail>().TableNoTracking().Where(x => x.CustomerWiseProductDiscountId == customerWiseProductDiscount.Id).AsQueryable();
                result = await queryable.Select(d => new ActiveCustomerWiseProductDiscountByCustomerIdViewModel
                {
                    CustomerWiseProductDiscountId = customerWiseProductDiscount.Id,
                    ProductId = d.ProductId,
                    InvoiceDiscountPerUnit = d.InvoiceDiscount,
                    CashDiscountPerUnit = d.CashDiscount,
                    SpecialDiscountPerUnit = d.SpecialDiscount
                }).ToListAsync();
            }
            return result;
        }

        public virtual async Task<bool> CheckAsync(Guid id)
        {
            var dbCustomerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>().FindAsync(x => x.Id == id, x => x.Include(x => x.Customer));
            if (dbCustomerWiseProductDiscount.Status == (int)CustomerWiseProductDiscountStatus.Pending)
            {
                dbCustomerWiseProductDiscount.Status = (int)CustomerWiseProductDiscountStatus.Checked;
                dbCustomerWiseProductDiscount.CheckedBy = _workContext.GetUserName();
                await _unitOfWork.Repository<CustomerWiseProductDiscount>().UpdateAsync(dbCustomerWiseProductDiscount);
                //remove pending notification
                await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbCustomerWiseProductDiscount.Id
                       && x.Status == (int)CustomerWiseProductDiscountStatus.Pending);
                //send notification for approval
                await _notificationService.SendNotificationAsync(_unitOfWork, dbCustomerWiseProductDiscount.Id
                    , "/configuration/customer-wise-product-discount/" + dbCustomerWiseProductDiscount.Id, SecondaryPermissions.CustomerWiseProductDiscounts.Approve,
                    "CustomerWise Product Discount " + dbCustomerWiseProductDiscount.Customer.Name + " is ready for Approval", (int)CustomerWiseProductDiscountStatus.Checked);
                bool isChecked = await _unitOfWork.SaveChangesAsync();
                await _hubContext.Clients.All.BroadcastMessage();
                return isChecked;
            }
            else throw new BadRequestException("Customer Wise Product Discount Status has already been checked by " + dbCustomerWiseProductDiscount?.CheckedBy);
        }

        public virtual async Task<bool> ApproveAsync(Guid id)
        {
            var approvedBy = _workContext.GetUserName();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<CustomerWiseProductDiscount>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.Status, x.ApprovedBy })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("Customer Wise Product Discount not found.");

            // Atomic approval (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<CustomerWiseProductDiscount>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == (int)CustomerWiseProductDiscountStatus.Checked)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, (int)CustomerWiseProductDiscountStatus.Approved)
                    .SetProperty(x => x.ApprovedBy, approvedBy));

            if (rowsUpdated == 0)
                throw new BadRequestException($"Customer Wise Product Discount already approved by {dbInfo.ApprovedBy}");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbCustomerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>().FindAsync(id);

            dbCustomerWiseProductDiscount.Status = (int)CustomerWiseProductDiscountStatus.Approved;
            dbCustomerWiseProductDiscount.ApprovedBy = approvedBy;

            //update CustomerWiseProductDiscount
            await _unitOfWork.Repository<CustomerWiseProductDiscount>().UpdateAsync(dbCustomerWiseProductDiscount);
            //remove checked notification
            await _unitOfWork.Repository<Notification>().DeleteAsync(x => x.GuidId == dbCustomerWiseProductDiscount.Id
                   && x.Status == (int)CustomerWiseProductDiscountStatus.Checked);

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
            //var financialYear = _workContext.GetCurrentFinancialYear();

            // Start transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            // Load minimal data first (no tracking)
            var dbInfo = await _unitOfWork.Repository<CustomerWiseProductDiscount>()
                .TableNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.Id, x.Status })
                .SingleOrDefaultAsync();

            if (dbInfo == null)
                throw new NotFoundResultException("CustomerWiseProductDiscount Not Found With this Id");

            //if (financialYear.Id != dbCustomerWiseProductDiscount.FinancialYearId) throw new BadRequestException("Unpost Denied: This CustomerWiseProductDiscount does not belong to the current financial year.");

            if (dbInfo.Status != fromStatus)
                throw new BadRequestException("Unpost failed: the record status has already changed.");

            if (dbInfo.Status != (int)CustomerWiseProductDiscountStatus.Checked && dbInfo.Status != (int)CustomerWiseProductDiscountStatus.Approved)
                throw new BadRequestException("Not Possible to Unpost !!");

            int newStatus = dbInfo.Status == (int)CustomerWiseProductDiscountStatus.Checked
                ? (int)CustomerWiseProductDiscountStatus.Pending
                : (int)CustomerWiseProductDiscountStatus.Checked;

            // Atomic unpost (ONLY ONE request will pass)
            int rowsUpdated = await _unitOfWork.Repository<CustomerWiseProductDiscount>()
                .TableNoTracking()
                .Where(x => x.Id == id && x.Status == dbInfo.Status)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.Status, newStatus)
                );

            if (rowsUpdated == 0)
                throw new BadRequestException("Unpost failed: this record has already been unposted or modified by another request.");

            // From here ONLY ONE THREAD EXECUTES

            // Reload FULL entity WITH tracking (inside transaction)
            var dbCustomerWiseProductDiscount = await _unitOfWork.Repository<CustomerWiseProductDiscount>().FindAsync(id);
            dbCustomerWiseProductDiscount.Status = newStatus;

            switch (dbInfo.Status)
            {
                case (int)CustomerWiseProductDiscountStatus.Checked:
                    dbCustomerWiseProductDiscount.CheckedBy = "";
                    break;
                case (int)CustomerWiseProductDiscountStatus.Approved:
                    dbCustomerWiseProductDiscount.ApprovedBy = "";
                    break;
            }

            await _unitOfWork.Repository<CustomerWiseProductDiscount>().UpdateAsync(dbCustomerWiseProductDiscount);
            await _unitOfWork.SaveChangesAsync();
            await transaction.CommitAsync();
            return dbCustomerWiseProductDiscount.Status;
        }




    }
}
