import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { CostCenter } from "app/views/configuration/models/cost-center/cost-center.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { CostCenterService } from "app/views/configuration/services/cost-center.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-day-wise-account-ledger",
  templateUrl: "./day-wise-account-ledger.component.html",
  styleUrls: ["./day-wise-account-ledger.component.scss"],
})
export class DayWiseAccountLedgerComponent implements OnInit {
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  costCenters: CostCenter[];
  controlAccounts: ControlAccount[];
  filterControlAccounts: ControlAccount[];
  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private costCenterService: CostCenterService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.getAllControlAccounts();
    this.getAllCostCenters();
    this.initializeForm();
  }
  private initializeForm() {
    this.searchForm = this.fb.group({
      accountId: [null, Validators.required],
      costCenterId: [null],
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      reportType: [1, Validators.required],
    });
  }

  getAllCostCenters() {
    this.costCenterService.getAllCostCenters().subscribe((res) => {
      this.costCenters = res?.data.item1;
    });
  }

  /** -----------Start Autocomplete----------- */
  getAllControlAccounts() {
    this.accountService.getAllControlAccounts().subscribe((res) => {
      this.filterControlAccounts = this.controlAccounts = res?.data;
    });
  }

  onControlAccountChange(event: any): void {
    const name = event.target?.name;
    if (name === "accountId") {
      const term = this.searchForm.get("accountId");
      this.filterControlAccount(term?.value || "");
    }
  }

  private filterControlAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filterControlAccounts = this.controlAccounts?.filter((option) =>
      option?.name?.toLowerCase().includes(filterValue)
    );
  }

  getControlAccountName(controlAccountId: string) {
    if (!controlAccountId) {
      return;
    }
    const controlAccount = this.controlAccounts?.find(
      (account) => account?.id === controlAccountId
    );

    return controlAccount?.name + " (" + controlAccount.code + ")";
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "accountId") {
      this.searchForm?.get("accountId").setValue(null);
    }
  }

  /** ------------------End For Autocomplete---------- */

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
        environment.apiURL + "/Account/day-wise-account-ledger-print",
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
          link.setAttribute("download", "Account_Ledger_Day_Wise.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
