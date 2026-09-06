import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Department_Id_SALES_AND_MARKETING } from "app/shared/consts/const";
import { EmployeeRequest } from "app/views/configuration/models/employee/employee-request.model";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { Zone } from "app/views/configuration/models/zone/zone.model";
import { EmployeeService } from "app/views/configuration/services/employee.service";
import { ZoneService } from "app/views/configuration/services/zone.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-customer-transaction-report",
  templateUrl: "./customer-transaction-report.component.html",
  styleUrls: ["./customer-transaction-report.component.scss"],
})
export class CustomerTransactionReportComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private zoneService: ZoneService,
    private employeeService: EmployeeService
  ) {}
  searchForm: FormGroup;
  marketingOfficers: Employee[];
  filterMarketingOfficers: Employee[];
  zones: Zone[];
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  ngOnInit(): void {
    this.initializeForm();
    this.getAllMarketingOfficers();
    this.getAllZones();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: ["", Validators.required],
      toDate: ["", Validators.required],
      customerZoneId: [null],
      customerMarketingOfficerId: [null],
      reportType: [1, Validators.required],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.customerZoneId === "") {
        this.searchForm
          .get("customerZoneId")
          ?.patchValue(null, { emitEvent: false });
      }
      if (value.customerMarketingOfficerId === "") {
        this.searchForm
          .get("customerMarketingOfficerId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  clearInput(evt: any): void {
    evt.stopPropagation();
    this.searchForm?.get("customerMarketingOfficerId").setValue(null);
  }

  /**---------- Product Autocomplete------------ */
  onMarketingOfficerChange(value: string): void {
    const term = value.toLowerCase();
    this.filterMarketingOfficer(term || "");
  }

  private filterMarketingOfficer(value: string): void {
    this.filterMarketingOfficers = this.marketingOfficers?.filter((option) =>
      (
        option.firstName.toLowerCase() +
        " " +
        option.lastName.toLowerCase()
      ).includes(value)
    );
  }

  getMarketingOfficerName(customerMarketingOfficerId: string) {
    if (!customerMarketingOfficerId) {
      return;
    }
    const marketingOfficer = this.marketingOfficers?.find(
      (marketingOfficer) => marketingOfficer?.id === customerMarketingOfficerId
    );
    if (!marketingOfficer) return "";
    return marketingOfficer.firstName + " " + marketingOfficer.lastName;
  }

  getAllMarketingOfficers(): void {
    let employeeRequest = new EmployeeRequest();
    employeeRequest.page = -1;
    employeeRequest.departmentId = Department_Id_SALES_AND_MARKETING;
    this.employeeService.getEmployees(employeeRequest).subscribe((res) => {
      this.filterMarketingOfficers = this.marketingOfficers = res?.data?.item1;
    });
  }

  /**---------- End Autocomplete----------- */

  getAllZones() {
    this.zoneService.getAllZones().subscribe((res) => {
      this.zones = res?.data?.item1;
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
        environment.apiURL + "/Account/customer-transaction-ledger-print",
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
          link.setAttribute("download", "customer_transaction.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
