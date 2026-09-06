import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatTableDataSource } from "@angular/material/table";
import { StockResponseDTO } from "app/views/report/models/stock-response-dto";
import { StockSearchRequestDTO } from "app/views/report/models/stock-search-request-dto";
import { StockService } from "app/views/report/services/stock.service";

@Component({
  selector: "app-dashboard-low-stock-report",
  templateUrl: "./dashboard-low-stock-report.component.html",
  styleUrls: ["./dashboard-low-stock-report.component.scss"],
})
export class DashboardLowStockReportComponent implements OnInit {
  panelOpenState: boolean;
  searchForm: FormGroup;
  displayedColumns: string[] = [
    "productName",
    "storeName",
    "quantity",
  ];
  dataSource: MatTableDataSource<StockResponseDTO>;
  totalCount: number;
  stockRequest = new StockSearchRequestDTO();
  constructor(private stockService: StockService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.getLowStockData();
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
    });
  }

  getLowStockData(): void {
    this.stockService.getLowStockData(this.stockRequest).subscribe((res) => {
      this.dataSource = new MatTableDataSource<StockResponseDTO>(
        res?.data?.item1
      );
      this.totalCount = res?.data?.item2;
    });
  }
  onSearch() {
    console.log(this.searchForm.value);
  }
  onPageChange(pageEvent) {
    this.stockRequest.page = pageEvent.pageIndex;
    this.stockRequest.rowsPerPage = pageEvent.pageSize;
    this.getLowStockData();
  }
}
