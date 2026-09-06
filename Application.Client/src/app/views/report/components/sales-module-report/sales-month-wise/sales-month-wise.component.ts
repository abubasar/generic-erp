import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { environment } from "environments/environment";

@Component({
  selector: "app-sales-month-wise",
  templateUrl: "./sales-month-wise.component.html",
  styleUrls: ["./sales-month-wise.component.scss"],
})
export class SalesMonthWiseComponent implements OnInit {
  constructor(private http: HttpClient, private fb: FormBuilder) {}
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  ngOnInit(): void {
    this.initializeForm();
  }
  initializeForm() {
    this.searchForm = this.fb.group({});
  }

  printPdf($event) {
    let reportType = 0;
    if ($event?.submitter?.name == "pdf") {
      this.isLoading1 = true;
      reportType = 1;
    }
    if ($event?.submitter?.name == "excel") {
      reportType = 2;
      this.isLoading2 = true;
    }
    this.http
      .post(
        environment.apiURL +
          "/SaleInvoice/sales-report-month-wise/print/" +
          reportType,
        this.searchForm.value,
        {
          responseType: "blob",
        }
      )
      .subscribe((response) => {
        if ($event?.submitter?.name == "pdf") {
          const file = new Blob([response], { type: "application/pdf" });
          const fileURL = URL.createObjectURL(file);
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
          link.setAttribute("download", "Sales_Month_Wise_Report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
