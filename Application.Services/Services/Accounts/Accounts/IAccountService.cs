
using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Services.Dtos.Accounts.AccountIncludingCustomerSupplier;
using Application.Services.SearchRequestModels.Accounts;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.Reports;

namespace Application.Services.Services.Accounts.Accounts
{
    public interface IAccountService : IBaseService<Account, AccountCreationDto, AccountUpdateDto, AccountRequestModel, AccountViewModel>
    {
        Task<bool> AccountExists(string name);
        Task<string> GetAccountCode(Account account);
        Task<IList<AccountDropdownDto>> PrepareAllParentAccounts();
        Task<IList<AccountDropdownDto>> PrepareAllControlAccounts();
        Task<IList<AccountDropdownDto>> PrepareAllControlAccountsExcludingCustomers();
        Task<IList<AccountDropdownDto>> PrepareAllControlAccountsExcludingSuppliers();
        Task<List<ControlAccount>> GetAllControlAccountsByAccountId(Guid accountId);
        Task<List<ControlAccount>> GetAllControlAccountsByAccountIdForAppUser(Guid accountId);
        Task<decimal> CalculateBalanceByAccountId(Guid accountId, Guid accountTypeId, Guid financialYearId, DateTime? fromDate, DateTime? toDate);
        Task HitAccount(IUnitOfWork unitOfWork, Guid? costCenterId, string vnumber, string description, Guid accountId, decimal debit, decimal credit, DateTime transactionDate, string contraAccountIds, string contraAccountNames, int accountTransactionType = 0, int transactionQty = 0, decimal transactionQtyValue = 0m, Guid? paymentModeId = null, bool isOpeningBalance = false);
        Task UpdateTransaction(IUnitOfWork unitOfWork, IList<Transaction> transactions);
        Task<IList<ChartOfAccountModel>> PrepareChartOfAccount(IEnumerable<Account> accounts, Guid? rootAccountHeadId, int level, int levelsToLoad);
        Task<IList<SubsidiaryLedgerViewModel>> SubsidiaryLedger(Guid accountId, DateTime? fromDate, DateTime? toDate, Guid? costCenterId = null);
        Task<IList<CustomerLedgerProductWiseViewModel>> CustomerLedgerProductWise(Guid customerId, Guid? costCenterId, DateTime? fromDate, DateTime? toDate);
        Task<IList<SupplierLedgerProductWiseViewModel>> SupplierLedgerProductWise(Guid supplierId, Guid? costCenterId, DateTime? fromDate, DateTime? toDate);
        Task<IList<TrialBalanceViewModel>> TrialBalanceReportPrint(AccountReportRequestModel requestModel);
        Task<(IList<AccountHeadWithBalanceViewModel> nonCurrentAssetHeads, IList<AccountHeadWithBalanceViewModel> currentAssetHeads, IList<AccountHeadWithBalanceViewModel> nonCurrentLiabilityHeads, IList<AccountHeadWithBalanceViewModel> currentLiabilityHeads, IList<AccountHeadWithBalanceViewModel> ownersEquityHeads, IList<AccountHeadWithBalanceViewModel> othersEquityHeads, RootAccountBalance rootAccount)> BalanceSheet(int level, int levelsToLoad, DateTime? fromDate, DateTime? toDate, Guid financialYearId);
        Task<(IList<AccountHeadWithBalanceViewModel> revenueHeads, IList<AccountHeadWithBalanceViewModel> cogsHeads, IList<AccountHeadWithBalanceViewModel> operatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> otherIncomeHeads, IList<AccountHeadWithBalanceViewModel> nonOperatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> csrFundHeads, IList<AccountHeadWithBalanceViewModel> taxHeads, RootAccountBalance rootAccount)> IncomeStatement(int level, int levelsToLoad, DateTime? fromDate, DateTime? toDate, Guid financialYearId);
        Task<List<SupplierTransactionLedgerViewModel>> PrepareSupplierTransactionLedger(DateTime? fromDate, DateTime? toDate);
        Task<List<CustomerTransactionLedgerViewModel>> PrepareCustomerTransactionLedger(CustomerTransactionRequestModel request);
        Task<List<CashBankTransactionLedgerViewModel>> PrepareCashBankTransactionLedger(DateTime? fromDate, DateTime? toDate);
        Task<List<CashBankTransactionDetailLedgerViewModel>> PrepareCashBankTransactionDetailLedger(DateTime? fromDate, DateTime? toDate);
        Task<CashBookReportViewModel> PrepareCashBookTransactionDetailLedger(CashBookReportRequestModel requestModel);
        Task<List<Transaction>> PreparePaymentReport(DateTime? fromDate, DateTime? toDate, int? transactionType, Guid? costCenterId, Guid? supplierId, Guid? paymentModeId);
        Task<List<Transaction>> PrepareCollectionReport(PaymentCollectionRequestModel requestModel);
        Task<string> GetContraAccountName(Transaction item);
        Task<List<SalesAndCollectionViewModel>> PrepareCustomerWiseSalesAndCollection(SalesAndCollectionRequestModel request);
        Task<List<MarketingOfficerMonthlySalesCollectionViewModel>> PrepareYearlyCustomerWiseSalesAndCollection(MarketingOfficerYearlySalesAndCollectionRequestModel request);


    }
}
