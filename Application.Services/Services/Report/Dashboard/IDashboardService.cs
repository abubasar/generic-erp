using Application.Services.ViewModels.Report.Dashboard;
using Application.Services.ViewModels.Report.Sales;

namespace Application.Services.Services.Report.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardStatisticsViewModel> PrepareStatistics(int filterType);
        Task<List<SalesChartModel>> GetDayWiseThisMonthSales();
        Task<List<SalesChartModel>> GetDayWiseLastMonthSales();
        Task<List<SalesChartModel>> GetFinancialYearSales(int year);
    }
}
