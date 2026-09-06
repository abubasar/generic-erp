import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-subsidiary-ledger",
  templateUrl: "./subsidiary-ledger.component.html",
  styleUrls: ["./subsidiary-ledger.component.scss"],
})
export class SubsidiaryLedgerComponent implements OnInit {
  searchForm: FormGroup;
  isLoading1: boolean = false;
  isLoading2: boolean = false;
  controlAccounts: ControlAccount[];
  filterControlAccounts: ControlAccount[];
  constructor(
    private fb: FormBuilder,
    private accountService: AccountService,
    private http: HttpClient
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.getAllControlAccounts();
  }
  private initializeForm() {
    this.searchForm = this.fb.group({
      accountId: [null, Validators.required],
      fromDate: [null, Validators.required],
      toDate: [null, Validators.required],
      reportType: [1, Validators.required],
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
      this.filterControlAccount(term.value || "");
    }
  }

  private filterControlAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filterControlAccounts = this.controlAccounts?.filter(
      (option) =>
        option.name.toLowerCase().includes(filterValue) ||
        option.code?.slice(-4).toLowerCase().includes(filterValue)
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
        environment.apiURL + "/Account/subsidiary-ledger-print",
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
          link.setAttribute("download", "Account-Statement_Report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
