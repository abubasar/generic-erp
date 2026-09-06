using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Exceptions;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Accounts.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using static iTextSharp.text.pdf.AcroFields;
using Application.Core.Extensions;
using Application.Core.Enums;

namespace Application.Services.Services.Accounts.Customers
{
    public class CustomerService : BaseService<Account, CustomerCreationDto, CustomerUpdateDto, CustomerRequestModel, CustomerViewModel>, ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IAccountService _accountService;
        private readonly ISmsService _smsService;
        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext,
            IAccountService accountService, ISmsService smsService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _accountService = accountService;
            _smsService = smsService;
        }
        public override async Task<Tuple<List<CustomerViewModel>, int>> SearchAsync(CustomerRequestModel request)
        {
            var queryable = _unitOfWork.Repository<Account>().TableNoTracking().Where(request.GetExpression()
                ).Where(x => x.ParentId == Guid.Parse(AccountHeadConstants.TradeReceivable));
            if (!queryable.Any()) throw new NotFoundResultException("Ooo! No Search Result Found.");
            queryable = request.CreateOrderByQueryable(queryable);
            int count = queryable.Count();
            queryable = request.SkipAndTake(queryable);
            queryable = request.IncludeParents(queryable);
            var list = await queryable.ToListAsync();
            var mapps = _mapper.Map<List<CustomerViewModel>>(list);
            return new Tuple<List<CustomerViewModel>, int>(mapps, count);
        }
        public override async Task<Guid> AddAsync(CustomerCreationDto customerCreationDto)
        {
            if (!string.IsNullOrWhiteSpace(customerCreationDto.Name))
                customerCreationDto.Name = customerCreationDto.Name.ToUpper();
            if (!string.IsNullOrWhiteSpace(customerCreationDto.OwnersName))
                customerCreationDto.OwnersName = customerCreationDto.OwnersName.ToUpper();
            if (!string.IsNullOrWhiteSpace(customerCreationDto.Address))
                customerCreationDto.Address = customerCreationDto.Address.ToUpper();
            var model = _mapper.Map<Account>(customerCreationDto);
            model.Id = Guid.NewGuid();
            model.AccountTypeId = Guid.Parse(AccountTypeConstants.CurrentAsset);
            model.Level = 5;
            model.ParentId = Guid.Parse(AccountHeadConstants.TradeReceivable);
            model.IsControlAccount = true;
            model.Code = await _accountService.GetAccountCode(model);
            //save customer bank account
            foreach (var item in model.BankAccounts)
            {
                item.Id = Guid.NewGuid();
                item.AccountId = model.Id;
                await _unitOfWork.Repository<BankAccount>().AddAsync(item);
            }
            await _unitOfWork.Repository<Account>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return model.Id;
        }
        public override async Task<Guid> UpdateAsync(CustomerUpdateDto customerUpdateDto)
        {
            var dbCustomer = await _unitOfWork.Repository<Account>().FindAsync(customerUpdateDto.Id);
            var customer = _mapper.Map(customerUpdateDto, dbCustomer);
            await _unitOfWork.Repository<Account>().UpdateAsync(customer);
            foreach (var item in customer.BankAccounts)
            {
                if (item.Id == Guid.Empty)
                {
                    item.Id = Guid.NewGuid();
                    item.AccountId = customer.Id;
                    await _unitOfWork.Repository<BankAccount>().AddAsync(item);
                }
                else await _unitOfWork.Repository<BankAccount>().UpdateAsync(item);
            }

            //Delete for BankAccounts
            if (!string.IsNullOrEmpty(customerUpdateDto.DeletedBankAccountIds))
            {
                foreach (var id in customerUpdateDto.DeletedBankAccountIds.Split(',').Where(x => x != ""))
                {
                    var bankAccount = await _unitOfWork.Repository<BankAccount>().FindAsync(new Guid(id));
                    bankAccount.Deleted = true;
                    await _unitOfWork.Repository<BankAccount>().UpdateAsync(bankAccount);
                }
            }
            await _unitOfWork.SaveChangesAsync();
            return customer.Id;
        }
        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var obj = await _unitOfWork.Repository<Account>().FindAsync(id);
            if (obj == null) throw new NotFoundResultException("Entity Not Found With this Id");
            var hasTransactionEntry = await _unitOfWork.Repository<Transaction>().TableNoTracking().AnyAsync(x => x.AccountId == obj.Id);
            if (hasTransactionEntry)
            {
                throw new BadRequestException("This Customer has already some transactions! Not Possible to delete !!!");
            }
            obj.Deleted = true;
            await _unitOfWork.Repository<Account>().UpdateAsync(obj);
            await _unitOfWork.SaveChangesAsync();
            return id;
        }
        public async Task<(decimal creditLimit, decimal balance, Guid? customerMarketingOfficerId, Guid? customerTerritoryId)> FindCustomerCreditLimitAndBalance(Guid customerId)
        {
            var financialYear = await _unitOfWork.Repository<FinancialYear>().TableNoTracking().FirstAsync(x => x.IsActive);
            var customer = await _unitOfWork.Repository<Account>().FindAsync(x => x.Id == customerId);
            var balance = await _accountService.CalculateBalanceByAccountId(customerId, Guid.Parse(AccountTypeConstants.CurrentAsset), financialYear.Id, null, null);
            var approvedSaleOrders=await _unitOfWork.Repository<SaleOrder>().TableNoTracking().Where(x => x.CustomerId == customerId && x.Status==(int)SaleOrderStatus.Approved).ToListAsync();
            var approvedOrderAmount=approvedSaleOrders.Sum(x=>x.NetTotal);
            return (customer.CustomerCreditLimit, balance+approvedOrderAmount, customer.CustomerMarketingOfficerId, customer.CustomerTerritoryId);
        }
        public async Task<decimal> FindCustomerBalance(Guid customerId)
        {
            var financialYear = await _unitOfWork.Repository<FinancialYear>().TableNoTracking().FirstAsync(x => x.IsActive);
            var balance = await _accountService.CalculateBalanceByAccountId(customerId, Guid.Parse(AccountTypeConstants.CurrentAsset), financialYear.Id, null, null);
            return balance;
        }
        public async Task<bool> CustomerExists(string? id, string name, string contactNo)
        {
            if (string.IsNullOrEmpty(id))
            {
                if (await _unitOfWork.Repository<Account>().TableNoTracking().AnyAsync(x => x.Name.ToLower() == name.ToLower() && x.ContactNo == contactNo && x.ParentId == Guid.Parse(AccountHeadConstants.TradeReceivable)))
                    return true;
                return false;
            }
            else
            {
                if (await _unitOfWork.Repository<Account>().TableNoTracking().AnyAsync(x => x.Id != Guid.Parse(id) && x.Name.ToLower() == name.ToLower() && x.ContactNo == contactNo && x.ParentId == Guid.Parse(AccountHeadConstants.TradeReceivable)))
                    return true;
                return false;
            }
        }

        public async Task SendSmsToActiveCustomers()
        {
            var customers = await _unitOfWork.Repository<Account>().TableNoTracking()
                .Where(x => x.ParentId == AccountHeadConstants.TradeReceivable.ToGuid()).ToListAsync();
            foreach (var customer in customers)
            {
                var hasTransaction = await _unitOfWork.Repository<Transaction>().TableNoTracking().AnyAsync(x => x.AccountId == customer.Id && x.IsOpeningBalance == false);
                if (hasTransaction)
                {
                    await _smsService.Send(customer.ContactNo, $"সম্মানিত গ্রাহক “আসন্ন পবিত্র ঈদুল আযহা উপলক্ষে " +
                        $"আপনাকে অগ্রিম ঈদ শুভেচ্ছা। এপি ফিডের সাথে থাকার জন্য ধন্যবাদ। " +
                        $"আগামী ১৬ জুন থেকে ১৯ জুন-২০২৪ ইং পর্যন্ত ফিড ডেলিভারি বন্ধ থাকবে।" +
                        $"সাময়িক অসুবিধার জন্য আমরা আন্তরিকভাবে দুঃখিত।জরুরি প্রয়োজনে " +
                        $"সংশ্লিষ্ট মার্কেটিং অফিসার অথবা ০১৩১৩০১৯১১৪ নাম্বারে  " +
                        $"যোগাযোগ করুন। " +
                        $"-আব্দুল্লাহ পোল্ট্রি ফিড মিল");
                }

            }

        }



    }
}
