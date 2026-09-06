using Application.Core.Common;
using Application.Core.Entities;
using Application.Services.Dtos.Accounts.AccountsReceivable.SaleInvoice;
using Application.Services.SearchRequestModels.Accounts.AccountsReceivable;
using Application.Services.SearchRequestModels.Production;
using Application.Services.SearchRequestModels.Report;
using Application.Services.SearchRequestModels.Sale;
using Application.Services.Services.Common;
using Application.Services.ViewModels.Accounts.AccountsReceivable;
using Application.Services.ViewModels.Accounts.AccountsReceivable.SaleInvoice;
using Application.Services.ViewModels.Purchase;
using Application.Services.ViewModels.Report.Sales;
using Application.Services.ViewModels.Sale;
using System.Dynamic;

namespace Application.Services.Services.Accounts.AccountsReceivable.SaleInvoices
{
    public interface ISaleInvoiceService : IBaseService<SaleInvoice, SaleInvoiceCreationDto, SaleInvoiceUpdateDto, SaleInvoiceRequestModel, SaleInvoiceViewModel>
    {
        Task<SaleInvoiceAggregatorModel> PrepareSaleInvoiceAggregatorModel(SaleInvoiceRequestModel request);
        Task<SaleInvoiceViewModel> GetByIdAsync(Guid id);
        new Task<AddUpdateResponseModel> AddAsync(SaleInvoiceCreationDto creationDto);
        new Task<AddUpdateResponseModel> UpdateAsync(SaleInvoiceUpdateDto updateDto);
        Task<PendingCheckedCountModel> GetPendingCheckedCountAsync();
        Task<bool> CheckAsync(Guid id);
        Task<bool> ApproveAsync(Guid id);
        Task<int> UnpostAsync(Guid id, int fromStatus);
        Task<bool> SendToCustomerAsync(Guid id);
        List<CustomerDiscountViewModel> GetCustomerMonthlyDiscountReport(int year, int? month, Guid? customerId);
        List<CustomerDiscountViewModel> GetCustomerDateWiseDiscountReport(CustomerDiscountRequestModel requestModel);
        Task<List<SalesItemViewModel>> CustomerWiseSalesItemSearchAsync(SaleInvoiceRequestModel request);
        Task<SalesReportViewModel> SalesReportSearchAsync(SaleInvoiceRequestModel request);
        Task<List<SalesTotalMonthWiseViewModel>> SalesTotalMonthWiseSearchAsync(SaleInvoiceRequestModel request);
        Task<List<SalesTotalDateWiseViewModel>> SalesTotalDateWiseSearchAsync(SaleInvoiceRequestModel request);
        Task<List<ExpandoObject>> MonthWiseProductSales();
        Task<List<CustomerLedgerFeedWiseViewModel>> CustomerLedgerFeedWiseSearchAsync(CustomerLedgerFeedWiseRequestModel request);
        Task<List<SalesItemDetailViewModel>> SalesItemSearchAsync(SalesItemDetailRequestModel request);
        Task<List<SalesAgingReportViewModel>> SalesAgingDataSearchAsync(SalesItemDetailRequestModel request);
    }
}
