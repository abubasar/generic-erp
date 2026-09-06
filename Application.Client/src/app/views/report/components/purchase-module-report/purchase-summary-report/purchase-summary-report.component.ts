import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { environment } from "environments/environment";

@Component({
  selector: "app-purchase-summary-report",
  templateUrl: "./purchase-summary-report.component.html",
  styleUrls: ["./purchase-summary-report.component.scss"],
})
export class PurchaseSummaryReportComponent implements OnInit {
  constructor(private http: HttpClient, private fb: FormBuilder) {}

  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: ["", Validators.required],
      toDate: ["", Validators.required],
      reportType: [1, Validators.required],
    });
  }

  printPdf($event) {
    if ($event?.submitter?.name == "pdf") {
      this.isLoading1 = true;
      this.searchForm.get("reportType").setValue(1);
    }
    if ($event?.submitter?.name == "excel") {
      this.isLoading2 = true;
      this.searchForm.get("reportType").setValue(2);
    }
    this.http
      .post(
        environment.apiURL + "/PurchaseInvoice/purchase-report-print",
        this.searchForm.value,
        {
          responseType: "blob",
        }
      )
      .subscribe((response) => {
        if ($event?.submitter?.name == "pdf") {
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
          link.setAttribute("download", "purchase_report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
