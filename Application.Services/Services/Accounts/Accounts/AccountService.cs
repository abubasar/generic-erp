using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Exceptions;
using Application.Core.Extensions;
using Application.Core.Industry;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.Reports;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;

namespace Application.Services.Services.Accounts.Accounts
{
    public class AccountService : BaseService<Account, AccountCreationDto, AccountUpdateDto, AccountRequestModel, AccountViewModel>, IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IWorkContext _workContext;
        private readonly IIndustryProfile _industry;

        public AccountService(IUnitOfWork unitOfWork, IMapper mapper, IWorkContext workContext, IIndustryProfile industry) : base(unitOfWork, mapper, workContext)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _workContext = workContext;
            _industry = industry;
        }



        #region Utilities
        private string PrepareTreeNameOfAccount(Account? accountHead)
        {
            if (accountHead == null)
                throw new ArgumentNullException("accounthead");

            var result = new List<Account>();

            //used to prevent circular references
            var alreadyProcessedAccountHeadIds = new List<Guid>() { };

            while (accountHead != null &&
                !alreadyProcessedAccountHeadIds.Contains(accountHead.Id)) //prevent circular references
            {
                result.Add(accountHead);
                alreadyProcessedAccountHeadIds.Add(accountHead.Id);
                accountHead = _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefault(x => x.Id == accountHead.ParentId);
            }
            result.Reverse();
            return string.Join(" > ", result.Select(x => x.Level + "." + x.Name).ToList());
        }

        private IList<Account> GetChildrenByParentId(Guid? parentAccountId, IEnumerable<Account> accountHeads)
        {
            return accountHeads.Where(x => x.ParentId == parentAccountId).OrderBy(x => x.Code).ToList();
        }

        private async Task<Account?> GetRootAccount(Guid? parentId)
        {
            return await _unitOfWork.Repository<Account>().TableNoTracking().SingleOrDefaultAsync(x => x.Id == parentId);
        }

        private async Task<int> GetAccountLevel(Guid? parentId)
        {
            var parentAccount = await _unitOfWork.Repository<Account>().TableNoTracking().SingleOrDefaultAsync(x => x.Id == parentId);
            if (parentAccount is null) return 1;
            return parentAccount.Level + 1;
        }

        private async Task<int> GetChildrenCount(Guid parentId)
        {
            return await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.ParentId == parentId).CountAsync();
        }

        private async Task<string> GetAccountTypeStartingNumber(Guid accountTypeId)
        {
            var accountType = await _unitOfWork.Repository<AccountType>().FindAsync(accountTypeId);
            return accountType.StartingNumber.ToString();
        }

        private async Task TraverseTree(Account parent, List<ControlAccount> controlAccounts)
        {
            var childNodes = await _unitOfWork.Repository<Account>().TableNoTracking().Where(t => t.ParentId == parent.Id).ToListAsync();
            if (childNodes.Count == 0)
            {
                if (parent.IsControlAccount)
                    controlAccounts.Add(new ControlAccount()
                    {
                        Id = parent.Id,
                        Name = parent.Name,
                        ParentId = parent.ParentId,
                        TreeName = PrepareTreeNameOfAccount(parent)
                    });
            }
            else
            {
                foreach (var childNode in childNodes)
                {
                    await TraverseTree(childNode, controlAccounts);
                }
            }
        }

        private async Task TraverseTreeForAppUser(Account parent, List<ControlAccount> controlAccounts)
        {
            var childNodes = await _unitOfWork.Repository<Account>().TableNoTracking().Where(t => t.ParentId == parent.Id).ToListAsync();
            if (childNodes.Count == 0)
            {
                if (parent.IsControlAccount && parent.ForAppUser)
                    controlAccounts.Add(new ControlAccount()
                    {
                        Id = parent.Id,
                        Name = parent.Name,
                        ParentId = parent.ParentId,
                        TreeName = PrepareTreeNameOfAccount(parent)
                    });
            }
            else
            {
                foreach (var childNode in childNodes)
                {
                    await TraverseTreeForAppUser(childNode, controlAccounts);
                }
            }
        }

        private async Task TraverseTreeForAccountIds(Account parent, List<Guid> controlAccountIds)
        {
            var childNodes = await _unitOfWork.Repository<Account>().TableNoTracking().Where(t => t.ParentId == parent.Id).ToListAsync();
            if (childNodes.Count == 0)
            {
                if (parent.IsControlAccount)
                    controlAccountIds.Add(parent.Id);
            }
            else
            {
                foreach (var childNode in childNodes)
                {
                    await TraverseTreeForAccountIds(childNode, controlAccountIds);
                }
            }
        }
        private async Task TraverseTreeForControlAccounts(Account parent, List<Account> controlAccounts)
        {
            var childNodes = await _unitOfWork.Repository<Account>().TableNoTracking().Where(t => t.ParentId == parent.Id).ToListAsync();
            if (childNodes.Count == 0)
            {
                if (parent.IsControlAccount)
                    controlAccounts.Add(parent);
            }
            else
            {
                foreach (var childNode in childNodes)
                {
                    await TraverseTreeForControlAccounts(childNode, controlAccounts);
                }
            }
        }
        private async Task TraverseTreeForAccountIds(IEnumerable<Account> allAccountHeads, Account parent, List<Guid> controlAccountIds)
        {
            var childNodes = allAccountHeads.Where(t => t.ParentId == parent.Id);
            if (childNodes.Count() == 0)
            {
                if (parent.IsControlAccount)
                    controlAccountIds.Add(parent.Id);
            }
            else
            {
                foreach (var childNode in childNodes)
                {
                    await TraverseTreeForAccountIds(allAccountHeads, childNode, controlAccountIds);
                }
            }
        }
        private async Task<List<Account>> GetControlAccountListByAccountId(Guid accountId)
        {
            var controlAccounts = new List<Account>();
            var account = await _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == accountId);
            if (account is null) throw new NotFoundResultException("Account Not found with this id");
            await TraverseTreeForControlAccounts(account, controlAccounts);
            return controlAccounts;
        }
        private async Task<List<Guid>> GetAllControlAccountIdsByAccountId(Guid accountId)
        {
            var controlAccountIds = new List<Guid>();
            var account = await _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == accountId);
            if (account is null) throw new NotFoundResultException("Account Not found with this id");
            await TraverseTreeForAccountIds(account, controlAccountIds);
            return controlAccountIds;
        }
        private async Task<List<Guid>> GetAllControlAccountIdsByAccount(IEnumerable<Account> allAccountHeads, Account account)
        {
            var controlAccountIds = new List<Guid>();
            await TraverseTreeForAccountIds(allAccountHeads, account, controlAccountIds);
            return controlAccountIds;
        }
        private async Task<List<Guid>> GetAllControlAccountIdsByAccountId(IEnumerable<Account> allAccountHeads, Guid accountId)
        {
            var controlAccountIds = new List<Guid>();
            var account = await _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == accountId);
            if (account is null) throw new NotFoundResultException("Account Not found with this id");
            await TraverseTreeForAccountIds(allAccountHeads, account, controlAccountIds);
            return controlAccountIds;
        }

        private async Task<TrialBalanceViewModel> PrepareTrialBalanceReportLine(IList<Transaction> transactionList, IEnumerable<Account> allAccountHeads, Account account, FinancialYear financialYear, DateTime? fromDate, DateTime? toDate)
        {
            var accountIds = await GetAllControlAccountIdsByAccount(allAccountHeads, account);
            var allTransactions = transactionList.Where(x => accountIds.Contains(x.AccountId)).ToList();
            //opening balance
            var openingBalanceTransactions = allTransactions.Where(x => x.IsOpeningBalance);
            var accountTransactions = allTransactions.Where(x => !x.IsOpeningBalance);
            IEnumerable<Transaction> transactionsUptoFromDate = new List<Transaction>().AsEnumerable();
            if (fromDate.HasValue)
                transactionsUptoFromDate = accountTransactions.Where(x => x.TransactionDate.Date >= financialYear!.StartDate.Date && x.TransactionDate.Date < fromDate.Value.Date);
            //this period transactions
            if (fromDate.HasValue && toDate.HasValue)
                accountTransactions = accountTransactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);

            var thisPeriodDebit = accountTransactions.Sum(x => x.Debit);
            var thisPeriodCredit = accountTransactions.Sum(x => x.Credit);
            var openingCredit = openingBalanceTransactions.Sum(x => x.Credit) +
                    (transactionsUptoFromDate.Any() ? transactionsUptoFromDate.Sum(x => x.Credit) : 0M);
            var openingDebit = openingBalanceTransactions.Sum(x => x.Debit) +
                    (transactionsUptoFromDate.Any() ? transactionsUptoFromDate.Sum(x => x.Debit) : 0M);

            if (account.AccountTypeId == Guid.Parse(AccountTypeConstants.NonCurrentLiabilities)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.CurrentLiabilities)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.OwnersEquity)
                 || account.AccountTypeId == Guid.Parse(AccountTypeConstants.OthersEquity)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.Revenue)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.OtherIncome))
            {//(credit nature)
                return new TrialBalanceViewModel
                {
                    Id = account.Id,
                    Code = account.Code,
                    Name = account.Name,
                    Level = account.Level,
                    ThisPeriodDebit = thisPeriodDebit,
                    ThisPeriodCredit = thisPeriodCredit,
                    OpeningDebit = 0M,
                    OpeningCredit = openingCredit - openingDebit,
                    BalanceDebit = 0M,
                    BalanceCredit = (thisPeriodCredit - thisPeriodDebit) + (openingCredit - openingDebit),
                };
            }
            return new TrialBalanceViewModel
            {
                Id = account.Id,
                Code = account.Code,
                Name = account.Name,
                Level = account.Level,
                ThisPeriodDebit = thisPeriodDebit,
                ThisPeriodCredit = thisPeriodCredit,
                OpeningDebit = openingDebit - openingCredit,
                OpeningCredit = 0M,
                BalanceDebit = (thisPeriodDebit - thisPeriodCredit) + (openingDebit - openingCredit),
                BalanceCredit = 0M
            };
        }



        #endregion
        public override async Task<Guid> AddAsync(AccountCreationDto accountCreationDto)
        {
            var model = _mapper.Map<Account>(accountCreationDto);
            model.Id = Guid.NewGuid();
            model.Level = await GetAccountLevel(accountCreationDto.ParentId);
            model.Code = await GetAccountCode(model);
            await _unitOfWork.Repository<Account>().AddAsync(model);
            await _unitOfWork.SaveChangesAsync();
            return model.Id;
        }

        public override async Task<Guid> DeleteAsync(Guid id)
        {
            var account = await _unitOfWork.Repository<Account>().FindAsync(id);
            if (account == null) throw new NotFoundResultException("Account Not Found With this Id");
            var childrenCount = await GetChildrenCount(account.Id);
            if (childrenCount > 0) throw new BadRequestException("This is parent Account.Please delete child account first");
            if (AccountHeadConstants.IsAccountIdExistsInConstants(id)) throw new BadRequestException("Not Possible to delete System Reserved Account!!!");
            var hasTransactionEntry = await _unitOfWork.Repository<Transaction>().TableNoTracking().AnyAsync(x => x.AccountId == account.Id);
            if (hasTransactionEntry)
            {
                throw new BadRequestException("This Account has already some transactions! Not Possible to delete !!!");
            }
            account.Deleted = true;
            await _unitOfWork.Repository<Account>().UpdateAsync(account);
            await _unitOfWork.SaveChangesAsync();
            return id;
        }

        public async Task<bool> AccountExists(string name)
        {
            if (await _unitOfWork.Repository<Account>().TableNoTracking().AnyAsync(x => x.Name.ToLower() == name.ToLower()))
                return true;
            return false;
        }

        public async Task<IList<AccountDropdownDto>> PrepareAllParentAccounts()
        {
            var list = new List<AccountDropdownDto>();
            foreach (var item in await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => !x.IsControlAccount).ToListAsync())
            {
                list.Add(new AccountDropdownDto
                {
                    Id = item.Id,
                    Code = item.Code,
                    Name = item.Name,
                    AccountTypeId = item.AccountTypeId,
                    TreeName = PrepareTreeNameOfAccount(item)
                });
            }
            return list.OrderBy(x => x.Name).ToList();
        }

        public async Task<IList<AccountDropdownDto>> PrepareAllControlAccounts()
        {
            var list = new List<AccountDropdownDto>();
            foreach (var item in await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.IsControlAccount).ToListAsync())
            {
                list.Add(new AccountDropdownDto
                {
                    Id = item.Id,
                    Code = item.Code,
                    Name = item.Name,
                    AccountTypeId = item.AccountTypeId,
                    TreeName = PrepareTreeNameOfAccount(item)
                });
            }
            return list.OrderBy(x => x.Name).ToList();
        }

        public async Task<IList<AccountDropdownDto>> PrepareAllControlAccountsExcludingCustomers()
        {
            var list = new List<AccountDropdownDto>();
            foreach (var item in await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.IsControlAccount).ToListAsync())
            {
                if (item.ParentId == AccountHeadConstants.TradeReceivable.ToGuid()) continue;
                list.Add(new AccountDropdownDto
                {
                    Id = item.Id,
                    Code = item.Code,
                    Name = item.Name,
                    AccountTypeId = item.AccountTypeId,
                    TreeName = PrepareTreeNameOfAccount(item)
                });
            }
            return list.OrderBy(x => x.Name).ToList();
        }

        public async Task<IList<AccountDropdownDto>> PrepareAllControlAccountsExcludingSuppliers()
        {
            var list = new List<AccountDropdownDto>();
            foreach (var item in await _unitOfWork.Repository<Account>().TableNoTracking().Where(x => x.IsControlAccount).ToListAsync())
            {
                if (item.ParentId == AccountHeadConstants.TradePayable.ToGuid()) continue;
                list.Add(new AccountDropdownDto
                {
                    Id = item.Id,
                    Code = item.Code,
                    Name = item.Name,
                    AccountTypeId = item.AccountTypeId,
                    TreeName = PrepareTreeNameOfAccount(item)
                });
            }
            return list.OrderBy(x => x.Name).ToList();
        }

        public async Task<List<ControlAccount>> GetAllControlAccountsByAccountId(Guid accountId)
        {
            var controlAccounts = new List<ControlAccount>();
            var account = await _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == accountId);
            if (account is null) throw new NotFoundResultException("Account Not found with this id");
            await TraverseTree(account, controlAccounts);
            return controlAccounts;
        }

        public async Task<List<ControlAccount>> GetAllControlAccountsByAccountIdForAppUser(Guid accountId)
        {
            var controlAccounts = new List<ControlAccount>();
            var account = await _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == accountId);
            if (account is null) throw new NotFoundResultException("Account Not found with this id");
            await TraverseTreeForAppUser(account, controlAccounts);
            return controlAccounts;
        }


        public async Task<string> GetAccountCode(Account account)
        {
            var parentAccount = await GetRootAccount(account?.ParentId);
            string startingNumber = await GetAccountTypeStartingNumber(account!.AccountTypeId);
            StringBuilder sb = new StringBuilder();
            if (parentAccount is not null)
            {
                int childrenCount = await GetChildrenCount(parentAccount.Id);
                sb.Append(parentAccount.Code);
                sb.Append(".");
                if (parentAccount.Id == AccountHeadConstants.TradePayable.ToGuid() || parentAccount.Id == AccountHeadConstants.TradeReceivable.ToGuid())
                    sb.Append((childrenCount + 1).ToString().PadLeft(4, '0'));
                else
                    sb.Append((childrenCount + 1).ToString().PadLeft(2, '0'));
            }
            else
            {
                sb.Append(startingNumber);
            }
            return sb.ToString();
        }


        public async Task HitAccount(IUnitOfWork unitOfWork, Guid? costCenterId, string vnumber, string description, Guid accountId, decimal debit, decimal credit, DateTime transactionDate, string contraAccountIds, string contraAccountNames, int accountTransactionType = 0, int transactionQty = 0, decimal transactionQtyValue = 0m, Guid? paymentModeId = null, bool isOpeningBalance = false)
        {
            var financialYear = unitOfWork.Repository<FinancialYear>().TableNoTracking().Where(x => x.IsActive)?.FirstOrDefault();
            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                FinancialYearId = financialYear!.Id,
                CostCenterId = costCenterId,
                Vnumber = vnumber,
                Description = description,
                AccountId = accountId,
                Debit = debit,
                Credit = credit,
                TransactionDate = transactionDate,
                ContraAccountIds = contraAccountIds,
                ContraAccountNames = contraAccountNames,
                AccountTransactionType = accountTransactionType,
                TransactionQty = transactionQty,
                TransactionQtyValue = transactionQtyValue,
                PaymentModeId = paymentModeId,
                IsOpeningBalance = isOpeningBalance,
            };
            await unitOfWork.Repository<Transaction>().AddAsync(transaction);
        }

        public async Task UpdateTransaction(IUnitOfWork unitOfWork, IList<Transaction> transactions)
        {
            foreach (var transaction in transactions)
            {
                await unitOfWork.Repository<Transaction>()
                .UpdateAsync(transaction);
            }
        }


        public async Task<IList<ChartOfAccountModel>> PrepareChartOfAccount(IEnumerable<Account> accounts, Guid? rootAccountHeadId, int level, int levelsToLoad)
        {
            var result = new List<ChartOfAccountModel>();
            foreach (var account in GetChildrenByParentId(rootAccountHeadId, accounts))
            {
                var accountModel = new ChartOfAccountModel()
                {
                    Id = account.Id,
                    Code = account.Code,
                    Type = account.AccountType?.Name,
                    Level = account.Level,
                    Name = account.Name,
                    ParentId = account.ParentId
                };

                //load sub accounts
                bool loadSubAccounts = true;
                if (levelsToLoad <= level)
                {
                    loadSubAccounts = false;
                }
                if (loadSubAccounts)
                {
                    var subAccounts = await PrepareChartOfAccount(accounts, account.Id, level + 1, levelsToLoad);
                    accountModel.SubAccountHeads.AddRange(subAccounts);
                }
                result.Add(accountModel);
            }
            return result;
        }

        public async Task<decimal> CalculateBalanceByAccountId(Guid accountId, Guid accountTypeId, Guid financialYearId, DateTime? fromDate, DateTime? toDate)
        {
            var accountIds = await GetAllControlAccountIdsByAccountId(accountId);
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.FinancialYearId == financialYearId).Where(x => accountIds.Contains(x.AccountId));
            if (fromDate.HasValue && toDate.HasValue)
                transactions = transactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
            if (accountTypeId == Guid.Parse(AccountTypeConstants.NonCurrentLiabilities)
               || accountTypeId == Guid.Parse(AccountTypeConstants.CurrentLiabilities)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OwnersEquity)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OthersEquity)
               || accountTypeId == Guid.Parse(AccountTypeConstants.Revenue)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OtherIncome))
                return transactions.Sum(x => x.Credit) - transactions.Sum(x => x.Debit);
            return transactions.Sum(x => x.Debit) - transactions.Sum(x => x.Credit);
        }
        private async Task<decimal> CalculateBalanceByAccountId(IList<Transaction> transactionList, IEnumerable<Account> allAccountHeads, Guid accountId, Guid accountTypeId, Guid financialYearId, DateTime? fromDate, DateTime? toDate)
        {
            var accountIds = await GetAllControlAccountIdsByAccountId(allAccountHeads, accountId);
            var transactions = transactionList.Where(x => accountIds.Contains(x.AccountId));
            if (fromDate.HasValue && toDate.HasValue)
                transactions = transactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
            if (accountTypeId == Guid.Parse(AccountTypeConstants.NonCurrentLiabilities)
               || accountTypeId == Guid.Parse(AccountTypeConstants.CurrentLiabilities)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OwnersEquity)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OthersEquity)
               || accountTypeId == Guid.Parse(AccountTypeConstants.Revenue)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OtherIncome))
                return transactions.Sum(x => x.Credit) - transactions.Sum(x => x.Debit);
            return transactions.Sum(x => x.Debit) - transactions.Sum(x => x.Credit);
        }
        private async Task<decimal> CalculateBalanceByAccount(IList<Transaction> transactionList, IEnumerable<Account> allAccountHeads, Account account, Guid accountTypeId, Guid financialYearId, DateTime? fromDate, DateTime? toDate)
        {
            var accountIds = await GetAllControlAccountIdsByAccount(allAccountHeads, account);
            var transactions = transactionList.Where(x => accountIds.Contains(x.AccountId));
            if (fromDate.HasValue && toDate.HasValue)
                transactions = transactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
            if (accountTypeId == Guid.Parse(AccountTypeConstants.NonCurrentLiabilities)
               || accountTypeId == Guid.Parse(AccountTypeConstants.CurrentLiabilities)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OwnersEquity)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OthersEquity)
               || accountTypeId == Guid.Parse(AccountTypeConstants.Revenue)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OtherIncome))
                return transactions.Sum(x => x.Credit) - transactions.Sum(x => x.Debit);
            return transactions.Sum(x => x.Debit) - transactions.Sum(x => x.Credit);
        }

        //ledger
        public async Task<IList<SubsidiaryLedgerViewModel>> SubsidiaryLedgerLegacy(Guid accountId, DateTime? fromDate, DateTime? toDate, Guid? costCenterId = null)
        {
            var subsidiaryLedgerReportLines = new List<SubsidiaryLedgerViewModel>();
            var account = await _unitOfWork.Repository<Account>().FindAsync(accountId);
            var financialYear = await _unitOfWork.Repository<FinancialYear>().TableNoTracking().FirstOrDefaultAsync(x => x.IsActive);

            var allTransactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountId == accountId)
                .Where(x => x.FinancialYearId == financialYear!.Id);
            if (costCenterId.HasValue)
            {
                allTransactions = allTransactions.Where(x => x.CostCenterId == costCenterId.Value);
            }

            //opening balance
            var openingBalanceTransactions = allTransactions.Where(x => x.IsOpeningBalance);
            var accountTransactions = allTransactions.Where(x => !x.IsOpeningBalance);
            IQueryable<Transaction> transactionsUptoFromDate = new List<Transaction>().AsQueryable();
            if (fromDate.HasValue)
                transactionsUptoFromDate = accountTransactions.Where(x => x.TransactionDate.Date >= financialYear!.StartDate.Date && x.TransactionDate.Date < fromDate.Value.Date);
            //this period transactions
            if (fromDate.HasValue && toDate.HasValue)
                accountTransactions = accountTransactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
            var openingCredit = openingBalanceTransactions.Sum(x => x.Credit) +
                    (transactionsUptoFromDate.Any() ? transactionsUptoFromDate.Sum(x => x.Credit) : 0M);
            var openingDebit = openingBalanceTransactions.Sum(x => x.Debit) +
                    (transactionsUptoFromDate.Any() ? transactionsUptoFromDate.Sum(x => x.Debit) : 0M);
            accountTransactions = accountTransactions.OrderBy(x => x.TransactionDate);

            if (account.AccountTypeId == Guid.Parse(AccountTypeConstants.NonCurrentLiabilities)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.CurrentLiabilities)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.OwnersEquity)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.OthersEquity)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.Revenue)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.OtherIncome))
            {
                subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                {
                    Vnumber = "",
                    VoucherDate = null,
                    AccountName = "",
                    Particular = "Opening Balance",
                    Debit = 0m,
                    Credit = 0m,
                    Balance = openingCredit - openingDebit
                });
                foreach (var item in accountTransactions)
                {
                    openingCredit += item.Credit;
                    openingDebit += item.Debit;
                    subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                    {
                        Vnumber = item.Vnumber,
                        VoucherDate = item.TransactionDate.ToString("dd/MM/yyyy"),
                        //AccountName = await GetContraAccountName(item),
                        AccountName = item.ContraAccountNames,
                        Particular = item.Description,
                        Debit = item.Debit,
                        Credit = item.Credit,
                        Balance = openingCredit - openingDebit
                    });
                }

            }
            else
            {
                subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                {
                    Vnumber = "",
                    VoucherDate = "",
                    AccountName = "",
                    Particular = "Opening Balance",
                    Debit = 0m,
                    Credit = 0m,
                    Balance = openingDebit - openingCredit
                });
                foreach (var item in accountTransactions)
                {
                    openingCredit += item.Credit;
                    openingDebit += item.Debit;

                    subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                    {
                        Vnumber = item.Vnumber,
                        VoucherDate = item.TransactionDate.ToString("dd/MM/yyyy"),
                        //AccountName = await GetContraAccountName(item),
                        AccountName = item.ContraAccountNames,
                        Particular = item.Description,
                        Debit = item.Debit,
                        Credit = item.Credit,
                        Balance = openingDebit - openingCredit
                    });
                }
            }

            return subsidiaryLedgerReportLines;
        }
        public async Task<IList<SubsidiaryLedgerViewModel>> SubsidiaryLedger(Guid accountId, DateTime? fromDate, DateTime? toDate, Guid? costCenterId = null)
        {
            var subsidiaryLedgerReportLines = new List<SubsidiaryLedgerViewModel>();
            var account = await _unitOfWork.Repository<Account>().FindAsync(accountId);
            var financialYears = await _unitOfWork.Repository<FinancialYear>().TableNoTracking()
                     .Where(fy => (fromDate == null || fy.EndDate >= fromDate)
                     && (toDate == null || fy.StartDate <= toDate)).ToListAsync();
            var financialYearIds = financialYears.Select(x => x.Id);
            var initialFinancialYear = financialYears.OrderBy(x => x.StartDate).FirstOrDefault();
            var transactionsQyeryable = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountId == accountId)
                .Where(x => financialYearIds.Contains(x.FinancialYearId));
            if (costCenterId.HasValue)
            {
                transactionsQyeryable = transactionsQyeryable.Where(x => x.CostCenterId == costCenterId.Value);
            }
            var accountTransactions = transactionsQyeryable.Where(x => !x.IsPreviousYearClosingBalance);
            //opening transactions
            IQueryable<Transaction> openingTransactions = new List<Transaction>().AsQueryable();
            if (fromDate.HasValue)
            {
                if (fromDate.Value.Date.Equals(initialFinancialYear!.StartDate.Date) == false)
                {
                    openingTransactions = transactionsQyeryable.Where(x => x.TransactionDate.Date >= initialFinancialYear!.StartDate.Date && x.TransactionDate.Date < fromDate.Value.Date);
                }
                else
                {
                    openingTransactions = transactionsQyeryable.Where(x => x.FinancialYearId == initialFinancialYear.Id && x.IsOpeningBalance);
                }

            }
            //this period transactions
            if (fromDate.HasValue && toDate.HasValue)
                accountTransactions = accountTransactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);

            var openingCredit = openingTransactions.Sum(x => x.Credit);

            var openingDebit = openingTransactions.Sum(x => x.Debit);

            accountTransactions = accountTransactions.OrderBy(x => x.TransactionDate).ThenBy(x => x.CreatedOn);

            if (account.AccountTypeId == Guid.Parse(AccountTypeConstants.NonCurrentLiabilities)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.CurrentLiabilities)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.OwnersEquity)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.OthersEquity)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.Revenue)
               || account.AccountTypeId == Guid.Parse(AccountTypeConstants.OtherIncome))
            {
                subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                {
                    Vnumber = "",
                    VoucherDate = null,
                    AccountName = "",
                    Particular = "Opening Balance",
                    Debit = 0m,
                    Credit = 0m,
                    Balance = openingCredit - openingDebit
                });
                foreach (var item in accountTransactions)
                {
                    openingCredit += item.Credit;
                    openingDebit += item.Debit;
                    subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                    {
                        Vnumber = item.Vnumber,
                        VoucherDate = item.TransactionDate.ToString("dd/MM/yyyy"),
                        //AccountName = await GetContraAccountName(item),
                        AccountName = item.ContraAccountNames,
                        Particular = item.Description,
                        Debit = item.Debit,
                        Credit = item.Credit,
                        Balance = openingCredit - openingDebit
                    });
                }

            }
            else
            {
                subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                {
                    Vnumber = "",
                    VoucherDate = "",
                    AccountName = "",
                    Particular = "Opening Balance",
                    Debit = 0m,
                    Credit = 0m,
                    Balance = openingDebit - openingCredit
                });
                foreach (var item in accountTransactions)
                {
                    openingCredit += item.Credit;
                    openingDebit += item.Debit;

                    subsidiaryLedgerReportLines.Add(new SubsidiaryLedgerViewModel
                    {
                        Vnumber = item.Vnumber,
                        VoucherDate = item.TransactionDate.ToString("dd/MM/yyyy"),
                        //AccountName = await GetContraAccountName(item),
                        AccountName = item.ContraAccountNames,
                        Particular = item.Description,
                        Debit = item.Debit,
                        Credit = item.Credit,
                        Balance = openingDebit - openingCredit
                    });
                }
            }

            return subsidiaryLedgerReportLines;
        }


        public async Task<IList<CustomerLedgerProductWiseViewModel>> CustomerLedgerProductWise(Guid customerId, Guid? costCenterId, DateTime? fromDate, DateTime? toDate)
        {
            var customerLedgerReportLines = new List<CustomerLedgerProductWiseViewModel>();
            var financialYears = await _unitOfWork.Repository<FinancialYear>().TableNoTracking()
                     .Where(fy => (fromDate == null || fy.EndDate >= fromDate)
                     && (toDate == null || fy.StartDate <= toDate)).ToListAsync();
            var financialYearIds = financialYears.Select(x => x.Id);
            var initialFinancialYear = financialYears.OrderBy(x => x.StartDate).FirstOrDefault();
            var transactionsQyeryable = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountId == customerId)
                .Where(x => financialYearIds.Contains(x.FinancialYearId));
            if (costCenterId.HasValue)
            {
                transactionsQyeryable = transactionsQyeryable.Where(x => x.CostCenterId == costCenterId.Value);
            }
            var accountTransactions = transactionsQyeryable.Where(x => !x.IsPreviousYearClosingBalance);
            //opening transactions
            IQueryable<Transaction> openingTransactions = new List<Transaction>().AsQueryable();
            if (fromDate.HasValue)
            {
                if (fromDate.Value.Date.Equals(initialFinancialYear!.StartDate.Date) == false)
                {
                    openingTransactions = transactionsQyeryable.Where(x => x.TransactionDate.Date >= initialFinancialYear!.StartDate.Date && x.TransactionDate.Date < fromDate.Value.Date);
                }
                else
                {
                    openingTransactions = transactionsQyeryable.Where(x => x.FinancialYearId == initialFinancialYear.Id && x.IsOpeningBalance);
                }

            }
            //this period transactions
            if (fromDate.HasValue && toDate.HasValue)
                accountTransactions = accountTransactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
            var thisPeriodTransactions = from tr in accountTransactions
                                         join si in _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Include(x => x.SaleInvoiceDetails) on tr.Vnumber equals si.InvoiceNo into transaction_Invoices
                                         from transactionInvoice in transaction_Invoices.DefaultIfEmpty()
                                         join sid in _unitOfWork.Repository<SaleInvoiceDetail>().TableNoTracking() on transactionInvoice.Id equals sid.SaleInvoiceId into sale_SaleInvoiceDetails
                                         from sale_SaleInvoiceDetail in sale_SaleInvoiceDetails.DefaultIfEmpty()
                                         join product in _unitOfWork.Repository<Product>().TableNoTracking() on sale_SaleInvoiceDetail.ProductId equals product.Id into detailProducts
                                         from detailProduct in detailProducts.DefaultIfEmpty()
                                         select new
                                         {
                                             BillNo = tr.Vnumber,
                                             Date = tr.TransactionDate,
                                             Description = detailProduct == null ? tr.Description : detailProduct.Name,
                                             Qty = sale_SaleInvoiceDetail == null ? 0 : sale_SaleInvoiceDetail.Quantity,
                                             TP = sale_SaleInvoiceDetail == null ? 0 : sale_SaleInvoiceDetail.Rate,
                                             Commission = sale_SaleInvoiceDetail == null ? 0 : sale_SaleInvoiceDetail.DiscountPerUnit,
                                             OfferDiscountPerUnit = sale_SaleInvoiceDetail == null ? 0 : sale_SaleInvoiceDetail.OfferDiscountPerUnit,
                                             OtherDiscountPerUnit = sale_SaleInvoiceDetail == null ? 0 : sale_SaleInvoiceDetail.OtherDiscountPerUnit,
                                             // IMPORTANT:
                                             // DiscountPerUnit = InvoiceDiscountPerUnit + CashDiscountPerUnit + SpecialDiscountPerUnit
                                             // sale_SaleInvoiceDetail.NetRate (DB) = Rate - DiscountPerUnit - OfferDiscountPerUnit
                                             // 'OtherDiscountPerUnit' is calculated separately and NOT included in sale_SaleInvoiceDetail.NetRate
                                             // For PDF Report, to display final 'NetRate' need to subtract 'OtherDiscountPerUnit'
                                             NetRate = sale_SaleInvoiceDetail == null ? 0 : sale_SaleInvoiceDetail.NetRate - sale_SaleInvoiceDetail.OtherDiscountPerUnit,
                                             TPAmount = sale_SaleInvoiceDetail == null ? 0 : sale_SaleInvoiceDetail.Rate * sale_SaleInvoiceDetail.Quantity,
                                             CommissionAmount = sale_SaleInvoiceDetail == null ? 0 : sale_SaleInvoiceDetail.DiscountPerUnit * sale_SaleInvoiceDetail.Quantity,
                                             OfferDiscountAmount = sale_SaleInvoiceDetail == null ? 0 : sale_SaleInvoiceDetail.OfferDiscountPerUnit * sale_SaleInvoiceDetail.Quantity,
                                             OtherDiscountAmount = sale_SaleInvoiceDetail == null ? 0 : sale_SaleInvoiceDetail.OtherDiscountPerUnit * sale_SaleInvoiceDetail.Quantity,
                                             // IMPORTANT:
                                             // NetRateAmount = (Quantity × DB NetRate) - (Quantity × OtherDiscountPerUnit)
                                             // Beacuse OtherDiscountPerUnit is not included in DB NetRate
                                             NetRateAmount = sale_SaleInvoiceDetail == null ? 0 : (sale_SaleInvoiceDetail.NetRate * sale_SaleInvoiceDetail.Quantity) - (sale_SaleInvoiceDetail.OtherDiscountPerUnit * sale_SaleInvoiceDetail.Quantity),
                                             DepoChargePerKg = sale_SaleInvoiceDetail == null ? 0 : transactionInvoice.DepoCharge / transactionInvoice.SaleInvoiceDetails.Sum(x => x.Quantity),
                                             TransportationCostPerUnit = sale_SaleInvoiceDetail == null ? 0 : sale_SaleInvoiceDetail.TransportationCostPerUnit,
                                             Paid = tr.Credit,
                                             Debit = (transactionInvoice == null && sale_SaleInvoiceDetail == null) ? tr.Debit : (sale_SaleInvoiceDetail.NetRate * sale_SaleInvoiceDetail.Quantity) + (transactionInvoice.DepoCharge / transactionInvoice.SaleInvoiceDetails.Sum(x => x.Quantity) * sale_SaleInvoiceDetail.Quantity) + (sale_SaleInvoiceDetail.TransportationCostPerUnit * sale_SaleInvoiceDetail.Quantity) - (sale_SaleInvoiceDetail.OtherDiscountPerUnit * sale_SaleInvoiceDetail.Quantity),
                                             Credit = tr.Credit
                                         };
            thisPeriodTransactions = thisPeriodTransactions.OrderBy(x => x.Date).ThenBy(x => x.BillNo);
            var openingCredit = openingTransactions.Sum(x => x.Credit);
            var openingDebit = openingTransactions.Sum(x => x.Debit);
            customerLedgerReportLines.Add(new CustomerLedgerProductWiseViewModel
            {
                Date = "",
                Description = "Opening Balance",
                Balance = openingDebit - openingCredit
            });
            foreach (var item in thisPeriodTransactions)
            {
                openingCredit += item.Credit;
                openingDebit += item.Debit;
                customerLedgerReportLines.Add(new CustomerLedgerProductWiseViewModel
                {
                    BillNo = item.BillNo,
                    Date = item.Date.ToString("dd/MM/yyyy"),
                    Description = item.Description,
                    Qty = item.Qty,
                    TP = item.TP,
                    Commission = item.Commission,
                    OfferDiscountPerUnit = item.OfferDiscountPerUnit,
                    OtherDiscountPerUnit = item.OtherDiscountPerUnit,
                    NetRate = item.NetRate,
                    TPAmount = item.TPAmount,
                    CommissionAmount = item.CommissionAmount,
                    OfferDiscountAmount = item.OfferDiscountAmount,
                    OtherDiscountAmount = item.OtherDiscountAmount,
                    NetRateAmount = item.NetRateAmount,
                    //OtherDiscountPerUnit = item.OtherDiscountPerUnit,
                    TransportationCostPerUnit = item.TransportationCostPerUnit,
                    DepoChargePerKg = item.DepoChargePerKg,
                    Paid = item.Paid,
                    Debit = item.Debit,
                    Credit = item.Credit,
                    Balance = openingDebit - openingCredit
                });
            }
            return customerLedgerReportLines;
        }

        public async Task<IList<SupplierLedgerProductWiseViewModel>> SupplierLedgerProductWise(Guid supplierId, Guid? costCenterId, DateTime? fromDate, DateTime? toDate)
        {
            var supplierLedgerReportLines = new List<SupplierLedgerProductWiseViewModel>();
            var financialYears = await _unitOfWork.Repository<FinancialYear>().TableNoTracking()
                     .Where(fy => (fromDate == null || fy.EndDate >= fromDate)
                     && (toDate == null || fy.StartDate <= toDate)).ToListAsync();
            var financialYearIds = financialYears.Select(x => x.Id);
            var initialFinancialYear = financialYears.OrderBy(x => x.StartDate).FirstOrDefault();
            var transactionsQyeryable = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountId == supplierId)
                .Where(x => financialYearIds.Contains(x.FinancialYearId));
            if (costCenterId.HasValue)
            {
                transactionsQyeryable = transactionsQyeryable.Where(x => x.CostCenterId == costCenterId.Value);
            }
            var accountTransactions = transactionsQyeryable.Where(x => !x.IsPreviousYearClosingBalance);
            //opening transactions
            IQueryable<Transaction> openingTransactions = new List<Transaction>().AsQueryable();
            if (fromDate.HasValue)
            {
                if (fromDate.Value.Date.Equals(initialFinancialYear!.StartDate.Date) == false)
                {
                    openingTransactions = transactionsQyeryable.Where(x => x.TransactionDate.Date >= initialFinancialYear!.StartDate.Date && x.TransactionDate.Date < fromDate.Value.Date);
                }
                else
                {
                    openingTransactions = transactionsQyeryable.Where(x => x.FinancialYearId == initialFinancialYear.Id && x.IsOpeningBalance);
                }

            }
            //this period transactions
            if (fromDate.HasValue && toDate.HasValue)
                accountTransactions = accountTransactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
            var thisPeriodTransactions = from tr in accountTransactions
                                         join pi in _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking() on tr.Vnumber equals pi.PurchaseInvoiceNo into transaction_Invoices
                                         from transactionInvoice in transaction_Invoices.DefaultIfEmpty()
                                         join pid in _unitOfWork.Repository<PurchaseInvoiceDetail>().TableNoTracking() on transactionInvoice.Id equals pid.PurchaseInvoiceId into purchase_PurchaseInvoiceDetails
                                         from purchase_PurchaseInvoiceDetail in purchase_PurchaseInvoiceDetails.DefaultIfEmpty()
                                         join product in _unitOfWork.Repository<Product>().TableNoTracking() on purchase_PurchaseInvoiceDetail.ProductId equals product.Id into detailProducts
                                         from detailProduct in detailProducts.DefaultIfEmpty()
                                         select new
                                         {
                                             BillNo = tr.Vnumber,
                                             Date = tr.TransactionDate,
                                             Description = detailProduct == null ? tr.Description : detailProduct.Name,
                                             PO = transactionInvoice == null ? "" : transactionInvoice.Ponumber,
                                             Qty = purchase_PurchaseInvoiceDetail == null ? 0 : purchase_PurchaseInvoiceDetail.Grnquantity,
                                             Rate = purchase_PurchaseInvoiceDetail == null ? 0 : purchase_PurchaseInvoiceDetail.Rate,
                                             Amount = purchase_PurchaseInvoiceDetail == null ? 0 : purchase_PurchaseInvoiceDetail.Amount,
                                             Paid = tr.Debit,
                                             Debit = tr.Debit,
                                             Credit = (transactionInvoice == null && purchase_PurchaseInvoiceDetail == null) ? tr.Credit : (purchase_PurchaseInvoiceDetail.Amount),
                                         };
            thisPeriodTransactions = thisPeriodTransactions.OrderBy(x => x.Date).ThenBy(x => x.BillNo);
            var openingCredit = openingTransactions.Sum(x => x.Credit);
            var openingDebit = openingTransactions.Sum(x => x.Debit);
            supplierLedgerReportLines.Add(new SupplierLedgerProductWiseViewModel
            {
                Date = "",
                Description = "Opening Balance",
                Balance = openingCredit - openingDebit
            });
            foreach (var item in thisPeriodTransactions)
            {
                openingCredit += item.Credit;
                openingDebit += item.Debit;
                supplierLedgerReportLines.Add(new SupplierLedgerProductWiseViewModel
                {
                    BillNo = item.BillNo,
                    Date = item.Date.ToString("dd/MM/yyyy"),
                    Description = item.Description,
                    PO = item.PO,
                    Qty = item.Qty,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    Paid = item.Paid,
                    Debit = item.Debit,
                    Credit = item.Credit,
                    Balance = openingCredit - openingDebit
                });
            }
            return supplierLedgerReportLines;
        }

        private async Task<IList<TrialBalanceViewModel>> TrialBalance(IList<Transaction> transactionList, IEnumerable<Account> allAccountHeads, Guid? rootAccountHeadId, int level, int levelsToLoad, FinancialYear financialYear, DateTime? fromDate, DateTime? toDate)
        {
            var result = new List<TrialBalanceViewModel>();
            var childAccounts = GetChildrenByParentId(rootAccountHeadId, allAccountHeads);
            foreach (var account in childAccounts)
            {
                var hasTransactionEntry = transactionList.Any(x => x.AccountId == account.Id);
                if (account.IsControlAccount && !hasTransactionEntry) continue;
                var trialBalanceReportLine = await PrepareTrialBalanceReportLine(transactionList, allAccountHeads, account, financialYear, fromDate, toDate);
                result.Add(trialBalanceReportLine);
                //load sub accounts
                bool loadSubAccounts = true;
                if (levelsToLoad <= level)
                {
                    loadSubAccounts = false;
                }
                if (loadSubAccounts)
                {
                    var subAccounts = await TrialBalance(transactionList, allAccountHeads, account.Id, level + 1, levelsToLoad, financialYear, fromDate, toDate);
                    result.AddRange(subAccounts);
                }
            }
            return result;
        }


        //trial balance
        public async Task<IList<TrialBalanceViewModel>> TrialBalanceReportPrint(AccountReportRequestModel requestModel)
        {
            var financialYear = await _unitOfWork.Repository<FinancialYear>().TableNoTracking().FirstAsync(x => x.Id == requestModel.FinancialYearId);
            var allAccountHeads = await _unitOfWork.Repository<Account>().TableNoTracking().ToListAsync();
            var transactionList = await _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.FinancialYearId == financialYear.Id).ToListAsync();
            return await TrialBalance(transactionList, allAccountHeads, requestModel.RootAccountHeadId, requestModel.Level, requestModel.LevelsToLoad, financialYear, requestModel.FromDate, requestModel.ToDate);
        }


        private async Task<IList<AccountHeadWithBalanceViewModel>> PrepareAccountHeadModelsWithBalance(IList<Transaction> transactionList, IEnumerable<Account> allAccountHeads, IEnumerable<Account> accounts, Guid? rootAccountHeadId, int level, int levelsToLoad, Guid financialYearId, DateTime? fromDate, DateTime? toDate, decimal profitLoss = 0)
        {
            var result = new List<AccountHeadWithBalanceViewModel>();
            var childAccounts = GetChildrenByParentId(rootAccountHeadId, accounts);
            foreach (var account in childAccounts)
            {
                var hasTransactionEntry = transactionList.Any(x => x.AccountId == account.Id);
                if (account.IsControlAccount && !hasTransactionEntry) continue;
                var accBalance = await CalculateBalanceByAccount(transactionList, allAccountHeads, account, account.AccountTypeId, financialYearId, fromDate, toDate);
                if (account.Id == Guid.Parse(AccountHeadConstants.OwnersEquity))
                {
                    accBalance = accBalance + profitLoss;
                }
                var accountModel = new AccountHeadWithBalanceViewModel()
                {
                    Id = account.Id,
                    Code = account.Code,
                    Name = account.Name,
                    Level = account.Level,
                    ParentId = account.ParentId,
                    Balance = accBalance
                };

                //load sub accounts
                bool loadSubAccounts = true;
                if (levelsToLoad <= level)
                {
                    loadSubAccounts = false;
                }
                if (loadSubAccounts)
                {
                    result.Add(accountModel);
                    var subAccounts = await PrepareAccountHeadModelsWithBalance(transactionList, allAccountHeads, accounts, account.Id, level + 1, levelsToLoad, financialYearId, fromDate, toDate, profitLoss);
                    result.AddRange(subAccounts);
                }

            }
            return result;
        }


        //balance sheet
        public async Task<(IList<AccountHeadWithBalanceViewModel> nonCurrentAssetHeads, IList<AccountHeadWithBalanceViewModel> currentAssetHeads, IList<AccountHeadWithBalanceViewModel> nonCurrentLiabilityHeads, IList<AccountHeadWithBalanceViewModel> currentLiabilityHeads, IList<AccountHeadWithBalanceViewModel> ownersEquityHeads, IList<AccountHeadWithBalanceViewModel> othersEquityHeads, RootAccountBalance rootAccount)> BalanceSheet(int level, int levelsToLoad, DateTime? fromDate, DateTime? toDate, Guid filterFinancialYearId)
        {
            var financialYear = _unitOfWork.Repository<FinancialYear>().TableNoTracking().Single(x => x.Id == filterFinancialYearId);
            fromDate = financialYear.StartDate.Date;
            var financialYearId = financialYear.Id;
            //all accounts
            var allaccountHeads = await _unitOfWork.Repository<Account>().TableNoTracking().ToListAsync();
            //transactions
            var transactionList = await _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.FinancialYearId == financialYearId).ToListAsync();
            //non current asset
            var nonCurrentAssetsHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.NonCurrentAsset));
            var nonCurrentAssetHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, nonCurrentAssetsHeadsList, Guid.Parse(AccountHeadConstants.NonCurrentAsset), level, levelsToLoad, financialYearId, fromDate, toDate);
            //current asset
            var currentAssetHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.CurrentAsset));
            var currentAssetHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, currentAssetHeadsList, Guid.Parse(AccountHeadConstants.CurrentAsset), level, levelsToLoad, financialYearId, fromDate, toDate);
            //non current liability
            var nonCurrentLiabilityHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.NonCurrentLiabilities));
            var nonCurrentLiabilityHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, nonCurrentLiabilityHeadsList, Guid.Parse(AccountHeadConstants.NonCurrentLiabilities), level, levelsToLoad, financialYearId, fromDate, toDate);
            //current liability
            var currentLiabilityHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.CurrentLiabilities));
            var currentLiabilityHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, currentLiabilityHeadsList, Guid.Parse(AccountHeadConstants.CurrentLiabilities), level, levelsToLoad, financialYearId, fromDate, toDate);
            //owners equity
            var ownersEquityHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.OwnersEquity));
            var ownersEquityHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, ownersEquityHeadsList, Guid.Parse(AccountHeadConstants.OwnersEquity), level, levelsToLoad, financialYearId, fromDate, toDate);
            //others equity
            var othersEquityHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.OthersEquity));
            var othersEquityHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, othersEquityHeadsList, Guid.Parse(AccountHeadConstants.OthersEquity), level, levelsToLoad, financialYearId, fromDate, toDate);
            //balance sheet item
            var nonCurrentAsset = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.NonCurrentAsset), Guid.Parse(AccountTypeConstants.NonCurrentAsset), financialYearId, fromDate, toDate);
            var currentAsset = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.CurrentAsset), Guid.Parse(AccountTypeConstants.CurrentAsset), financialYearId, fromDate, toDate);
            var nonCurrentLiability = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.NonCurrentLiabilities), Guid.Parse(AccountTypeConstants.NonCurrentLiabilities), financialYearId, fromDate, toDate);
            var currentLiability = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.CurrentLiabilities), Guid.Parse(AccountTypeConstants.CurrentLiabilities), financialYearId, fromDate, toDate);
            var OwnersEquity = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.OwnersEquity), Guid.Parse(AccountTypeConstants.OwnersEquity), financialYearId, fromDate, toDate);
            var OthersEquity = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.OthersEquity), Guid.Parse(AccountTypeConstants.OthersEquity), financialYearId, fromDate, toDate);
            //income statement item
            var revenue = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.Revenue), Guid.Parse(AccountTypeConstants.Revenue), financialYearId, fromDate, toDate);
            var cogs = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.CostOfGoodsSold), Guid.Parse(AccountTypeConstants.CostOfGoodsSold), financialYearId, fromDate, toDate);
            var operatingExpenses = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.OperatingExpenses), Guid.Parse(AccountTypeConstants.OperatingExpenses), financialYearId, fromDate, toDate);
            var otherIncome = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.OtherIncome), Guid.Parse(AccountTypeConstants.OtherIncome), financialYearId, fromDate, toDate);
            var nonOperatingExpenses = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.NonOperatingExpenses), Guid.Parse(AccountTypeConstants.NonOperatingExpenses), financialYearId, fromDate, toDate);
            var csrFund = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.ContributiontoCSRFund), Guid.Parse(AccountTypeConstants.ContributiontoCSRFund), financialYearId, fromDate, toDate);
            var tax = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.CurrentTax), Guid.Parse(AccountTypeConstants.CurrentTax), financialYearId, fromDate, toDate);
            var pl = (revenue + otherIncome) - (cogs + operatingExpenses + nonOperatingExpenses + csrFund + tax);
            var rootAccountBalance = new RootAccountBalance
            {
                NonCurrentAsset = nonCurrentAsset,
                CurrentAsset = currentAsset,
                NonCurrentLiability = nonCurrentLiability,
                CurrentLiability = currentLiability,
                OwnersEquity = OwnersEquity + pl,
                OthersEquity = OthersEquity,
                PL = pl
            };

            return (nonCurrentAssetHeads, currentAssetHeads, nonCurrentLiabilityHeads, currentLiabilityHeads, ownersEquityHeads, othersEquityHeads, rootAccountBalance);

        }

        //income statement
        public async Task<(IList<AccountHeadWithBalanceViewModel> revenueHeads, IList<AccountHeadWithBalanceViewModel> cogsHeads, IList<AccountHeadWithBalanceViewModel> operatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> otherIncomeHeads, IList<AccountHeadWithBalanceViewModel> nonOperatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> csrFundHeads, IList<AccountHeadWithBalanceViewModel> taxHeads, RootAccountBalance rootAccount)> IncomeStatement(int level, int levelsToLoad, DateTime? fromDate, DateTime? toDate, Guid filterFinancialYearId)
        {
            var financialYearId = _unitOfWork.Repository<FinancialYear>().TableNoTracking().First(x => x.Id == filterFinancialYearId).Id;
            //all accounts
            var allaccountHeads = await _unitOfWork.Repository<Account>().TableNoTracking().ToListAsync();
            //transactions
            var transactionList = await _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.FinancialYearId == financialYearId).ToListAsync();
            //revenue
            var revenueHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.Revenue));
            var revenueHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, revenueHeadsList, Guid.Parse(AccountHeadConstants.Revenue), level, levelsToLoad, financialYearId, fromDate, toDate);
            //cogs
            var cogsHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.CostOfGoodsSold));
            var cogsHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, cogsHeadsList, Guid.Parse(AccountHeadConstants.CostOfGoodsSold), level, levelsToLoad, financialYearId, fromDate, toDate);
            //operating expenses
            var operatingExpensesHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.OperatingExpenses));
            var operatingExpensesHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, operatingExpensesHeadsList, Guid.Parse(AccountHeadConstants.OperatingExpenses), level, levelsToLoad, financialYearId, fromDate, toDate);
            //other income
            var otherIncomeHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.OtherIncome));
            var otherIncomeHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, otherIncomeHeadsList, Guid.Parse(AccountHeadConstants.OtherIncome), level, levelsToLoad, financialYearId, fromDate, toDate);
            //non operating expenes
            var nonOperatingExpensesHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.NonOperatingExpenses));
            var nonOperatingExpensesHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, nonOperatingExpensesHeadsList, Guid.Parse(AccountHeadConstants.NonOperatingExpenses), level, levelsToLoad, financialYearId, fromDate, toDate);
            //csr fund
            var csrFundHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.ContributiontoCSRFund));
            var csrFundHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, csrFundHeadsList, Guid.Parse(AccountHeadConstants.ContributiontoCSRFund), level, levelsToLoad, financialYearId, fromDate, toDate);
            //tax
            var taxHeadsList = allaccountHeads.Where(x => x.AccountTypeId == Guid.Parse(AccountTypeConstants.CurrentTax));
            var taxHeads = await PrepareAccountHeadModelsWithBalance(transactionList, allaccountHeads, taxHeadsList, Guid.Parse(AccountHeadConstants.CurrentTax), level, levelsToLoad, financialYearId, fromDate, toDate);
            //root accounts balance
            var revenue = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.Revenue), Guid.Parse(AccountTypeConstants.Revenue), financialYearId, fromDate, toDate);
            var cogs = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.CostOfGoodsSold), Guid.Parse(AccountTypeConstants.CostOfGoodsSold), financialYearId, fromDate, toDate);
            var operatingExpenses = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.OperatingExpenses), Guid.Parse(AccountTypeConstants.OperatingExpenses), financialYearId, fromDate, toDate);
            var otherIncome = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.OtherIncome), Guid.Parse(AccountTypeConstants.OtherIncome), financialYearId, fromDate, toDate);
            var nonOperatingExpenses = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.NonOperatingExpenses), Guid.Parse(AccountTypeConstants.NonOperatingExpenses), financialYearId, fromDate, toDate);
            var csrFund = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.ContributiontoCSRFund), Guid.Parse(AccountTypeConstants.ContributiontoCSRFund), financialYearId, fromDate, toDate);
            var tax = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.CurrentTax), Guid.Parse(AccountTypeConstants.CurrentTax), financialYearId, fromDate, toDate);
            var pl = (revenue + otherIncome) - (cogs + operatingExpenses + nonOperatingExpenses + csrFund + tax);
            //others
            var directExpenses_Standard = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.DirectExpenses_Standard), Guid.Parse(AccountTypeConstants.CostOfGoodsSold), financialYearId, fromDate, toDate);
            var factoryOverhead_Standard = await CalculateBalanceByAccountId(transactionList, allaccountHeads, Guid.Parse(AccountHeadConstants.FactoryOverhead_Standard), Guid.Parse(AccountTypeConstants.CostOfGoodsSold), financialYearId, fromDate, toDate);
            var rootAccountBalance = new RootAccountBalance
            {
                Revenue = revenue,
                Cogs = cogs,
                OperatingExpenses = operatingExpenses,
                OtherIncome = otherIncome,
                NonOperatingExpenses = nonOperatingExpenses,
                CsrFund = csrFund,
                Tax = tax,
                PL = pl,
                DirectExpenses_Standard = directExpenses_Standard,
                FactoryOverhead_Standard = factoryOverhead_Standard
            };
            return (revenueHeads, cogsHeads, operatingExpensesHeads, otherIncomeHeads, nonOperatingExpensesHeads, csrFundHeads, taxHeads, rootAccountBalance);

        }


        public async Task<List<SupplierTransactionLedgerViewModel>> PrepareSupplierTransactionLedger(DateTime? fromDate, DateTime? toDate)
        {
            // Initialize result list
            var result = new List<SupplierTransactionLedgerViewModel>();

            // Fetch financial years based on date range
            var financialYears = await _unitOfWork.Repository<FinancialYear>().TableNoTracking()
                .Where(fy => (fromDate == null || fy.EndDate >= fromDate)
                && (toDate == null || fy.StartDate <= toDate)).ToListAsync();

            var financialYearIds = financialYears.Select(x => x.Id);
            var initialFinancialYear = financialYears.OrderBy(x => x.StartDate).FirstOrDefault();

            // Get accounts related to Trade Payable
            var accounts = await GetControlAccountListByAccountId(AccountHeadConstants.TradePayable.ToGuid());
            // Fetch all relevant transactions in bulk
            var allTransactions = await _unitOfWork.Repository<Transaction>().TableNoTracking()
                .Where(x => financialYearIds.Contains(x.FinancialYearId) && accounts.Select(x => x.Id).Contains(x.AccountId)).ToArrayAsync();

            // Group transactions by AccountId
            var transactionsByAccount = allTransactions.GroupBy(x => x.AccountId);

            foreach (var accountTransactionsGroup in transactionsByAccount)
            {
                var account = accounts.SingleOrDefault(x => x.Id == accountTransactionsGroup.Key);
                if (account == null) continue; // Handle missing account if necessary

                // Filter out previous year closing balances
                var accountTransactions = accountTransactionsGroup.Where(x => !x.IsPreviousYearClosingBalance);

                // Determine opening transactions
                IEnumerable<Transaction> openingTransactions;
                if (fromDate.HasValue)
                {
                    if (fromDate.Value.Date != initialFinancialYear!.StartDate.Date)
                    {
                        openingTransactions = accountTransactionsGroup
                            .Where(x => x.TransactionDate.Date >= initialFinancialYear!.StartDate.Date
                                     && x.TransactionDate.Date < fromDate.Value.Date);
                    }
                    else
                    {
                        openingTransactions = accountTransactionsGroup.Where(x => x.FinancialYearId == initialFinancialYear.Id && x.IsOpeningBalance);
                    }
                }
                else
                {
                    openingTransactions = Enumerable.Empty<Transaction>();
                }

                // Filter transactions within the specified date range
                if (fromDate.HasValue && toDate.HasValue)
                {
                    accountTransactions = accountTransactions
                        .Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
                }

                var thisPeriodPurchaseQty = accountTransactions.Sum(x => x.TransactionQty);

                var thisPeriodPurchaseValueByPurchaseInvoice = accountTransactions
                    .Where(x => x.Vnumber.StartsWith("PI"))
                    .Sum(x => x.Credit);

                var thisPeriodReturnValueByPurchaseReturn = accountTransactions
                    .Where(x => x.Vnumber.StartsWith("R"))
                    .Sum(x => x.Debit);

                var thisPeriodPurchaseClearingAccountValueByGRN = accountTransactions
                    .Where(x => x.Vnumber.StartsWith("GRN"))
                    .Sum(x => x.Credit);

                var thisPeriodPurchaseClearingAccountValueByPI = accountTransactions
                    .Where(x => x.Vnumber.StartsWith("PI"))
                    .Sum(x => x.Debit);
                var thisPeriodPurchaseClearingAccountValue = thisPeriodPurchaseClearingAccountValueByGRN - thisPeriodPurchaseClearingAccountValueByPI;

                var thisPeriodPurchaseValue = thisPeriodPurchaseValueByPurchaseInvoice + thisPeriodPurchaseClearingAccountValue - thisPeriodReturnValueByPurchaseReturn;

                var thisPeriodAdjustmentByJournalEntry = accountTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.JournalEntry)
                    .Sum(x => x.Debit) - accountTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.JournalEntry)
                    .Sum(x => x.Credit);

                var thisPeriodAdjustmentByPPA = accountTransactions
                    .Where(x => x.Vnumber.StartsWith("PPA"))
                    .Sum(x => x.Debit) - accountTransactions
                    .Where(x => x.Vnumber.StartsWith("PPA"))
                    .Sum(x => x.Credit);

                var thisPeriodAdjustmentByCashReceiveVoucher = accountTransactions
                    .Where(x => x.Vnumber.StartsWith("CRV"))
                    .Sum(x => x.Credit);

                var thisPeriodAdjustmentByLCCostEntry = accountTransactions
                    .Where(x => x.Vnumber.StartsWith("LCC"))
                    .Sum(x => x.Credit);

                var thisPeriodAdjustmentByLCAdjustment = accountTransactions
                    .Where(x => x.Vnumber.StartsWith("LCA"))
                    .Sum(x => x.Debit);

                var thisPeriodNetPurchaseAdjustment = thisPeriodAdjustmentByJournalEntry + thisPeriodAdjustmentByPPA - thisPeriodAdjustmentByCashReceiveVoucher - thisPeriodAdjustmentByLCCostEntry + thisPeriodAdjustmentByLCAdjustment;

                var thisPeriodPayment = accountTransactions.Where(x => x.AccountTransactionType == (int)AccountTransactonType.SupplierPayment)
                    .Sum(x => x.Debit);

                var thisPeriodDue = thisPeriodPurchaseValue - thisPeriodPayment - thisPeriodNetPurchaseAdjustment;

                // Calculate opening balances
                var openingCredit = openingTransactions.Sum(x => x.Credit);
                var openingDebit = openingTransactions.Sum(x => x.Debit);
                var openingDue = openingCredit - openingDebit;

                // Add to result list
                result.Add(new SupplierTransactionLedgerViewModel()
                {
                    Code = account.Code,
                    Name = account.Name,
                    OpeningDue = openingDue,
                    ThisPeriodPurchaseQty = thisPeriodPurchaseQty,
                    ThisPeriodPurchaseValue = thisPeriodPurchaseValue,
                    ThisPeriodPayment = thisPeriodPayment,
                    ThisPeriodAdjustment = thisPeriodNetPurchaseAdjustment,
                    Due = openingDue + thisPeriodDue > 0 ? openingDue + thisPeriodDue : 0m,
                    Advance = openingDue + thisPeriodDue > 0 ? 0m : openingDue + thisPeriodDue
                });
            }

            // Return the result ordered by name
            return result.OrderBy(x => x.Name).ToList();
        }

        public async Task<List<Transaction>> PreparePaymentReport(DateTime? fromDate, DateTime? toDate, int? transactionType, Guid? costCenterId, Guid? supplierId, Guid? paymentModeId)
        {
            var financialYears = await _unitOfWork.Repository<FinancialYear>().TableNoTracking()
                    .Where(fy => (fromDate == null || fy.EndDate >= fromDate)
                    && (toDate == null || fy.StartDate <= toDate)).ToListAsync();
            var financialYearIds = financialYears.Select(x => x.Id);
            var accountTransactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => new List<int> { (int)AccountTransactonType.SupplierPayment, (int)AccountTransactonType.OtherPayment }.Contains(x.AccountTransactionType) && financialYearIds.Contains(x.FinancialYearId)).AsQueryable();
            //this period transactions
            if (fromDate.HasValue && toDate.HasValue)
                accountTransactions = accountTransactions.Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
            if (transactionType.HasValue) accountTransactions = accountTransactions.Where(x => x.AccountTransactionType == transactionType.Value);
            if (costCenterId.HasValue) accountTransactions = accountTransactions.Where(x => x.CostCenterId == costCenterId.Value);
            if (supplierId.HasValue) accountTransactions = accountTransactions.Where(x => x.AccountId == supplierId.Value);
            if (paymentModeId.HasValue) accountTransactions = accountTransactions.Where(x => x.PaymentModeId == paymentModeId.Value);
            return await accountTransactions.OrderBy(x => x.TransactionDate).ToListAsync();
        }

        public async Task<List<Transaction>> PrepareCollectionReport(PaymentCollectionRequestModel requestModel)
        {
            var financialYears = await _unitOfWork.Repository<FinancialYear>().TableNoTracking()
                    .Where(fy => (requestModel.FromDate == null || fy.EndDate >= requestModel.FromDate)
                    && (requestModel.ToDate == null || fy.StartDate <= requestModel.ToDate)).ToListAsync();

            var financialYearIds = financialYears.Select(x => x.Id);
            List<Account> customers = await GetControlAccountListByAccountId(AccountHeadConstants.TradeReceivable.ToGuid());
            var accountTransactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => new List<int> { (int)AccountTransactonType.CustomerReceipt, (int)AccountTransactonType.OtherReceipt }.Contains(x.AccountTransactionType) && financialYearIds.Contains(x.FinancialYearId)).AsQueryable();

            //this period transactions
            if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue) accountTransactions = accountTransactions.Where(x => x.TransactionDate.Date >= requestModel.FromDate.Value.Date && x.TransactionDate.Date <= requestModel.ToDate.Value.Date);
            if (requestModel.AccountTransactionType.HasValue) accountTransactions = accountTransactions.Where(x => x.AccountTransactionType == requestModel.AccountTransactionType.Value);
            if (requestModel.CostCenterId.HasValue) accountTransactions = accountTransactions.Where(x => x.CostCenterId == requestModel.CostCenterId.Value);
            if (requestModel.CustomerZoneId.HasValue)
            {
                if (_industry.Sales.CollectionReportGroupsByZone) //distributor (feed) groups by zone; others by region
                {
                    customers = customers.Where(x => x.CustomerZoneId == requestModel.CustomerZoneId.Value).ToList();
                }
                else
                {
                    customers = customers.Where(x => x.CustomerRegionId == requestModel.CustomerZoneId.Value).ToList();
                }

                accountTransactions = accountTransactions.Where(x => customers.Select(c => c.Id).Contains(x.AccountId));
            }
            if (requestModel.CustomerMarketingOfficerId.HasValue)
            {
                customers = customers.Where(x => x.CustomerMarketingOfficerId == requestModel.CustomerMarketingOfficerId.Value).ToList();
                accountTransactions = accountTransactions.Where(x => customers.Select(c => c.Id).Contains(x.AccountId));
            }
            if (requestModel.CustomerId.HasValue) accountTransactions = accountTransactions.Where(x => x.AccountId == requestModel.CustomerId.Value);
            if (requestModel.PaymentModeId.HasValue) accountTransactions = accountTransactions.Where(x => x.PaymentModeId == requestModel.PaymentModeId.Value);

            return await accountTransactions.OrderBy(x => x.TransactionDate).ToListAsync();
        }

        public async Task<List<CustomerTransactionLedgerViewModel>> PrepareCustomerTransactionLedger(CustomerTransactionRequestModel request)
        {
            List<CustomerTransactionLedgerViewModel> result = new List<CustomerTransactionLedgerViewModel>();
            var financialYears = await _unitOfWork.Repository<FinancialYear>().TableNoTracking()
                               .Where(fy => (request.FromDate == null || fy.EndDate >= request.FromDate)
                               && (request.ToDate == null || fy.StartDate <= request.ToDate)).ToListAsync();
            var financialYearIds = financialYears.Select(x => x.Id);
            var initialFinancialYear = financialYears.OrderBy(x => x.StartDate).FirstOrDefault();
            var accounts = await GetControlAccountListByAccountId(AccountHeadConstants.TradeReceivable.ToGuid());
            if (request.CustomerZoneId.HasValue)
            {
                accounts = accounts.Where(x => x.CustomerZoneId == request.CustomerZoneId).ToList();
            }
            if (request.CustomerMarketingOfficerId.HasValue)
            {
                accounts = accounts.Where(x => x.CustomerMarketingOfficerId == request.CustomerMarketingOfficerId).ToList();
            }
            // Fetch all necessary transactions in bulk
            var allTransactions = await _unitOfWork.Repository<Transaction>().TableNoTracking()
                .Where(x => financialYearIds.Contains(x.FinancialYearId) && accounts.Select(x => x.Id).Contains(x.AccountId)).ToArrayAsync();
            // Group transactions by AccountId
            var transactionsByAccount = allTransactions.GroupBy(x => x.AccountId);
            // Process each account
            foreach (var accountTransactionsGroup in transactionsByAccount)
            {
                var account = accounts.SingleOrDefault(x => x.Id == accountTransactionsGroup.Key);
                if (account == null) continue; // Handle missing account if necessary
                                               // Calculate opening transactions
                var accountTransactions = accountTransactionsGroup.Where(x => !x.IsPreviousYearClosingBalance);
                //opening transactions
                IEnumerable<Transaction> openingTransactions = new List<Transaction>().AsQueryable();
                if (request.FromDate.HasValue)
                {
                    if (request.FromDate.Value.Date.Equals(initialFinancialYear!.StartDate.Date) == false)
                    {
                        openingTransactions = accountTransactionsGroup.Where(x => x.TransactionDate.Date >= initialFinancialYear!.StartDate.Date && x.TransactionDate.Date < request.FromDate.Value.Date);
                    }
                    else
                    {
                        openingTransactions = accountTransactionsGroup.Where(x => x.FinancialYearId == initialFinancialYear.Id && x.IsOpeningBalance);
                    }

                }
                // Filter transactions within the specified date range
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                    accountTransactions = accountTransactions.Where(x => x.TransactionDate.Date >= request.FromDate.Value.Date && x.TransactionDate.Date <= request.ToDate.Value.Date);

                // Calculate Sales
                var thisPeriodSaleQty = accountTransactions.Sum(x => x.TransactionQty);

                var thisPeriodSaleValueBySaleInvoice = accountTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.Sale)
                    .Sum(x => x.Debit);

                var thisPeriodReturnValueBySaleReturn = accountTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.SaleReturn)
                    .Sum(x => x.Credit);

                var thisPeriodSaleValue = thisPeriodSaleValueBySaleInvoice - thisPeriodReturnValueBySaleReturn;

                // Calculate collections
                var thisPeriodCollection = accountTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.CustomerReceipt || x.Vnumber.StartsWith("MRAS"))
                    .Sum(x => x.Credit);

                // Calculate adjustments
                var thisPeriodAdjustmentByJournalEntry = accountTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.JournalEntry)
                    .Sum(x => x.Credit) - accountTransactions.Where(x => x.AccountTransactionType == (int)AccountTransactonType.JournalEntry)
                    .Sum(x => x.Debit);

                var thisPeriodAdjustmentByPaymentVoucher = accountTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.OtherPayment)
                    .Sum(x => x.Credit) - accountTransactions.Where(x => x.AccountTransactionType == (int)AccountTransactonType.OtherPayment)
                    .Sum(x => x.Debit);

                var thisPeriodAdjustment = thisPeriodAdjustmentByJournalEntry + thisPeriodAdjustmentByPaymentVoucher;

                // Calculate due
                var thisPeriodDue = thisPeriodSaleValue - thisPeriodCollection - thisPeriodAdjustment;

                // Calculate opening balances
                var openingCredit = openingTransactions.Sum(x => x.Credit);
                var openingDebit = openingTransactions.Sum(x => x.Debit);
                var openingDue = openingDebit - openingCredit;
                //Marketing Officer
                var marketingOfficerName = _unitOfWork.Repository<Employee>().TableNoTracking().FirstOrDefault(x => x.Id == account.CustomerMarketingOfficerId)?.FullName;
                // Add to result list
                result.Add(new CustomerTransactionLedgerViewModel()
                {
                    Id = account.Id,
                    Code = account.Code,
                    Name = account.Name,
                    CustomerMarketingOfficerId = account.CustomerMarketingOfficerId,
                    CustomerMarketingOfficerName = marketingOfficerName,
                    OpeningDue = openingDue,
                    ThisPeriodSaleQty = thisPeriodSaleQty,
                    ThisPeriodSaleValue = thisPeriodSaleValue,
                    ThisPeriodCollection = thisPeriodCollection,
                    ThisPeriodAdjustment = thisPeriodAdjustment,
                    ThisPeriodDue = thisPeriodDue,
                    Due = openingDue + thisPeriodDue,
                    CreditLimit = account.CustomerCreditLimit
                });
            }
            return result.OrderBy(x => x.Name).ToList();
        }

        public async Task<List<CashBankTransactionLedgerViewModel>> PrepareCashBankTransactionLedger(DateTime? fromDate, DateTime? toDate)
        {
            var result = new List<CashBankTransactionLedgerViewModel>();

            // Get the relevant financial years
            var financialYears = await _unitOfWork.Repository<FinancialYear>().TableNoTracking()
                .Where(fy => (fromDate == null || fy.EndDate >= fromDate) && (toDate == null || fy.StartDate <= toDate))
                .ToListAsync();

            var financialYearIds = financialYears.Select(x => x.Id);
            var initialFinancialYear = financialYears.OrderBy(x => x.StartDate).FirstOrDefault();

            // Get all control account IDs for Cash and Cash Equivalents
            var accounts = await GetControlAccountListByAccountId(AccountHeadConstants.CashAndCashEquivalents.ToGuid());

            // Fetch all necessary transactions in bulk
            var allTransactions = await _unitOfWork.Repository<Transaction>().TableNoTracking()
                .Where(x => financialYearIds.Contains(x.FinancialYearId) && accounts.Select(a => a.Id).Contains(x.AccountId))
                .ToListAsync();

            // Group transactions by AccountId
            var transactionsByAccount = allTransactions.GroupBy(x => x.AccountId);

            // Process each account
            foreach (var accountTransactionsGroup in transactionsByAccount)
            {
                var account = accounts.SingleOrDefault(x => x.Id == accountTransactionsGroup.Key);
                if (account == null) continue; // Handle missing account if necessary

                var accountTransactions = accountTransactionsGroup.Where(x => !x.IsPreviousYearClosingBalance);
                //opening transactions
                IEnumerable<Transaction> openingTransactions = new List<Transaction>().AsQueryable();
                if (fromDate.HasValue)
                {
                    if (fromDate.Value.Date.Equals(initialFinancialYear!.StartDate.Date) == false)
                    {
                        openingTransactions = accountTransactionsGroup.Where(x => x.TransactionDate.Date >= initialFinancialYear!.StartDate.Date && x.TransactionDate.Date < fromDate.Value.Date);
                    }
                    else
                    {
                        openingTransactions = accountTransactionsGroup.Where(x => x.FinancialYearId == initialFinancialYear.Id && x.IsOpeningBalance);
                    }

                }

                // Transactions within the specified date range
                if (fromDate.HasValue && toDate.HasValue)
                {
                    accountTransactions = accountTransactions
                        .Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
                }

                // Calculate Fund Transfer collections
                var thisPeriodFundTransferReceipt = accountTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.FundTransfer || x.Vnumber.StartsWith("FT"))
                    .Sum(x => x.Debit);
                // Calculate Fund Transfer payments
                var thisPeriodFundTransferPayment = accountTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.FundTransfer || x.Vnumber.StartsWith("FT"))
                    .Sum(x => x.Credit);
                //Bank charge from fund transfer table
                var bankCharge = 0m;
                if (fromDate.HasValue && toDate.HasValue)
                {
                    bankCharge = _unitOfWork.Repository<FundTransfer>().TableNoTracking().Where(x => x.TransferFromAccountId == account.Id && x.Status >= (int)FundTransferStatus.Approved && x.FundTransferDate.Date >= fromDate.Value.Date && x.FundTransferDate.Date <= toDate.Value.Date).Sum(x => x.Charges);
                }
                else
                {
                    bankCharge = _unitOfWork.Repository<FundTransfer>().TableNoTracking().Where(x => x.TransferFromAccountId == account.Id && x.Status >= (int)FundTransferStatus.Approved).Sum(x => x.Charges);
                }

                // Calculate metrics for the specified period
                var thisPeriodReceipt = accountTransactions.Sum(x => x.Debit);
                var thisPeriodPayment = accountTransactions.Sum(x => x.Credit);
                var thisPeriodBalance = thisPeriodReceipt - thisPeriodPayment;

                // Calculate opening balances
                var openingCredit = openingTransactions.Sum(x => x.Credit);
                var openingDebit = openingTransactions.Sum(x => x.Debit);
                var openingBalance = openingDebit - openingCredit;

                // Add to the result list
                result.Add(new CashBankTransactionLedgerViewModel()
                {
                    Id = account.Id,
                    Code = account.Code,
                    Name = account.Name,
                    ParentId = account.ParentId,
                    OpeningBalance = openingBalance,
                    ThisPeriodReceipt = thisPeriodReceipt,
                    ThisPeriodFundTransferReceipt = thisPeriodFundTransferReceipt,
                    ThisPeriodPayment = thisPeriodPayment,
                    ThisPeriodFundTransferPayment = thisPeriodFundTransferPayment,
                    ThisPeriodFundTransferCharge = bankCharge,
                    ThisPeriodBalance = thisPeriodBalance,
                    Balance = openingBalance + thisPeriodBalance
                });
            }

            return result.OrderBy(x => x.ParentId).ThenBy(x => x.Name).ToList();
        }

        public async Task<List<CashBankTransactionDetailLedgerViewModel>> PrepareCashBankTransactionDetailLedger(DateTime? fromDate, DateTime? toDate)
        {
            var result = new List<CashBankTransactionDetailLedgerViewModel>();

            // Get the relevant financial years
            var financialYears = await _unitOfWork.Repository<FinancialYear>().TableNoTracking()
                .Where(fy => (fromDate == null || fy.EndDate >= fromDate) && (toDate == null || fy.StartDate <= toDate))
                .ToListAsync();

            var financialYearIds = financialYears.Select(x => x.Id);
            var initialFinancialYear = financialYears.OrderBy(x => x.StartDate).FirstOrDefault();

            // Get all control account IDs for Cash and Cash Equivalents
            var accounts = await GetControlAccountListByAccountId(AccountHeadConstants.CashAndCashEquivalents.ToGuid());

            // Fetch all necessary transactions in bulk
            var allTransactions = await _unitOfWork.Repository<Transaction>().TableNoTracking()
                .Where(x => financialYearIds.Contains(x.FinancialYearId) && accounts.Select(a => a.Id).Contains(x.AccountId))
                .ToListAsync();

            // Group transactions by AccountId
            var transactionsByAccount = allTransactions.GroupBy(x => x.AccountId);

            foreach (var accountTransactionsGroup in transactionsByAccount)
            {
                var account = accounts.SingleOrDefault(x => x.Id == accountTransactionsGroup.Key);
                if (account == null) continue; // Handle missing account if necessary

                var accountTransactions = accountTransactionsGroup.Where(x => !x.IsPreviousYearClosingBalance);
                //opening transactions
                IEnumerable<Transaction> openingTransactions = new List<Transaction>().AsQueryable();
                if (fromDate.HasValue)
                {
                    if (fromDate.Value.Date.Equals(initialFinancialYear!.StartDate.Date) == false)
                    {
                        openingTransactions = accountTransactionsGroup.Where(x => x.TransactionDate.Date >= initialFinancialYear!.StartDate.Date && x.TransactionDate.Date < fromDate.Value.Date);
                    }
                    else
                    {
                        openingTransactions = accountTransactionsGroup.Where(x => x.FinancialYearId == initialFinancialYear.Id && x.IsOpeningBalance);
                    }

                }

                // Transactions within the specified date range
                if (fromDate.HasValue && toDate.HasValue)
                {
                    accountTransactions = accountTransactions
                        .Where(x => x.TransactionDate.Date >= fromDate.Value.Date && x.TransactionDate.Date <= toDate.Value.Date);
                }

                // Calculate this period metrics
                var thisPeriodReceipt = accountTransactions.Sum(x => x.Debit);
                var thisPeriodPayment = accountTransactions.Sum(x => x.Credit);
                var thisPeriodBalance = thisPeriodReceipt - thisPeriodPayment;

                // Calculate opening balances
                var openingCredit = openingTransactions.Sum(x => x.Credit);
                var openingDebit = openingTransactions.Sum(x => x.Debit);
                var openingBalance = openingDebit - openingCredit;

                // Prepare receipt and payment details
                var receiptDetails = new List<ReceiptDetail>();
                var paymentDetails = new List<PaymentDetail>();

                foreach (var item in accountTransactions)
                {
                    //var accountName = await GetContraAccountName(item);
                    if (item.Debit > 0)
                    {
                        receiptDetails.Add(new ReceiptDetail
                        {
                            Vnumber = item.Vnumber,
                            Date = item.TransactionDate,
                            Particular = item.ContraAccountNames,
                            Amount = item.Debit
                        });
                    }
                    else if (item.Credit > 0)
                    {
                        paymentDetails.Add(new PaymentDetail
                        {
                            Vnumber = item.Vnumber,
                            Date = item.TransactionDate,
                            Particular = item.ContraAccountNames,
                            Amount = item.Credit
                        });
                    }
                }

                // Add to the result list
                result.Add(new CashBankTransactionDetailLedgerViewModel()
                {
                    Id = account.Id,
                    Code = account.Code,
                    Name = account.Name,
                    ParentId = account.ParentId,
                    OpeningBalance = openingBalance,
                    ThisPeriodReceipt = thisPeriodReceipt,
                    ThisPeriodPayment = thisPeriodPayment,
                    ReceiptDetails = receiptDetails.OrderBy(x => x.Date).ToList(),
                    PaymentDetails = paymentDetails.OrderBy(x => x.Date).ToList(),
                    Balance = openingBalance + thisPeriodBalance
                });
            }

            return result.OrderBy(x => x.ParentId).ThenBy(x => x.Name).ToList();
        }



        public async Task<string> GetContraAccountName(Transaction item)
        {
            var particularTransactions = await _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.Vnumber == item.Vnumber && x.AccountId != item.AccountId).ToListAsync();
            var accountNames = new List<string>();
            foreach (var transaction in particularTransactions)
            {
                var particularAccount = await _unitOfWork.Repository<Account>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == transaction.AccountId);
                if (item.Debit > 0)
                {
                    if (transaction.Credit > 0)
                    {
                        accountNames.Add(particularAccount?.Name ?? "");
                    }
                }
                else
                {
                    if (transaction.Debit > 0)
                    {
                        accountNames.Add(particularAccount?.Name ?? "");
                    }
                }

            }
            return string.Join(", ", accountNames);
        }

        public async Task<CashBookReportViewModel> PrepareCashBookTransactionDetailLedger(CashBookReportRequestModel requestModel)
        {
            List<CombinedReceiptAndPayment> combinedData = new List<CombinedReceiptAndPayment>();
            var account = await _unitOfWork.Repository<Account>().FindAsync(requestModel.CashBookAccountId);
            //opening balance
            var financialYears = await _unitOfWork.Repository<FinancialYear>().TableNoTracking()
                    .Where(fy => (requestModel.FromDate == null || fy.EndDate >= requestModel.FromDate)
                    && (requestModel.ToDate == null || fy.StartDate <= requestModel.ToDate)).ToListAsync();
            var financialYearIds = financialYears.Select(x => x.Id);
            var initialFinancialYear = financialYears.OrderBy(x => x.StartDate).FirstOrDefault();
            var transactionsQyeryable = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountId == requestModel.CashBookAccountId)
                .Where(x => financialYearIds.Contains(x.FinancialYearId));

            var accountTransactions = transactionsQyeryable.Where(x => !x.IsPreviousYearClosingBalance);
            //opening transactions
            IQueryable<Transaction> openingTransactions = new List<Transaction>().AsQueryable();
            if (requestModel.FromDate.HasValue)
            {
                if (requestModel.FromDate.Value.Date.Equals(initialFinancialYear!.StartDate.Date) == false)
                {
                    openingTransactions = transactionsQyeryable.Where(x => x.TransactionDate.Date >= initialFinancialYear!.StartDate.Date && x.TransactionDate.Date < requestModel.FromDate.Value.Date);
                }
                else
                {
                    openingTransactions = transactionsQyeryable.Where(x => x.FinancialYearId == initialFinancialYear.Id && x.IsOpeningBalance);
                }

            }


            //this period transactions
            if (requestModel.FromDate.HasValue && requestModel.ToDate.HasValue)
                accountTransactions = accountTransactions.Where(x => x.TransactionDate.Date >= requestModel.FromDate.Value.Date && x.TransactionDate.Date <= requestModel.ToDate.Value.Date);
            accountTransactions = accountTransactions.OrderBy(x => x.TransactionDate);
            var thisPeriodReceipt = accountTransactions.Sum(x => x.Debit);
            var thisPeriodPayment = accountTransactions.Sum(x => x.Credit);
            var thisPeriodBalance = thisPeriodReceipt - thisPeriodPayment;

            //opening transactions
            var openingCredit = openingTransactions.Sum(x => x.Credit);
            var openingDebit = openingTransactions.Sum(x => x.Debit);
            var openingBalance = openingDebit - openingCredit;
            var thisPeriodOB_TRDM = openingBalance + thisPeriodReceipt;
            //details start

            List<Receipt> receipts = new();
            List<Payment> payments = new();
            receipts.Add(new Receipt
            {
                Vnumber1 = "",
                Date1 = "",
                CostCenterName1 = "",
                HeadOfAccountName1 = "",
                Particular1 = "Opening Balance",
                Amount1 = openingBalance
            });
            foreach (var item in accountTransactions)
            {
                var costCenter = await _unitOfWork.Repository<CostCenter>().TableNoTracking().FirstOrDefaultAsync(x => x.Id == item.CostCenterId);
                //var accountName = await GetContraAccountName(item);
                if (item.Debit > 0m)
                {
                    receipts.Add(new Receipt
                    {
                        Vnumber1 = item.Vnumber,
                        Date1 = item.TransactionDate.ToString("dd/MM/yyyy"),
                        CostCenterName1 = costCenter?.Name ?? "",
                        HeadOfAccountName1 = item.ContraAccountNames,
                        Particular1 = item.Description,
                        Amount1 = item.Debit
                    });
                }
                else
                {
                    payments.Add(new Payment
                    {
                        Vnumber2 = item.Vnumber,
                        Date2 = item.TransactionDate.ToString("dd/MM/yyyy"),
                        CostCenterName2 = costCenter?.Name ?? "",
                        HeadOfAccountName2 = item.ContraAccountNames,
                        Particular2 = item.Description,
                        Amount2 = item.Credit
                    });

                }
            }
            for (int i = 0; i < Math.Max(receipts.Count, payments.Count); i++)
            {
                var combinedItem = new CombinedReceiptAndPayment();

                if (i < receipts.Count)
                {
                    combinedItem.Vnumber1 = receipts[i].Vnumber1;
                    combinedItem.Date1 = receipts[i].Date1;
                    combinedItem.CostCenterName1 = receipts[i].CostCenterName1;
                    combinedItem.HeadOfAccountName1 = receipts[i].HeadOfAccountName1;
                    combinedItem.Particular1 = receipts[i].Particular1;
                    combinedItem.Amount1 = receipts[i].Amount1.ToString("#,##0.00");
                }

                if (i < payments.Count)
                {
                    combinedItem.Vnumber2 = payments[i].Vnumber2;
                    combinedItem.Date2 = payments[i].Date2;
                    combinedItem.CostCenterName2 = payments[i].CostCenterName2;
                    combinedItem.HeadOfAccountName2 = payments[i].HeadOfAccountName2;
                    combinedItem.Particular2 = payments[i].Particular2;
                    combinedItem.Amount2 = payments[i].Amount2.ToString("#,##0.00");
                }

                combinedData.Add(combinedItem);
            }
            var result = new CashBookReportViewModel()
            {
                Id = account.Id,
                Code = account.Code,
                Name = account.Name,
                ParentId = account.ParentId,
                OpeningBalance = openingBalance,
                ThisPeriodOB_TRDM = thisPeriodOB_TRDM,
                ThisPeriodReceipt = thisPeriodReceipt,
                ThisPeriodPayment = thisPeriodPayment,
                CombinedReceiptsAndPayments = combinedData,
                Balance = openingBalance + thisPeriodBalance
            };

            return result;
        }


        public async Task PrepareNewFiscalYear()//string newFiscalYearId
        {
            var toDate = new DateTime(2024, 6, 30);
            //all accounts
            var allaccountHeads = await _unitOfWork.Repository<Account>().TableNoTracking().ToListAsync();
            var previousFiscalYear = await _unitOfWork.Repository<FinancialYear>().TableNoTracking().FirstAsync(x => x.IsActive);
            //inactive previous financial year
            previousFiscalYear.IsActive = false;
            await _unitOfWork.Repository<FinancialYear>().UpdateAsync(previousFiscalYear);
            //add new financial year
            var newFinancialYear = new FinancialYear
            {
                Id = Guid.NewGuid(),
                Code = "2425",
                Name = "2024-2025",
                StartDate = new DateTime(2024, 7, 1),
                EndDate = new DateTime(2025, 6, 30),
                IsActive = true
            };
            await _unitOfWork.Repository<FinancialYear>().AddAsync(newFinancialYear);
            //calculate profit loss
            var revenue = await CalculateBalanceByAccountIdForFiscalYearChange(allaccountHeads, Guid.Parse(AccountHeadConstants.Revenue), Guid.Parse(AccountTypeConstants.Revenue), previousFiscalYear, toDate);
            var cogs = await CalculateBalanceByAccountIdForFiscalYearChange(allaccountHeads, Guid.Parse(AccountHeadConstants.CostOfGoodsSold), Guid.Parse(AccountTypeConstants.CostOfGoodsSold), previousFiscalYear, toDate);
            var operatingExpenses = await CalculateBalanceByAccountIdForFiscalYearChange(allaccountHeads, Guid.Parse(AccountHeadConstants.OperatingExpenses), Guid.Parse(AccountTypeConstants.OperatingExpenses), previousFiscalYear, toDate);
            var otherIncome = await CalculateBalanceByAccountIdForFiscalYearChange(allaccountHeads, Guid.Parse(AccountHeadConstants.OtherIncome), Guid.Parse(AccountTypeConstants.OtherIncome), previousFiscalYear, toDate);
            var nonOperatingExpenses = await CalculateBalanceByAccountIdForFiscalYearChange(allaccountHeads, Guid.Parse(AccountHeadConstants.NonOperatingExpenses), Guid.Parse(AccountTypeConstants.NonOperatingExpenses), previousFiscalYear, toDate);
            var csrFund = await CalculateBalanceByAccountIdForFiscalYearChange(allaccountHeads, Guid.Parse(AccountHeadConstants.ContributiontoCSRFund), Guid.Parse(AccountTypeConstants.ContributiontoCSRFund), previousFiscalYear, toDate);
            var tax = await CalculateBalanceByAccountIdForFiscalYearChange(allaccountHeads, Guid.Parse(AccountHeadConstants.CurrentTax), Guid.Parse(AccountTypeConstants.CurrentTax), previousFiscalYear, toDate);
            var pl = (revenue + otherIncome) - (cogs + operatingExpenses + nonOperatingExpenses + csrFund + tax);

            //transfer all control account balance to new financial year
            var accountHeads = allaccountHeads.Where(x => x.IsControlAccount && new[] { AccountTypeConstants.CurrentAsset.ToGuid(), AccountTypeConstants.NonCurrentAsset.ToGuid(),
                AccountTypeConstants.CurrentLiabilities.ToGuid(),
                AccountTypeConstants.NonCurrentLiabilities.ToGuid(), AccountTypeConstants.OwnersEquity.ToGuid(),
                AccountTypeConstants.OthersEquity.ToGuid()}.Contains(x.AccountTypeId)).ToList();
            foreach (var account in accountHeads)
            {
                bool isDebit = false;
                if (account.AccountTypeId == AccountTypeConstants.CurrentAsset.ToGuid() || account.AccountTypeId == AccountTypeConstants.NonCurrentAsset.ToGuid())
                {
                    isDebit = true;
                }
                var accBalance = await CalculateBalanceByAccountIdForFiscalYearChange(account.Id, account.AccountTypeId, previousFiscalYear, toDate);
                if (account.Id == AccountHeadConstants.CurrentYearsProfitLoss.ToGuid())
                {
                    accBalance += pl;
                }

                //opening Balance Entry from previous financial year
                var transaction = new Transaction
                {
                    Id = Guid.NewGuid(),
                    FinancialYearId = newFinancialYear.Id,
                    CostCenterId = "3BFECD5F-368B-4828-94B7-8C111FADCA04".ToGuid(),//head office
                    Vnumber = "Opening2024-2025",
                    Description = "Opening Balance Entry from previous financial year",
                    AccountId = account.Id,
                    Debit = isDebit ? accBalance : 0m,
                    Credit = isDebit ? 0m : accBalance,
                    TransactionDate = new DateTime(2024, 7, 1),
                    IsOpeningBalance = true,
                    IsPreviousYearClosingBalance = true
                };
                await _unitOfWork.Repository<Transaction>().AddAsync(transaction);

            }
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<decimal> CalculateBalanceByAccountIdForFiscalYearChange(Guid accountId, Guid accountTypeId, FinancialYear previousFinancialYear, DateTime? toDate)
        {
            var accountIds = await GetAllControlAccountIdsByAccountId(accountId);
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.FinancialYearId == previousFinancialYear.Id).Where(x => accountIds.Contains(x.AccountId));
            if (toDate.HasValue)
                transactions = transactions.Where(x => x.TransactionDate.Date >= previousFinancialYear.StartDate!.Date && x.TransactionDate.Date <= toDate.Value.Date);
            if (accountTypeId == Guid.Parse(AccountTypeConstants.NonCurrentLiabilities)
               || accountTypeId == Guid.Parse(AccountTypeConstants.CurrentLiabilities)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OwnersEquity)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OthersEquity)
               || accountTypeId == Guid.Parse(AccountTypeConstants.Revenue)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OtherIncome))
                return transactions.Sum(x => x.Credit) - transactions.Sum(x => x.Debit);
            return transactions.Sum(x => x.Debit) - transactions.Sum(x => x.Credit);
        }
        private async Task<decimal> CalculateBalanceByAccountIdForFiscalYearChange(IEnumerable<Account> allAccountHeads, Guid accountId, Guid accountTypeId, FinancialYear previousFinancialYear, DateTime? toDate)
        {
            var accountIds = await GetAllControlAccountIdsByAccountId(allAccountHeads, accountId);
            var transactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.FinancialYearId == previousFinancialYear.Id).Where(x => accountIds.Contains(x.AccountId));
            if (toDate.HasValue)
                transactions = transactions.Where(x => x.TransactionDate.Date >= previousFinancialYear.StartDate!.Date && x.TransactionDate.Date <= toDate.Value.Date);
            if (accountTypeId == Guid.Parse(AccountTypeConstants.NonCurrentLiabilities)
               || accountTypeId == Guid.Parse(AccountTypeConstants.CurrentLiabilities)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OwnersEquity)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OthersEquity)
               || accountTypeId == Guid.Parse(AccountTypeConstants.Revenue)
               || accountTypeId == Guid.Parse(AccountTypeConstants.OtherIncome))
                return transactions.Sum(x => x.Credit) - transactions.Sum(x => x.Debit);
            return transactions.Sum(x => x.Debit) - transactions.Sum(x => x.Credit);
        }

        public async Task<List<SalesAndCollectionViewModel>> PrepareCustomerWiseSalesAndCollection(SalesAndCollectionRequestModel request)
        {
            List<SalesAndCollectionViewModel> result = new List<SalesAndCollectionViewModel>();
            var financialYears = await _unitOfWork.Repository<FinancialYear>().TableNoTracking()
                               .Where(fy => (request.FromDate == null || fy.EndDate >= request.FromDate)
                               && (request.ToDate == null || fy.StartDate <= request.ToDate)).ToListAsync();
            var financialYearIds = financialYears.Select(x => x.Id);
            var initialFinancialYear = financialYears.OrderBy(x => x.StartDate).FirstOrDefault();
            var accounts = await GetControlAccountListByAccountId(AccountHeadConstants.TradeReceivable.ToGuid());
            if (request.CustomerRegionId.HasValue) accounts = accounts.Where(x => x.CustomerRegionId == request.CustomerRegionId.Value).ToList();
            if (request.CustomerZoneId.HasValue) accounts = accounts.Where(x => x.CustomerZoneId == request.CustomerZoneId.Value).ToList();
            if (request.CustomerAreaId.HasValue) accounts = accounts.Where(x => x.CustomerAreaId == request.CustomerAreaId.Value).ToList();
            if (request.CustomerTerritoryId.HasValue) accounts = accounts.Where(x => x.CustomerTerritoryId == request.CustomerTerritoryId.Value).ToList();
            if (request.CustomerId.HasValue) accounts = accounts.Where(x => x.Id == request.CustomerId.Value).ToList();
            if (request.CustomerMarketingOfficerId.HasValue) accounts = accounts.Where(x => x.CustomerMarketingOfficerId == request.CustomerMarketingOfficerId.Value).ToList();
            // Fetch all necessary transactions in bulk
            var allTransactions = await _unitOfWork.Repository<Transaction>().TableNoTracking()
                .Where(x => financialYearIds.Contains(x.FinancialYearId) && accounts.Select(x => x.Id).Contains(x.AccountId)).ToArrayAsync();
            // Group transactions by AccountId
            var transactionsByAccount = allTransactions.GroupBy(x => x.AccountId);
            // Process each account
            foreach (var accountTransactionsGroup in transactionsByAccount)
            {
                var account = accounts.SingleOrDefault(x => x.Id == accountTransactionsGroup.Key);
                if (account == null) continue; // Handle missing account if necessary
                                               // Calculate opening transactions
                var accountTransactions = accountTransactionsGroup.Where(x => !x.IsPreviousYearClosingBalance);
                //opening transactions
                IEnumerable<Transaction> openingTransactions = new List<Transaction>().AsQueryable();
                if (request.FromDate.HasValue)
                {
                    if (request.FromDate.Value.Date.Equals(initialFinancialYear!.StartDate.Date) == false)
                    {
                        openingTransactions = accountTransactionsGroup.Where(x => x.TransactionDate.Date >= initialFinancialYear!.StartDate.Date && x.TransactionDate.Date < request.FromDate.Value.Date);
                    }
                    else
                    {
                        openingTransactions = accountTransactionsGroup.Where(x => x.FinancialYearId == initialFinancialYear.Id && x.IsOpeningBalance);
                    }

                }

                // Filter transactions within the specified date range
                if (request.FromDate.HasValue && request.ToDate.HasValue)
                    accountTransactions = accountTransactions.Where(x => x.TransactionDate.Date >= request.FromDate.Value.Date && x.TransactionDate.Date <= request.ToDate.Value.Date);

                // Calculate metrics for the specified period
                var thisPeriodSaleValueWithAdjustment = accountTransactions.Sum(x => x.Debit);

                // Sale value without adjustments
                var thisPeriodSaleValue = accountTransactions
                    .Where(x => x.Vnumber.StartsWith("SI"))
                    .Sum(x => x.Debit);

                var thisPeriodSaleAdjustment = thisPeriodSaleValueWithAdjustment - thisPeriodSaleValue;


                // Calculate adjustments and collections
                var thisPeriodAdjustment = accountTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.JournalEntry)
                    .Sum(x => x.Credit) - thisPeriodSaleAdjustment;

                var thisPeriodSaleReturnValue = accountTransactions.Where(x => x.AccountTransactionType == (int)AccountTransactonType.SaleReturn).Sum(x => x.Credit);
                var thisPeriodCollectionWithAdjustment = accountTransactions.Where(x => x.AccountTransactionType != (int)AccountTransactonType.SaleReturn).Sum(x => x.Credit);
                var receivePaymentAgainstSale = accountTransactions.Where(x => x.AccountTransactionType != (int)AccountTransactonType.SaleReturn && x.AccountTransactionType != (int)AccountTransactonType.JournalEntry).Sum(x => x.Credit);
                var thisPeriodDue = thisPeriodSaleValueWithAdjustment - thisPeriodCollectionWithAdjustment - thisPeriodSaleReturnValue;

                // Calculate opening balances
                var openingCredit = openingTransactions.Sum(x => x.Credit);
                var openingDebit = openingTransactions.Sum(x => x.Debit);
                var openingBalance = openingDebit - openingCredit;
                // Add to result list
                result.Add(new SalesAndCollectionViewModel()
                {
                    Id = account.Id,
                    RegionId = account.CustomerRegionId,
                    ZoneId = account.CustomerZoneId,
                    AreaId = account.CustomerAreaId,
                    TerritoryId = account.CustomerTerritoryId,
                    CustomerMarketingOfficerId = account.CustomerMarketingOfficerId,
                    CustomerCode = account.Code,
                    CustomerName = account.Name,
                    CustomerAddress = account.Address,
                    OpeningBalance = openingBalance,
                    ThisPeriodSaleValue = thisPeriodSaleValue,
                    ThisPeriodSaleReturnValue = thisPeriodSaleReturnValue,
                    ThisPeriodCollection = receivePaymentAgainstSale,
                    ThisPeriodAdjustment = thisPeriodAdjustment,
                    ClosingBalance = openingBalance + thisPeriodDue
                });
            }
            return result.OrderBy(x => x.CustomerName).ToList();
        }

        public async Task<List<MarketingOfficerMonthlySalesCollectionViewModel>> PrepareYearlyCustomerWiseSalesAndCollection(MarketingOfficerYearlySalesAndCollectionRequestModel request)
        {

            var year = request.Year;
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            // Step 1️: Get all accounts
            var accounts = await GetControlAccountListByAccountId(AccountHeadConstants.TradeReceivable.ToGuid());
            if (request.CustomerMarketingOfficerId.HasValue)
                accounts = accounts.Where(x => x.CustomerMarketingOfficerId == request.CustomerMarketingOfficerId.Value).ToList();

            var accountIds = accounts.Select(x => x.Id).ToList();

            // Step 2️: Get all transactions for that year and those accounts
            var allTransactions = await _unitOfWork.Repository<Transaction>().TableNoTracking()
                .Where(x => accountIds.Contains(x.AccountId)
                            && x.TransactionDate >= startDate
                            && x.TransactionDate <= endDate
                            && !x.IsPreviousYearClosingBalance)
                .ToArrayAsync();

            // Step 3️: Group transactions by month
            var monthlyGroups = allTransactions.GroupBy(x => x.TransactionDate.Month)
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<MarketingOfficerMonthlySalesCollectionViewModel>();

            // Process each account
            for (int month = 1; month <= 12; month++)
            {
                monthlyGroups.TryGetValue(month, out var monthTransactions);
                monthTransactions ??= new List<Transaction>();

                var monthName = new DateTime(year, month, 1).ToString("MMMM", CultureInfo.InvariantCulture);

                if (!monthTransactions.Any())
                {
                    result.Add(new MarketingOfficerMonthlySalesCollectionViewModel
                    {
                        Year = year,
                        Month = month,
                        MonthName = monthName,
                        ThisPeriodSaleValue = 0,
                        ThisPeriodSaleReturnValue = 0,
                        ThisPeriodCollection = 0,
                        ThisPeriodAdjustment = 0,
                        ThisPeriodBalance = 0
                    });
                    continue;
                }


                // Calculate metrics for the specified period
                var totalSalesWithAdjustment = monthTransactions.Sum(x => x.Debit);

                // Sale value without adjustments
                var totalSales = monthTransactions
                    .Where(x => x.Vnumber.StartsWith("SI"))
                    .Sum(x => x.Debit);
                var salesAdjustment = totalSalesWithAdjustment - totalSales;

                // Calculate adjustments and collections

                var totalSaleReturn = monthTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.SaleReturn)
                    .Sum(x => x.Credit);

                var totalCollectionWithAdjustment = monthTransactions
                    .Where(x => x.AccountTransactionType != (int)AccountTransactonType.SaleReturn)
                    .Sum(x => x.Credit);

                var receivePaymentAgainstSale = monthTransactions
                    .Where(x => x.AccountTransactionType != (int)AccountTransactonType.SaleReturn
                                && x.AccountTransactionType != (int)AccountTransactonType.JournalEntry)
                    .Sum(x => x.Credit);

                var totalAdjustment = monthTransactions
                    .Where(x => x.AccountTransactionType == (int)AccountTransactonType.JournalEntry)
                    .Sum(x => x.Credit) - salesAdjustment;

                var thisPeriodDue = totalSalesWithAdjustment - totalCollectionWithAdjustment - totalSaleReturn;

                // Add to result list
                result.Add(new MarketingOfficerMonthlySalesCollectionViewModel()
                {
                    Year = year,
                    Month = month,
                    MonthName = monthName,
                    ThisPeriodSaleValue = totalSales,
                    ThisPeriodSaleReturnValue = totalSaleReturn,
                    ThisPeriodCollection = receivePaymentAgainstSale,
                    ThisPeriodAdjustment = totalAdjustment,
                    ThisPeriodBalance = thisPeriodDue,
                });
            }
            return result.OrderBy(x => x.Month).ToList();
        }

    }
}