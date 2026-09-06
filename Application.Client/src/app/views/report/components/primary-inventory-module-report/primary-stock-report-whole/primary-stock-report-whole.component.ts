import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { environment } from "environments/environment";

@Component({
  selector: "app-primary-stock-report-whole",
  templateUrl: "./primary-stock-report-whole.component.html",
  styleUrls: ["./primary-stock-report-whole.component.scss"],
})
export class PrimaryStockReportWholeComponent implements OnInit {
  constructor(private http: HttpClient, private fb: FormBuilder) {}
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  ngOnInit(): void {
    this.initializeForm();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      storeId: [null],
      reportType: [1, Validators.required],
    });
  }

  printPdf($event) {
    if ($event?.submitter?.name == "pdf") {
      this.isLoading1 = true;
      this.searchForm.get("reportType").setValue(1);
    }
    if ($event?.submitter?.name == "excel") {
      this.searchForm.get("reportType").setValue(2);
      this.isLoading2 = true;
    }
    this.http
      .post(
        environment.apiURL + "/Report/primary-stock-report-whole/print",
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
          link.setAttribute("download", "Stock_Report_Whole.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
