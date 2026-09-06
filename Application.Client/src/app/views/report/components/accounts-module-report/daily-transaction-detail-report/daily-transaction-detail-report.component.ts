import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { environment } from 'environments/environment';

@Component({
  selector: 'app-daily-transaction-detail-report',
  templateUrl: './daily-transaction-detail-report.component.html',
  styleUrls: ['./daily-transaction-detail-report.component.scss']
})
export class DailyTransactionDetailReportComponent implements OnInit {
  constructor(private http: HttpClient, private fb: FormBuilder) {}
  searchForm: FormGroup;
  isLoading: boolean = false;
  ngOnInit(): void {
    this.initializeForm();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: ["", Validators.required],
      toDate: ["", Validators.required],
    });
  }

  printPdf() {
    this.isLoading = true;
    this.http
      .post(
        environment.apiURL + "/Account/cash-bank-transaction-detail-ledger-print",
        this.searchForm.value,
        {
          responseType: "blob",
        }
      )
      .subscribe((response) => {
        this.isLoading = false;
        //Create a Blob from the PDF Stream
        const file = new Blob([response], { type: "application/pdf" });
        //Build a URL from the file
        const fileURL = URL.createObjectURL(file);
        //Open the URL on new Window
        const pdfWindow = window.open();
        pdfWindow.location.href = fileURL;
      });
  }
}
