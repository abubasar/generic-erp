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
  selector: "app-accounts-report",
  templateUrl: "./accounts-report.component.html",
  styleUrls: ["./accounts-report.component.scss"],
})
export class AccountsReportComponent implements OnInit {
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  isLoading3: boolean = false;
  // Define financial year date range here
  financialYears: FinancialYear[];
  currentFinancialYear: FinancialYear;
  financialYearStartDate: Date; // Jul 1, 2023
  financialYearEndDate: Date; // Jun 30, 2024
  currentFinancialId: string;

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    private financialYearService: FinancialYearService,
    private jwtAuth: JwtAuthService
  ) {}

  ngOnInit(): void {
    this.getFinancialYearDateRangeAndId();
    this.getAllFinancialYears();
  }

  private initializeForm() {
    this.searchForm = this.fb.group({
      level: [null, Validators.required],
      levelsToLoad: [null, Validators.required],
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

  print($event) {
    if ($event?.submitter?.name == "trial-balance-print") {
      this.isLoading1 = true;
    }
    if ($event?.submitter?.name == "balance-sheet-print") {
      this.isLoading2 = true;
    }
    if ($event?.submitter?.name == "income-statement-print") {
      this.isLoading3 = true;
    }
    this.http
      .post(
        environment.apiURL + "/Account/" + $event?.submitter?.name,
        this.searchForm.value,
        { responseType: "blob" }
      )
      .subscribe((response) => {
        this.isLoading1 = false;
        this.isLoading2 = false;
        this.isLoading3 = false;
        if (response.type === "text/plain") {
          window.alert("No Result Found");
          return;
        }
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
