using Application.Core.StoredProcedureResult;

namespace Application.Services.Services.Report.Ledger
{
    public interface ILedgerService
    {
        Task<IList<SPSupplierLedgerResult>> SubsidiaryLedger(Guid supplierId, DateTime? fromDate, DateTime? toDate);
    }
}
