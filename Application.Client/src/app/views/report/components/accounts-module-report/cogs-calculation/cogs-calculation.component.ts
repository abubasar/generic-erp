import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { UserProfile } from "app/shared/models/user-profile-model";
import { JwtAuthService } from "app/shared/services/auth/jwt-auth.service";
import { inFinancialYearValidator } from "app/shared/validators/in-financial-year-validator";
import { FinancialYear } from "app/views/configuration/models/financial-year/financial-year.model";
import { FinancialYearService } from "app/views/configuration/services/financial-year.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-cogs-calculation",
  templateUrl: "./cogs-calculation.component.html",
  styleUrls: ["./cogs-calculation.component.scss"],
})
export class CogsCalculationComponent implements OnInit {
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  financialYears: FinancialYear[];
  currentFinancialYear: FinancialYear;
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialId: string;

  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private financialYearService: FinancialYearService,
    private jwtAuth: JwtAuthService
  ) {}

  ngOnInit(): void {
    this.getFinancialYearDateRangeAndId();
    this.getAllFinancialYears();
  }
  initializeForm() {
    this.searchForm = this.fb.group({
      financialYearId: [this.currentFinancialId, Validators.required],
      fromDate: [
        null,
        [
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ],
      ],
      toDate: [
        null,
        [
          Validators.required,
          inFinancialYearValidator(
            this.financialYearStartDate,
            this.financialYearEndDate
          ),
        ],
      ],
      reportType: [1, Validators.required],
    });
  }

  private getFinancialYearDateRangeAndId() {
    this.jwtAuth.userProfile.subscribe((res: UserProfile) => {
      this.financialYearStartDate = new Date(res.fystartdate);
      this.financialYearEndDate = new Date(res.fyenddate);
      this.currentFinancialId = res.fyid;
      this.initializeForm(); // Initialize form once the date range and ID are set
    });
  }

  handleFinancialYearChange(financialYearId: string) {
    this.currentFinancialId = financialYearId;
    const selectedYear = this.financialYears.find(
      (year) => year.id == financialYearId
    );
    if (selectedYear) {
      this.updateFinancialYear(selectedYear);
      this.resetDateFields();
      this.updateDateValidators();
    }
  }

  private updateFinancialYear(selectedYear: FinancialYear) {
    this.financialYearStartDate = new Date(selectedYear.startDate);
    this.financialYearEndDate = new Date(selectedYear.endDate);
    this.searchForm.get("financialYearId").setValue(this.currentFinancialId);
  }

  private resetDateFields() {
    this.searchForm.get("fromDate").setValue(null);
    this.searchForm.get("toDate").setValue(null);
  }

  private updateDateValidators() {
    const fromDateControl = this.searchForm.get("fromDate");
    const toDateControl = this.searchForm.get("toDate");

    fromDateControl.setValidators([
      Validators.required,
      inFinancialYearValidator(
        this.financialYearStartDate,
        this.financialYearEndDate
      ),
    ]);
    toDateControl.setValidators([
      Validators.required,
      inFinancialYearValidator(
        this.financialYearStartDate,
        this.financialYearEndDate
      ),
    ]);

    fromDateControl.updateValueAndValidity();
    toDateControl.updateValueAndValidity();
  }

  getAllFinancialYears() {
    this.financialYearService.getAllFinancialYears().subscribe((res) => {
      this.financialYears = res?.data.item1 || [];
      this.currentFinancialYear = res?.data.item1.find(
        (x) => x.id == this.currentFinancialId
      );
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
        environment.apiURL + "/Cogs/cogs-calculation-print",
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
          link.setAttribute("download", "COGS_Calculation_Report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
