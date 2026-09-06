using Application.Core.Constants;
using Application.Core.Entities;
using Application.Core.Enums;
using Application.Core.Extensions;
using Application.Core.Interfaces;
using Application.Services.Services.Common;
using Application.Services.Services.Configuration.Tenants;
using Application.Services.ViewModels.Report.Dashboard;
using Application.Services.ViewModels.Report.Sales;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Services.Report.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWorkContext _workContext;
        private readonly ITenantService _tenantService;
        public DashboardService(IWorkContext workContext, IUnitOfWork unitOfWork, ITenantService tenantService)
        {
            _workContext = workContext;
            _unitOfWork = unitOfWork;
            _tenantService = tenantService;
        }

        public async Task<DashboardStatisticsViewModel> PrepareStatistics(int filterType)
        {
            Guid? tenantId = _workContext.GetTenantId();
            var tenantData = await _tenantService.GetByIdAsync(tenantId);
            DateTime fromDate = DateTime.Now;
            DateTime toDate = DateTime.Now;
            var today = DateTime.Today;
            var firstDateOfThisMonth = new DateTime(today.Year, today.Month, 1);
            var firstDateOfLastMonth = firstDateOfThisMonth.AddMonths(-1);
            var lastDateOfLastMonth = firstDateOfThisMonth.AddDays(-1);
            switch (filterType)
            {
                case (int)DashboardFilterType.Today:
                    fromDate = fromDate.Date;
                    toDate = toDate.Date;
                    break;
                case (int)DashboardFilterType.Last_Day:
                    fromDate = fromDate.AddDays(-1).Date;
                    toDate = toDate.AddDays(-1).Date;
                    break;
                case (int)DashboardFilterType.Last_Seven_Days:
                    fromDate = fromDate.AddDays(-6).Date;
                    toDate = toDate.Date;
                    break;
                case (int)DashboardFilterType.This_Month:
                    fromDate = firstDateOfThisMonth.Date;
                    toDate = toDate.Date;
                    break;
                case (int)DashboardFilterType.Last_Month:
                    fromDate = firstDateOfLastMonth.Date;
                    toDate = lastDateOfLastMonth.Date;
                    break;
                default:
                    break;
            }
            return new DashboardStatisticsViewModel
            {
                PurchaseTotal = tenantData.BusinessType == 2 ? await GetPurchaseTotal(fromDate, toDate) : 0m,
                RMPurchaseValueTotal = tenantData.BusinessType == 1 ? await GetRMPurchaseValueTotal(fromDate, toDate) : 0m,
                RmConsumptionTotal = await GetRmConsumptionTotal(fromDate, toDate),
                ProductionTotalQty = tenantData.BusinessType == 2 ? await GetProductionQtyTotal(fromDate, toDate) : 0m,
                ProductionTotalValue = tenantData.BusinessType == 1 ? await GetProductionValueTotal(fromDate, toDate) : 0m,
                SalesTotalQty = tenantData.BusinessType == 2 ? await GetSaleQtyTotal(fromDate, toDate) : 0m,
                SalesTotalValue = await GetSaleTotal(fromDate, toDate),
                FGPurchaseValueTotal = tenantData.BusinessType == 1 ? await GetFGPurchaseValueTotal(fromDate, toDate) : 0m,
                SupplierPaymentTotal = await GetSupplierPayment(fromDate, toDate),
                PaymentForExpenseTotal = await GetOtherPayment(fromDate, toDate),
                ReceivedFromCustomerTotal = await GetCustomerReceipt(fromDate, toDate),
                ReceivedFromOtherSourceTotal = await GetOtherReceipt(fromDate, toDate),
                RMValueTotal = await GetRMValueTotal(fromDate, toDate),
                FGQtyTotal = tenantData.BusinessType == 2 ? await GetFGQtyTotal(fromDate, toDate) : 0m,
                FGValueTotal = tenantData.BusinessType == 1 ? await GetFGValueTotal(fromDate, toDate) : 0m,
            };
        }

        private async Task<decimal> GetProductionQtyTotal(DateTime createdFrom, DateTime createdTo)
        {
            var production = _unitOfWork.Repository<Production>().TableNoTracking().Where(x => x.Status >= (int)ProductionStatus.Approved
              && createdFrom <= x.ProductionDate.Date && createdTo >= x.ProductionDate.Date);
            return await production.SumAsync(x => x.ActualProductionQuantity);
        }
        private async Task<decimal> GetProductionValueTotal(DateTime createdFrom, DateTime createdTo)
        {
            var production = _unitOfWork.Repository<Production>().TableNoTracking().Where(x => x.Status >= (int)ProductionStatus.Approved
              && createdFrom <= x.ProductionDate.Date && createdTo >= x.ProductionDate.Date);
            return await production.SumAsync(x => x.TotalCost + x.TotalAdjustmentCost);
        }
        private async Task<decimal> GetRMValueTotal(DateTime createdFrom, DateTime createdTo)
        {
            var rmStocksQueryable = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => !x.IsUnpostedEntry &&
            x.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Raw_Materials.ToGuid());
            var openingTransactions = rmStocksQueryable.Where(x => x.StockInDate.Date < createdFrom);
            decimal openingValue = await openingTransactions.SumAsync(x => x.InQty * (x.InRate + x.InTransportCost)) - openingTransactions.Sum(x => x.OutQty * x.OutRate);
            //this period transactions
            var thisPeriodTransactions = rmStocksQueryable.Where(x => x.StockInDate.Date >= createdFrom && x.StockInDate.Date <= createdTo);
            decimal receivedValue = await thisPeriodTransactions.SumAsync(x => x.InQty * (x.InRate + x.InTransportCost));
            decimal issuedValue = await thisPeriodTransactions.SumAsync(x => x.OutQty * x.OutRate);
            return Math.Round((openingValue + receivedValue - issuedValue), 2);
        }
        private async Task<decimal> GetFGQtyTotal(DateTime createdFrom, DateTime createdTo)
        {
            var rmStocksQueryable = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => !x.IsUnpostedEntry &&
            x.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid());
            var openingTransactions = rmStocksQueryable.Where(x => x.StockInDate.Date < createdFrom);
            decimal openingQuantity = await openingTransactions.SumAsync(x => x.InQty) - openingTransactions.Sum(x => x.OutQty);
            //this period transactions
            var thisPeriodTransactions = rmStocksQueryable.Where(x => x.StockInDate.Date >= createdFrom && x.StockInDate.Date <= createdTo);
            decimal receivedQuantity = await thisPeriodTransactions.SumAsync(x => x.InQty);
            decimal issuedQuantity = await thisPeriodTransactions.SumAsync(x => x.OutQty);
            return Math.Round((openingQuantity + receivedQuantity - issuedQuantity), 2);
        }
        private async Task<decimal> GetFGValueTotal(DateTime createdFrom, DateTime createdTo)
        {
            var fgStocksQueryable = _unitOfWork.Repository<Stock>().TableNoTracking().Where(x => !x.IsUnpostedEntry &&
            x.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid());
            var openingTransactions = fgStocksQueryable.Where(x => x.StockInDate.Date < createdFrom);
            decimal openingFGValue = await openingTransactions.SumAsync(x => x.InQty * (x.InRate + x.InTransportCost)) - openingTransactions.Sum(x => x.OutQty * x.OutRate);
            //this period transactions
            var thisPeriodTransactions = fgStocksQueryable.Where(x => x.StockInDate.Date >= createdFrom && x.StockInDate.Date <= createdTo);
            decimal receivedValue = await thisPeriodTransactions.SumAsync(x => x.InQty * (x.InRate + x.InTransportCost));
            decimal issuedValue = await thisPeriodTransactions.SumAsync(x => x.OutQty * x.OutRate);
            return Math.Round((openingFGValue + receivedValue - issuedValue), 2);
        }
        private async Task<decimal> GetSaleQtyTotal(DateTime createdFrom, DateTime createdTo)
        {
            var sale = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountTransactionType
             == (int)AccountTransactonType.Sale && createdFrom <= x.TransactionDate.Date && createdTo >= x.TransactionDate.Date);
            return await sale.SumAsync(x => x.TransactionQty);
        }
        private async Task<decimal> GetPurchaseTotal(DateTime createdFrom, DateTime createdTo)
        {
            var purchase = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountTransactionType
             == (int)AccountTransactonType.Purchase && createdFrom <= x.TransactionDate.Date && createdTo >= x.TransactionDate.Date);
            return await purchase.SumAsync(x => x.Credit);
        }
        private async Task<decimal> GetFGPurchaseValueTotal(DateTime createdFrom, DateTime createdTo)
        {
            var purchase = await _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().Include(x => x.Store).Where(x => x.Status >= (int)PurchaseInvoiceStatus.Approved
            && x.Store.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Finished_Goods.ToGuid()
            && x.InvoiceDate.Date >= createdFrom.Date && x.InvoiceDate.Date <= createdTo).ToListAsync();
            var fgPurchaseValueTotal = 0m;
            foreach (var item in purchase)
            {
                if (item.IsImportPurchase == true) fgPurchaseValueTotal += item.PurchaseOrderTotal;
                else fgPurchaseValueTotal += item.Total;
            }
            return fgPurchaseValueTotal;
        }
        private async Task<decimal> GetRMPurchaseValueTotal(DateTime createdFrom, DateTime createdTo)
        {
            var purchase = await _unitOfWork.Repository<PurchaseInvoice>().TableNoTracking().Include(x => x.Store).Where(x => x.Status >= (int)PurchaseInvoiceStatus.Approved
            && x.Store.InventoryTypeId == InventoryTypeConstants.Inventory_Type_Id_Raw_Materials.ToGuid()
            && x.InvoiceDate.Date >= createdFrom.Date && x.InvoiceDate.Date <= createdTo).ToListAsync();
            var rmPurchaseValueTotal = 0m;
            foreach (var item in purchase)
            {
                if (item.IsImportPurchase == true) rmPurchaseValueTotal += item.PurchaseOrderTotal;
                else rmPurchaseValueTotal += item.Total;
            }
            return rmPurchaseValueTotal;
        }
        private async Task<decimal> GetRmConsumptionTotal(DateTime createdFrom, DateTime createdTo)
        {
            var manufacturingOrders = _unitOfWork.Repository<ManufacturingOrder>().TableNoTracking().Where(x => x.Status
             >= (int)ManufacturingOrderStatus.Approved && createdFrom <= x.ScheduledDate.Date && createdTo >= x.ScheduledDate.Date);
            return await manufacturingOrders.SumAsync(x => x.RmCost);
        }
        private async Task<decimal> GetSaleTotal(DateTime createdFrom, DateTime createdTo)
        {
            var sale = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountId == AccountHeadConstants.Sales.ToGuid() && createdFrom <= x.TransactionDate.Date && createdTo >= x.TransactionDate.Date);
            return await sale.SumAsync(x => x.Credit);
        }

        private async Task<decimal> GetSupplierPayment(DateTime createdFrom, DateTime createdTo)
        {
            var supplierPayment = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountTransactionType
            == (int)AccountTransactonType.SupplierPayment && createdFrom <= x.TransactionDate.Date && createdTo >= x.TransactionDate.Date);
            return await supplierPayment.SumAsync(x => x.Debit);

        }
        private async Task<decimal> GetOtherPayment(DateTime createdFrom, DateTime createdTo)
        {
            var otherPayment = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountTransactionType
            == (int)AccountTransactonType.OtherPayment && createdFrom <= x.TransactionDate.Date && createdTo >= x.TransactionDate.Date);
            return await otherPayment.SumAsync(x => x.Debit);

        }
        private async Task<decimal> GetCustomerReceipt(DateTime createdFrom, DateTime createdTo)
        {
            var customerReceipt = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountTransactionType
            == (int)AccountTransactonType.CustomerReceipt && createdFrom <= x.TransactionDate.Date && createdTo >= x.TransactionDate.Date);
            var receivePaymentAgainstSale = await _unitOfWork.Repository<ReceivePaymentAgainstSale>().TableNoTracking().Where(x => x.Status >= (int)ReceivePaymentStatus.Approved && x.PaymentDate.Date >= createdFrom.Date && x.PaymentDate.Date <= createdTo.Date).ToListAsync();
            return await customerReceipt.SumAsync(x => x.Credit) + receivePaymentAgainstSale.Sum(x => x.Amount);
        }
        private async Task<decimal> GetOtherReceipt(DateTime createdFrom, DateTime createdTo)
        {
            var otherReceipt = _unitOfWork.Repository<Transaction>().TableNoTracking().Where(x => x.AccountTransactionType
            == (int)AccountTransactonType.OtherReceipt && createdFrom <= x.TransactionDate.Date && createdTo >= x.TransactionDate.Date);
            return await otherReceipt.SumAsync(x => x.Credit);

        }

        public async Task<List<SalesChartModel>> GetDayWiseThisMonthSales()
        {
            var today = DateTime.Today;
            var firstDateOfThisMonth = new DateTime(today.Year, today.Month, 1);
            var fromDate = firstDateOfThisMonth.Date;
            var toDate = DateTime.Now.Date;

            // Generate a list of all days in the current month
            var allDaysInMonth = Enumerable.Range(1, DateTime.DaysInMonth(today.Year, today.Month))
                                           .Select(day => new DateTime(today.Year, today.Month, day))
                                           .ToList();

            var salesData = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking()
                .Where(x => x.Status >= (int)SaleInvoiceStatus.Approved && fromDate <= x.InvoiceDate.Date && toDate >= x.InvoiceDate.Date)
                .GroupBy(s => s.InvoiceDate.Date)
                .Select(group => new SalesChartModel
                {
                    Day = group.Key.Day,
                    TotalSales = group.Sum(s => s.NetTotal)
                })
                .OrderBy(x => x.Day)
                .ToListAsync();

            // Left join with allDaysInMonth to fill in the gaps
            var result = allDaysInMonth
                .GroupJoin(salesData, day => day.Day, sale => sale?.Day ?? 0, (day, sales) => new SalesChartModel
                {
                    Day = day.Day,
                    TotalSales = sales.Any() ? sales.Sum(s => s?.TotalSales ?? 0) : 0
                })
                .OrderBy(x => x.Day)
                .ToList();

            return result;
        }

        public async Task<List<SalesChartModel>> GetDayWiseLastMonthSales()
        {
            var today = DateTime.Today;
            var firstDateOfThisMonth = new DateTime(today.Year, today.Month, 1);
            var firstDateOfLastMonth = firstDateOfThisMonth.AddMonths(-1);
            var lastDateOfLastMonth = firstDateOfThisMonth.AddDays(-1);
            var fromDate = firstDateOfLastMonth.Date;
            var toDate = lastDateOfLastMonth.Date;

            // Generate a list of all days in the last month
            var allDaysInLastMonth = Enumerable.Range(1, DateTime.DaysInMonth(firstDateOfLastMonth.Year, firstDateOfLastMonth.Month))
                                               .Select(day => new DateTime(firstDateOfLastMonth.Year, firstDateOfLastMonth.Month, day))
                                               .ToList();

            var salesData = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking()
                .Where(x => x.Status >= (int)SaleInvoiceStatus.Approved && fromDate <= x.InvoiceDate.Date && toDate >= x.InvoiceDate.Date)
                .GroupBy(s => s.InvoiceDate.Date)
                .Select(group => new SalesChartModel
                {
                    Day = group.Key.Day,
                    TotalSales = group.Sum(s => s.NetTotal)
                })
                .OrderBy(x => x.Day)
                .ToListAsync();

            // Left join with allDaysInLastMonth to fill in the gaps
            var result = allDaysInLastMonth
                .GroupJoin(salesData, day => day.Day, sale => sale?.Day ?? 0, (day, sales) => new SalesChartModel
                {
                    Day = day.Day,
                    TotalSales = sales.Any() ? sales.Sum(s => s?.TotalSales ?? 0) : 0
                })
                .OrderBy(x => x.Day)
                .ToList();

            return result;
        }

        public async Task<List<SalesChartModel>> GetFinancialYearSales(int year)
        {
            var startDate = new DateTime(year, 7, 1); // Start of the financial year (July 1st)
            var endDate = new DateTime(year + 1, 6, 30); // End of the financial year (June 30th of the next year)
            var salesData = await _unitOfWork.Repository<SaleInvoice>().TableNoTracking().Include(x => x.Customer)
                 .Where(x => x.Status >= (int)SaleInvoiceStatus.Approved && startDate <= x.InvoiceDate.Date && endDate >= x.InvoiceDate.Date)
                 .GroupBy(s => new { s.InvoiceDate.Year, s.InvoiceDate.Month, s.Customer.CustomerZoneId })
                 .Select(group => new SalesChartModel
                 {
                     Year = group.Key.Year,
                     Month = group.Key.Month,
                     ZoneId = group.Key.CustomerZoneId,
                     TotalSales = group.Sum(s => s.NetTotal)
                 }).OrderBy(x => x.Year).ThenBy(x => x.Month).ToListAsync();
            return salesData;
        }
    }

}

