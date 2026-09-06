import { Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { matxAnimations } from "app/shared/animations/matx-animations";
import { BdEastId, BdNorthId, BdSouthId } from "app/shared/consts/const";
import { DashboardFilterType } from "app/shared/enums/dashboardFilterType";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { ITheme, ThemeService } from "app/shared/services/theme.service";
import { DashboardLastMonthSaleViewModel } from "../models/dashboard-last-month-sale-view-model";
import { DashboardSalesFinancialYearViewModel } from "../models/dashboard-sales-financial-year-view-model";
import { DashboardStatisticsViewModel } from "../models/dashboard-statistics-view-model";
import { DashboardThisMonthSaleViewModel } from "../models/dashboard-this-month-sale-view-model";
import { DashboardDataService } from "../services/dashboard-data.service";

@Component({
  selector: "app-analytics",
  templateUrl: "./analytics.component.html",
  styleUrls: ["./analytics.component.scss"],
  animations: matxAnimations,
})
export class AnalyticsComponent implements OnInit {
  filterTypes = Object.keys(DashboardFilterType).map((key) => ({
    name: key,
    value: DashboardFilterType[key],
  }));
  cardData: DashboardStatisticsViewModel;
  thisMonthSaleData: DashboardThisMonthSaleViewModel[] = [];
  lastMonthSaleData: DashboardLastMonthSaleViewModel[] = [];
  salesFinancialYearData: DashboardSalesFinancialYearViewModel[] = [];
  bdSouths: DashboardSalesFinancialYearViewModel[] = [];
  bdEasts: DashboardSalesFinancialYearViewModel[] = [];
  bdNorths: DashboardSalesFinancialYearViewModel[] = [];
  dayValues: string[] = [];
  dayAmount: number[] = [];
  days: string[] = [];
  amounts: number[] = [];
  bdSouthMonths: number[] = [];
  bdSouthMonthNames: string[] = [];
  bdSouthSales: number[] = [];
  bdEastSales: number[] = [];
  bdNorthSales: number[] = [];
  year: string = "2025";
  filterType: string = "1";
  filterTypeName: string = "Today";
  options: any;
  zoneOptions: any;
  businessType: string;
  constructor(
    private themeService: ThemeService,
    private dashboardDataService: DashboardDataService,
    private activatedRoute: ActivatedRoute,
    private jwtAuth: JwtAuthService
  ) {}

  ngOnInit(): void {
    this.activatedRoute.data.subscribe((response: any) => {
      this.cardData = response?.dashboard?.data;
    });
    this.getDashboardThisMonthSales();
    this.getDashboardLastMonthSales();
    this.getDashboardSalesFinancialYear();
    // this.getData();
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    this.themeService.onThemeChange.subscribe((activeTheme) => {
      this.initLineChart(
        activeTheme,
        this.dayAmount,
        this.dayValues,
        this.amounts,
        this.days
      );
      this.initYearLineChart(
        activeTheme,
        this.bdSouthMonthNames,
        this.bdSouthSales,
        this.bdEastSales,
        this.bdNorthSales
      );
    });
    this.initLineChart(
      this.themeService.activatedTheme,
      this.dayAmount,
      this.dayValues,
      this.amounts,
      this.days
    );
    this.initYearLineChart(
      this.themeService.activatedTheme,
      this.bdSouthMonthNames,
      this.bdSouthSales,
      this.bdEastSales,
      this.bdNorthSales
    );
  }

  onSearch() {
    this.getDashboardStatistics();
  }

  getDashboardStatistics() {
    this.dashboardDataService
      .getDashboardStatistics(parseInt(this.filterType))
      .subscribe((response) => {
        this.cardData = response.data;
      });
  }
  getDashboardThisMonthSales() {
    this.dashboardDataService
      .getDashboardThisMonthSales()
      .subscribe((response) => {
        this.thisMonthSaleData = response.data;
        this.dayValues = this.thisMonthSaleData.map((item) =>
          item.day.toString()
        );
        this.dayAmount = this.thisMonthSaleData.map((item) => item.totalSales);
        this.initLineChart(
          this.themeService.activatedTheme,
          this.dayAmount,
          this.dayValues,
          this.amounts,
          this.days
        );
      });
  }
  getDashboardLastMonthSales() {
    this.dashboardDataService
      .getDashboardLastMonthSales()
      .subscribe((response) => {
        this.lastMonthSaleData = response.data;
        this.days = this.lastMonthSaleData.map((item) => item.day.toString());
        this.amounts = this.lastMonthSaleData.map((item) => item.totalSales);
        this.initLineChart(
          this.themeService.activatedTheme,
          this.dayAmount,
          this.dayValues,
          this.amounts,
          this.days
        );
      });
  }
  getDashboardSalesFinancialYear() {
    this.dashboardDataService
      .getDashboardSalesFinancialYear(parseInt(this.year))
      .subscribe((response) => {
        this.salesFinancialYearData = response.data;
        this.salesFinancialYearData.forEach((zone) => {
          switch (zone.zoneId) {
            case BdSouthId:
              this.bdSouths.push(zone);
              break;
            case BdEastId:
              this.bdEasts.push(zone);
              break;
            case BdNorthId:
              this.bdNorths.push(zone);
              break;
            default:
              break;
          }
        });
        this.bdSouthSales = this.bdSouths.map((sales) => sales.totalSales);
        this.bdEastSales = this.bdEasts.map((sales) => sales.totalSales);
        this.bdNorthSales = this.bdNorths.map((sales) => sales.totalSales);
        this.bdSouthMonths = this.bdNorths.map((sales) => sales.month);

        this.bdSouths.forEach((data) => {
          switch (data.month) {
            case 1:
              this.bdSouthMonthNames.push(
                "Jan'" + data.year.toString().substring(2, 4)
              );
              break;
            case 2:
              this.bdSouthMonthNames.push(
                "Feb'" + data.year.toString().substring(2, 4)
              );
              break;
            case 3:
              this.bdSouthMonthNames.push(
                "Mar'" + data.year.toString().substring(2, 4)
              );
              break;
            case 4:
              this.bdSouthMonthNames.push(
                "Apr'" + data.year.toString().substring(2, 4)
              );
              break;
            case 5:
              this.bdSouthMonthNames.push(
                "May'" + data.year.toString().substring(2, 4)
              );
              break;
            case 6:
              this.bdSouthMonthNames.push(
                "Jun'" + data.year.toString().substring(2, 4)
              );
              break;
            case 7:
              this.bdSouthMonthNames.push(
                "Jul'" + data.year.toString().substring(2, 4)
              );
              break;
            case 8:
              this.bdSouthMonthNames.push(
                "Aug'" + data.year.toString().substring(2, 4)
              );
              break;
            case 9:
              this.bdSouthMonthNames.push(
                "Sep'" + data.year.toString().substring(2, 4)
              );
              break;
            case 10:
              this.bdSouthMonthNames.push(
                "Oct'" + data.year.toString().substring(2, 4)
              );
              break;
            case 11:
              this.bdSouthMonthNames.push(
                "Nov'" + data.year.toString().substring(2, 4)
              );
              break;
            case 12:
              this.bdSouthMonthNames.push(
                "Dec'" + data.year.toString().substring(2, 4)
              );
              break;
            default:
              break;
          }
        });
        this.initYearLineChart(
          this.themeService.activatedTheme,
          this.bdSouthMonthNames,
          this.bdSouthSales,
          this.bdEastSales,
          this.bdNorthSales
        );
      });
  }

  onSelectedFilterType() {
    if (this.filterType == "1") this.filterTypeName = "Today";
    else if (this.filterType == "2") this.filterTypeName = "Last Day";
    else if (this.filterType == "3") this.filterTypeName = "Last Seven Days";
    else if (this.filterType == "4") this.filterTypeName = "This Month";
    else if (this.filterType == "5") this.filterTypeName = "Last Month";
    else this.filterTypeName = "";
    this.getDashboardStatistics();
  }

  initLineChart(
    theme: ITheme,
    thisMonthAmounts: number[],
    thisMonthDays: string[],
    lastMonthAmounts: number[],
    lastMonthDays: string[]
  ) {
    this.options = {
      tooltip: {
        show: true,
        trigger: "axis",
        backgroundColor: "#fff",
        extraCssText: "box-shadow: 0 0 3px rgba(0, 0, 0, 0.3); color: #444",
        axisPointer: {
          type: "line",
          animation: true,
        },
      },
      grid: {
        top: "10%",
        left: "110",
        right: "15",
        bottom: "60",
      },
      xAxis: {
        type: "category",
        data:
          lastMonthDays.length > thisMonthDays.length
            ? lastMonthDays
            : thisMonthDays,
        axisLine: {
          show: false,
        },
        axisLabel: {
          show: true,
          margin: 30,
          color: "#888",
        },
        axisTick: {
          show: false,
        },
      },
      yAxis: {
        type: "value",
        axisLine: {
          show: false,
        },
        axisLabel: {
          show: true,
          margin: 20,
          color: "#888",
        },
        axisTick: {
          show: false,
        },
        splitLine: {
          show: true,
          lineStyle: {
            type: "dashed",
          },
        },
      },
      series: [
        {
          data: lastMonthAmounts,
          type: "line",
          name: "Last Month",
          smooth: true,
          lineStyle: {
            opacity: 1,
            width: 3,
          },
          itemStyle: {
            opacity: 0,
            color: "rgba(25, 181, 254, 1)",
          },
          emphasis: {
            itemStyle: {
              color: "rgba(25, 181, 254, 1)",
              borderColor: "rgba(25, 181, 254, .4)",
              opacity: 1,
              borderWidth: 8,
            },
            label: {
              show: false,
              backgroundColor: "#fff",
            },
          },
        },
        {
          data: thisMonthAmounts, // Use thisMonthAmounts for the second series
          type: "line",
          name: "This Month",
          smooth: true,
          lineStyle: {
            opacity: 1,
            width: 3,
          },
          itemStyle: {
            opacity: 0,
            color: "rgba(38, 194, 129, 1)",
          },
          emphasis: {
            itemStyle: {
              color: "rgba(38, 194, 129, 1)",
              borderColor: "rgba(38, 194, 129, .4)",
              opacity: 1,
              borderWidth: 8,
            },
            label: {
              show: false,
              backgroundColor: "#fff",
            },
          },
        },
      ],
    };
  }
  initYearLineChart(
    theme: ITheme,
    months: string[],
    bdSouthSales: number[],
    bdEastSales: number[],
    bdNorthSales: number[]
  ) {
    this.zoneOptions = {
      tooltip: {
        show: true,
        trigger: "axis",
        backgroundColor: "#fff",
        extraCssText: "box-shadow: 0 0 3px rgba(0, 0, 0, 0.3); color: #444",
        axisPointer: {
          type: "line",
          animation: true,
        },
      },
      grid: {
        top: "10%",
        left: "110",
        right: "15",
        bottom: "60",
      },
      xAxis: {
        type: "category",
        data: months,
        axisLine: {
          show: false,
        },
        axisLabel: {
          show: true,
          margin: 30,
          color: "#888",
        },
        axisTick: {
          show: false,
        },
      },
      yAxis: {
        type: "value",
        axisLine: {
          show: false,
        },
        axisLabel: {
          show: true,
          margin: 20,
          color: "#888",
        },
        axisTick: {
          show: false,
        },
        splitLine: {
          show: true,
          lineStyle: {
            type: "dashed",
          },
        },
      },
      series: [
        {
          data: bdSouthSales,
          type: "line",
          name: "BD-SOUTH",
          smooth: true,
          lineStyle: {
            opacity: 1,
            width: 3,
          },
          itemStyle: {
            opacity: 0,
            color: "rgba(25, 181, 254, 1)",
          },
          emphasis: {
            itemStyle: {
              color: "rgba(25, 181, 254, 1)",
              borderColor: "rgba(25, 181, 254, .4)",
              opacity: 1,
              borderWidth: 8,
            },
            label: {
              show: false,
              backgroundColor: "#fff",
            },
          },
        },
        {
          data: bdEastSales, // Use thisMonthAmounts for the second series
          type: "line",
          name: "BD-EAST",
          smooth: true,
          lineStyle: {
            opacity: 1,
            width: 3,
          },
          itemStyle: {
            opacity: 0,
            color: "rgba(38, 194, 129, 1)",
          },
          emphasis: {
            itemStyle: {
              color: "rgba(38, 194, 129, 1)",
              borderColor: "rgba(38, 194, 129, .4)",
              opacity: 1,
              borderWidth: 8,
            },
            label: {
              show: false,
              backgroundColor: "#fff",
            },
          },
        },
        {
          data: bdNorthSales, // Use thisMonthAmounts for the second series
          type: "line",
          name: "BD-NORTH",
          smooth: true,
          lineStyle: {
            opacity: 1,
            width: 3,
          },
          itemStyle: {
            opacity: 0,
            color: "rgba(244, 208, 63, 1)",
          },
          emphasis: {
            itemStyle: {
              color: "rgba(244, 208, 63, 1)",
              borderColor: "rgba(244, 208, 63, .4)",
              opacity: 1,
              borderWidth: 8,
            },
            label: {
              show: false,
              backgroundColor: "#fff",
            },
          },
        },
      ],
    };
  }
}
