using Application.Core.Entities;
using Application.Core.Interfaces;
using Application.Core.StoredProcedureResult;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Report.Ledger
{
    public class LedgerService : ILedgerService
    {
        private readonly IUnitOfWork _unitOfWork;
        public LedgerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<SPSupplierLedgerResult>> SubsidiaryLedger(Guid supplierId, DateTime? fromDate, DateTime? toDate)
        {
            var result = new List<SPSupplierLedgerResult>();
            var financialYear = await _unitOfWork.Repository<FinancialYear>().TableNoTracking().FirstOrDefaultAsync(x => x.IsActive);

            var allTransactions = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountId == supplierId)
                .Where(x => x.FinancialYearId == financialYear!.Id);

            //opening balance
            var openingBalanceTransactions = allTransactions.Where(x => x.IsOpeningBalance).ToList();
            //call stored procedure
            var sqlParameters = new[]
             {
                new SqlParameter
                {
                    ParameterName = "supplierId",
                    Value = supplierId,
                    SqlDbType = System.Data.SqlDbType.UniqueIdentifier,
                },
                new SqlParameter
                {
                    ParameterName = "fromDate",
                    Value = fromDate ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.DateTime,
                },
                new SqlParameter
                {
                    ParameterName = "toDate",
                    Value = toDate ?? Convert.DBNull,
                    SqlDbType = System.Data.SqlDbType.DateTime,
                }
            };
            var accountTransactions = await _unitOfWork.Repository<SPSupplierLedgerResult>().ExecuteStoredProcedureAsync("spSupplierLedger", sqlParameters);
            IList<SPSupplierLedgerResult> transactionsUptoFromDate = new List<SPSupplierLedgerResult>();
            if (fromDate.HasValue)
                transactionsUptoFromDate = accountTransactions.Where(x => x.BillDate.Date >= financialYear!.StartDate.Date && x.BillDate.Date < fromDate.Value.Date).ToList();
            //this period transactions
            if (fromDate.HasValue && toDate.HasValue)
                accountTransactions = accountTransactions.Where(x => x.BillDate.Date >= fromDate.Value.Date && x.BillDate.Date <= toDate.Value.Date).ToList();
            var openingCredit = openingBalanceTransactions.Sum(x => x.Credit) +
                    (transactionsUptoFromDate.Any() ? transactionsUptoFromDate.Sum(x => x.Credit) : 0M);
            var openingDebit = openingBalanceTransactions.Sum(x => x.Debit) +
                    (transactionsUptoFromDate.Any() ? transactionsUptoFromDate.Sum(x => x.Debit) : 0M);

            accountTransactions = accountTransactions.OrderBy(x => x.BillDate).ToList();
            foreach (var item in accountTransactions)
            {
                openingCredit += item.Credit;
                openingDebit += item.Debit;
                result.Add(new SPSupplierLedgerResult
                {
                    BillDate = item.BillDate,
                    BillNo = item.BillNo,
                    Particular = item.Particular,
                    Po = item.Po,
                    Qty = item.Qty,
                    Rate = item.Rate,
                    Amount = item.Amount,
                    Debit = item.Debit,
                    Credit = item.Credit,
                    Balance = openingCredit - openingDebit
                });
            }
            return result;
        }
    }
}
