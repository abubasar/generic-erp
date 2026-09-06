import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Department_Id_SALES_AND_MARKETING } from "app/shared/consts/const";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { EmployeeRequest } from "app/views/configuration/models/employee/employee-request.model";
import { Employee } from "app/views/configuration/models/employee/employee.model";
import { FinancialYear } from "app/views/configuration/models/financial-year/financial-year.model";
import { EmployeeService } from "app/views/configuration/services/employee.service";
import { FinancialYearService } from "app/views/configuration/services/financial-year.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-primary-month-wise-sales-collection-and-due-report",
  templateUrl:
    "./primary-month-wise-sales-collection-and-due-report.component.html",
  styleUrls: [
    "./primary-month-wise-sales-collection-and-due-report.component.scss",
  ],
})
export class PrimaryMonthWiseSalesCollectionAndDueReportComponent
  implements OnInit
{
  businessType: string;
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private employeeService: EmployeeService,
    private financialYearService: FinancialYearService,
    private jwtAuth: JwtAuthService
  ) {}
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  yearList: number[] = [];
  financialYears: FinancialYear[];
  marketingOfficers: Employee[];
  filterMarketingOfficers: Employee[];

  ngOnInit(): void {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.businessType = res.businesstype;
    });
    this.initializeForm();
    this.getAllFinancialYears();
    this.getAllMarketingOfficers();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      year: [null, Validators.required],
      customerMarketingOfficerId: [null],
      reportType: [1, Validators.required],
    });

    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      ["customerMarketingOfficerId"].forEach((field) => {
        if (value[field] === "") {
          this.searchForm.get(field)?.patchValue(null, { emitEvent: false });
          this.handleFilterChange(field);
        }
      });
    });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    this.searchForm.patchValue({ [fieldName]: null });
    this.handleFilterChange(fieldName);
  }

  getAllFinancialYears(): void {
    this.financialYearService.getAllFinancialYears().subscribe((res) => {
      this.financialYears = res?.data?.item1;
      this.generateYearList();
    });
  }

  private generateYearList(): void {
    const currentYear = new Date().getFullYear();

    const startYear = currentYear - this.calculateYearDifference();
    const endYear = currentYear;

    for (let y = endYear; y >= startYear; y--) {
      this.yearList.push(y);
    }
  }

  calculateYearDifference(): number {
    const startYears = this.financialYears.map((fy) =>
      new Date(fy.startDate).getFullYear()
    );
    const minYear = Math.min(...startYears);
    const currentYear = new Date().getFullYear();
    const difference = currentYear - minYear;
    return difference;
  }

  handleFilterChange(fieldName: string): void {
    const value = this.searchForm?.value;
    switch (fieldName) {
      case "customerMarketingOfficerId":
        this.filterMarketingOfficers = this.marketingOfficers;
        break;
    }
  }

  getAllMarketingOfficers(): void {
    let employeeRequest = new EmployeeRequest();
    employeeRequest.page = -1;
    employeeRequest.departmentId = Department_Id_SALES_AND_MARKETING;
    this.employeeService.getEmployees(employeeRequest).subscribe((res) => {
      this.filterMarketingOfficers = this.marketingOfficers = res?.data?.item1;
    });
  }

  handleMarketingOfficerSearch(): void {
    const term = this.searchForm
      ?.get("customerMarketingOfficerId")
      ?.value?.toLowerCase();
    this.filterMarketingOfficers = this.marketingOfficers?.filter((option) =>
      (option.firstName + " " + option.lastName).toLowerCase().includes(term)
    );
  }

  getMarketingOfficerName(customerMarketingOfficerId: string) {
    if (!customerMarketingOfficerId) {
      return;
    }
    const marketingOfficerData = this.marketingOfficers?.find(
      (marketingOfficer) => marketingOfficer?.id === customerMarketingOfficerId
    );
    return (
      marketingOfficerData?.firstName + " " + marketingOfficerData?.lastName
    );
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
        environment.apiURL +
          "/Account/primary-month-wise-sales-collection-and-due-report-print",
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
          link.setAttribute(
            "download",
            "Month_Wise_Sales_Collection_And_Due_Report_Short.xlsx"
          );
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
