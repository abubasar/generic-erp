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

namespace Application.Services.Services.Accounts.Suppliers
{
    public class SupplierService : BaseService<Account, SupplierCreationDto, SupplierUpdateDto, SupplierRequestModel, SupplierViewModel>, ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IAccountService _accountService;

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, IAccountService accountService) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _accountService = accountService;
        }

        public override async Task<Tuple<List<SupplierViewModel>, int>> SearchAsync(SupplierRequestModel request)
        {
            var queryable = _unitOfWork.Repository<Account>().TableNoTracking().Where(request.GetExpression()
                ).Where(x => x.ParentId == Guid.Parse(AccountHeadConstants.TradePayable));
            if (!queryable.Any()) throw new NotFoundResultException("Ooo! No Search Result Found.");
            queryable = request.CreateOrderByQueryable(queryable);
            int count = queryable.Count();
            queryable = request.SkipAndTake(queryable);
            queryable = request.IncludeParents(queryable);
            var supplierList = _mapper.Map<List<SupplierViewModel>>(await queryable.ToListAsync());
            var list = new List<SupplierViewModel>();
            foreach (var item in supplierList)
            {
                item.SuppliedProductIds = _unitOfWork.Repository<SupplierProductMapping>().TableNoTracking()
                    .Where(x => x.SupplierId == item.Id).Select(x => x.ProductId).ToList();
                list.Add(item);
            }
            return new Tuple<List<SupplierViewModel>, int>(list, count);
        }
        public override async Task<Guid> AddAsync(SupplierCreationDto supplierCreationDto)
        {
            var model = _mapper.Map<Account>(supplierCreationDto);
            model.Id = Guid.NewGuid();
            model.AccountTypeId = Guid.Parse(AccountTypeConstants.CurrentLiabilities);
            model.Level = 5;
            model.ParentId = Guid.Parse(AccountHeadConstants.TradePayable);
            model.IsControlAccount = true;
            model.Code = await _accountService.GetAccountCode(model);
            //save supplier bank account
            foreach (var item in model.BankAccounts)
            {
                item.Id = Guid.NewGuid();
                item.AccountId = model.Id;
                await _unitOfWork.Repository<BankAccount>().AddAsync(item);
            }
            //save supplier products
            if (supplierCreationDto.SuppliedProductIds != null && supplierCreationDto.SuppliedProductIds.Any())
            {
                foreach (var id in supplierCreationDto.SuppliedProductIds)
                {
                    SupplierProductMapping supplierProduct = new SupplierProductMapping
                    {
                        Id = Guid.NewGuid(),
                        SupplierId = model.Id,
                        ProductId = id
                    };
                    await _unitOfWork.Repository<SupplierProductMapping>().AddAsync(supplierProduct);
                }
            }
            await _unitOfWork.Repository<Account>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return model.Id;
        }
        public override async Task<Guid> UpdateAsync(SupplierUpdateDto supplierUpdateDto)
        {
            var dbSupplier = await _unitOfWork.Repository<Account>().FindAsync(supplierUpdateDto.Id);
            var supplier = _mapper.Map(supplierUpdateDto, dbSupplier);
            await _unitOfWork.Repository<Account>().UpdateAsync(supplier);
            foreach (var item in supplier.BankAccounts)
            {
                if (item.Id == Guid.Empty)
                {
                    item.Id = Guid.NewGuid();
                    item.AccountId = supplier.Id;
                    await _unitOfWork.Repository<BankAccount>().AddAsync(item);
                }
                else await _unitOfWork.Repository<BankAccount>().UpdateAsync(item);
            }
            //update supplier products
            if (supplierUpdateDto.SuppliedProductIds != null && supplierUpdateDto.SuppliedProductIds.Any())
            {
                IList<SupplierProductMapping> supplierProducts = _unitOfWork.Repository<SupplierProductMapping>().TableNoTracking().Where(x => x.SupplierId == supplier.Id).ToList();
                if (supplierProducts.Any())
                    await _unitOfWork.Repository<SupplierProductMapping>().DeleteAsync(supplierProducts);
                foreach (var id in supplierUpdateDto.SuppliedProductIds)
                {
                    SupplierProductMapping supplierProduct = new SupplierProductMapping
                    {
                        Id = Guid.NewGuid(),
                        SupplierId = supplier.Id,
                        ProductId = id
                    };
                    await _unitOfWork.Repository<SupplierProductMapping>().AddAsync(supplierProduct);
                }
            }
            //Delete for BankAccounts
            if (!string.IsNullOrEmpty(supplierUpdateDto.DeletedBankAccountIds))
            {
                foreach (var id in supplierUpdateDto.DeletedBankAccountIds.Split(',').Where(x => x != ""))
                {
                    var bankAccount = await _unitOfWork.Repository<BankAccount>().FindAsync(new Guid(id));
                    bankAccount.Deleted = true;
                    await _unitOfWork.Repository<BankAccount>().UpdateAsync(bankAccount);
                }
            }
            await _unitOfWork.SaveChangesAsync();
            return supplier.Id;
        }
        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var obj = await _unitOfWork.Repository<Account>().FindAsync(id);
            if (obj == null) throw new NotFoundResultException("Entity Not Found With this Id");
            var hasTransactionEntry = await _unitOfWork.Repository<Transaction>().TableNoTracking().AnyAsync(x => x.AccountId == obj.Id);
            if (hasTransactionEntry)
            {
                throw new BadRequestException("This supplier has already some transactions! Not Possible to delete !!!");
            }
            obj.Deleted = true;
            await _unitOfWork.Repository<Account>().UpdateAsync(obj);
            await _unitOfWork.SaveChangesAsync();
            return id;
        }
        public async Task<bool> SupplierExists(string name)
        {
            if (await _unitOfWork.Repository<Account>().TableNoTracking().AnyAsync(x => x.Name.ToLower() == name.ToLower() && x.ParentId == Guid.Parse(AccountHeadConstants.TradePayable)))
                return true;
            return false;
        }

    }
}