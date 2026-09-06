using Application.Services.SearchRequestModels.Accounts;
using Application.Services.ViewModels.Accounts;
using Application.Services.ViewModels.Accounts.Reports;
using iTextSharp.text.pdf;

namespace Application.Services.Services.Accounts.Pdf
{
    public interface IAccountPdfService
    {
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintReportToPdfAsync<T>(int numberOfColumns, MemoryStream stream, List<TableHeader> headers, List<float> columnWidths, List<T> tableData, string reportTitleName, DateTime? fromDate = null, DateTime? toDate = null, List<Func<T, decimal>>? sumProperties = null, bool isLandscape = false);
        Task PrintSubsidiaryLedgerReportToPdfAsync(MemoryStream stream, IList<SubsidiaryLedgerViewModel> subsidiaryLedgerReportLines, string reportName, DateTime? fromDate, DateTime? toDate);
        Task PrintBalanceSheetReportToPdfAsync(MemoryStream stream, (IList<AccountHeadWithBalanceViewModel> nonCurrentAssetHeads, IList<AccountHeadWithBalanceViewModel> currentAssetHeads, IList<AccountHeadWithBalanceViewModel> nonCurrentLiabilityHeads, IList<AccountHeadWithBalanceViewModel> currentLiabilityHeads, IList<AccountHeadWithBalanceViewModel> ownersEquityHeads, IList<AccountHeadWithBalanceViewModel> othersEquityHeads, RootAccountBalance rootAccount) result, string reportName, DateTime? fromDate, DateTime? toDate);
        Task PrintIncomeStatementReportToPdfAsync(MemoryStream stream, (IList<AccountHeadWithBalanceViewModel> revenueHeads, IList<AccountHeadWithBalanceViewModel> cogsHeads, IList<AccountHeadWithBalanceViewModel> operatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> otherIncomeHeads, IList<AccountHeadWithBalanceViewModel> nonOperatingExpensesHeads, IList<AccountHeadWithBalanceViewModel> csrFundHeads, IList<AccountHeadWithBalanceViewModel> taxHeads, RootAccountBalance rootAccount) isHeads, string reportName, DateTime? fromDate, DateTime? toDate);
        Task PrintTrailBalanceReportToPdfAsync(MemoryStream stream, IList<TrialBalanceViewModel> list, string reportName, DateTime? fromDate, DateTime? toDate);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintSupplierTransactionLedgerReportToPdfAsync(MemoryStream stream, List<SupplierTransactionLedgerViewModel> result, string reportName, DateTime? fromDate, DateTime? toDate);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerTransactionLedgerReportToPdfAsync(MemoryStream stream, List<CustomerTransactionLedgerViewModel> result, string reportName, CustomerTransactionRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintDailyTransactionLedgerReportToPdfAsync(MemoryStream stream, List<CashBankTransactionLedgerViewModel> result, string reportName, DateTime? fromDate, DateTime? toDate);
        Task PrintDailyTransactionDetailLedgerReportToPdfAsync(MemoryStream stream, List<CashBankTransactionDetailLedgerViewModel> result, string reportName, DateTime? fromDate, DateTime? toDate);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCashBankBalanceReportToPdfAsync(MemoryStream stream, List<CashBankTransactionLedgerViewModel> result, string reportName, DateTime? fromDate, DateTime? toDate);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCashBookTransactionDetailLedgerReportToPdfAsync(MemoryStream stream, CashBookReportViewModel result, string reportTitle, CashBookReportRequestModel requestModel);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCogsCalculationReportToPdfAsync(MemoryStream stream, CogsViewModel result, string headerText, CogsCalculationRequestModel request);
        Task PrintFundTransferReportToPdf(MemoryStream stream, List<FundTransferViewModel> fundTransferViewModels, string reportTitle, bool isDetails, FundTransferRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintCustomerWiseSalesAndCollectionReportToPdfAsync(MemoryStream stream, List<SalesAndCollectionViewModel> result, string headerText, SalesAndCollectionRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintNationalWiseSalesCollectionAndDueReportToPdfAsync(MemoryStream stream, List<SalesAndCollectionViewModel> result, string headerText, SalesAndCollectionRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintMarketingOfficerWiseSalesCollectionAndDueReportToPdfAsync(MemoryStream stream, List<SalesAndCollectionViewModel> result, string headerText, SalesAndCollectionRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintMarketingOfficerWiseSalesCollectionAndDueDetailsReportToPdfAsync(MemoryStream stream, List<SalesAndCollectionViewModel> result, string headerText, SalesAndCollectionRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintMarketingOfficerWiseSalesCollectionAndDueReportShortToPdfAsync(MemoryStream stream, List<SalesAndCollectionViewModel> result, string headerText, SalesAndCollectionRequestModel request);
        Task<(PdfPTable filterTable, PdfPTable dataTable)> PrintMonthlySalesCollectionAndDueReportToPdfAsync(MemoryStream stream, List<MarketingOfficerMonthlySalesCollectionViewModel> result, string headerText, MarketingOfficerYearlySalesAndCollectionRequestModel request);
    }
}
