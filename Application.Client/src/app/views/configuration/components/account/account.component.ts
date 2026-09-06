import { HttpClient } from "@angular/common/http";
import { Component, OnInit } from "@angular/core";
import { FormBuilder, FormGroup } from "@angular/forms";
import { MatDialog } from "@angular/material/dialog";
import { MatTableDataSource } from "@angular/material/table";
import { Page_Size_Options } from "app/shared/consts/const";
import { ConfirmDialogModel } from "app/shared/models/confirm-dialog.model";
import { ConfirmDialogService } from "app/shared/services/confirm-dialog.service";
import { environment } from "environments/environment";
import { ToastrService } from "ngx-toastr";
import { AccountTypeRequest } from "../../models/account-type/account-type-request.model";
import { AccountType } from "../../models/account-type/account-type.model";
import { AccountRequest } from "../../models/account/account-request.model";
import { Account } from "../../models/account/account.model";
import { ParentAccount } from "../../models/account/parent-account.model";
import { AccountTypeService } from "../../services/account-type.service";
import { AccountService } from "../../services/account.service";
import { AccountFormComponent } from "./account-form/account-form.component";
@Component({
  selector: "app-account",
  templateUrl: "./account.component.html",
  styleUrls: ["./account.component.scss"],
})
export class AccountComponent implements OnInit {
  loading: boolean = true;
  panelOpenState: boolean;
  accountTypes: AccountType[];
  parentAccounts: ParentAccount[];
  parentNames: ParentAccount[];
  searchForm: FormGroup;
  displayedColumns: string[] = [
    "actions",
    "code",
    "name",
    "accountTypeId",
    "level",
    "parentId",
    "isControlAccount",
  ];
  dataSource: MatTableDataSource<Account>;
  totalCount: number;
  accountRequest = new AccountRequest();
  pageSizeOptions: [] = Page_Size_Options;
  constructor(
    private accountService: AccountService,
    private accountTypeService: AccountTypeService,
    public dialog: MatDialog,
    private fb: FormBuilder,
    public toastr: ToastrService,
    private http: HttpClient,
    private confirmDialogService: ConfirmDialogService
  ) {}

  ngOnInit(): void {
    this.getAccounts(this.accountRequest);
    this.getAllAccountTypes();
    this.getParentAccounts();
    this.initializeForm();
  }

  initializeForm() {
    this.searchForm = this.fb.group({
      keyword: [""],
      accountTypeId: [null],
      parentId: [null],
    });
  }

  getAllAccountTypes(): void {
    let accountTypeRequest = new AccountTypeRequest();
    accountTypeRequest.page = -1;
    this.accountTypeService
      .getAccountTypes(accountTypeRequest)
      .subscribe((res) => {
        this.accountTypes = res?.data?.item1;
      });
  }

  getParentAccounts(): void {
    this.accountService.getParentAccounts().subscribe((res) => {
      this.parentAccounts = res?.data;
    });
  }

  onSelectedAccountType(id: string) {
    this.searchForm.get("parentId").reset();
    this.parentNames = this.parentAccounts?.filter(
      (item) => item.accountTypeId === id
    );
  }

  getAccounts(request: AccountRequest): void {
    this.accountService.getAccounts(request).subscribe((res) => {
      this.dataSource = new MatTableDataSource<Account>(res?.data?.item1);
      this.totalCount = res?.data?.item2;
      this.loading = false;
    });
  }

  remove(id): void {
    this.accountService.deleteAccount(id).subscribe((res) => {
      if (res?.succeeded) {
        this.getAccounts(this.accountRequest);
        this.toastr.info(res?.message);
      } else {
        this.toastr.error(res?.message);
      }
    });
  }
  openForm(account?: Account): void {
    const dialogRef = this.dialog.open(AccountFormComponent, {
      disableClose: true,
      data: account,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) {
        this.getAccounts(this.accountRequest);
      }
    });
  }

  // Function to confirm deletion before calling the remove function
  confirmDelete(id: string, code: string = "") {
    // Open a confirmation dialog with a custom message and callback function
    const data: ConfirmDialogModel = {
      title: "Confirm Remove",
      message: `Deleting this item ${code} will affect related data. Confirm deletion?`,
    };
    this.confirmDialogService.confirmDialog(
      id,
      this.remove.bind(this),
      data.message
    );
  }

  reload() {
    this.loading = true;
    this.searchForm.reset();
    this.accountRequest = new AccountRequest();
    this.getAccounts(this.accountRequest);
  }

  onSearch() {
    this.loading = true;
    this.updateAccountRequest();
    this.getAccounts(this.accountRequest);
  }

  onPageChange(pageEvent) {
    this.updateAccountRequest(pageEvent.pageIndex, pageEvent.pageSize);
    this.getAccounts(this.accountRequest);
  }

  private updateAccountRequest(
    pageIndex = this.accountRequest.page,
    pageSize = this.accountRequest.rowsPerPage
  ) {
    this.accountRequest = {
      ...this.accountRequest,
      ...this.searchForm.value,
      page: pageIndex,
      rowsPerPage: pageSize,
    };
  }
  printPdf() {
    this.http
      .post(environment.apiURL + "/Account/print", this.searchForm.value, {
        responseType: "blob",
      })
      .subscribe((response) => {
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
