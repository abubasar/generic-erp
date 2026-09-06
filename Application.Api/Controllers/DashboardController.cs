using Application.Core.Common;
using Application.Core.Enums;
using Application.Core.Interfaces;
using Application.Services.Services.Report.Dashboard;
using Application.Services.ViewModels.Report.Dashboard;
using Application.Services.ViewModels.Report.Sales;
using Microsoft.AspNetCore.Mvc;

namespace Application.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDashboardService _dashboardService;
        public DashboardController(IUnitOfWork unitOfWork, IDashboardService dashboardService)
        {
            _unitOfWork = unitOfWork;
            _dashboardService = dashboardService;
        }
        //[Authorize(Permissions.Dashboard.DashboardStatistics)]
        [Route("statistics/{filterType}")]
        [HttpGet]
        public async Task<Result<DashboardStatisticsViewModel>> Statistics(int filterType = (int)DashboardFilterType.Today)
        {
            return await Result<DashboardStatisticsViewModel>.SuccessAsync(await _dashboardService.PrepareStatistics(filterType), "Result Found");
        }
        [Route("this-month-sale")]
        [HttpGet]
        public async Task<Result<List<SalesChartModel>>> GetDayWiseThisMonthSales()
        {
            return await Result<List<SalesChartModel>>.SuccessAsync(await _dashboardService.GetDayWiseThisMonthSales(), "Result Found");
        }
        [Route("last-month-sale")]
        [HttpGet]
        public async Task<Result<List<SalesChartModel>>> GetDayWiseLastMonthSales()
        {
            return await Result<List<SalesChartModel>>.SuccessAsync(await _dashboardService.GetDayWiseLastMonthSales(), "Result Found");
        }
        [Route("sales-financial-year/{year}")]
        [HttpGet]
        public async Task<Result<List<SalesChartModel>>> GetFinancialYearSales(int year)
        {
            return await Result<List<SalesChartModel>>.SuccessAsync(await _dashboardService.GetFinancialYearSales(year), "Result Found");
        }

    }
}
