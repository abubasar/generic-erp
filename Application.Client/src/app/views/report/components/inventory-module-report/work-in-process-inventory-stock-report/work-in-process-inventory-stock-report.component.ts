import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { MatTableDataSource } from "@angular/material/table";
import { environment } from "environments/environment";
import { StockResponseDTO } from "../../../models/stock-response-dto";
import { StockService } from "../../../services/stock.service";

@Component({
  selector: "app-work-in-process-inventory-stock-report",
  templateUrl: "./work-in-process-inventory-stock-report.component.html",
  styleUrls: ["./work-in-process-inventory-stock-report.component.scss"],
})
export class WorkInProcessInventoryStockReportComponent implements OnInit {
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  displayedColumns: string[] = ["productName", "quantity", "value"];
  dataSource: MatTableDataSource<StockResponseDTO>;

  constructor(private stockService: StockService, private http: HttpClient) {}

  ngOnInit(): void {
    this.getWorkInProcessInventoryStockData();
  }

  getWorkInProcessInventoryStockData(): void {
    this.stockService.getWorkInProcessInventoryStockData().subscribe((res) => {
      this.dataSource = new MatTableDataSource<StockResponseDTO>(res);
    });
  }
  printPdf(buttonName: string) {
    let reportType;
    console.log(buttonName);
    if (buttonName == "pdf") {
      reportType = 1;
      this.isLoading1 = true;
    }
    if (buttonName == "excel") {
      reportType = 2;
      this.isLoading2 = true;
    }
    this.http
      .get(
        environment.apiURL +
          `/Report/work-in-process-inventory-stock/print/${reportType}`,
        {
          responseType: "blob",
        }
      )
      .subscribe((response) => {
        if (buttonName == "pdf") {
          //Create a Blob from the PDF Stream
          const file = new Blob([response], { type: "application/pdf" });
          //Build a URL from the file
          const fileURL = URL.createObjectURL(file);
          //Open the URL on new Window
          const pdfWindow = window.open();
          pdfWindow.location.href = fileURL;
          this.isLoading1 = false;
        } else {
          const blob = new Blob([response], {
            type: "application/octet-stream",
          });
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement("a");
          link.href = url;
          link.setAttribute("download", "work_in_process_inventory-stock.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
