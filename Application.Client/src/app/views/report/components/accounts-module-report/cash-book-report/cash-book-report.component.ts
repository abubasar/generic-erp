import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { Control_Accounts_Parent_Id } from "app/shared/consts/const";
import { ControlAccount } from "app/views/configuration/models/account/control-account.model";
import { AccountService } from "app/views/configuration/services/account.service";
import { environment } from "environments/environment";

@Component({
  selector: "app-cash-book-report",
  templateUrl: "./cash-book-report.component.html",
  styleUrls: ["./cash-book-report.component.scss"],
})
export class CashBookReportComponent implements OnInit {
  constructor(
    private http: HttpClient,
    private fb: FormBuilder,
    private accountService: AccountService
  ) {}
  searchForm: FormGroup;
  cashBankAccounts: ControlAccount[];
  filteredCashBankAccounts: ControlAccount[];
  isLoading1: boolean = false;
  isLoading2: boolean = false;

  ngOnInit(): void {
    this.initializeForm();
    this.getAllCashBankAccounts(Control_Accounts_Parent_Id);
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      fromDate: ["", Validators.required],
      toDate: ["", Validators.required],
      cashBookAccountId: [null, Validators.required],
      reportType: [1, Validators.required],
    });
    /**
     * * this code will be executed for instant changes of SearchForm's field
     */
    this.searchForm.valueChanges.subscribe((value) => {
      // Convert empty string to null
      if (value.cashBookAccountId === "") {
        this.searchForm
          .get("cashBookAccountId")
          ?.patchValue(null, { emitEvent: false });
      }
    });
  }

  getCashBankAccountName(cashBankAccountId: string) {
    if (!cashBankAccountId) {
      return;
    }
    const cashBankAccount = this.cashBankAccounts?.find(
      (cashBankAccount) => cashBankAccount?.id === cashBankAccountId
    ); //|| this.data?.cashBankAccount;
    return cashBankAccount?.name;
  }

  cashBankLoading: boolean = false;
  getAllCashBankAccounts(parentId) {
    this.cashBankLoading = true;
    this.accountService
      .getControlAccountsByParentId(parentId)
      .subscribe((res) => {
        this.filteredCashBankAccounts = this.cashBankAccounts = res?.data;
        this.cashBankLoading = false;
      });
  }

  clearInput(evt: any, fieldName: string): void {
    evt.stopPropagation();
    if (fieldName === "cashBookAccountId") {
      this.searchForm?.get("cashBookAccountId").setValue(null);
    }
  }

  handleCashBankAccountSearch(event: any): void {
    const name = event.target?.name;
    if (name === "cashBookAccountId") {
      const term = this.searchForm.get("cashBookAccountId");
      this.filterCashBankAccount(term.value || "");
    }
  }

  private filterCashBankAccount(value: string) {
    const filterValue = value.toLowerCase();
    this.filteredCashBankAccounts = this.cashBankAccounts?.filter((option) =>
      option.name.toLowerCase().includes(filterValue)
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
          "/Account/cash-book-transaction-detail-ledger-print",
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
          link.setAttribute("download", "cash_book_report.xlsx");
          document.body.appendChild(link);
          link.click();
          this.isLoading2 = false;
        }
      });
  }
}
