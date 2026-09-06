import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { environment } from 'environments/environment';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: "app-excel-upload",
  templateUrl: "./excel-upload.component.html",
  styleUrls: ["./excel-upload.component.scss"],
})
export class ExcelUploadComponent implements OnInit {
  constructor(private http: HttpClient, private toastr: ToastrService) {}
  loading1: boolean = false;
  loading2: boolean = false;
  loading3: boolean = false;
  loading4: boolean = false;
  loading5: boolean = false;
  ngOnInit(): void {}
  selectedRMFile = null;
  selectedFGFile = null;
  selectedBankBalanceFile = null;
  selectedCustomerBalanceFile = null;
  selectedSupplierBalanceFile = null;
  onRMFileChange($event) {
    this.selectedRMFile = $event.target.files[0];
  }
  onRMFileUpload() {
    this.loading1 = true;
    const formData = new FormData();
    formData.append("importexcelfile", this.selectedRMFile);
    this.http
      .post<any>(environment.apiURL + "/ImportFromExcel/rm-upload", formData)
      .subscribe((res) => {
        this.toastr.success(res?.message);
        this.loading1 = false;
      });
  }
  onFGFileChange($event) {
    this.selectedFGFile = $event.target.files[0];
  }
  onFGFileUpload() {
    this.loading2 = true;
    const formData = new FormData();
    formData.append("importexcelfile", this.selectedFGFile);
    this.http
      .post<any>(environment.apiURL + "/ImportFromExcel/fg-upload", formData)
      .subscribe((res) => {
        this.toastr.success(res?.message);
        this.loading2 = false;
      });
  }
  onBankBalanceFileChange($event) {
    this.selectedBankBalanceFile = $event.target.files[0];
  }
  onBankBalanceFileUpload() {
    this.loading3 = true;
    const formData = new FormData();
    formData.append("importexcelfile", this.selectedBankBalanceFile);
    this.http
      .post<any>(
        environment.apiURL + "/ImportFromExcel/bank-balance-upload",
        formData
      )
      .subscribe((res) => {
        this.toastr.success(res?.message);
        this.loading3 = false;
      });
  }
  onCustomerBalanceFileChange($event) {
    this.selectedCustomerBalanceFile = $event.target.files[0];
  }
  onCustomerBalanceFileUpload() {
    this.loading4 = true;
    const formData = new FormData();
    formData.append("importexcelfile", this.selectedCustomerBalanceFile);
    this.http
      .post<any>(
        environment.apiURL + "/ImportFromExcel/customer-balance-upload",
        formData
      )
      .subscribe((res) => {
        this.toastr.success(res?.message);
        this.loading4 = false;
      });
  }
  onSupplierBalanceFileChange($event) {
    this.selectedSupplierBalanceFile = $event.target.files[0];
  }
  onSupplierBalanceFileUpload() {
    this.loading5 = true;
    const formData = new FormData();
    formData.append("importexcelfile", this.selectedSupplierBalanceFile);
    this.http
      .post<any>(
        environment.apiURL + "/ImportFromExcel/supplier-balance-upload",
        formData
      )
      .subscribe((res) => {
        this.toastr.success(res?.message);
        this.loading5 = false;
      });
  }
  download(fileName:string) {
    this.http
      .get(
        environment.apiURL + "/ImportFromExcel/download/"+fileName,
        {
          responseType: "blob",
        }
      )
      .subscribe((res) => {
       const blob = new Blob([res], { type: "application/octet-stream" });
       const url = window.URL.createObjectURL(blob);
       const link = document.createElement("a");
       link.href = url;
       link.setAttribute("download", fileName+".xlsx");
       document.body.appendChild(link);
       link.click();
      });
  }
}
